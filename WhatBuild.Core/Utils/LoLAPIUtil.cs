using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.ViewModels;

namespace WhatBuild.Core.Utils
{
    public class LoLAPIUtil
    {
        private static Dictionary<string, string> _dictKnownChampionAlias = new Dictionary<string, string>()
        {
            { "MonkeyKing", "Wukong" }
        };

        /// <summary>
        /// Returns League of Legends metadata such as version, CDN url, etc.
        /// </summary>
        public static async Task<LoLMetadataViewModel> FetchAPIMetadataAsync()
        {
            using HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync("https://ddragon.leagueoflegends.com/realms/na.json");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            LoLMetadataViewModel metadata = JsonConvert.DeserializeObject<LoLMetadataViewModel>(responseBody);
            if (metadata == null)
            {
                throw new NullReferenceException("Cannot find version from League of Legends API");
            }

            return metadata;
        }

        public static string GetFormattedAPIUrl(string apiUrl, string version, string dataType)
        {
            return $"{apiUrl}/{version}/data/en_US/{dataType}.json";
        }

        public static async Task<string> FetchDataByTypeAsync(string apiUrl, string version, string dataType)
        {
            using HttpClient client = new HttpClient();

            string formattedAPIUrl = GetFormattedAPIUrl(apiUrl, version, dataType);
            HttpResponseMessage response = await client.GetAsync(formattedAPIUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            Dictionary<string, object> rawResults = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
            // Returns only results with "data" key 
            string data = rawResults["data"].ToString();
            if (string.IsNullOrEmpty(data))
            {
                throw new KeyNotFoundException($"data key was not found");
            }

            return data;
        }

        public static async Task<List<ChampionViewModel>> FetchAllChampionsAsync(string apiUrl, string version)
        {
            string data = await FetchDataByTypeAsync(apiUrl, version, "champion");

            Dictionary<string, ChampionViewModel> dictChampions =
                JsonConvert.DeserializeObject<Dictionary<string, ChampionViewModel>>(data);

            if (dictChampions?.Count == 0)
            {
                throw new NullReferenceException("No champions found, check that the API is valid");
            }

            List<ChampionViewModel> champions = dictChampions.Values.ToList();

            // Some champions (like Wukong named MonkeyKing) are known to have alias, fix them with their normal name
            // So "MonkeyKing" becomes "Wukong"

            // FIXME: I'm commenting it for now, OP.GG (the only build source we're using) has decided to come back to "MonkeyKing" for "Wukong"
            // PostFixChampionNames(champions);

            return champions;
        }

        private static void PostFixChampionNames(List<ChampionViewModel> champions)
        {
            foreach (var championAlias in _dictKnownChampionAlias)
            {
                ChampionViewModel championToFix = champions.FirstOrDefault(c => c.Name == championAlias.Key);
                championToFix.Name = championAlias.Value;
            }
        }
    }
}
