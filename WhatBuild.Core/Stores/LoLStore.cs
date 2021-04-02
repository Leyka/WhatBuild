using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Helpers;
using WhatBuild.Core.ViewModels;

namespace WhatBuild.Core.Stores
{
    public class LoLStore
    {
        public string BaseUrlAPI { get; }

        public string Version { get; }

        private List<ChampionViewModel> _champions;
        public List<ChampionViewModel> Champions
        {
            get
            {
                if (_champions == null)
                {
                    _champions = LoLAPIUtil.FetchAllChampionsAsync(BaseUrlAPI, Version).Result; // Note: Blocking sync function
                }

                return _champions;
            }
        }

        private List<ItemViewModel> _items;
        public List<ItemViewModel> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = LoLAPIUtil.FetchAllItemAsync(BaseUrlAPI, Version).Result; // Note: Blocking sync function
                }

                return _items;
            }
        }

        public LoLStore()
        {
            LoLMetadataViewModel metadata = LoLAPIUtil.FetchAPIMetadataAsync().Result;
            BaseUrlAPI = metadata.BaseUrlAPI;
            Version = metadata.Version;
        }
    }
}
