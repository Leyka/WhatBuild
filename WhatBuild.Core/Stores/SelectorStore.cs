using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;
using WhatBuild.Core.Interfaces;
using WhatBuild.Core.Utils;
using WhatBuild.Core.ViewModels;

namespace WhatBuild.Core.Stores
{
    public class SelectorStore : IStore
    {
        public Dictionary<BuildSourceType, SelectorViewModel> SelectorsDictionary { get; set; } =
            new Dictionary<BuildSourceType, SelectorViewModel>();

        public async Task InitAsync()
        {
            string content = await SelectorUtil.GetSelectorsFileContentAsync();

            var selectorBySourceNames = JsonConvert.DeserializeObject<Dictionary<string, SelectorViewModel>>(content);

            // Change string source type to BuildSourceType
            foreach (var selectorBySourceName in selectorBySourceNames)
            {
                BuildSourceType buildSourceType = BuildSourceTypeUtil.Parse(selectorBySourceName.Key);
                SelectorsDictionary.Add(buildSourceType, selectorBySourceName.Value);
            }
        }
    }
}
