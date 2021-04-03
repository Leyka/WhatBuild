using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;
using WhatBuild.Core.ViewModels;

namespace WhatBuild.Core.Utils
{
    public class SelectorUtil
    {
        /// <summary>
        /// Read Selectors file content
        /// </summary>
        /// <returns>Selectors json file content</returns>
        public static async Task<string> GetSelectorsFileContentAsync()
        {
            string content;

#if DEBUG
            // In debug mode, read from local file only
            content = await ReadSelectorsFileFromLocalAsync();
#else
            try
            {
                // Read from Github source code first
                content = await ReadSelectorsFileFromCloudAsync();
            }
            catch (Exception)
            {
                // Fallback for local file if Github failed
                content = await ReadSelectorsFileFromLocalAsync();
            }
#endif

            return content;
        }

        /// <summary>
        /// Read Selectors file content from local computer
        /// </summary>
        /// <returns>Selectors json file content</returns>
        private static Task<string> ReadSelectorsFileFromLocalAsync()
        {
            string dataPath = Path.Join(Environment.CurrentDirectory, "Data");
            string jsonFilePath = Path.Join(dataPath, "selectors.json");

            return File.ReadAllTextAsync(jsonFilePath);
        }

        /// <summary>
        /// Read Selectors file content from Cloud (Github).
        /// Ideal strategy to keep selectors up to date if HTML of Build sources change
        /// </summary>
        /// <returns>Selectors json file content</returns>
        private static async Task<string> ReadSelectorsFileFromCloudAsync()
        {
            using HttpClient client = new HttpClient();

            // TODO: Once config is set, use the key "Selectors:CloudFile" in App.config 
            string url = "https://raw.githubusercontent.com/Leyka/WhatBuild/master/WhatBuild.Core/Data/selectors.json";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }
}
