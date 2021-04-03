using System;
using System.Collections.Generic;
using System.Text;
using WhatBuild.Core.Interfaces;

namespace WhatBuild.Core
{
    public class ItemSetGenerator
    {
        public IBuildSource BuildSource { get; set; }
        public string LoLInstallPath { get; set; }

        public ItemSetGenerator(IBuildSource source, string lolInstallPath)
        {
            BuildSource = source;
            LoLInstallPath = lolInstallPath;
        }

        /// <summary>
        /// Create a Item Set JSON file for all champions under LoL install path 
        /// </summary>
        public void GenerateItemSetForAllChampions()
        {
            throw new NotSupportedException();
        }
    }
}
