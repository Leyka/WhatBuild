using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.Utils
{
    public class EnumUtil
    {
        public static string ToString<T>(T enumerator) where T : Enum
        {
            return Enum.GetName(typeof(T), enumerator);
        }
    }
}