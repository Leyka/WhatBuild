using Dasync.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private Action<string> OutputLoggerAction { get; set; }

        public ItemSetGenerator(ConfigurationViewModel config, Action<string> logAction)
        {
            BuildSourceName = typeof(T).Name;
            Configuration = config;
            OutputLoggerAction = logAction;
        }

        /// <summary>
        /// Create a Item Set JSON file for all champions under LoL install path 
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public async Task GenerateItemSetForAllChampionsAsync(CancellationToken cancelToken = default)
        {
            LoLStore loLStore = await StoreManager<LoLStore>.GetAsync();
            List<ChampionViewModel> champions = loLStore.Champions;

            string startingLog = LoggerUtil.FormatLogByBuildSource(BuildSourceName, "Start downloading item sets...");
            OutputLoggerAction(startingLog);

            // Parallel downloads 
            await champions.ParallelForEachAsync(async champion =>
            {
                await TryGenerateItemSetByChampion(champion, cancelToken);
            },
            maxDegreeOfParallelism: MAX_CONNECTIONS_PER_DOMAIN,
            cancellationToken: cancelToken);

            string finishedLog = LoggerUtil.FormatLogByBuildSource(BuildSourceName, "Finished");
            OutputLoggerAction(finishedLog);
        }

        /// <summary>
        /// Generate item set by champion and returns true if successfully generated
        /// </summary>
        /// <param name="champion"></param>
        /// <param name="cancelToken"></param>
        /// <returns>True if successfully generated item set</returns>
        private async Task TryGenerateItemSetByChampion(ChampionViewModel champion, CancellationToken cancelToken)
        {
            try
            {
                await GenerateItemSetByChampion(champion, cancelToken);

                string log = LoggerUtil.FormatLogByBuildSource(BuildSourceName, "Succefully downloaded item set", champion.Name);
                OutputLoggerAction(log);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                string errLog = LoggerUtil.FormatLogByBuildSource(BuildSourceName, "Failed to download item set", champion.Name, e.Message);
                OutputLoggerAction(errLog);
            }
        }

        private async Task GenerateItemSetByChampion(ChampionViewModel champion, CancellationToken cancelToken)
        {
            // Create a unique instance of BuildSource to be thread safe
            // I'm afraid of having corruption with a shared "Document" property if I use a single shared instance
            IBuildSource buildSource = (T)Activator.CreateInstance(typeof(T));
            await buildSource.InitAsync(champion.Name);

            if (!buildSource.IsValidContent())
            {
                throw new InvalidOperationException("Invalid content");
            }

            LoLItemSetViewModel itemSetViewModel = ItemSetUtil.CreateItemSetPerChampion(buildSource, champion, Configuration.ShowSkillsOrder);

            if (itemSetViewModel == null)
            {
                throw new InvalidOperationException("LoLItemSetViewModel is null");
            }

            // Create Item set JSON file into LoL directory
            string itemSetDir = LoLPathUtil.CreateItemSetDirectory(Configuration.LoLDirectory, champion.Name);

            string itemSetFileName = ItemSetUtil.GetFormattedItemSetFileName(buildSource, Configuration.ApplicationPrefixName);
            string itemSetAbsolutePath = Path.Combine(itemSetDir, itemSetFileName);

            await FileUtil.CreateJsonFileAsync(itemSetAbsolutePath, itemSetViewModel, cancelToken);
        }
    }
}
