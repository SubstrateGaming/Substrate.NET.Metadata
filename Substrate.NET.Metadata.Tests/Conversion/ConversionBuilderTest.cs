using Microsoft.VisualBasic;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NetApi.Model.Types.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Tests
{
    internal class ConversionBuilderTest
    {
        private ConversionBuilder _builder;
        #region Setup
        [SetUp]
        public void Setup() {
            _builder = new ConversionBuilder(InitPortableRegistry().ToList());
        }

        private static PortableType[] InitPortableRegistry()
        {
            var portableRegistry = new PortableRegistry();
            portableRegistry.Create(new List<PortableType>().ToArray());
            var pt = portableRegistry.Value;
            Assert.That(pt.Length, Is.EqualTo(0));
            return pt;
        }
        #endregion

        [Test]
        [TestCase("u32")]
        public void GetNode_FromPrimitive_ShouldSucceed(string primitiveType)
        {
            var res = _builder.GetPortableType(primitiveType);

            Assert.That(res.portableType.Ty.TypeDef.Value, Is.EqualTo(TypeDefEnum.Primitive));
            Assert.That(((BaseEnum<TypeDefPrimitive>)res.portableType.Ty.TypeDef.Value2).Value, Is.EqualTo(TypeDefPrimitive.U32));
        }

        [Test]
        [TestCase("TaskAddress<BlockNumber>", 29, 2)]
        [TestCase("SignedSubmissionOf<T>", 504, 61)]
        [TestCase("BalanceOf<T>", 6, 1)]
        [TestCase("SubmissionIndicesOf<T>", 502, 5)]
        [TestCase("[BalanceOf<T>; 4]", 1000, 1)]
        [TestCase("Approvals", 14, 2)]
        [TestCase("(Vec<ProxyDefinition<T::AccountId, T::ProxyType, T::BlockNumber>>,\n BalanceOf<T>)", 1000, 1)]
        [TestCase("&[u8]", 10, 2, "Claims")]
        [TestCase("Vec<Vec<(ParaId, CollatorId)>>", 1000, 1)]
        [TestCase("Vec<BalanceOf<T>>", 1000, 1)]
        [TestCase("LeasePeriod", 4, 1)]
        [TestCase("LockIdentifier", 125, 2)]
        [TestCase("(T::BlockNumber, Hash)", 1000, 1)]
        [TestCase("(BalanceOf<T>, Vec<T::AccountId>)", 1000, 1)]
        [TestCase("AuthorityId", 46, 4, "ImOnline")]
        [TestCase("AuthorityId", 137, 4, "Babe")]
        [TestCase("Vec<(AuthorityId, BabeAuthorityWeight)>", 367, 7, "Babe")]
        [TestCase("Vec<IdentificationTuple>", 48, 10)]
        [TestCase("Vec<(T::ValidatorId, T::Keys)>", 411, 14)]
        [TestCase("Vec<T::ValidatorId>", 55, 4)]
        [TestCase("ReportIdOf<T>", 9, 3)]
        [TestCase("(T::AccountId, slashing::SpanIndex)", 400, 5)]
        [TestCase("slashing::SpanRecord<BalanceOf<T>>", 406, 2)]
        [TestCase("(EraIndex, T::AccountId)", 396, 5)]
        [TestCase("Exposure<T::AccountId, BalanceOf<T>>", 50, 8)]
        [TestCase("Multiplier", 384, 2)]
        [TestCase("MaybeRandomness", 370, 3)]
        [TestCase("AccountInfo<T::Index, T::AccountData>", 3, 4)]
        [TestCase("schnorrkel::Randomness", 1, 2)]
        [TestCase("[u8;32]", 1, 2)]
        [TestCase("Option<Vec<u8>>", 30, 3)]
        [TestCase("Vec<(T::BlockNumber, EventIndex)>", 105, 3)]
        [TestCase("Vec<(EraIndex, SessionIndex)>", 105, 3)]
        [TestCase("u32", 4, 1)]
        [TestCase("ExtrinsicsWeight", 7, 2)] // Should be bound with PerDispatchClass
        [TestCase("DispatchInfo", 22, 4)]
        [TestCase("Weight", 8, 1)]
        [TestCase("T::BlockNumber", 4, 1)]
        [TestCase("T::AccountId", 0, 3)]
        [TestCase("Vec<AccountId>", 55, 4)]
        //[TestCase("Vec<DeferredOffenceOf<T>>", 22, 4)]
        public void GetIndex_FromComposite_ShouldSucceed(string rustType, int indexExpected, int lengthExpected, string palletContext = "")
        {
            _builder.CurrentPallet = palletContext;
            var res = _builder.BuildPortableTypes(rustType);

            Assert.That(res.Value, Is.EqualTo(indexExpected), "Index are not equals");
            Assert.That(_builder.PortableTypes.Count, Is.EqualTo(lengthExpected), "Portable elements are not equals");
        }

        [Test]
        [TestCase("Vec<AccountId>")] // 61
        [TestCase("Vec<(ParaId, T::BlockNumber)>")]
        [TestCase("Vec<(AccountId, Balance)>")]
        [TestCase("Vec<DeferredOffenceOf<T>>")]
        public void Build_FromSequence_ShouldSucceed(string sequenceClass)
        {
            var node = new NodeBuilderTypeUndefined(sequenceClass, string.Empty);
            var res = ConversionBuilderTree.Build(node);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.TypeDef == TypeDefEnum.Sequence);
        }

        [Test]
        [TestCase("AccountId, Balance", new string[] { "AccountId", "Balance" })]
        [TestCase("Toto, Titi<A, B>", new string[] { "Toto", "Titi<A, B>" })]
        [TestCase("Titi<A, B>, Toto", new string[] { "Titi<A, B>", "Toto" })]
        [TestCase("Titi<A<XX, YY>, B>, Titi<A<WW, TT>, L>", new string[] { "Titi<A<XX, YY>, B>", "Titi<A<WW, TT>, L>" })]
        public void ExtractParameters_ShouldSucceed(string input, string[] expected)
        {
            var res = ConversionBuilderTree.ExtractParameters(input);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.Count, Is.EqualTo(expected.Length));

            for (int i = 0; i < res.Count; i++)
            {
                Assert.That(res[i], Is.EqualTo(expected[i]));
            }
        }

        [Test]
        [TestCase("[u8; 4]", "u8", 4)]
        public void ExtractArray_ShouldSucceed(string input, string typeExpected, int lengthExpected)
        {
            var res = ConversionBuilderTree.ExtractArray(new NodeBuilderTypeUndefined(input, ""));

            Assert.That(res, Is.Not.Null);
            Assert.That(res.Children[0].Adapted, Is.EqualTo(typeExpected));
            Assert.That(res.Length, Is.EqualTo(lengthExpected));
        }

        [Test]
        public void ExtractPrimitive_ShouldSucceed()
        {
            var res = ConversionBuilderTree.ExtractPrimitive(new NodeBuilderTypeUndefined("u32", ""));

            Assert.That(res, Is.Not.Null);
            Assert.That(res.Primitive, Is.EqualTo(TypeDefPrimitive.U32));
        }

        [Test]
        [TestCase("Vec<u32>", typeof(NodeBuilderTypeSequence))]
        [TestCase("Option<u32>", typeof(NodeBuilderTypeOption))]
        [TestCase("AccountData<u32>", typeof(NodeBuilderTypeComposite))]
        public void ExtractGeneric_ShouldSucceed(string input, Type typeDefExpected)
        {
            var res = ConversionBuilderTree.ExtractGeneric(new NodeBuilderTypeUndefined(input, ""));

            Assert.That(res, Is.Not.Null);
            Assert.That(res.GetType(), Is.EqualTo(typeDefExpected));
        }
    }
}
