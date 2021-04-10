using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WhatBuild.Core.Utils
{
    public class LoLPathUtil
    {
        private const string LOL_WINDOWS_EXE = "leagueclient.exe";

        public static bool IsValidLeagueOfLegendsDirectory(string lolInstallPath)
        {
            if (string.IsNullOrEmpty(lolInstallPath) || !Directory.Exists(lolInstallPath)) return false;

            // Check if league executable exists
            string[] files = Directory.GetFiles(lolInstallPath);
            return files.Any(file => file.ToLower().Contains(LOL_WINDOWS_EXE));
        }

        public static string CreateItemSetDirectory(string lolInstallPath, string championName)
        {
            // Item set are usually create under : {LOL}/Config/Champions/{ChampionName}/Recommended
            string championItemSetPath = Path.Combine(lolInstallPath, "Config", "Champions", championName, "Recommended");

            Directory.CreateDirectory(championItemSetPath);

            return championItemSetPath;
        }

        public static void DeleteItemSets(string lolInstallPath, string appPrefix)
        {
            // Deletes all item sets under {LOL}/Config/Champions/{ChampionName}/Recommended/WB_*.json
            string championsDirectoryPath = Path.Combine(lolInstallPath, "Config", "Champions");

            foreach (string championDirectory in Directory.EnumerateDirectories(championsDirectoryPath))
            {
                string recommendedItemSetPath = Path.Join(championDirectory, "Recommended");

                foreach (string itemSetFile in Directory.EnumerateFiles(recommendedItemSetPath, $"{appPrefix}_*.json"))
                {
                    File.Delete(itemSetFile);
                }
            }
        }
    }
}
