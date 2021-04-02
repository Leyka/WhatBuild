using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.ViewModels.LoL
{
    public class ChampionViewModel
    {
        [JsonProperty("key")]
        public int Id { get; set; }

        [JsonProperty("id")]
        public string Name { get; set; }
    }
}
