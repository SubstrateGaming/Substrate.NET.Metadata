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
        //[TestCase("<T as Trait<I>>::Proposal", 46, 4, "ImOnline")]
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
        [TestCase("TaskAddress<BlockNumber>", 29, 2)]
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
        [TestCase("BalanceOf<T>", 6, 1)]
        //[TestCase("Vec<DeferredOffenceOf<T>>", 22, 4)]
        public void GetIndex_FromComposite_ShouldSucceed(string rustType, int indexExpected, int lengthExpected, string palletContext = "")
        {
            _builder.CurrentPallet = palletContext;
            var res = _builder.BuildPortableTypes(rustType);

            Assert.That(res.Value, Is.EqualTo(indexExpected), "Index are not equals");
            Assert.That(_builder.PortableTypes.Count, Is.EqualTo(lengthExpected), "Portable elements are not equals");

            var elementState = _builder.ElementsState.Single(x => x.ClassName.ToLower() == rustType.ToLower());
            Assert.That(elementState.IsSuccessfullyMapped, Is.True);
        }

        [Test]
        [TestCase("Vec<EventRecord<T::Event, T::Hash>>")]
        public void BuildNode_ShouldSucceed(string rustType)
        {
            Assert.Fail();
        }

        [Test]
        [TestCase("u32")]
        public void GetIndex_FromPrimitive_ShouldSucceed(string primitiveType)
        {
            var res = _builder.BuildPortableTypes(primitiveType);

            Assert.That(res.Value, Is.EqualTo(0));
            Assert.That(_builder.PortableTypes.Count, Is.EqualTo(1));
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
        [TestCase("Option<Vec<u8>>")]
        public void BuildTypeDictionnary_FromComplexePrimitive_ShouldSucceed(string primitiveType)
        {

        }

        [Test]
        [TestCase("Option<Vec<u8>>")]
        public void BuildTypeDictionnary_FromTuple_ShouldSucceed(string primitiveType)
        {

        }

        [Test]
        [TestCase("Vec<(<T as frame_system::Config>::AccountId, BalanceOf<T>)>", new string[] { "AccountId", "BalanceOf" })]
        [TestCase("[Hasher = BlakeTwo128Concat / Key = AccountId / Value = AccountInfo<Index, AccountData>]", new string[] { "AccountId", "AccountInfo", "Index", "AccountData" })]
        [TestCase("Vec<(AccountId, Balance)>", new string[] { "AccountId", "Balance" })]
        [TestCase("DigestOf<T>", new string[] { "DigestOf" })]
        [TestCase("Vec<EventRecord<Event, Hash>>", new string[] { "EventRecord", "Event", "Hash" })]
        [TestCase("TaskAddress<T::BlockNumber>", new string[] { "TaskAddress", "BlockNumber" })]
        [TestCase("MaybeRandomness", new string[] { "MaybeRandomness" })]
        [TestCase("Vec<WeightToFeeCoefficient<BalanceOf<T>>>", new string[] { "WeightToFeeCoefficient", "BalanceOf" })]
        [TestCase("Option<Vec<u8>>", new string[] { "Option", "u8" })]
        [TestCase("Vec<T::AccountId>", new string[] { "AccountId" })]
        [TestCase("[Hasher = BlakeTwo128Concat / Key = AccountId / Value = StakingLedger<AccountId, BalanceOf<T>>]", new string[] { "AccountId", "StakingLedger", "BalanceOf" })]
        [TestCase("[Hasher = Twox64Concat / Key = (AccountId, slashing::SpanIndex) / Value = slashing::SpanRecord<BalanceOf<T>>]", new string[] { "AccountId", "slashing::SpanIndex", "slashing::SpanRecord", "BalanceOf" })]
        [TestCase("Option<Vec<u8>>", new string[] { "Option", "u8" })]
        [TestCase("ElectionStatus<BlockNumber>", new string[] { "ElectionStatus", "BlockNumber" })]
        [TestCase("ElectionStatus<BlockNumber>", new string[] { "ElectionStatus", "BlockNumber" })]
        [TestCase("Vec<Vec<(ParaId, CollatorId)>>", new string[] { "ParaId", "CollatorId" })]
        [TestCase("Vec<(ParaId, Option<(CollatorId, Retriable)>)>", new string[] { "ParaId", "Option", "CollatorId", "Retriable" })]
        [TestCase("Vec<DeferredOffenceOf<T>>", new string[] { "DeferredOffenceOf" })]
        [TestCase("[Hasher = Identity / Key = Hash / Value = (BlockNumber, Vec<AccountId>)]", new string[] { "Hash", "BlockNumber", "AccountId" })]
        [TestCase("OpenTip<AccountId, BalanceOf<T>, BlockNumber, Hash>", new string[] { "OpenTip", "AccountId", "BalanceOf", "BlockNumber", "Hash" })]
        [TestCase("[Key1 = BlockNumber / Key1Hasher = Twox64Concat / Key2 = Hash / Key2Hasher = Identity / Value = BlockAttestations<T>]", new string[] { "BlockNumber", "Hash", "BlockAttestations" })]
        [TestCase("[Hasher = Identity / Key = EthereumAddress / Value = (BalanceOf<T>, BalanceOf<T>, BlockNumber)]", new string[] { "EthereumAddress", "BalanceOf", "BlockNumber" })]
        [TestCase("[Hasher = Identity / Key = EthereumAddress / Value = (BalanceOf<T>, BalanceOf<T>, BlockNumber)]", new string[] { "EthereumAddress", "BalanceOf", "BlockNumber" })]
        [TestCase("[Hasher = Identity / Key = [u8; 32] / Value = (Vec<u8>, AccountId, BalanceOf<T>)]", new string[] { "u8", "AccountId", "BalanceOf" })]
        //[TestCase("Hey, (Titi<A<(XX, YY)>, B>, Titi<A<YY, TT>, L>)", new string[] { "Hey", "Titi", "A", "XX", "YY", "B", "TT", "L" })]
        public void HarmonizeFullType_WithMap_ShouldSucceed(string original, string[] mapped)
        {
            // Todo Romain : gérer les tableaux du genre [u8; 32]
            var res = ConversionBuilder.HarmonizeFullType(original).Distinct().ToList();

            Assert.That(res.Count, Is.EqualTo(mapped.Length));

            for (int i = 0; i < res.Count; i++)
            {
                Assert.That(res[i], Is.EqualTo(mapped[i]));
            }
        }




        [Test]
        [TestCase("AccountId, Balance", new string[] { "AccountId", "Balance" })]
        [TestCase("Toto, Titi<A, B>", new string[] { "Toto", "Titi<A, B>" })]
        [TestCase("Titi<A, B>, Toto", new string[] { "Titi<A, B>", "Toto" })]
        [TestCase("Titi<A<XX, YY>, B>, Titi<A<WW, TT>, L>", new string[] { "Titi<A<XX, YY>, B>", "Titi<A<WW, TT>, L>" })]
        public void ExtractParameters_ShouldSucceed(string input, string[] expected)
        {
            var res = ConversionBuilder.ExtractParameters(input);

            Assert.That(res, Is.Not.Null);
            Assert.That(res.Count, Is.EqualTo(expected.Length));

            for (int i = 0; i < res.Count; i++)
            {
                Assert.That(res[i], Is.EqualTo(expected[i]));
            }
        }
    }
}
