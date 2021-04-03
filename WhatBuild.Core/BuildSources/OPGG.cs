using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.Utils;
using WhatBuild.Core.ViewModels;
using WhatBuild.Core.Stores;

namespace WhatBuild.Core.BuildSources
{
    /// <summary>
    /// Fetch the most popular position build
    /// </summary>
    /// <see cref="https://op.gg"/>
    public class OPGG : IBuildSource
    {
        private readonly string _baseUrl = "https://www.op.gg/champion/";

        private HtmlDocument Document { get; set; }

        private SelectorViewModel Selector { get; set; }

        public async Task InitAsync(string championName)
        {
            Task<HtmlDocument> fetchHtmlTask = FetchHtmlAsync(championName);
            Task<SelectorViewModel> fetchSelectorTask = FetchSelectorDataAsync();

            await Task.WhenAll(fetchHtmlTask, fetchSelectorTask);

            Document = fetchHtmlTask.Result;
            Selector = fetchSelectorTask.Result;
        }

        private Task<HtmlDocument> FetchHtmlAsync(string championName)
        {
            string championUrl = _baseUrl + championName;

            HtmlWeb web = new HtmlWeb();

            // TODO/v2: Handle multiple positions
            // Right now, 1 document is assigned to one champion per popular position
            return web.LoadFromWebAsync(championUrl);
        }

        private async Task<SelectorViewModel> FetchSelectorDataAsync()
        {
            SelectorStore selectorStore = await StoreManager<SelectorStore>.GetAsync();

            return selectorStore.SelectorsDictionary[BuildSourceType.OPGG];
        }

        #region Positions
        public ChampionPosition GetChampionPosition()
        {
            HtmlNode nodePosition = Document.DocumentNode.SelectSingleNode(Selector.ChampionPosition);

            if (nodePosition == null)
            {
                throw new NullReferenceException("[OP.GG] Node position was not found");
            }

            string position = nodePosition.Attributes["data-position"]?.Value;

            return ChampionPositionUtil.Parse(position);
        }
        #endregion

        #region Skills
        /// <summary>
        /// Returns the 4 first skills to level up, and general skills right next to it
        /// Example: "W.Q.E.Q [Q -> W -> E]"
        /// </summary>
        public string GetFormattedSkills()
        {
            string firstSkills = GetFirstSkillsOrder();
            string generalSkills = GetGeneralSkillsOrder();

            return $"{firstSkills} [{generalSkills}]";
        }

        /// <summary>
        /// Example: Q -> W -> E
        /// </summary>
        private string GetGeneralSkillsOrder()
        {
            HtmlNodeCollection nodes = Document.DocumentNode.SelectNodes(Selector.GeneralSkillsOrder);

            if (nodes.Count != 3)
            {
                throw new InvalidOperationException("General skills are only composed of 3 main skills");
            }

            return $"{nodes[0].InnerText} -> {nodes[1].InnerText} -> {nodes[2].InnerText}";
        }

        /// <summary>
        /// Returns the 4 first skills to level up
        /// Example: W.Q.E.Q
        /// </summary>
        private string GetFirstSkillsOrder()
        {
            HtmlNodeCollection nodes = Document.DocumentNode.SelectNodes(Selector.FirstSkillsOrder);

            if (nodes.Count < 4)
            {
                throw new InvalidOperationException($"There should be at least 4 first skills to level");
            }

            string formattedFirstSkills =
                $"{nodes[0].InnerText}.{nodes[1].InnerText}.{nodes[2].InnerText}.{nodes[3].InnerText}";

            // We need to clean string (full of \n and \t)
            formattedFirstSkills = formattedFirstSkills
                .Replace("\t", string.Empty)
                .Replace("\n", string.Empty);

            return formattedFirstSkills;
        }

        #endregion

        #region Item builds

        public List<int> GetStarterItemIds()
        {
            // TODO/v2: Automatically fetch next category without hard coding it
            // Starter items start from the Starter until we arrive to Recommended/Core items
            int startIndex = GetRowIndexByItemCategory(ItemCategory.Starter);
            int endIndex = GetRowIndexByItemCategory(ItemCategory.Core);

            for (int i = startIndex; i < endIndex; i++)
            {
                // TODO: Continue
                // Get Items by img path
            }

            throw new NotSupportedException();
        }

        public List<int> GetCoreItemIds()
        {
            throw new NotSupportedException();
        }

        public List<int> GetExtraItemIds()
        {
            throw new NotSupportedException();
        }

        public List<int> GetBootItemIds()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Helpers
        /// <summary>
        /// After looking in table containing items, returns the index row of each wanted category
        /// </summary>
        /// <param name="category">Category type</param>
        /// <returns>Index row</returns>
        private int GetRowIndexByItemCategory(ItemCategory category)
        {
            HtmlNodeCollection nodes = Document.DocumentNode.SelectNodes(Selector.AllItemCategories);

            // Set keyword to look for, depending on category
            string keyword = category switch
            {
                ItemCategory.Starter => "starter",
                ItemCategory.Boots => "boots",
                ItemCategory.Core => "recommended",
                ItemCategory.Extra => "recommended",
                _ => throw new NotImplementedException(),
            };

            HtmlNode foundNode = nodes.FirstOrDefault(n => n.InnerText.ToLower().Contains(keyword));
            if (foundNode == null)
            {
                throw new NullReferenceException($"Index row was not found with keyword: {keyword}");
            }

            return nodes.IndexOf(foundNode);
        }

        private int GetTotalItemCategories()
        {
            return Document.DocumentNode.SelectNodes(Selector.AllItemCategories).Count;
        }
        #endregion
    }
}
