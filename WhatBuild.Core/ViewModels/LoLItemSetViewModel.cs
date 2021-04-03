using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.ViewModels
{
    public class LoLItemSetViewModel
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "custom";

        [JsonProperty("map")]
        public string Map { get; set; } = "any";

        [JsonProperty("mode")]
        public string Mode { get; set; } = "any";

        [JsonProperty("sortrank")]
        public int SortRank { get; set; } = 1;

        [JsonProperty("associatedChampions")]
        public List<int> AssociatedChampions { get; set; }

        [JsonProperty("associatedMaps")]
        public List<int> AssociatedMaps { get; set; }

        [JsonProperty("blocks")]
        public List<LoLItemSetBlockViewModel> Blocks { get; set; }
    }

    public class LoLItemSetBlockViewModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("items")]
        public List<LoLItemSetBlockItemViewModel> Items { get; set; }
    }

    public class LoLItemSetBlockItemViewModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; } = 1;
    }
}
