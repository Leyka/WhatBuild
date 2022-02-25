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
        private HtmlDocument Document { get; set; }

        private SelectorViewModel Selector { get; set; }

        private Dictionary<ItemCategory, (int, int)> _itemCategoryOrders;

        private Dictionary<ItemCategory, (int, int)> ItemCategoriesDictionary
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

        public async Task InitAsync(string championName, LoLMode mode = LoLMode.Classic)
        {
            Task<SelectorViewModel> fetchSelectorTask = FetchSelectorDataAsync();
            await Task.WhenAll(fetchSelectorTask);
            Selector = fetchSelectorTask.Result;

            Task<HtmlDocument> fetchHtmlTask = FetchHtmlAsync(championName, mode);
            await Task.WhenAll(fetchHtmlTask);

            Document = fetchHtmlTask.Result;
        }

        private string GetUrl(LoLMode mode, string championName)
        {
            string classicLink = Selector.Links.Classic;
            string aramLink = Selector.Links.ARAM;

            return mode switch
            {
                LoLMode.Classic => String.Format(classicLink, championName),
                LoLMode.ARAM => String.Format(aramLink, championName),
                _ => throw new NotSupportedException()
            };
        }

        private Task<HtmlDocument> FetchHtmlAsync(string championName, LoLMode mode)
        {
            HtmlWeb web = new HtmlWeb();

            // TODO/v2: Handle multiple positions
            // Right now, 1 document is assigned to one champion per popular position
            string championUrl = GetUrl(mode, championName);
            return web.LoadFromWebAsync(championUrl);
        }

        private async Task<SelectorViewModel> FetchSelectorDataAsync()
        {
            SelectorStore selectorStore = await StoreManager<SelectorStore>.GetAsync();

            return selectorStore.SelectorsDictionary[BuildSourceType.OPGG];
        }

        public bool IsValidContent()
        {
            if (Document?.DocumentNode == null) return false;

            // We shouldn't see this error message in order to be valide in OPGG
            string notValidText = "an error has occurred";
            string pageContent = StringUtil.CleanString(Document.DocumentNode.InnerText);

            return !pageContent.ToLower().Contains(notValidText);
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

            string position = nodePosition.InnerText;

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

        #endregion

        #region Helpers
        private List<int> GetItemsIdsByCategory(ItemCategory category)
        {
            if (!ItemCategoriesDictionary.ContainsKey(category))
            {
                return null;
            }

            (int startIndex, int countRows) = ItemCategoriesDictionary[category];

            int endIndex = startIndex + countRows;

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

            HtmlNodeCollection allItemCategorieNodes = Document.DocumentNode.SelectNodes(Selector.AllItemsRows);
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

        /// <summary>
        /// Returns a dictionary of each section with their (Start row number, Count row number) section in OP.GG website
        /// Ex. Starter section starts at index 0 and contains 3 rows then Core starts at index 2 and contains 4 rows etc.
        /// </summary>
        /// <returns></returns>
        private Dictionary<ItemCategory, (int, int)> GetItemCategoriesOrdered()
        {
            var itemCategories = new Dictionary<ItemCategory, (int, int)>
            {
                { ItemCategory.Starter, GetRowsByItemCategory(ItemCategory.Starter) },
                { ItemCategory.Core, GetRowsByItemCategory(ItemCategory.Core) },
                { ItemCategory.Boots, GetRowsByItemCategory(ItemCategory.Boots) },
            };

            // Returns sorted list by RowIndex
            return itemCategories;
        }

        /// <summary>
        /// After looking in table containing items, returns the index row of each wanted category
        /// </summary>
        /// <param name="category">Category type</param>
        /// <returns>Index row</returns>
        private (int, int) GetRowsByItemCategory(ItemCategory category)
        {
            // This will get the th element; we can then extract the # of rows for each category from rowspan
            HtmlNodeCollection nodes = Document.DocumentNode.SelectNodes(Selector.AllItemsCategories);

            // get # of rows for each th

            List<int> rowsByCategory = new List<int>();

            foreach (HtmlNode node in nodes)
            {
                int nbOfRows = node.GetAttributeValue<int>("rowspan", 0);
                rowsByCategory.Add(nbOfRows);
            }

            (int startRowIndex, int countRows) = category switch
            {
                ItemCategory.Starter => (0, rowsByCategory[0]),
                ItemCategory.Core => (rowsByCategory[0], rowsByCategory[1]),
                ItemCategory.Boots when nodes.Count > 2 => (rowsByCategory[0] + rowsByCategory[1], rowsByCategory[2]),
                _ => (0, 0)
            };

            return (startRowIndex, countRows);
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

    }
}
