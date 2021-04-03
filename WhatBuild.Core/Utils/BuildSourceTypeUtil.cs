using System;
using System.Collections.Generic;
using System.Text;
using WhatBuild.Core.Enums;

namespace WhatBuild.Core.Utils
{
    public class BuildSourceTypeUtil
    {
        public static BuildSourceType Parse(string source)
        {
            source = source.ToLower();

            if (source == "opgg") return BuildSourceType.OPGG;

            throw new NotSupportedException("Unknown source");
        }
    }
}
