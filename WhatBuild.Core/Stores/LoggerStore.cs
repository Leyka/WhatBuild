using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.Interfaces;

namespace WhatBuild.Core.Stores
{
    public class LoggerStore : IStore
    {
        public List<string> Logs { get; set; }

        public Task InitAsync()
        {
            Logs = new List<string>();
            return Task.CompletedTask;
        }

        public string StringifyLogs()
        {
            StringBuilder builder = new StringBuilder();

            Logs.ForEach(log => builder.Append(log + Environment.NewLine));

            return builder.ToString();
        }
    }
}
