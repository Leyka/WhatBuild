using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WhatBuild.Core.Utils
{
    public class PathUtil
    {
        private const string LOL_WINDOWS_EXE = "leagueclient.exe";

        public static bool IsValidLeagueOfLegendsDirectory(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return false;

            // Check if league executable exists
            string[] files = Directory.GetFiles(path);
            return files.Any(file => file.ToLower().Contains(LOL_WINDOWS_EXE));
        }
    }
}
