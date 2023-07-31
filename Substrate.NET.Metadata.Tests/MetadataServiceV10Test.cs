using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V10;
using Substrate.NET.Metadata.V9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV10Test : MetadataBaseTest
    {
        private MetadataService _metadataService;

        [SetUp]
        public void Setup()
        {
            _metadataService = new MetadataService();
        }

        [Test]
        public void MetadataV10_SpecVersionCompare_V1032_And_V1039_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V10\\MetadataV10_Kusama_1032");
            var metadataDestination = readMetadataFromFile("V10\\MetadataV10_Kusama_1039");

            // TODO : debug this
            //Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V10));

            var res = _metadataService.MetadataCompareV10(
                new MetadataV10(metadataSource),
                new MetadataV10(metadataDestination));

            Assert.That(res, Is.Not.Null);
        }
    }
}
