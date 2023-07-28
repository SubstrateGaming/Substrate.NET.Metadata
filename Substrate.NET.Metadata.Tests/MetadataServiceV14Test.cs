using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V14;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV14Test : MetadataBaseTest
    {
        private MetadataService _metadataService;

        [SetUp]
        public void Setup()
        {
            _metadataService = new MetadataService();
        }

        [Test]
        public async Task MetadataV14_SpecVersionCompare_V9110_And_V9122_ShouldSucceedAsync()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9110");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9122");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V14));

            var res = await _metadataService.MetadataCompareV14Async(
                new MetadataV14(metadataSource),
                new MetadataV14(metadataDestination));

            Assert.That(res, Is.Not.Null);
        }

        [Test]
        public async Task MetadataV14_SpecVersionCompare_V9420_And_V9430_ShouldSucceedAsync()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9420");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9430");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V14));

            var res = await _metadataService.MetadataCompareV14Async(
                new MetadataV14(metadataSource),
                new MetadataV14(metadataDestination));

            Assert.That(res, Is.Not.Null);
        }

        [Test]
        public async Task MetadataV14_SpecVersionCompare_V9370_And_V9420_ShouldSucceedAsync()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9370");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9420");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V14));

            var res = await _metadataService.MetadataCompareV14Async(
                new MetadataV14(metadataSource),
                new MetadataV14(metadataDestination));

            Assert.That(res, Is.Not.Null);

            Assert.IsTrue(res.AddedModules.Any(x => x.ModuleName == "ConvictionVoting"));
            Assert.IsTrue(res.AddedModules.Any(x => x.ModuleName == "Referenda"));
            Assert.IsTrue(res.AddedModules.Any(x => x.ModuleName == "Whitelist"));

            // Some basic other assertions on Balance pallet I checked with file compare
            var palletBalance = res.ChangedModules.FirstOrDefault(x => x.ModuleName == "Balances");
            


        }

        [Test]
        public async Task MetadataV14_SpecVersionCompare_V9270_And_V9280_ShouldSucceedAsync()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9270");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9280");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V14));

            var res = await _metadataService.MetadataCompareV14Async(
                new MetadataV14(metadataSource),
                new MetadataV14(metadataDestination));

            // For this version, NominationPools pallet has been added
            Assert.That(res, Is.Not.Null);
            Assert.That(res.AddedModules.Count, Is.EqualTo(1));
            Assert.That(res.AddedModules.First().ModuleName, Is.EqualTo("NominationPools"));
        }
    }
}
