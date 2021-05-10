using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Enums;

namespace WhatBuild.Core.Interfaces
{
    public interface IMetadata
    {
        Task InitAsync();
        string GetVersion();
        string GetSourceName();
        Dictionary<string, ChampionPosition> GetChampionsWithPositions();
    }
}
