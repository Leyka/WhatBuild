using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.BuildSources;
using WhatBuild.Core.Enums;
using Xunit;

namespace WhatBuild.Tests.BuildSources
{
    public class OPGGMetadataTest
    {
        private readonly OPGG opggClient = new OPGG();

        [Fact]
        public async Task GetChampionPosition_ChampionPosition_IsValid()
        {
            // Ahri should be middle
            await opggClient.InitAsync("ahri");
            Assert.True(opggClient.GetChampionPosition() == ChampionPosition.Mid);

            // Try with another champion
            await opggClient.InitAsync("thresh");
            Assert.True(opggClient.GetChampionPosition() == ChampionPosition.Support);
        }

        [Fact]
        public async Task GetFormattedSkills_Skills_IsFormatted()
        {
            await opggClient.InitAsync("annie");
            string formattedSkills = opggClient.GetFormattedSkills();

            // Should match this format example:
            // W.Q.E.Q [Q -> W -> E]
            Assert.Matches(@"^\w.\w.\w.\w\s\[\w\s->\s\w\s->\s\w]$", formattedSkills);
        }

        [Fact]
        public async Task GetStarterItemIds_StarterItems_NotEmpty()
        {
            await opggClient.InitAsync("annie");
            List<int> items = opggClient.GetStarterItemIds();

            Assert.True(items?.Count > 0);
        }

        [Fact]
        public async Task GetCoreItemIds_CoreItems_NotEmpty()
        {
            await opggClient.InitAsync("annie");
            List<int> items = opggClient.GetCoreItemIds();

            Assert.True(items?.Count > 0);
        }

        [Fact]
        public async Task GetBootItemIds_Boots_NotEmptyForAnnie()
        {
            await opggClient.InitAsync("annie");
            List<int> items = opggClient.GetBootItemIds();

            Assert.True(items?.Count > 0);
        }

        [Fact]
        public async Task GetBootItemIds_Boots_IsEmptyForCassiopeia()
        {
            await opggClient.InitAsync("cassiopeia");
            List<int> items = opggClient.GetBootItemIds();

            Assert.Null(items);
        }

        [Fact]
        public async Task GetStarterItemIds_Aram_IsValid()
        {
            await opggClient.InitAsync("annie", LoLMode.ARAM);
            List<int> items = opggClient.GetStarterItemIds();

            Assert.True(items?.Count > 0);
        }
    }
}
