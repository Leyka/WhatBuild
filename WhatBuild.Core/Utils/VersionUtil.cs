using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.Utils
{
    public class VersionUtil
    {
        public static bool ShouldUpdateItemSet(string sourceVersion, string localVersion)
        {
            if (string.IsNullOrEmpty(localVersion))
            {
                // Nothing to update since we don't have any local version stored
                return false;
            }

            return localVersion != sourceVersion;
        }
    }
}
