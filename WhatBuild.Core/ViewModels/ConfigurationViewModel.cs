using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.ViewModels
{
    public class ConfigurationViewModel
    {
        /// <summary>
        /// Prefix will be used to name item set build files. 
        /// This will make filter easier (in order to remove these files later)
        /// </summary>
        public string ApplicationPrefixName { get; set; }

        public string LoLDirectory { get; set; }

        public bool RemoveOutdatedItems { get; set; }

        public bool ShowSkillsOrder { get; set; }
    }
}
