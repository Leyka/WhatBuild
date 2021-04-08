using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;

namespace WhatBuild.Core.Interfaces
{
    public interface IBuildSource
    {
        Task InitAsync(string championName);

        string GetVersion();

        string GetSourceName();

        ChampionPosition GetChampionPosition();

        string GetFormattedSkills();

        List<int> GetStarterItemIds();

        List<int> GetCoreItemIds();

        List<int> GetExtraItemIds();

        List<int> GetBootItemIds();

        bool HasBootsCategory();

        bool HasExtraCategory();
    }
}
