using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.Utils
{
    public class EnumUtil<T> where T : Enum
    {
        public static string ToString(T enumerator)
        {
            return Enum.GetName(typeof(T), enumerator);
        }
    }
}
