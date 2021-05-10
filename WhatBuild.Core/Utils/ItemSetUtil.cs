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
        public static string GetFormattedItemSetFileName(IBuildSource source, IMetadata metadata, LoLMode mode, string appPrefix)
        {
            string sourceName = metadata.GetSourceName();
            string sourceVersion = metadata.GetVersion();
            // Either show champion position as info, or LoL mode if not classic
            string championInfo = LoLModeUtil.FormatChampionInfoByMode(source, mode);

            return $"{appPrefix}_{sourceName}v{sourceVersion}_{championInfo}.json";
        }

        public static LoLItemSetViewModel CreateItemSetPerChampion(IBuildSource source, IMetadata metadata, ChampionViewModel champion, LoLMode mode, bool showSkillsOrder)
        {
            return new LoLItemSetViewModel()
            {
                Title = GetPageTitle(source, metadata, mode, champion),
                Map = LoLModeUtil.GetMapNameByMode(mode),
                AssociatedChampions = new List<int> { champion.Id },
                Blocks = CreateBlockItems(source, showSkillsOrder)
            };
        }

        private static string GetPageTitle(IBuildSource source, IMetadata metadata, LoLMode mode, ChampionViewModel champion)
        {
            string sourceName = metadata.GetSourceName();
            string sourceVersion = metadata.GetVersion();
            string championName = champion.Name;
            string championInfo = LoLModeUtil.FormatChampionInfoByMode(source, mode);

            // Example for classic mode: OPGG - Annie Middle - v11.07 
            // Example for ARAM mode: OPGG - Annie ARAM - v11.07 
            return $"{sourceName} - {championName} {championInfo} - v{sourceVersion}";
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
