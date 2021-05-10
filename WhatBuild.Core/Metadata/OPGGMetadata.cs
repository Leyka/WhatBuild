using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.Stores;
using WhatBuild.Core.Utils;
using WhatBuild.Core.ViewModels;

namespace WhatBuild.Core.BuildSources
{
    public class OPGGMetadata : IMetadata
    {
        private MetadataSelectorViewModel MetadataSelector { get; set; }
        private HtmlDocument Document { get; set; }

        public async Task InitAsync()
        {
            Task<HtmlDocument> fetchHtmlTask = FetchHtmlAsync();
            Task<SelectorViewModel> fetchSelectorTask = FetchSelectorDataAsync();

            await Task.WhenAll(fetchHtmlTask, fetchSelectorTask);

            Document = fetchHtmlTask.Result;
            MetadataSelector = fetchSelectorTask.Result.Metadata;
        }

        private Task<HtmlDocument> FetchHtmlAsync()
        {
            HtmlWeb web = new HtmlWeb();

            string url = "https://na.op.gg/champion/statistics";

            return web.LoadFromWebAsync(url);
        }

        private async Task<SelectorViewModel> FetchSelectorDataAsync()
        {
            SelectorStore selectorStore = await StoreManager<SelectorStore>.GetAsync();

            return selectorStore.SelectorsDictionary[BuildSourceType.OPGG];
        }

        public string GetSourceName()
        {
            return "OP.GG";
        }

        public string GetVersion()
        {
            HtmlNode nodeVersion = Document.DocumentNode.SelectSingleNode(MetadataSelector.Version);
            string version = nodeVersion.InnerText.Split(':').Last();

            // Clean string with \t and \n
            version = StringUtil.CleanString(version);
            return version;
        }

        public Dictionary<string, ChampionPosition> GetChampionsWithPositions()
        {
            throw new NotImplementedException();
        }


    }
}
