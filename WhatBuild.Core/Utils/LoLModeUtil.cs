using System;
using System.Collections.Generic;
using System.Text;
using WhatBuild.Core.Enums;
using WhatBuild.Core.Interfaces;

namespace WhatBuild.Core.Utils
{
    public class LoLModeUtil
    {
        /// <summary>
        /// Either returns champion position if LoLMode = classic (SR), 
        /// Else, the mode itself (ex. "ARAM")
        /// </summary>
        /// <param name="source"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static string FormatChampionInfoByMode(IBuildSource source, LoLMode mode)
        {
            return mode switch
            {
                LoLMode.Classic => EnumUtil.ToString<ChampionPosition>(source.GetChampionPosition()),
                LoLMode.ARAM => "ARAM",
                _ => throw new NotSupportedException()
            };
        }

        /// <summary>
        /// Returns map name written EXACTLY how league will read it according to mode
        /// </summary>
        /// <see cref="https://leagueoflegends.fandom.com/wiki/Item_set"/>
        /// <returns></returns>
        public static string GetMapNameByMode(LoLMode mode)
        {
            return mode switch
            {
                LoLMode.Classic => "any", // Keep it "any" for now, but normally it's "SR" (Summoner's Rift)
                LoLMode.ARAM => "HA", // HA = Howling Abyss
                _ => "any"
            };
        }
    }
}
