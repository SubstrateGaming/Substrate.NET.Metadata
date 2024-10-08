﻿using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Exceptions;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.V15;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceV15Test : MetadataBaseTest
    {
        [Test]
        public void MetadataV15_Encode_ShouldSucceed()
        {
            var metadataSource = readMetadataFromFile("V15\\MetadataV15_FromPolkadotJs");

            Assert.That(new MetadataV15(metadataSource).Encode(), Is.Not.Null);
            Assert.That(new MetadataV15(metadataSource).Encode(), Is.Not.Null);
        }

        [Test]
        public void MetadataV15_WhenInstanciate_ShouldSucceed()
        {
            var metadataHex = readMetadataFromFile("V15\\MetadataV15_FromPolkadotJs");

            Assert.That(MetadataUtils.GetMetadataVersion(metadataHex), Is.EqualTo(MetadataVersion.V15));

            var res = new MetadataV15(metadataHex);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.Version, Is.EqualTo(MetadataVersion.V15));

            Assert.That(res.RuntimeMetadataData.Types.Value, Has.Length.GreaterThan(0));
            Assert.That(res.RuntimeMetadataData.Modules.Value, Has.Length.GreaterThan(0));
            
            Assert.That((int)res.RuntimeMetadataData.OuterEnums.CallType.Value, Is.GreaterThanOrEqualTo(0));
            Assert.That((int)res.RuntimeMetadataData.OuterEnums.ErrorType.Value, Is.GreaterThanOrEqualTo(0));
            Assert.That((int)res.RuntimeMetadataData.OuterEnums.EventType.Value, Is.GreaterThanOrEqualTo(0));
            
            Assert.That(res.RuntimeMetadataData.Apis.Value, Has.Length.GreaterThan(0));

            Assert.That(res.RuntimeMetadataData.Custom.Map.Value, Is.Not.Null);
        }
    }
}
