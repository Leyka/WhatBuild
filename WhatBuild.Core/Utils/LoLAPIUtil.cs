using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.ViewModels.LoL;

namespace WhatBuild.Core.Helpers
{
    public class LoLAPIUtil
    {
        /// <summary>
        /// Returns League of Legends metadata such as version, CDN url, etc.
        /// </summary>
        public static async Task<LoLMetadataViewModel> FetchAPIMetadata()
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

        public static async Task<string> FetchDataByTypeAsync(string apiUrl, string version, string dataType)
        {
            string formattedAPIUrl = $"{apiUrl}/{version}/data/en_US/{dataType}.json";

            using HttpClient client = new HttpClient();
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

            return dictChampions.Values.ToList();
        }

        public static async Task<List<ItemViewModel>> FetchAllItemAsync(string apiUrl, string version)
        {
            string data = await FetchDataByTypeAsync(apiUrl, version, "item");

            Dictionary<int, ItemViewModel> dictItems =
                JsonConvert.DeserializeObject<Dictionary<int, ItemViewModel>>(data);

            if (dictItems?.Count == 0)
            {
                throw new NullReferenceException("No items found, check that the API is valid");
            }

            List<ItemViewModel> itemsWithId = dictItems.Values
                .Select((itemViewModel, index) =>
                {
                    // The API store the item ID only as "key" 
                    // Manually bind "id" to view model
                    itemViewModel.Id = dictItems.Keys.ElementAt(index);
                    return itemViewModel;
                })
                .ToList();

            return itemsWithId;
        }
    }
}
