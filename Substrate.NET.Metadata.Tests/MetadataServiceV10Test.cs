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
        [Test]
        public void MetadataV10_Encode_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V10\\MetadataV10_Kusama_1032");

            Assert.That(new MetadataV10(metadataSource).Encode(), Is.Not.Null);
        }

        [Test]
        public void MetadataV10_SpecVersionCompare_V1032_And_V1039_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V10\\MetadataV10_Kusama_1032");
            var metadataDestination = readMetadataFromFile("V10\\MetadataV10_Kusama_1039");

            // TODO : debug this
            //Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V10));

            var res = MetadataUtils.MetadataCompareV10(
                new MetadataV10(metadataSource),
                new MetadataV10(metadataDestination));

            Assert.That(res, Is.Not.Null);
        }

        [Test]
        public void MetadataV10_SpecVersionCompare_V1039_And_V1040_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V10\\MetadataV10_Kusama_1039");
            var metadataDestination = readMetadataFromFile("V10\\MetadataV10_Kusama_1040");

            Assert.That(MetadataUtils.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V10));

            var res = MetadataUtils.MetadataCompareV10(
                new MetadataV10(metadataSource),
                new MetadataV10(metadataDestination));

            Assert.That(res, Is.Not.Null);
            Assert.That(res.AddedModules.First(x => x.ModuleName == "Society"), Is.Not.Null);
        }

        [Test]
        public void MetadataV10_V1039_And_V1040_PalletHasChanged_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V10\\MetadataV10_Kusama_1039");
            var metadataDestination = readMetadataFromFile("V10\\MetadataV10_Kusama_1040");

            Assert.That(MetadataUtils.HasPalletChangedVersionBetween("Society", metadataSource, metadataDestination), Is.False);
            Assert.That(MetadataUtils.HasPalletChangedVersionBetween("Balances", metadataSource, metadataDestination), Is.False);
        }
    }
}
