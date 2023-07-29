using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V11;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV11Test : MetadataBaseTest
    {
        private MetadataService _metadataService;

        [SetUp]
        public void Setup()
        {
            _metadataService = new MetadataService();
        }

        [Test]
        public void MetadataV11_SpecVersionCompare_V0_And_V1_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V11\\MetadataV11_0");
            var metadataDestination = readMetadataFromFile("V11\\MetadataV11_1");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V11));

            var res = _metadataService.MetadataCompareV11(
                new MetadataV11(metadataSource),
                new MetadataV11(metadataDestination));

            Assert.That(res, Is.Not.Null);
            Assert.That(res.AllModulesDiff.Count(), Is.EqualTo(31));
            Assert.That(res.UnchangedModules.Count(), Is.EqualTo(30));
            Assert.That(res.ChangedModules.Count(), Is.EqualTo(1));

            Assert.That(res.ChangedModules.First().ModuleName, Is.EqualTo("Claims"));
            var errors = res.ChangedModules.First().Errors.ToList();

            Assert.That(errors.Count, Is.EqualTo(2));

            // VestedBalanceExists error has been added
            Assert.That(errors[0].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(errors[0].Item2.Name.Value, Is.EqualTo("VestedBalanceExists"));

            // DestinationVesting error has been removed
            Assert.That(errors[1].Item1, Is.EqualTo(CompareStatus.Removed));
            Assert.That(errors[1].Item2.Name.Value, Is.EqualTo("DestinationVesting"));
        }

        [Test]
        public void MetadataV11_SpecVersionCompare_V6_And_V7_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V11\\MetadataV11_6");
            var metadataDestination = readMetadataFromFile("V11\\MetadataV11_7");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V11));

            var res = _metadataService.MetadataCompareV11(
                new MetadataV11(metadataSource),
                new MetadataV11(metadataDestination));

            Assert.That(res, Is.Not.Null);
            Assert.That(res.AllModulesDiff.Count(), Is.EqualTo(34));
            Assert.That(res.UnchangedModules.Count(), Is.EqualTo(33));
            Assert.That(res.ChangedModules.Count(), Is.EqualTo(1));

            Assert.That(res.ChangedModules.First().ModuleName, Is.EqualTo("Indices"));

            var calls = res.ChangedModules.First().Calls.ToList();
            Assert.That(calls.Count, Is.EqualTo(1));
            Assert.That(calls[0].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(calls[0].Item2.Name.Value, Is.EqualTo("freeze"));

            var events = res.ChangedModules.First().Events.ToList();
            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(events[0].Item2.Name.Value, Is.EqualTo("IndexFrozen"));
        }

        [Test]
        public void MetadataV11_SpecVersionCompare_V23_And_V24_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V11\\MetadataV11_23");
            var metadataDestination = readMetadataFromFile("V11\\MetadataV11_24");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V11));

            var res = _metadataService.MetadataCompareV11(
                new MetadataV11(metadataSource),
                new MetadataV11(metadataDestination));

            Assert.That(res, Is.Not.Null);

            Assert.That(res.AddedModules, Is.Not.Null);
            Assert.That(res.AddedModules.First().ModuleName, Is.EqualTo("DummyPurchase"));

            Assert.That(res.RemovedModules, Is.Not.Null);
            Assert.That(res.RemovedModules.First().ModuleName, Is.EqualTo("Purchase"));

            Assert.That(res.ChangedModules.Count(), Is.EqualTo(3));

            if (res.ChangedModules.ToList() is [var first, var second, var third])
            {
                Assert.That(first.ModuleName, Is.EqualTo("DummyRegistrar"));
                Assert.That(first.Events.Count, Is.EqualTo(1));
                Assert.That(first.Events.First().Item1, Is.EqualTo(CompareStatus.Added));
                Assert.That(first.Events.First().Item2.Name.Value, Is.EqualTo("Dummy"));

                Assert.That(second.ModuleName, Is.EqualTo("DummySlots"));
                Assert.That(second.Events.Count, Is.EqualTo(1));
                Assert.That(second.Events.First().Item1, Is.EqualTo(CompareStatus.Added));
                Assert.That(second.Events.First().Item2.Name.Value, Is.EqualTo("Dummy"));

                Assert.That(third.ModuleName, Is.EqualTo("Multisig"));
                var constants = third.Constants.ToList();
                Assert.That(constants.Count, Is.EqualTo(3));

                Assert.That(constants[0].Item2.Name.Value, Is.EqualTo("DepositBase"));
                Assert.That(constants[0].Item2.ConstantType.Value, Is.EqualTo("BalanceOf<T>"));

                Assert.That(constants[1].Item2.Name.Value, Is.EqualTo("DepositFactor"));
                Assert.That(constants[1].Item2.ConstantType.Value, Is.EqualTo("BalanceOf<T>"));

                Assert.That(constants[2].Item2.Name.Value, Is.EqualTo("MaxSignatories"));
                Assert.That(constants[2].Item2.ConstantType.Value, Is.EqualTo("u16"));

            }
            else
                Assert.Fail();
        }
    }
}
