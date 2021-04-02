using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.ViewModels.LoL
{
    public class ItemViewModel
    {
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
