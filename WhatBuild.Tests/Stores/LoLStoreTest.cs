using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Stores;
using Xunit;

namespace WhatBuild.Tests.Stores
{
    public class LoLStoreTest
    {
        [Fact]
        public async Task InitAsync_LoLStore_IsInitialized()
        {
            LoLStore lolStore = await StoreManager<LoLStore>.GetAsync();

            Assert.NotNull(lolStore.BaseUrlAPI);
            Assert.NotNull(lolStore.Version);

            Assert.True(lolStore.Champions.Count > 0);
        }
    }
}
