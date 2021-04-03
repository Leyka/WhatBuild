using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.ViewModels
{
    /// <summary>
    /// XPath selectors are used to browse HTML nodes
    /// </summary>
    public class SelectorViewModel
    {
        [JsonProperty("championPosition")]
        public string ChampionPosition { get; set; }

        [JsonProperty("firstSkillsOrder")]
        public string FirstSkillsOrder { get; set; }

        [JsonProperty("generalSkillsOrder")]
        public string GeneralSkillsOrder { get; set; }

        [JsonProperty("allItemCategories")]
        public string AllItemCategories { get; set; }
    }
}
