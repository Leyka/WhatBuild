using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;

namespace WhatBuild.Core.Interfaces
{
    public interface IBuildSource
    {
        Task ReadHtmlDocumentAsync(string championName);

        ChampionPosition GetChampionPosition();

        string GetFormattedSkills();
    }
}
