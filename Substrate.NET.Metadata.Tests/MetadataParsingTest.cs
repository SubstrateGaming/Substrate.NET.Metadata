using Substrate.NET.Metadata;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V12;
using Substrate.NET.Metadata.V13;
using Substrate.NET.Metadata.V14;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataParsingTest : MetadataBaseTest
    {
        [Test]
        [TestCase("V11\\MetadataV11_0", 11)]
        [TestCase("V12\\MetadataV12_26", 12)]
        [TestCase("V13\\MetadataV13_9050", 13)]
        [TestCase("V14\\MetadataV14_9110", 14)]
        public void ValidMetadata_CheckVersion_ShouldWork(string metadata, int version)
        {
            var metadataInfo = new CheckRuntimeMetadata(readMetadataFromFile(metadata));

            Assert.That(metadataInfo.MetaDataInfo, Is.Not.Null);
            Assert.That(metadataInfo.MetaDataInfo.Version.Value, Is.EqualTo(version));
        }

        /// <summary>
        /// This is the metadata from block 1
        /// This is V11 and SpecVersion = 0
        /// Verification of this unit test is done directly from Subscan : https://polkadot.subscan.io/runtime?version=0
        /// </summary>
        [Test]
        public void MetadataV11_ShouldBeParsed()
        {
            var metadata = new MetadataV11();
            metadata.Create(readMetadataFromFile("V11\\MetadataV11_0"));

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.RuntimeMetadataData.Modules.Value.Count, Is.EqualTo(31));
        }

        /// <summary>
        /// This is the metadata from block 3_000_000
        /// This is V12 and SpecVersion = 26
        /// Verification of this unit test is done directly from Subscan : https://polkadot.subscan.io/runtime?version=26
        /// </summary>
        [Test]
        public void MetadataV12_ShouldBeParsed()
        {
            var metadata = new MetadataV12();
            metadata.Create(readMetadataFromFile("V12\\MetadataV12_26"));

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.RuntimeMetadataData.Modules.Value.Count, Is.EqualTo(28));
        }

        /// <summary>
        /// This is the metadata from block 6_000_000
        /// This is V13 and SpecVersion = 9,050
        /// Verification of this unit test is done directly from Subscan : https://polkadot.subscan.io/runtime?version=9050
        /// </summary>
        [Test]
        public void MetadataV13_ShouldBeParsed()
        {
            var metadata = new MetadataV13();
            metadata.Create(readMetadataFromFile("V13\\MetadataV13_9050"));

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.RuntimeMetadataData.Modules.Value.Count, Is.EqualTo(31));
        }

        /// <summary>
        /// This is the metadata from block 16_500_000
        /// This is V14 and SpecVersion = 9,430
        /// Verification of this unit test is done directly from Subscan : https://polkadot.subscan.io/runtime?version=9430
        /// </summary>
        [Test]
        public void MetadataV14_ShouldBeParsed()
        {
            var metadata = new MetadataV14();
            metadata.Create(readMetadataFromFile("V14\\MetadataV14_9430"));

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.RuntimeMetadataData.Modules.Value.Count, Is.EqualTo(57));
        }
    }
}
