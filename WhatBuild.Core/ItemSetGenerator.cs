using Dasync.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.Stores;
using WhatBuild.Core.Utils;
using WhatBuild.Core.ViewModels;

namespace WhatBuild.Core
{
    public class ItemSetGenerator<T> where T : IBuildSource
    {
        private const int MAX_CONNECTIONS_PER_DOMAIN = 6;

        public string BuildSourceName { get; }

        public ConfigurationViewModel Configuration { get; set; }

        private Action<string> LogHandler { get; set; }

        private Action<double> UpdateProgressHandler { get; set; }

        public ItemSetGenerator(ConfigurationViewModel config, Action<string> logHandler, Action<double> updateProgressHandler)
        {
            BuildSourceName = typeof(T).Name;
            Configuration = config;
            LogHandler = logHandler;
            UpdateProgressHandler = updateProgressHandler;
        }

        /// <summary>
        /// Create a Item Set JSON file for all champions under LoL install path 
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns>List of failed champion item sets</returns>
        public async Task<List<ChampionViewModel>> GenerateItemSetForAllChampionsAsync(CancellationToken cancelToken = default)
        {
            LoLStore loLStore = await StoreManager<LoLStore>.GetAsync();
            List<ChampionViewModel> champions = loLStore.Champions;

            string startingLog = LoggerUtil.FormatLogByBuildSource(BuildSourceName, "Start downloading item sets...");
            LogHandler(startingLog);

            // Store the failed champions to return later
            List<ChampionViewModel> failedChampions = new List<ChampionViewModel>();

            // Parallel downloads 
            await champions.ParallelForEachAsync(async (champion, index) =>
            {
                bool success = await TryGenerateItemSetByChampion(champion, cancelToken);

                if (!success)
                {
                    failedChampions.Add(champion);
                }

                double progress = (double)(index + 1) / champions.Count;
                UpdateProgressHandler(progress * 100);
            },
            maxDegreeOfParallelism: MAX_CONNECTIONS_PER_DOMAIN,
            cancellationToken: cancelToken);

            LogHandler(LoggerUtil.FormatLogByBuildSource(BuildSourceName, "Finished"));

            return failedChampions;
        }

        /// <summary>
        /// Generate item set by champion and returns true if successfully generated
        /// </summary>
        /// <param name="champion"></param>
        /// <param name="cancelToken"></param>
        /// <returns>True if successfully generated item set</returns>
        private async Task<bool> TryGenerateItemSetByChampion(ChampionViewModel champion, CancellationToken cancelToken)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                // Mode = classic
                Task generateItemSetClassic = GenerateItemSetByChampion(champion, LoLMode.Classic, cancelToken);
                tasks.Add(generateItemSetClassic);

                // Mode = aram (?)
                if (Configuration.DownloadAramBuilds)
                {
                    Task generateItemSetAram = GenerateItemSetByChampion(champion, LoLMode.ARAM, cancelToken);
                    tasks.Add(generateItemSetAram);
                }

                await Task.WhenAll(tasks);

                // Log success
                string successMessage = "Succefully downloaded item set";
                if (Configuration.DownloadAramBuilds) successMessage += " + ARAM";
                string log = LoggerUtil.FormatLogByBuildSource(BuildSourceName, successMessage, champion.Name);
                LogHandler(log);

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                string errLog = LoggerUtil.FormatLogByBuildSource(BuildSourceName, "Failed to download item set", champion.Name, e.Message);
                LogHandler(errLog);

                return false;
            }
        }

        private async Task GenerateItemSetByChampion(ChampionViewModel champion, LoLMode mode, CancellationToken cancelToken)
        {
            // Create a unique instance of BuildSource to be thread safe
            // I'm afraid of having corruption with a shared "Document" property if I use a single shared instance
            IBuildSource buildSource = (T)Activator.CreateInstance(typeof(T));
            await buildSource.InitAsync(champion.Name, mode);

            if (!buildSource.IsValidContent())
            {
                throw new InvalidOperationException("Invalid content");
            }

            LoLItemSetViewModel itemSetViewModel = ItemSetUtil.CreateItemSetPerChampion(buildSource, champion, mode, Configuration.ShowSkillsOrder);

            if (itemSetViewModel == null)
            {
                throw new InvalidOperationException("LoLItemSetViewModel is null");
            }

            // Create Item set JSON file into LoL directory
            string itemSetDir = LoLPathUtil.CreateItemSetDirectory(Configuration.LoLDirectory, champion.Name);

            string itemSetFileName = ItemSetUtil.GetFormattedItemSetFileName(buildSource, mode, Configuration.ApplicationPrefixName);
            string itemSetAbsolutePath = Path.Combine(itemSetDir, itemSetFileName);

            await FileUtil.CreateJsonFileAsync(itemSetAbsolutePath, itemSetViewModel, cancelToken);
        }
    }
}
