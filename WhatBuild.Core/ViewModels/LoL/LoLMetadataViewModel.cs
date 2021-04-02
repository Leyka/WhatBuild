using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace WhatBuild.Core.ViewModels.LoL
{
    public class LoLMetadataViewModel
    {
        [JsonProperty("cdn")]
        public string BaseUrlAPI { get; set; }

        [JsonProperty("v")]
        public string Version { get; set; }
    }
}
