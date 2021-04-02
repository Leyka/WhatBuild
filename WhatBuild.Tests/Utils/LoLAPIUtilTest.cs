using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhatBuild.Core.Helpers;
using WhatBuild.Core.ViewModels;
using WhatBuild.Core.ViewModels.LoL;
using Xunit;

namespace WhatBuild.Tests.Helpers
{
    public class LoLAPIUtilTest
    {
        [Fact]
        public async Task FetchAPIMetadata_Version_ReturnsLatestVersion()
        {
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadata();
            string version = metadata.Version;

            Assert.NotNull(version);
            Assert.Matches(@"\d+.\d+.\d+", version);
        }

        [Fact]
        public async Task FetchAPIMetadata_BaseUrlAPI_ReturnsKnownUrl()
        {
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadata();
            string url = metadata.BaseUrlAPI;

            Assert.NotNull(url);
            // If the URL changes, I prefer to be advised here 
            Assert.Equal("https://ddragon.leagueoflegends.com/cdn", url);
        }

        [Fact]
        public async Task FetchAllChampions_LoLChampionViewModel_ReturnsAllChampions()
        {
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadata();

            List<ChampionViewModel> champions = await LoLAPIUtil.FetchAllChampionsAsync(metadata.BaseUrlAPI, metadata.Version);

            Assert.True(champions.Count > 0);
        }

        [Fact]
        public async Task FetchAllItemsIds_ItemId_ReturnsAllItemsWithValidIds()
        {
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadata();

            List<ItemViewModel> items = await LoLAPIUtil.FetchAllItemAsync(metadata.BaseUrlAPI, metadata.Version);

            Assert.True(items.Count > 0);

            // Ensure that we have ID > 0, test with the first element
            Assert.True(items[0].Id > 0);
        }
    }
}
