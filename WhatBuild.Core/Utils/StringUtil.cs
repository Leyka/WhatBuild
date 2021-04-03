using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.Utils
{
    public class StringUtil
    {
        /// <summary>
        /// Clean string from \t,\r,\n
        /// </summary>
        public static string CleanString(string s)
        {
            return s
                .Replace("\t", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
        }
    }
}
