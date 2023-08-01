using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V12;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV12Test : MetadataBaseTest
    {
        private MetadataService _metadataService;

        [SetUp]
        public void Setup()
        {
            _metadataService = new MetadataService();
        }

        [Test]
        public void MetadataV12_SpecVersionCompare_V27_And_V28_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V12\\MetadataV12_27");
            var metadataDestination = readMetadataFromFile("V12\\MetadataV12_28");

            Assert.That(_metadataService.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V12));

            var res = _metadataService.MetadataCompareV12(
                new MetadataV12(metadataSource),
                new MetadataV12(metadataDestination));

            Assert.That(res, Is.Not.Null);

            var addedModules = res.AddedModules.ToList();
            Assert.That(addedModules.Count, Is.EqualTo(2));

            Assert.That(addedModules[0].ModuleName, Is.EqualTo("Bounties"));
            Assert.That(addedModules[1].ModuleName, Is.EqualTo("Tips"));

            var changedModules = res.ChangedModules.ToList();
            Assert.That(changedModules.Count, Is.EqualTo(7));

            Assert.That(changedModules[0].ModuleName, Is.EqualTo("Babe"));
            Assert.That(changedModules[0].Storage.First().Item2.status, Is.EqualTo(CompareStatus.Added));
            Assert.That(changedModules[0].Storage.First().Item2.storage.Name.Value, Is.EqualTo("NextAuthorities"));
            Assert.IsTrue(changedModules[0].HasStorageAdded("NextAuthorities"));

            // Now let's test ElectionsPhragmen which changed a lot
            var electionPhragmenModule = changedModules[1];
            Assert.That(electionPhragmenModule.ModuleName, Is.EqualTo("ElectionsPhragmen"));

            // No storage changes
            var electionPhragmenStorages = electionPhragmenModule.Storage.ToList();
            Assert.That(electionPhragmenStorages.Count, Is.EqualTo(0));

            var electionPhragmenCalls = electionPhragmenModule.Calls.ToList();
            Assert.That(electionPhragmenCalls[0].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(electionPhragmenCalls[0].Item2.Name.Value, Is.EqualTo("clean_defunct_voters"));
            Assert.IsTrue(electionPhragmenModule.HasCallAdded("clean_defunct_voters"));

            Assert.That(electionPhragmenCalls[1].Item1, Is.EqualTo(CompareStatus.Removed));
            Assert.That(electionPhragmenCalls[1].Item2.Name.Value, Is.EqualTo("report_defunct_voter"));
            Assert.IsTrue(electionPhragmenModule.HasCallRemoved("report_defunct_voter"));

            var electionPhragmenConstants = electionPhragmenModule.Constants.ToList();
            Assert.That(electionPhragmenConstants[0].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(electionPhragmenConstants[0].Item2.Name.Value, Is.EqualTo("VotingBondBase"));
            Assert.IsTrue(electionPhragmenModule.HasConstantAdded("VotingBondBase"));

            Assert.That(electionPhragmenConstants[1].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(electionPhragmenConstants[1].Item2.Name.Value, Is.EqualTo("VotingBondFactor"));

            Assert.That(electionPhragmenConstants[2].Item1, Is.EqualTo(CompareStatus.Removed));
            Assert.That(electionPhragmenConstants[2].Item2.Name.Value, Is.EqualTo("VotingBond"));
            Assert.IsTrue(electionPhragmenModule.HasConstantRemoved("VotingBond"));

            var electionPhragmenEvents = electionPhragmenModule.Events.ToList();
            Assert.That(electionPhragmenEvents[0].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(electionPhragmenEvents[0].Item2.Name.Value, Is.EqualTo("Renounced"));
            Assert.IsTrue(electionPhragmenModule.HasEventAdded("Renounced"));

            Assert.That(electionPhragmenEvents[1].Item1, Is.EqualTo(CompareStatus.Removed));
            Assert.That(electionPhragmenEvents[1].Item2.Name.Value, Is.EqualTo("MemberRenounced"));
            Assert.IsTrue(electionPhragmenModule.HasEventRemoved("MemberRenounced"));

            Assert.That(electionPhragmenEvents[2].Item1, Is.EqualTo(CompareStatus.Removed));
            Assert.That(electionPhragmenEvents[2].Item2.Name.Value, Is.EqualTo("VoterReported"));

            var electionPhragmenErrors = electionPhragmenModule.Errors.ToList();
            Assert.That(electionPhragmenErrors[0].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(electionPhragmenErrors[0].Item2.Name.Value, Is.EqualTo("RunnerUpSubmit"));
            Assert.IsTrue(electionPhragmenModule.HasErrorAdded("RunnerUpSubmit"));

            Assert.That(electionPhragmenErrors[1].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(electionPhragmenErrors[1].Item2.Name.Value, Is.EqualTo("InvalidWitnessData"));

            Assert.That(electionPhragmenErrors[2].Item1, Is.EqualTo(CompareStatus.Removed));
            Assert.That(electionPhragmenErrors[2].Item2.Name.Value, Is.EqualTo("RunnerSubmit"));

            Assert.That(electionPhragmenErrors[3].Item1, Is.EqualTo(CompareStatus.Removed));
            Assert.That(electionPhragmenErrors[3].Item2.Name.Value, Is.EqualTo("InvalidCandidateCount"));
            Assert.IsTrue(electionPhragmenModule.HasErrorRemoved("InvalidCandidateCount"));
        }

        [Test]
        public void MetadataV12_V27_And_V28_PalletHasChanged_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V12\\MetadataV12_27");
            var metadataDestination = readMetadataFromFile("V12\\MetadataV12_28");

            Assert.IsTrue(_metadataService.HasPalletChangedVersionBetween("ElectionsPhragmen", metadataSource, metadataDestination));

            Assert.IsFalse(_metadataService.HasPalletChangedVersionBetween("Balances", metadataSource, metadataDestination));
        }
    }
}
