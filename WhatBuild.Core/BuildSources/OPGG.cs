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
using System.Text.RegularExpressions;

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

        private SortedList<ItemCategory, int> _itemCategoryOrders;
        private SortedList<ItemCategory, int> ItemCategoriesDictionary
        {
            get
            {
                if (_itemCategoryOrders == null)
                {
                    _itemCategoryOrders = GetItemCategoriesOrdered();
                }

                return _itemCategoryOrders;
            }
        }

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

        public string GetSourceName()
        {
            return "OP.GG";
        }

        public string GetVersion()
        {
            HtmlNode nodeVersion = Document.DocumentNode.SelectSingleNode(Selector.Version);
            string version = nodeVersion.InnerText.Split(':').Last();

            // Clean string with \t and \n
            version = StringUtil.CleanString(version);
            return version;
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
            formattedFirstSkills = StringUtil.CleanString(formattedFirstSkills);

            return formattedFirstSkills;
        }

        #endregion

        #region Item builds

        public List<int> GetStarterItemIds()
        {
            return GetItemsIdsByCategory(ItemCategory.Starter);
        }

        public List<int> GetCoreItemIds()
        {
            return GetItemsIdsByCategory(ItemCategory.Core);
        }
        public List<int> GetBootItemIds()
        {
            return GetItemsIdsByCategory(ItemCategory.Boots);
        }

        public List<int> GetExtraItemIds()
        {
            return null;
        }

        public bool HasBootsCategory()
        {
            return GetBootItemIds()?.Count > 0;
        }

        public bool HasExtraCategory()
        {
            return GetExtraItemIds()?.Count > 0;
        }

        #region Helpers
        private List<int> GetItemsIdsByCategory(ItemCategory category)
        {
            if (!ItemCategoriesDictionary.ContainsKey(category))
            {
                return null;
            }

            int startIndex = ItemCategoriesDictionary[category];
            int endIndex = GetNextItemCategoryRowIndex(category);

            return GetUniqueItemIds(startIndex, endIndex);
        }

        /// <summary>
        /// Returns a unique list of item ids based on the category index
        /// In OP.GG, the item ID can be found in "1234.png" in <img> tag 
        /// Note: A category is 
        /// </summary>
        /// <param name="startIndexCategory">Where the category starts</param>
        /// <param name="endIndexCategory"></param>
        /// <returns></returns>
        private List<int> GetUniqueItemIds(int startIndexCategory, int endIndexCategory)
        {
            // Only unique ID will be added, therefore -> hashset
            HashSet<int> uniqueItemIds = new HashSet<int>();

            HtmlNodeCollection allItemCategorieNodes = Document.DocumentNode.SelectNodes(Selector.AllItemCategories);
            for (int i = startIndexCategory; i < endIndexCategory; i++)
            {
                HtmlNode itemCategory = allItemCategorieNodes[i];

                HtmlNodeCollection itemNodes = itemCategory.SelectNodes(Selector.Items);

                // Get Item id from item node
                IEnumerable<int> itemIds = itemNodes.Select(itemNode =>
                {
                    string srcImg = itemNode.Attributes[0].Value;
                    return GetItemIdFromImageSrc(srcImg);
                });

                uniqueItemIds.UnionWith(itemIds);
            }

            return uniqueItemIds.ToList();
        }

        private SortedList<ItemCategory, int> GetItemCategoriesOrdered()
        {
            Dictionary<ItemCategory, int> itemCategories = new Dictionary<ItemCategory, int>
            {
                { ItemCategory.Starter, GetRowIndexByItemCategory(ItemCategory.Starter) },
                { ItemCategory.Boots, GetRowIndexByItemCategory(ItemCategory.Boots) },
                { ItemCategory.Core, GetRowIndexByItemCategory(ItemCategory.Core) },
            };

            var orderedItemCategories =
                itemCategories
                    .Where(x => x.Value >= 0) // Filter from Row index -1, which doesn't exist
                    .ToDictionary(x => x.Key, x => x.Value);

            // Returns sorted list by RowIndex
            return new SortedList<ItemCategory, int>(
                orderedItemCategories,
                Comparer<ItemCategory>.Create((k1, k2) => itemCategories[k1].CompareTo(itemCategories[k2]))
            );
        }

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
                return -1;
            }

            return nodes.IndexOf(foundNode);
        }

        private int GetNextItemCategoryRowIndex(ItemCategory current)
        {
            int indexCurrent = ItemCategoriesDictionary.IndexOfKey(current);

            // Check if last category item, returns the item categories size
            bool isLastIndex = indexCurrent == ItemCategoriesDictionary.Count - 1;
            if (isLastIndex)
            {
                return GetTotalItemCategories();
            }

            // Return next row index 
            int nextCategoryRowIndex = ItemCategoriesDictionary.Values[indexCurrent + 1];
            return nextCategoryRowIndex;
        }

        private int GetTotalItemCategories()
        {
            return Document.DocumentNode.SelectNodes(Selector.AllItemCategories).Count;
        }

        private int GetItemIdFromImageSrc(string imageSrc)
        {
            Match m = new Regex(@".*\/(\d+).*").Match(imageSrc);
            if (m.Success)
            {
                return int.Parse(m.Groups[1].Value);
            }

            throw new NullReferenceException($"Image ID was not found with imageSrc: {imageSrc}");
        }

        #endregion

        #endregion
    }
}
