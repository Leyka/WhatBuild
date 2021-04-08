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
    }
}
