using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Tests.Conversion
{
    internal class CustomNodesBuilderTests
    {
        [Test]
        public void CreateCustomComposite_ShouldReturnCompositeBuilder()
        {
            var conversion = new ConversionBuilder(new List<PortableType>());
            var customNodeBuilder = new CustomNodeBuilder();

            var accountInfoBuilder = customNodeBuilder
                .CreateCustomComposite()
                .AddField("nonce", "Index")
                .AddField("refcount", "u8")
                .AddField("data", "AccountData")
                .WithPath("frame_system", "accountInfo")
                .Build(conversion);

            Assert.That(accountInfoBuilder, Is.Not.Null);
        }

        [Test]
        public void CreateCustomComposite_WhenCallByParentNode_ShouldReturnTheOverrideOne()
        {
            var conversion = new ConversionBuilder(new List<PortableType>());
            var customNodeBuilder = new CustomNodeBuilder();

            var accountDataBuilder = customNodeBuilder
                .CreateCustomComposite()
                .AddField("free", "Balance")
                .AddField("notFree", "Balance")
                .WithPath("pallet_balance", "types", "AccountData");

            customNodeBuilder.Build(conversion);

            var res = conversion.BuildPortableTypes("AccountInfo");

            // We should get the AccountInfo from v14 but with our override of AccountData
            var accountInfo = conversion.PortableTypes.SingleOrDefault(x => x.Id.Value == res.Value);

            Assert.That(accountInfo, Is.Not.Null);

            var accountInfoComposite = accountInfo.Ty.TypeDef.Value2 as TypeDefComposite;

            var accountData = conversion.PortableTypes.SingleOrDefault(x => x.Id.Value == accountInfoComposite!.Fields.Value[^1].FieldTy.Value);
            var accountDataComposite = accountData!.Ty.TypeDef.Value2 as TypeDefComposite;

            Assert.That(accountDataComposite!.Fields.Value[0].Name.Value.Value, Is.EqualTo("free"));
            Assert.That(accountDataComposite!.Fields.Value[1].Name.Value.Value, Is.EqualTo("notFree"));
        }

        [Test]
        public void CustomComposite_WithSpecifiedVersionRange()
        {
            var customNodeBuilder = new CustomNodeBuilder();

            var accountDataBuilder = customNodeBuilder
                .CreateCustomComposite()
                .FromVersion(10)
                .ToVersion(20);

            Assert.That(accountDataBuilder.IsVersionValid(10), Is.True);
            Assert.That(accountDataBuilder.IsVersionValid(20), Is.True);
            Assert.That(accountDataBuilder.IsVersionValid(15), Is.True);
            Assert.That(accountDataBuilder.IsVersionValid(9), Is.False);
            Assert.That(accountDataBuilder.IsVersionValid(30), Is.False);
        }

        [Test]
        public void CustomComposite_WithNoVersionSpecified()
        {
            var customNodeBuilder = new CustomNodeBuilder();

            var accountDataBuilder = customNodeBuilder
                .CreateCustomComposite();

            Assert.That(accountDataBuilder.IsVersionValid(10), Is.True);
        }
    }
}
