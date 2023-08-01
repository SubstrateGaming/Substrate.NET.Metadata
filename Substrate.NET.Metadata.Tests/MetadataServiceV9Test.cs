using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV9Test : MetadataBaseTest
    {

        private MetadataService _metadataService;

        [SetUp]
        public void Setup()
        {
            _metadataService = new MetadataService();
        }

        [Test]
        public void MetadataV9_SpecVersionCompare_V1020_And_V1022_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V9\\MetadataV9_Kusama_1020");
            var metadataDestination = readMetadataFromFile("V9\\MetadataV9_Kusama_1022");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V9));

            var res = _metadataService.MetadataCompareV9(
                new MetadataV9(metadataSource),
                new MetadataV9(metadataDestination));

            Assert.That(res, Is.Not.Null);
        }

        [Test]
        public void MetadataV9_V1020_And_V1022_PalletHasChanged_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V9\\MetadataV9_Kusama_1020");
            var metadataDestination = readMetadataFromFile("V9\\MetadataV9_Kusama_1022");

            Assert.IsTrue(_metadataService.HasPalletChangedVersionBetween("Democracy", metadataSource, metadataDestination));
            Assert.IsFalse(_metadataService.HasPalletChangedVersionBetween("Balances", metadataSource, metadataDestination));
        }
    }
}
