using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using WhatBuild.Core.Utils;
using WhatBuild.Core.ViewModels;
using Xunit;


namespace WhatBuild.Tests.Utils
{
    public class LoLAPIUtilTest
    {
        [Fact]
        public async Task FetchAPIMetadata_Version_NotEmpty()
        {
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadataAsync();
            string version = metadata.Version;

            Assert.NotNull(version);
            Assert.Matches(@"\d+.\d+.\d+", version);
        }

        [Fact]
        public async Task FetchAPIMetadata_BaseUrlAPI_NotEmpty()
        {
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadataAsync();
            string url = metadata.BaseUrlAPI;

            Assert.NotNull(url);
        }

        [Fact]
        public async Task GetFormattedAPIUrl_Url_IsAPIUrlValid()
        {
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadataAsync();

            // Test with "champion" as datatype but any other datatype would be same
            string formattedAPIUrl = LoLAPIUtil.GetFormattedAPIUrl(metadata.BaseUrlAPI, metadata.Version, "champion");

            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(formattedAPIUrl);

            Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.Content.Headers.ContentType.MediaType == "application/json");
        }

        [Fact]
        public async Task FetchAllChampions_LoLChampionViewModel_ReturnsAllChampions()
        {
            LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadataAsync();

            List<ChampionViewModel> champions = await LoLAPIUtil.FetchAllChampionsAsync(metadata.BaseUrlAPI, metadata.Version);

            Assert.True(champions.Count > 0);
        }

        //[Fact]
        //public async Task FetchAllChampions_LoLChampionViewModel_DoesNotContainMonkeyKing()
        //{
        //    LoLMetadataViewModel metadata = await LoLAPIUtil.FetchAPIMetadataAsync();

        //    List<ChampionViewModel> champions = await LoLAPIUtil.FetchAllChampionsAsync(metadata.BaseUrlAPI, metadata.Version);

        //    // Test case where Wukong is named MonkeyKing (shouldn't pass)
        //    Assert.DoesNotContain(champions, champion => champion.Name == "MonkeyKing");
        //    Assert.Contains(champions, champion => champion.Name == "Wukong");
        //}
    }
}
