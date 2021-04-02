using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Interfaces;

namespace WhatBuild.Core.Stores
{
    public static class StoreManager<T> where T : IStore
    {
        /// <summary>
        /// Store dictionary with: key = fullName of the class type, value = reference of the store
        /// </summary>
        private static Dictionary<string, T> _stores = new Dictionary<string, T>();

        /// <summary>
        /// Key represents the full name of the store type (namespace + class) to avoid any collision
        /// </summary>
        private static string _key = typeof(T).FullName;

        /// <summary>
        /// Returns singleton instance of the given store type. 
        /// If the store doesn't exist yet, it will initalize it first
        /// </summary>
        /// <returns>Reference of the store</returns>
        public static async Task<T> GetAsync()
        {
            if (!_stores.ContainsKey(_key))
            {
                // Create a single instance of the store
                T store = Activator.CreateInstance<T>();
                // Init the store
                await store.InitAsync();
                // Store the reference in the static dictionary
                _stores[_key] = store;
            }

            return _stores[_key];
        }
    }
}
