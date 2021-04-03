using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.BuildSources;
using WhatBuild.Core.Enums;
using Xunit;

namespace WhatBuild.Tests.BuildSources
{
    public class OPGGTest
    {
        private readonly OPGG opggClient = new OPGG();

        [Fact]
        public async Task GetChampionPosition_ChampionPosition_IsValid()
        {
            // Annie should be middle
            await opggClient.InitAsync("annie");
            Assert.True(opggClient.GetChampionPosition() == ChampionPosition.Middle);

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
        public async Task GetStarterItemIds_Items_NotEmpty()
        {
            await opggClient.InitAsync("annie");
            List<int> items = opggClient.GetStarterItemIds();

            Assert.True(items?.Count > 0);
        }
    }
}
