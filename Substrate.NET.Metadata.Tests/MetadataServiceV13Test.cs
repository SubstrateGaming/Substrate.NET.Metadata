using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V13;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV13Test : MetadataBaseTest
    {
        private MetadataService _metadataService;

        [SetUp]
        public void Setup()
        {
            _metadataService = new MetadataService();
        }

        [Test]
        public void MetadataV13_SpecVersionCompare_V9080_And_V9090_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V13\\MetadataV13_9080");
            var metadataDestination = readMetadataFromFile("V13\\MetadataV13_9090");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V13));

            var res = _metadataService.MetadataCompareV13(
                new MetadataV13(metadataSource),
                new MetadataV13(metadataDestination));

            Assert.That(res, Is.Not.Null);
            var changedModules = res.ChangedModules.ToList();

            Assert.That(changedModules[0].ModuleName, Is.EqualTo("Authorship"));
            Assert.That(changedModules[0].HasConstantAdded("UncleGenerations"));

            Assert.That(changedModules[1].ModuleName, Is.EqualTo("Balances"));
            Assert.That(changedModules[1].HasConstantAdded("MaxLocks"));
            Assert.That(changedModules[1].HasConstantAdded("MaxReserves"));

            Assert.That(changedModules[2].ModuleName, Is.EqualTo("Democracy"));
            Assert.That(changedModules[2].Constants.Count(), Is.EqualTo(2));
            Assert.That(changedModules[2].HasConstantAdded("InstantAllowed"));
            Assert.That(changedModules[2].HasConstantAdded("MaxProposals"));
            Assert.That(changedModules[2].Errors.Count(), Is.EqualTo(5));
            Assert.That(changedModules[2].HasErrorAdded("test"), Is.False);
            Assert.That(changedModules[2].HasErrorRemoved("test2"), Is.False);
        }

        [Test]
        public void MetadataV13_V9080_And_V9090_PalletHasChanged_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V13\\MetadataV13_9080");
            var metadataDestination = readMetadataFromFile("V13\\MetadataV13_9090");

            Assert.That(_metadataService.HasPalletChangedVersionBetween("Balances", metadataSource, metadataDestination));
            Assert.That(_metadataService.HasPalletChangedVersionBetween("Babe", metadataSource, metadataDestination), Is.False);
        }
    }
}
