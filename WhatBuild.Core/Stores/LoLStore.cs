using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.Utils;
using WhatBuild.Core.ViewModels;

namespace WhatBuild.Core.Stores
{
    public class LoLStore : IStore
    {
        public string BaseUrlAPI { get; set; }

        public string Version { get; set; }

        public List<ChampionViewModel> Champions { get; set; }

        public List<ItemViewModel> Items { get; set; }

        public async Task InitAsync()
        {
            // 1- Fetch metadata
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadataAsync();

            BaseUrlAPI = metadata.BaseUrlAPI;
            Version = metadata.Version;

            // 2- Fetch champions and items
            var fetchChampionsTask = LoLAPIUtil.FetchAllChampionsAsync(BaseUrlAPI, Version);
            var fetchItemsTask = LoLAPIUtil.FetchAllItemAsync(BaseUrlAPI, Version);
            await Task.WhenAll(fetchChampionsTask, fetchItemsTask);

            Champions = fetchChampionsTask.Result;
            Items = fetchItemsTask.Result;
        }
    }
}
