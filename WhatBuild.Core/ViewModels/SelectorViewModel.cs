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
        [JsonProperty("links")]
        public LinksViewModel Links { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("championPosition")]
        public string ChampionPosition { get; set; }

        [JsonProperty("firstSkillsOrder")]
        public string FirstSkillsOrder { get; set; }

        [JsonProperty("generalSkillsOrder")]
        public string GeneralSkillsOrder { get; set; }

        [JsonProperty("allItemsCategories")]
        public string allItemsCategories { get; set; }

        [JsonProperty("allItemsRows")]
        public string allItemsRows { get; set; }

        [JsonProperty("items")]
        public string Items { get; set; }
    }

    public class LinksViewModel
    {
        [JsonProperty("classic")]
        public string Classic { get; set; }

        [JsonProperty("aram")]
        public string ARAM { get; set; }
    }
}
