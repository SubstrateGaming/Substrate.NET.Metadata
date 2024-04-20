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
        public void MetadataV14_SpecVersionCompare_V9110_And_V9122_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9110");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9122");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V14));

            var res = _metadataService.MetadataCompareV14(
                new MetadataV14(metadataSource),
                new MetadataV14(metadataDestination));

            Assert.That(res, Is.Not.Null);
        }

        [Test]
        public void MetadataV14_SpecVersionCompare_V9420_And_V9430_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9420");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9430");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V14));

            var res = _metadataService.MetadataCompareV14(
                new MetadataV14(metadataSource),
                new MetadataV14(metadataDestination));

            Assert.That(res, Is.Not.Null);
        }

        [Test]
        public void MetadataV14_SpecVersionCompare_V9370_And_V9420_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9370");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9420");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V14));

            var res = _metadataService.MetadataCompareV14(
                new MetadataV14(metadataSource),
                new MetadataV14(metadataDestination));

            Assert.That(res, Is.Not.Null);

            Assert.That(res.AddedModules.Any(x => x.ModuleName == "ConvictionVoting"), Is.True);
            Assert.That(res.AddedModules.Any(x => x.ModuleName == "Referenda"), Is.True);
            Assert.That(res.AddedModules.Any(x => x.ModuleName == "Whitelist"), Is.True);

            // Some basic other assertions on Balance pallet I checked with file compare
            var palletBalance = res.ChangedModules.FirstOrDefault(x => x.ModuleName == "Balances");


            // Some calls has been added and one has been removed
            var callsMethodChanged = palletBalance.Calls.LookupDifferentialType.TypeVariant.Elems;
            Assert.That(callsMethodChanged.Count(), Is.GreaterThan(1)); 
            Assert.That(callsMethodChanged.Any(x => x.Item1 == CompareStatus.Added && x.Item2.Name.Value == "set_balance_deprecated"), Is.True);
            Assert.That(callsMethodChanged.Any(x => x.Item1 == CompareStatus.Removed && x.Item2.Name.Value == "set_balance"), Is.True);

            Assert.That(callsMethodChanged.Any(x => x.Item1 == CompareStatus.Added && x.Item2.Name.Value == "transfer_allow_death"), Is.True);
            Assert.That(callsMethodChanged.Any(x => x.Item1 == CompareStatus.Added && x.Item2.Name.Value == "set_balance_deprecated"), Is.True);
            Assert.That(callsMethodChanged.Any(x => x.Item1 == CompareStatus.Added && x.Item2.Name.Value == "force_set_balance"), Is.True);

            var upgradeAccounts = callsMethodChanged.FirstOrDefault(x => x.Item1 == CompareStatus.Added && x.Item2.Name.Value == "upgrade_accounts");
            Assert.That(upgradeAccounts.Item2.VariantFields.Value.Count(), Is.EqualTo(1));
            Assert.That(upgradeAccounts.Item2.VariantFields.Value.First().FieldTypeName.Value.Value, Is.EqualTo("Vec<T::AccountId>"));
        }

        [Test]
        public void MetadataV14_SpecVersionCompare_V9270_And_V9280_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9270");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9280");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V14));

            var res = _metadataService.MetadataCompareV14(
                new MetadataV14(metadataSource),
                new MetadataV14(metadataDestination));

            // For this version, NominationPools pallet has been added
            Assert.That(res, Is.Not.Null);
            Assert.That(res.AddedModules.Count, Is.EqualTo(1));
            Assert.That(res.AddedModules.First().ModuleName, Is.EqualTo("NominationPools"));
        }

        [Test]
        public void MetadataV14_V9270_And_V9280_PalletHasChanged_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V14\\MetadataV14_9270");
            var metadataDestination = readMetadataFromFile("V14\\MetadataV14_9280");

            Assert.That(_metadataService.HasPalletChangedVersionBetween("Auctions", metadataSource, metadataDestination), Is.True);

            Assert.That(_metadataService.HasPalletChangedVersionBetween("AuthorityDiscovery", metadataSource, metadataDestination), Is.False);
        }
    }
}
