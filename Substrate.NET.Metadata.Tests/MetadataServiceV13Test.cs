using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V13;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV13Test : MetadataBaseTest
    {
        [Test]
        public void MetadataV13_Encode_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V13\\MetadataV13_9080");

            Assert.That(new MetadataV13(metadataSource).Encode(), Is.Not.Null);
        }

        [Test]
        public void MetadataV13_SpecVersionCompare_V9080_And_V9090_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V13\\MetadataV13_9080");
            var metadataDestination = readMetadataFromFile("V13\\MetadataV13_9090");

            Assert.That(MetadataUtils.EnsureMetadataVersion(metadataSource, metadataDestination), Is.EqualTo(MetadataVersion.V13));

            var res = MetadataUtils.MetadataCompareV13(
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

            Assert.That(MetadataUtils.HasPalletChangedVersionBetween("Balances", metadataSource, metadataDestination));
            Assert.That(MetadataUtils.HasPalletChangedVersionBetween("Babe", metadataSource, metadataDestination), Is.False);
        }

        [Test]
        public void StorageEntryMetadataV13_WhenNMap_ShouldThrowException()
        {
            var storageEntry = new StorageEntryMetadataV13();
            storageEntry.StorageType = new NetApi.Model.Types.Base.BaseEnumExt<StorageType.Type, NetApi.Model.Types.Primitive.Str, V11.StorageEntryTypeMapV11, V11.StorageEntryTypeDoubleMapV11, StorageEntryTypeNMapV13>();
            
            storageEntry.StorageType.Value = StorageType.Type.NMap;

            Assert.Throws<MetadataConversionException>(() => storageEntry.ToStorageEntryMetadataV14(new Conversion.Internal.ConversionBuilder(new List<Base.Portable.PortableType>())));
        }
    }
}
