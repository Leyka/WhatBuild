using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.ViewModels
{
    public class ChampionViewModel
    {
        [JsonProperty("key")]
        public int Id { get; set; }

        [JsonProperty("id")]
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Id}:{Name}";
        }
    }
}
