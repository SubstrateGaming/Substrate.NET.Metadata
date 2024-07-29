using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Exceptions;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.V15;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV15Test : MetadataBaseTest
    {
        private MetadataService _metadataService;

        [SetUp]
        public void Setup()
        {
            _metadataService = new MetadataService();
        }

        [Test]
        public void MetadataV15_WhenInstanciate_ShouldSucceed()
        {
            var metadataHex = readMetadataFromFile("V15\\MetadataV15_FromPolkadotJs");

            Assert.That(_metadataService.GetMetadataVersion(metadataHex), Is.EqualTo(MetadataVersion.V15));

            var res = new MetadataV15(metadataHex);
            Assert.That(res, Is.Not.Null);
        }
    }
}
