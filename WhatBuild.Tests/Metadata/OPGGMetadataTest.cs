using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WhatBuild.Core.BuildSources;
using WhatBuild.Core.Enums;
using Xunit;

namespace WhatBuild.Tests.Metadata
{
    public class OPGGMetadataTest
    {
        OPGGMetadata opggMetadata = new OPGGMetadata();

        public OPGGMetadataTest()
        {
            opggMetadata.InitAsync().Wait();
        }

        [Fact]
        public void GetVersion_Version_IsValid()
        {
            string version = opggMetadata.GetVersion();
            Assert.Matches(@"\d+.\d+", version);
        }

    }


}
