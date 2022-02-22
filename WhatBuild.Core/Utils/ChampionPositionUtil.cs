using System;
using System.Collections.Generic;
using System.Text;
using WhatBuild.Core.Enums;
using System.Linq;

namespace WhatBuild.Core.Utils
{
    public static class ChampionPositionUtil
    {
        private static Dictionary<ChampionPosition, List<string>> _positionsDict =
            new Dictionary<ChampionPosition, List<string>>
        {
            { ChampionPosition.Top, new List<string> { "top"} },
            { ChampionPosition.Jungle, new List<string> { "jungle", "jg"} },
            { ChampionPosition.Middle, new List<string> { "mid", "middle"} },
            { ChampionPosition.Adc, new List<string> { "adc", "bottom"} },
            { ChampionPosition.Support, new List<string> { "support", "supp"} },
        };

        public static ChampionPosition Parse(string position)
        {
            position = position.ToLower();

            if (!_positionsDict.Any(x => x.Value.Contains(position)))
            {
                throw new ArgumentException($"Position '{position}' was not found in the dictionary");
            }

            ChampionPosition championPosition = _positionsDict.FirstOrDefault(x => x.Value.Contains(position)).Key;
            return championPosition;
        }
    }
}
