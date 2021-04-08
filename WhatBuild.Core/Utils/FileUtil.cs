using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WhatBuild.Core.Utils
{
    public class FileUtil
    {
        public static Task CreateJsonFileAsync(string fileAbsolutePath, object objToSerialize, CancellationToken cancelToken = default)
        {
            string jsonContent = JsonConvert.SerializeObject(objToSerialize);

            return File.WriteAllTextAsync(fileAbsolutePath, jsonContent, cancelToken);
        }
    }
}
