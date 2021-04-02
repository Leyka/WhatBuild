using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.Stores
{
    public static class StoreManager<T>
    {
        private static Dictionary<string, T> _stores = new Dictionary<string, T>();

        /// <summary>
        /// Key represents the full name of the store type (namespace + class) to avoid any collision
        /// </summary>
        private static string Key => typeof(T).FullName;

        public static T Store
        {
            get
            {
                if (!_stores.ContainsKey(Key))
                {
                    // Create a single instance of the store
                    _stores[Key] = Activator.CreateInstance<T>();
                }

                return _stores[Key];
            }
        }
    }
}
