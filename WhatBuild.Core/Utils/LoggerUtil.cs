using System;
using System.Collections.Generic;
using System.Text;

namespace WhatBuild.Core.Utils
{
    public class LoggerUtil
    {
        /// <summary>
        /// Returns formatted log according to this format: [{buildSourceName}] {message} for {championName}
        /// </summary>
        /// <param name="buildSourceName"></param>
        /// <param name="message"></param>
        /// <param name="championName"></param>
        /// <param name="exceptionMessage"></param>
        /// <returns></returns>
        public static string FormatLogByBuildSource(string buildSourceName, string message, string championName = null, string exceptionMessage = null)
        {
            string formattedLog = $"[{buildSourceName}] {message}";

            // Append champion name if any
            if (!string.IsNullOrEmpty(championName))
            {
                formattedLog += $" for {championName}";
            }

#if DEBUG
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                formattedLog += $". Error: {exceptionMessage}";
            }
#endif

            return formattedLog;
        }
    }
}
