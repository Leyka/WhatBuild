using System;
using System.Collections.Generic;
using System.Text;
using WhatBuild.Core.Enums;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.ViewModels;
using System.Linq;

namespace WhatBuild.Core.Utils
{
    public class ItemSetUtil
    {
        /// <summary>
        /// Returns formatted Item set file name with JSON extension
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetFormattedItemSetFileName(IBuildSource source, string appPrefix)
        {
            string sourceName = source.GetSourceName();
            string sourceVersion = source.GetVersion();

            return $"{appPrefix}_{sourceName}_v{sourceVersion}.json";
        }

        public static LoLItemSetViewModel CreateItemSetPerChampion(IBuildSource source, ChampionViewModel champion, bool showSkillsOrder)
        {
            return new LoLItemSetViewModel()
            {
                Title = GetPageTitle(source, champion),
                AssociatedChampions = new List<int> { champion.Id },
                Blocks = CreateBlockItems(source, showSkillsOrder)
            };
        }

        private static string GetPageTitle(IBuildSource source, ChampionViewModel champion)
        {
            string sourceName = source.GetSourceName();
            string championName = champion.Name;
            string championPosition = EnumUtil.ToString<ChampionPosition>(source.GetChampionPosition());
            string sourceVersion = source.GetVersion();

            // Example: OPGG - Annie Middle - v11.07 
            return $"{sourceName} - {championName} {championPosition} - v{sourceVersion}";
        }

        private static List<LoLItemSetBlockViewModel> CreateBlockItems(IBuildSource source, bool showSkillsOrder)
        {
            var blockItems = new List<LoLItemSetBlockViewModel>
            {
                CreateStarterBlockItems(source, showSkillsOrder),
                CreateCoreBlockItems(source),
            };

            // Handle special cases: Boots and Extra items
            if (source.HasBootsCategory())
            {
                blockItems.Add(CreateBootBlockItems(source));
            }
            if (source.HasExtraCategory())
            {
                blockItems.Add(CreateExtraBlockItems(source));
            }

            return blockItems;
        }

        private static LoLItemSetBlockViewModel CreateStarterBlockItems(IBuildSource source, bool showSkillsOrder)
        {
            string title = "Starter Items";

            if (showSkillsOrder)
            {
                // Append skill orders to title
                title += $" - {source.GetFormattedSkills()}";
            }

            List<int> itemIds = source.GetStarterItemIds();

            return new LoLItemSetBlockViewModel
            {
                Type = title,
                Items = ConvertItemIdsToBlockItem(itemIds)
            };
        }

        private static LoLItemSetBlockViewModel CreateBootBlockItems(IBuildSource source)
        {
            List<int> itemIds = source.GetBootItemIds();

            return new LoLItemSetBlockViewModel
            {
                Type = "Boots",
                Items = ConvertItemIdsToBlockItem(itemIds)
            };
        }

        private static LoLItemSetBlockViewModel CreateCoreBlockItems(IBuildSource source)
        {
            List<int> itemIds = source.GetCoreItemIds();

            return new LoLItemSetBlockViewModel
            {
                Type = "Core Items",
                Items = ConvertItemIdsToBlockItem(itemIds)
            };
        }

        private static LoLItemSetBlockViewModel CreateExtraBlockItems(IBuildSource source)
        {
            List<int> itemIds = source.GetExtraItemIds();

            return new LoLItemSetBlockViewModel
            {
                Type = "Extra/Situational Items",
                Items = ConvertItemIdsToBlockItem(itemIds)
            };
        }

        private static List<LoLItemSetBlockItemViewModel> ConvertItemIdsToBlockItem(List<int> itemIds)
        {
            return itemIds
                .Select(id => new LoLItemSetBlockItemViewModel { Id = id.ToString() })
                .ToList();
        }
    }
}
