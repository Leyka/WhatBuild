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

        public ConfigurationViewModel Configuration { get; set; }

        public ItemSetGenerator(ConfigurationViewModel config)
        {
            Configuration = config;
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

            await champions.ParallelForEachAsync(async champion =>
            {
                string sourceName = typeof(T).Name;

                try
                {
                    await GenerateItemSetByChampion(champion, cancelToken);
                    Debug.WriteLine($"{sourceName}: Succefully downloaded item set for {champion.Name}");
                }
                catch (Exception e)
                {
                    // TODO: Add logger
                    // Silent exception 
                    Debug.WriteLine($"{sourceName}: Failed to download item set for {champion.Name}. Error: {e.Message}");
                }
            },
            maxDegreeOfParallelism: MAX_CONNECTIONS_PER_DOMAIN,
            cancellationToken: cancelToken);
        }

        private async Task GenerateItemSetByChampion(ChampionViewModel champion, CancellationToken cancelToken)
        {
            // Create a unique instance of BuildSource to be thread safe
            // I'm afraid of having corruption with a shared "Document" property if I use a single shared instance
            IBuildSource buildSource = (T)Activator.CreateInstance(typeof(T));
            await buildSource.InitAsync(champion.Name);

            LoLItemSetViewModel itemSetViewModel = ItemSetUtil.CreateItemSetPerChampion(buildSource, champion, Configuration.ShowSkillsOrder);

            if (itemSetViewModel != null)
            {
                // Create Item set JSON file into LoL directory
                string itemSetDir = LoLPathUtil.CreateItemSetDirectory(Configuration.LoLDirectory, champion.Name);

                string itemSetFileName = ItemSetUtil.GetFormattedItemSetFileName(buildSource, Configuration.ApplicationPrefixName);
                string itemSetAbsolutePath = Path.Combine(itemSetDir, itemSetFileName);

                await FileUtil.CreateJsonFileAsync(itemSetAbsolutePath, itemSetViewModel, cancelToken);
            }
        }
    }
}
