using NUnit.Framework.Internal.Execution;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Compare.TypeDef;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V10;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V12;
using Substrate.NET.Metadata.V13;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.V15;
using Substrate.NET.Metadata.V9;
using Substrate.NetApi;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Metadata;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Tests
{
    internal class StorageClass
    {
        public StorageClass(StorageType type, string moduleName, string propName, string className)
        {
            Type = type;
            ModuleName = moduleName;
            PropName = propName;
            ClassName = className;
            FoundInV14 = false;
        }

        public bool FoundInV14 { get; private set; }
        public int? Index_v14_9110 { get; private set; }
        public StorageType Type { get; set; }
        public string ModuleName { get; set; }
        public string PropName { get; set; }
        public string ClassName { get; set; }

        private string? classNameMapped = null;
        public string ClassNameMapped
        {
            get
            {
                if (classNameMapped is null)
                    classNameMapped = CustomMapping(ClassName);

                return classNameMapped;
            }
        }

        public static string HarmonizeTypeName(string className)
        {
            return className.Replace("T::", "");
        }

        public static List<string> HarmonizeFullType(string className)
        {
            var res = new List<string>();

            if (ExtractMap(className) is (string, string) map)
            {
                res.Add(map.key);
                res.Add(map.value);
                return ExtractDeeper(res);
            }

            if (ExtractDoubleMap(className) is (string, string, string) doubleMap)
            {
                res.Add(doubleMap.key1);
                res.Add(doubleMap.key2);
                res.Add(doubleMap.value);

                return ExtractDeeper(res);
            }

            if (ExtractTuple(className) is List<string> tuples)
            {
                res.AddRange(tuples);
                return ExtractDeeper(res);
            }

            if (ExtractArray(className) is List<string> array)
            {
                res.AddRange(array);
                return ExtractDeeper(res);
            }

            if (ExtractParameters(className) is List<string> parameters)
            {
                res.AddRange(parameters);
                return ExtractDeeper(res);
            }

            var extractGeneric = ExtractGeneric(className);

            if (extractGeneric is not null)
            {
                res.AddRange(extractGeneric.Value.genericParameters);

                return ExtractDeeper(res);
            }

            if (ExtractRustGeneric(className) is string param)
            {
                res.Add(param);
                return ExtractDeeper(res);
            }

            res.Add(HarmonizeTypeName(className));
            return res;
        }

        /// <summary>
        /// Extract Rust generic from a class name
        /// For example <T as frame_system::Config>::AccountId => AccountId
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        internal static string? ExtractRustGeneric(string className)
        {
            string pattern = @"<[^>]+>::(\w+)";

            Match match = Regex.Match(className, pattern);
            
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        internal static List<string>? ExtractParameters(string className)
        {
            List<string> result = new();

            var splitted = className.Split(new[] { "," }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
            result.AddRange(splitted);

            if (splitted.Count > 1)
            {
                List<int> indexBracketOpen = new();
                List<int> indexBracketClose = new();
                int lastIndexRemoved = 0;

                for (int i = 0; i < splitted.Count; i++)
                {
                    var diff = splitted[i].Count(i => i == '<') - splitted[i].Count(i => i == '>');

                    if (diff > 0)
                    {
                        Enumerable.Range(0, diff).ToList().ForEach(x => indexBracketOpen.Add(i));
                    }

                    if (diff < 0)
                    {
                        Enumerable.Range(0, Math.Abs(diff)).ToList().ForEach(x => indexBracketClose.Add(i));

                        if (indexBracketClose.Count == indexBracketOpen.Count)
                        {
                            var start = indexBracketOpen.First();
                            var end = indexBracketClose.Last();

                            var sub = string.Join(", ", splitted.GetRange(start, end - start + 1));

                            //result.RemoveRange(start - lastIndexRemoved, end - start + 1 - lastIndexRemoved);
                            Enumerable.Range(start, end - start + 1).ToList().ForEach(x => result.Remove(splitted[x]));
                            result.Insert(start - lastIndexRemoved, sub);

                            lastIndexRemoved = end;
                            indexBracketOpen.Clear();
                            indexBracketClose.Clear();
                        }
                    }
                }

                // At the end, let's check if something has changed
                return result.First() == className ? null : result;
            }

            return null;
        }

        private static List<string>? ExtractArray(string className)
        {
            string pattern = @"\[(.*);\s*(\d+)\]";
            Match match = Regex.Match(className, pattern);

            if (match.Success)
            {
                //match.Groups[1].Value -> array size
                return new List<string>() { match.Groups[1].Value };
            }

            return null;
        }

        private static List<string>? ExtractTuple(string className)
        {
            string pattern = @"\((.*)\)$";
            Match match = Regex.Match(className, pattern);

            if (match.Success)
            {
                return new List<string>() { match.Groups[1].Value };
            }

            return null;
        }

        private static List<string> ExtractDeeper(List<string> res)
        {
            //if (res.Count > 1)
            //{
            for (int i = 0; i < res.Count; i++)
            {
                var r = HarmonizeFullType(res[i]);
                if (r.Count > 1)
                {
                    res.Remove(res[i]);
                    res.AddRange(r);
                    i = -1;
                }
                else
                {
                    res[i] = r[0];
                }
            }
            //}

            return res;
        }

        public static (string key1, string key2, string value)? ExtractDoubleMap(string className)
        {
            string pattern = @"Key1\s*=\s*([^\/]+)\s*\/\s*Key1Hasher\s*=\s*([^\/]+)\s*\/\s*Key2\s*=\s*([^\/]+)\s*\/\s*Key2Hasher\s*=\s*([^\/]+)\s*\/\s*Value\s*=\s*([^\]]+)";

            Match match = Regex.Match(className, pattern);
            if (match.Success)
            {
                string key1 = match.Groups[1].Value.Trim();
                string key2 = match.Groups[3].Value.Trim();
                string value = match.Groups[5].Value.Trim();
                return (key1, key2, value);
            }

            return null;
        }
        public static (string key, string value)? ExtractMap(string className)
        {
            string pattern = @"Key\s*=\s*([^\/]+)\s*\/\s*Value\s*=\s*([^\]]+)";

            Match match = Regex.Match(className, pattern);
            if (match.Success)
            {
                string key = match.Groups[1].Value.Trim();
                string value = match.Groups[2].Value.Trim();
                return (key, value);
            }

            return null;
        }

        public static (TypeDefEnum typeDefEnum, List<string> genericParameters)? ExtractGeneric(string className)
        {
            //string pattern = @"(\w*)<\(?([^(>|))]+)\)?>";
            string pattern = @"([a-zA-Z:]*|)<(.*)>$";
            Match match = Regex.Match(className, pattern);

            if (match.Success)
            {
                var genericParameters = match.Groups[2].Value;

                // A valid pattern should have the same number of '<' and '>' and '<' should be before '>'
                if (!HaveValidParametersPattern(genericParameters))
                {
                    return null;
                }

                var typeDef = GetTypeDefFromString(match.Groups[1].Value);

                List<string> genericValue = new();
                if (!GenericValueToIgnore.Contains(genericParameters))
                {
                    genericValue.Add(genericParameters);
                }

                // A composite is basically a class, so let's add it
                if (typeDef == TypeDefEnum.Composite)
                {
                    genericValue.Insert(0, match.Groups[1].Value);
                }
                return (typeDef, genericValue);
            }

            return null;
        }

        /// <summary>
        /// A valid pattern should have the same number of '<' and '>' and '<' should be before '>'
        /// </summary>
        /// <param name="genericParameters"></param>
        /// <returns></returns>
        private static bool HaveValidParametersPattern(string genericParameters)
        {
            var nbOpenBracket = genericParameters.Count(x => x == '<');
            var nbClosedBracket = genericParameters.Count(x => x == '<');

            return nbOpenBracket == nbClosedBracket &&
                nbOpenBracket == 0 || (genericParameters.IndexOf('<') < genericParameters.IndexOf('>'));
        }

        public static string[] GenericValueToIgnore = ["T"];

        public static TypeDefEnum GetTypeDefFromString(string genericType)
        {
            return genericType switch
            {
                "Vec" => TypeDefEnum.Sequence,
                _ => TypeDefEnum.Composite
            };
        }

        public string CustomMapping(string className)
        {
            return className switch
            {
                "TaskAddress<BlockNumber>" => "TaskAddress<T::BlockNumber>",
                "Vec<IdentificationTuple>" => "Vec<IdentificationTuple<T>>",
                "Vec<AccountId>" => "Vec<T::AccountId>",
                "Vec<(AccountId, Balance)>" => "Vec<(<T as frame_system::Config>::AccountId, BalanceOf<T>)>",
                "sp_std::marker::PhantomData<(AccountId, Event)>" => "BaseVoid", // Pas sur de moi
                "Timepoint<BlockNumber>" => "Timepoint<T::BlockNumber>",
                "limits::BlockWeights" => "BlockWeights",
                "limits::BlockLength" => "BlockLength",
                "TransactionPriority" => "u64",
                "& 'static[u8]" => "vec<u8>",
                "&[u8]" => "vec<u8>",
                "Moment" => "u64",
                "ModuleId" => "PalletId",
                "LeasePeriod" => "LeasePeriodOf<T>",
                _ => className
            };
        }

        public static int? TryHardBinding(string className)
        {
            return className switch
            {
                "u8" => 2,
                "u16" => 73,
                "u32" => 4,
                "u64" => 8,
                "u128" => 6,
                "bool" => 58,
                "Str" => 108,
                "Vec<WeightToFeeCoefficient<BalanceOf<T>>>" => 386,
                "LockIdentifier" => 125,
                "[u8;8]" => 125,
                _ => null
            };
        }

        public void SetToFoundInV14(int index)
        {
            FoundInV14 = true;
            Index_v14_9110 = index;
        }

        public static string WriteStoragePlain(Str plain) => WriteStoragePlain(plain.Value);
        public static string WriteStoragePlain(string plain)
        {
            return StorageClass.HarmonizeTypeName(plain.ToString());
        }

        public static string WriteStorageMap(string hasher, string key, string value, bool? linked)
        {
            return $"[Hasher = {HarmonizeTypeName(hasher)} / Key = {HarmonizeTypeName(key)} / Value = {HarmonizeTypeName(value)}]";
        }

        public static string WriteStorageDoubleMap(string key1, string key1Hasher, string key2, string key2Hasher, string value)
        {
            return $"Key1 = {HarmonizeTypeName(key1)} / Key1Hasher = {HarmonizeTypeName(key1Hasher)} / Key2 = {HarmonizeTypeName(key2)} / Key2Hasher = {HarmonizeTypeName(key2Hasher)} / Value = {HarmonizeTypeName(value)}";
        }

        public static string WriteStorageMap(StorageEntryTypeMapV9 typeMap)
            => WriteStorageMap(typeMap.Hasher.Value.ToString(), typeMap.Key.Value, typeMap.Value.Value, typeMap.Linked.Value);

        public static string WriteStorageMap(StorageEntryTypeMapV10 typeMap)
            => WriteStorageMap(typeMap.Hasher.Value.ToString(), typeMap.Key.Value, typeMap.Value.Value, typeMap.Linked.Value);

        public static string WriteStorageMap(StorageEntryTypeMapV11 typeMap)
            => WriteStorageMap(typeMap.Hasher.Value.ToString(), typeMap.Key.Value, typeMap.Value.Value, typeMap.Linked.Value);

        public static string WriteStorageMap(StorageEntryTypeMapV14 typeMap, MetadataV14 metadataV14)
        {
            return WriteStorageMap(
                typeMap.Hashers.Value.First().Value.ToString(),
                WriteStorageKey(typeMap, metadataV14),
                WriteStorageValue(typeMap, metadataV14), null);
        }

        public static string WriteStorageKey(StorageEntryTypeMapV14 typeMap, MetadataV14 metadataV14)
        {
            return WriteStorage(metadataV14.RuntimeMetadataData.Lookup, typeMap.Key.Value);
        }

        public static string WriteStorageValue(StorageEntryTypeMapV14 typeMap, MetadataV14 metadataV14)
        {
            return WriteStorage(metadataV14.RuntimeMetadataData.Lookup, typeMap.Value.Value);
        }

        private static string WriteStorage(PortableRegistry p, int index)
        {
            if (p.Value[index].Ty.TypeDef.Value2 is BaseEnum<TypeDefPrimitive> typeDefPrimitive)
            {
                return StorageClass.HarmonizeTypeName(typeDefPrimitive.Value.ToString());
            }
            else
            {
                if (p.Value[index].Ty.Path.Value.Count() == 0)
                {

                }
                else
                {
                    return StorageClass.HarmonizeTypeName(p.Value[index].Ty.Path.Value.Last().Value);
                }
            }

            return "";
        }

        public static string WriteStorageDoubleMap(StorageEntryTypeDoubleMapV9 doubleMap)
            => WriteStorageDoubleMap(doubleMap.Key1.Value, doubleMap.Key1Hasher.Value.ToString(), doubleMap.Key2.Value, doubleMap.Key2Hasher.Value.ToString(), doubleMap.Value.Value);

        public static string WriteStorageDoubleMap(StorageEntryTypeDoubleMapV10 doubleMap)
            => WriteStorageDoubleMap(doubleMap.Key1.Value, doubleMap.Key1Hasher.Value.ToString(), doubleMap.Key2.Value, doubleMap.Key2Hasher.Value.ToString(), doubleMap.Value.Value);

        public static string WriteStorageDoubleMap(StorageEntryTypeDoubleMapV11 doubleMap)
            => WriteStorageDoubleMap(doubleMap.Key1.Value, doubleMap.Key1Hasher.Value.ToString(), doubleMap.Key2.Value, doubleMap.Key2Hasher.Value.ToString(), doubleMap.Value.Value);

        public static string WriteStorageNMap(StorageEntryTypeNMapV13 nMap)
        {
            var hasher = string.Join(",", nMap.Hashers.Value.Select(x => x.Value));
            var keyVec = string.Join(",", nMap.KeyVec.Value.Select(x => x.Value));
            return $"[Hashers = [{hasher} / Keys = {keyVec} / Value = {nMap.Value.Value}";
        }

        public override string ToString()
        {
            return $"{ModuleName}|{Type}|{PropName}|{ClassName}|{(FoundInV14 ? ClassNameMapped : "")}|{FoundInV14}|{(Index_v14_9110 is not null ? Index_v14_9110.Value : "")}";
        }

        public enum StorageType
        {
            Call,
            Event,
            Storage,
            Constant,
            Error
        }
    }

    internal class MetadataVersionCompact
    {
        public MetadataVersionCompact(int majorVersion, int minorVersion, string blockHash)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            BlockHash = blockHash;
        }

        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public string BlockHash { get; set; }

        public List<StorageClass> StorageClasses { get; set; } = new List<StorageClass>();

        public List<string> Flatten()
        {
            var res = new List<string>();

            foreach (var storageClass in StorageClasses)
            {
                res.Add($"{MajorVersion}|{MinorVersion}|{storageClass.ToString()}");
            }

            return res;
        }

        public void AddOrInc(Dictionary<string, int> uniqueName, string name)
        {
            if (uniqueName.ContainsKey(name))
            {
                uniqueName[name] += 1;
            }
            else
            {
                uniqueName[name] = 1;
            }
        }

        public void GetDistinctStorageClasses(Dictionary<string, int> uniqueName)
        {
            var res = new List<string>();

            var distinctClasses = StorageClasses.Select(x => x.ClassNameMapped).Distinct().ToList();

            for (int i = 0; i < distinctClasses.Count; i++)
            {
                var classes = StorageClass.HarmonizeFullType(distinctClasses[i]);

                foreach (var c in classes)
                {
                    AddOrInc(uniqueName, c);
                }
            }
        }

        public override string ToString() => $"{MajorVersion}.{MinorVersion}.{BlockHash}";
    }

    internal class TmpTests
    {
        private MetadataService _metadataService;
        private SubstrateClient _substrateClient;

        [SetUp]
        public async Task SetupAsync()
        {
            _metadataService = new MetadataService();
            _substrateClient = new SubstrateClient(new Uri("wss://polkadot-rpc.dwellir.com"), ChargeTransactionPayment.Default());

            //await _substrateClient.ConnectAsync();
            //Assert.That(_substrateClient.IsConnected, Is.True);
        }

        [TearDown]
        public void Teardown()
        {
            _substrateClient.Dispose();
        }

        public string v14Hex
        {
            get
            {
                return _substrateClient.State.GetMetaDataAtAsync(Utils.HexToByteArray("0x7a233affefea10b90b3b5b2b37a6e9d8ef0029ef7add14ac4a56a3520b602927"), CancellationToken.None).Result;
            }
        }

        private MetadataV14? _v14 = null;
        public MetadataV14 v14
        {
            get
            {
                if (_v14 == null)
                {
                    _v14 = new MetadataV14(v14Hex);
                }

                return _v14;
            }
        }

        public Substrate.NetApi.Model.Meta.MetaData MetadataNetApi
        {
            get
            {
                var mdv14 = new Substrate.NetApi.Model.Types.Metadata.RuntimeMetadata();
                mdv14.Create(v14Hex);
                return new MetaData(mdv14, "");
            }
        }

        //[Test]
        //[TestCase("[Hasher = BlakeTwo128Concat / Key = AccountId / Value = AccountInfo<Index, AccountData>]", new string[] { "AccountId", "AccountInfo", "Index", "AccountData" })]
        //[TestCase("Vec<(AccountId, Balance)>", new string[] { "AccountId", "Balance" })]
        //[TestCase("DigestOf<T>", new string[] { "DigestOf" })]
        //[TestCase("Vec<EventRecord<Event, Hash>>", new string[] { "EventRecord", "Event", "Hash" })]
        //[TestCase("TaskAddress<T::BlockNumber>", new string[] { "TaskAddress", "BlockNumber" })]
        //[TestCase("MaybeRandomness", new string[] { "MaybeRandomness" })]
        //[TestCase("Vec<WeightToFeeCoefficient<BalanceOf<T>>>", new string[] { "WeightToFeeCoefficient", "BalanceOf" })]
        //[TestCase("Option<Vec<u8>>", new string[] { "Option", "u8" })]
        //[TestCase("Vec<T::AccountId>", new string[] { "AccountId" })]
        //[TestCase("[Hasher = BlakeTwo128Concat / Key = AccountId / Value = StakingLedger<AccountId, BalanceOf<T>>]", new string[] { "AccountId", "StakingLedger", "BalanceOf" })]
        //[TestCase("[Hasher = Twox64Concat / Key = (AccountId, slashing::SpanIndex) / Value = slashing::SpanRecord<BalanceOf<T>>]", new string[] { "AccountId", "slashing::SpanIndex", "slashing::SpanRecord", "BalanceOf" })]
        //[TestCase("Option<Vec<u8>>", new string[] { "Option", "u8" })]
        //[TestCase("ElectionStatus<BlockNumber>", new string[] { "ElectionStatus", "BlockNumber" })]
        //[TestCase("ElectionStatus<BlockNumber>", new string[] { "ElectionStatus", "BlockNumber" })]
        //[TestCase("Vec<Vec<(ParaId, CollatorId)>>", new string[] { "ParaId", "CollatorId" })]
        //[TestCase("Vec<(ParaId, Option<(CollatorId, Retriable)>)>", new string[] { "ParaId", "Option", "CollatorId", "Retriable" })]
        //[TestCase("Vec<DeferredOffenceOf<T>>", new string[] { "DeferredOffenceOf" })]
        //[TestCase("[Hasher = Identity / Key = Hash / Value = (BlockNumber, Vec<AccountId>)]", new string[] { "Hash", "BlockNumber", "AccountId" })]
        //[TestCase("Vec<(<T as frame_system::Config>::AccountId, BalanceOf<T>)>", new string[] { "AccountId", "BalanceOf" })]
        //[TestCase("OpenTip<AccountId, BalanceOf<T>, BlockNumber, Hash>", new string[] { "OpenTip", "AccountId", "BalanceOf", "BlockNumber", "Hash" })]
        //[TestCase("[Key1 = BlockNumber / Key1Hasher = Twox64Concat / Key2 = Hash / Key2Hasher = Identity / Value = BlockAttestations<T>]", new string[] { "BlockNumber", "Hash", "BlockAttestations" })]
        //[TestCase("[Hasher = Identity / Key = EthereumAddress / Value = (BalanceOf<T>, BalanceOf<T>, BlockNumber)]", new string[] { "EthereumAddress", "BalanceOf", "BlockNumber" })]
        //[TestCase("[Hasher = Identity / Key = EthereumAddress / Value = (BalanceOf<T>, BalanceOf<T>, BlockNumber)]", new string[] { "EthereumAddress", "BalanceOf", "BlockNumber" })]
        //[TestCase("[Hasher = Identity / Key = [u8; 32] / Value = (Vec<u8>, AccountId, BalanceOf<T>)]", new string[] { "u8", "AccountId", "BalanceOf" })]
        ////[TestCase("Hey, (Titi<A<(XX, YY)>, B>, Titi<A<YY, TT>, L>)", new string[] { "Hey", "Titi", "A", "XX", "YY", "B", "TT", "L" })]
        //public void HarmonizeFullType_WithMap_ShouldSucceed(string original, string[] mapped)
        //{
        //    // Todo Romain : gérer les tableaux du genre [u8; 32]
        //    var res = StorageClass.HarmonizeFullType(original).Distinct().ToList();

        //    Assert.That(res.Count, Is.EqualTo(mapped.Length));

        //    for (int i = 0; i < res.Count; i++)
        //    {
        //        Assert.That(res[i], Is.EqualTo(mapped[i]));
        //    }
        //}


        

        //[Test]
        //[TestCase("AccountId, Balance", new string[] { "AccountId", "Balance" })]
        //[TestCase("Toto, Titi<A, B>", new string[] { "Toto", "Titi<A, B>" })]
        //[TestCase("Titi<A, B>, Toto", new string[] { "Titi<A, B>", "Toto" })]
        //[TestCase("Titi<A<XX, YY>, B>, Titi<A<WW, TT>, L>", new string[] { "Titi<A<XX, YY>, B>", "Titi<A<WW, TT>, L>" })]
        //public void ExtractParameters_ShouldSucceed(string input, string[] expected)
        //{
        //    var res = StorageClass.ExtractParameters(input);

        //    Assert.That(res, Is.Not.Null);
        //    Assert.That(res.Count, Is.EqualTo(expected.Length));

        //    for (int i = 0; i < res.Count; i++)
        //    {
        //        Assert.That(res[i], Is.EqualTo(expected[i]));
        //    }
        //}

        [Test]
        public async Task GetAllStorageClasses_FromEveryVersionBeforeV14_ShouldSucceedAsync()
        {
            //var v14HexLast = await _substrateClient.State.GetMetaDataAsync();
            //var v14Hex = await _substrateClient.State.GetMetaDataAtAsync(Utils.HexToByteArray("0x7a233affefea10b90b3b5b2b37a6e9d8ef0029ef7add14ac4a56a3520b602927"), CancellationToken.None);

            var metadatas = new List<MetadataVersionCompact>
            {
                new MetadataVersionCompact(11, 1, "0xa15bf122d7a9b2cd07956c3af8f7eda61298aff0a3bd1193018df413d982a4ef"),
                new MetadataVersionCompact(11, 5, "0x49a695416fcf55487adf9b9f5808619cee6618bd285a0119a78db60c5a7f3e13"),
                new MetadataVersionCompact(11, 6, "0xdab56553594fd489adc085d2f83f3dcb65f5de5b1878325d5547fcf72b6dd6b3"),
                new MetadataVersionCompact(11, 7, "0xaff11447ba47e853bcadb73920c0780013e581a0286831eadce0e48bf1db9942"),
                new MetadataVersionCompact(11, 8, "0x3eef3df8dd7eedfe6baaed544dcef9c36ee7a142035bd6ae3fd7fd2969ad5933"),
                new MetadataVersionCompact(11, 9, "0x773e8d8c1fdcbc94d79bb2cdcaf2f7fca5ff869d34c50171fe50ac1640f3e05c"),
                new MetadataVersionCompact(11, 10, "0x209d24943d81e5910bfb7a57c8a5c3037c4a434af7c486a536d4f96a2160c9f5"),
                new MetadataVersionCompact(11, 11, "0x3eef3df8dd7eedfe6baaed544dcef9c36ee7a142035bd6ae3fd7fd2969ad5933"),
                new MetadataVersionCompact(11, 12, "0x4f61e8e6017cce5a10e2de7340061a037895411c19e7bc27f607953d8a56a943"),
                new MetadataVersionCompact(11, 13, "0x7cf0e5e3671b02ca341ab439453e1a9ca6616c6e351bdfa8cbb6f5124a8224b8"),
                new MetadataVersionCompact(11, 14, "0x349399f6b6481070047626f184d0152699bd02cb71040ef045adefc4f8daec5b"),
                new MetadataVersionCompact(11, 15, "0xab6f40697e72d0a4ab0d105145b1ddc1bd8cead0bb1d6a13f4ac1a05508b1882"),
                new MetadataVersionCompact(11, 16, "0x78433cba37c0cbc777faad71f27e21094defe24451033dd4d9a0443a1dc291b2"),
                new MetadataVersionCompact(11, 17, "0x9a18fb9522d9688eadefb59bf7389fc0deaf219b080a462d6db5ee1d3ec79562"),
                new MetadataVersionCompact(11, 18, "0xe187cabcfb9d30b66fae7257f3820b947c89b65e6e20f6ead2af4fd78926825f"),
                new MetadataVersionCompact(11, 23, "0xd85988f4f7bc3594200fd5f8e738153260f2ba342b56d9bf4edabf9209c65ae0"),
                new MetadataVersionCompact(11, 24, "0x8a0f28f399d3a85643413435e5a001c4ade7b8195eaf7e91f399549f192a56c7"),
                new MetadataVersionCompact(12, 25, "0x0b40f111395a0871b1a59da4bd8587bcddf662e3e7b4ba06d8862a44dacf6256"),
                new MetadataVersionCompact(12, 26, "0x780cd3091ef9cdc3a94dbcdfa7908d6120f5e037a1e534b4f18249c37005a533"),
                new MetadataVersionCompact(12, 27, "0x26fa4c7e27aad01f5a8367aefece17f8c0940b75423628d52a8c07a203bd2458"),
                new MetadataVersionCompact(12, 28, "0xd00026d2bdca30cd20f7c361544e9651041107a1fd35ea447d8dfabb5001960f"),
                new MetadataVersionCompact(12, 29, "0x766a02e9abe37328ad0dee3a64aad557dd1bade9e9f27849a9cf5838c0e7e6bb"),
                new MetadataVersionCompact(12, 30, "0x8a8a7aa9ac863359b2cc609b836e3ebd7086d775c31f8c177f183ed9ffa71fa5"),
                new MetadataVersionCompact(13, 9050, "0x89f4608b568a05a89737bc826a32f8677f2646a5773312d7dee3a667458e5a98"),
                new MetadataVersionCompact(13, 9080, "0xddca674d4e6e4935b1e5e81ed697f7ce027c2a9602523424bcc8dee3184e4863"),
                new MetadataVersionCompact(13, 9090, "0x4b34bd42835a084af0f441c5986d216b18a85abb4d03762096692e6f06365203"),
                new MetadataVersionCompact(13, 9100, "0x8192df1609d4478d1c1fcf5d39975596a8162d42d745c141c22148b0931708f2")
            };

            foreach (var metadata in metadatas)
            {
                var metadataHex = await _substrateClient.State.GetMetaDataAtAsync(Utils.HexToByteArray(metadata.BlockHash), CancellationToken.None);
                var metadataVersion = _metadataService.GetMetadataVersion(metadataHex);
                Assert.That(metadata.MajorVersion, Is.EqualTo((int)metadataVersion));

                switch (metadata.MajorVersion)
                {
                    case 9:
                        StoreV9(metadata, metadataHex);
                        break;
                    case 10:
                        StoreV10(metadata, metadataHex);
                        break;
                    case 11:
                        StoreV11(metadata, metadataHex);
                        break;
                    case 12:
                        StoreV12(metadata, metadataHex);
                        break;
                    case 13:
                        StoreV13(metadata, metadataHex);
                        break;
                }
            }

            var uniqueName = new Dictionary<string, int>();
            metadatas[0].GetDistinctStorageClasses(uniqueName);
            var classes = new List<string>();
            var v14Classed = BuildV14ClassesDic(v14);

            foreach (var searchType in classes)
            {
                var index = FindInV14(v14, searchType);

                var flattenClasses = metadatas.SelectMany(x => x.StorageClasses).Where(x => x.ClassNameMapped.Equals(searchType, StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (index is not null)
                {
                    flattenClasses.ForEach(x => x.SetToFoundInV14(index.Value));
                }
            }

            var flatten = metadatas.SelectMany(x => x.Flatten()).ToList();
            var flattenHuman = string.Join("\n", flatten);

            Assert.Pass();
        }

        [Test]
        public async Task SearchInV14Async()
        {
            var v14Hex = await _substrateClient.State.GetMetaDataAtAsync(Utils.HexToByteArray("0x7a233affefea10b90b3b5b2b37a6e9d8ef0029ef7add14ac4a56a3520b602927"), CancellationToken.None);
            var v14 = new MetadataV14(v14Hex);

            List<string> searchTypes = ["BlockNumber", "AccountId", "u32", "TaskAddress<BlockNumber>", "Option<Vec<u8>>", "DispatchResult", "AccountIndex", "Balance", "EraIndex", "SessionIndex", "ElectionCompute", "Kind", "OpaqueTimeSlot", "bool", "AuthorityList", "AuthorityId", "DispatchInfo", "DispatchError"];

            //var t1 = FindInV14(v14, "AccountId");

            foreach (var searchType in searchTypes)
            {
                var lookupIndex = FindInV14(v14, searchType);
            }

        }

        public void AddDic(Dictionary<string, int> dic, string key, int index)
        {
            key = StorageClass.HarmonizeTypeName(key);

            if (!dic.ContainsKey(key))
            {
                dic.Add(key, index);
            }
        }

        public void LoopType(Dictionary<string, int> dic, int index, MetadataV14 metadataV14)
        {
            var ty = metadataV14.RuntimeMetadataData.Lookup.Value[index].Ty;

            if (ty.TypeDef.Value2 is BaseEnum<TypeDefPrimitive> typeDefPrimitive)
            {
                AddDic(dic, typeDefPrimitive.Value.ToString(), index);
            }
            else if (ty.TypeDef.Value2 is TypeDefVariant variant)
            {
                foreach (var field in variant.TypeParam.Value.SelectMany(x => x.VariantFields.Value))
                {
                    LoopType(dic, field.FieldTy.Value, metadataV14);
                    //AddDic(dic, field.FieldTypeName.Value.Value, field.FieldTy.Value);
                }
            }
            else if (ty.TypeDef.Value2 is TypeDefComposite composite)
            {
                AddDic(dic, ty.Path.Value[^1].Value, index);

                foreach (var field in composite.Fields.Value)
                {
                    LoopType(dic, field.FieldTy.Value, metadataV14);
                    //AddDic(dic, field.FieldTypeName.Value.Value, field.FieldTy.Value);
                }
            }
            else if (ty.TypeDef.Value2 is TypeDefArray array)
            {
                LoopType(dic, array.ElemType.Value, metadataV14);
            }
            else if (ty.TypeDef.Value2 is TypeDefSequence sequence)
            {
                LoopType(dic, sequence.ElemType.Value, metadataV14);
            }
            else if (ty.TypeDef.Value2 is TypeDefTuple tuple)
            {
                foreach (var field in tuple.Fields.Value)
                {
                    LoopType(dic, field.Value, metadataV14);
                }
            }
            else if (ty.TypeDef.Value2 is TypeDefCompact compact)
            {
                LoopType(dic, compact.ElemType.Value, metadataV14);
            }
            else
            {

            }
        }

        public Dictionary<string, int> BuildV14ClassesDic(MetadataV14 metadataV14)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            var modules = metadataV14.RuntimeMetadataData.Modules.Value;

            foreach (var module in modules)
            {
                var storage = module.Storage.Value;
                if (storage == null)
                    continue;

                foreach (var entry in storage.Entries.Value)
                {
                    //AddDic(dic, entry.Name.Value, ((CompactIntegerType)entry.StorageType.Value2).Value);

                    if (entry.StorageType.Value == StorageType.Type.Plain)
                    {
                        var storagePlainIndex = entry.StorageType.Value2 as TType ?? throw new InvalidOperationException();
                        LoopType(dic, storagePlainIndex.Value, metadataV14);
                    }
                    else if (entry.StorageType.Value == StorageType.Type.Map)
                    {
                        var storageMap = entry.StorageType.Value2 as StorageEntryTypeMapV14 ?? throw new InvalidOperationException();

                        var keyString = WriteType(storageMap.Key.Value);
                        var valueString = WriteType(storageMap.Value.Value);

                        LoopType(dic, storageMap.Key.Value, metadataV14);
                        LoopType(dic, storageMap.Value.Value, metadataV14);
                    }
                    else if (entry.StorageType.Value == StorageType.Type.DoubleMap)
                    {
                        throw new InvalidOperationException();
                    }
                    else if (entry.StorageType.Value == StorageType.Type.NMap)
                    {
                        throw new InvalidOperationException();
                    }

                }

                if (module.Events.OptionFlag)
                {
                    LoopType(dic, module.Events.Value.ElemType.Value, metadataV14);
                }

                foreach (var constant in module.Constants.Value)
                {
                    LoopType(dic, constant.ConstantType.Value, metadataV14);
                }
            }

            return dic;
        }

        public int? FindInV14(MetadataV14 metadataV14, string storageClass)
        {
            var modules = metadataV14.RuntimeMetadataData.Modules.Value;

            foreach (var module in modules)
            {
                var storage = module.Storage.Value;
                if (storage == null)
                {
                    continue;
                }

                foreach (var entry in storage.Entries.Value)
                {
                    if (entry.Name.Value == storageClass)
                    {
                        return ((CompactIntegerType)entry.StorageType.Value2).Value;
                    }

                    if (entry.StorageType.Value == StorageType.Type.Plain)
                    {
                        var storagePlainIndex = entry.StorageType.Value2 as TType ?? throw new InvalidOperationException();
                        var typeDef = metadataV14.RuntimeMetadataData.Lookup.Value[storagePlainIndex.Value].Ty.TypeDef;
                        var storagePlainName = "";
                        if (typeDef.Value2 is BaseEnum<TypeDefPrimitive> typeDefPrimitive)
                        {
                            storagePlainName = typeDefPrimitive.Value.ToString();
                        }
                        else
                        {

                        }

                        if (StorageClass.WriteStoragePlain(storagePlainName).Equals(storageClass, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return ((CompactIntegerType)entry.StorageType.Value2).Value;
                        }
                    }
                    else if (entry.StorageType.Value == StorageType.Type.Map)
                    {
                        var storageMap = entry.StorageType.Value2 as StorageEntryTypeMapV14 ?? throw new InvalidOperationException();

                        if (StorageClass.WriteStorageKey(storageMap, metadataV14).Equals(storageClass, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return storageMap.Key.Value;
                        }
                        if (StorageClass.WriteStorageValue(storageMap, metadataV14).Equals(storageClass, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return storageMap.Value.Value;
                        }
                    }
                    else if (entry.StorageType.Value == StorageType.Type.DoubleMap)
                    {
                        throw new InvalidOperationException();
                    }
                    else if (entry.StorageType.Value == StorageType.Type.NMap)
                    {
                        throw new InvalidOperationException();
                    }


                }

                if (module.Events.OptionFlag)
                {
                    var events = metadataV14.RuntimeMetadataData.Lookup.Value[(int)module.Events.Value.ElemType.Value];
                    if (events.Ty.TypeDef.Value2 is TypeDefVariant variant)
                    {
                        foreach (var field in variant.TypeParam.Value.SelectMany(x => x.VariantFields.Value))
                        {
                            var fieldValue = StorageClass.HarmonizeTypeName(field.FieldTypeName.Value.Value);
                            if (fieldValue.Equals(storageClass, StringComparison.CurrentCultureIgnoreCase))
                            {
                                return field.FieldTy.Value;
                            }
                        }
                    }
                }

                foreach (var constant in module.Constants.Value)
                {
                    var associatedLookup = metadataV14.RuntimeMetadataData.Lookup.Value[constant.ConstantType.Value];

                    if (associatedLookup.Ty.Path is not null &&
                        associatedLookup.Ty.Path.Value.Length > 0 &&
                        associatedLookup.Ty.Path.Value.Last().Value.Equals(storageClass, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return constant.ConstantType.Value;
                    }
                }
            }

            // Last chance bro
            return StorageClass.TryHardBinding(storageClass);
        }

        [Test]
        public async Task DebugAsync()
        {
            var v14Hex = await _substrateClient.State.GetMetaDataAtAsync(Utils.HexToByteArray("0x7a233affefea10b90b3b5b2b37a6e9d8ef0029ef7add14ac4a56a3520b602927"), CancellationToken.None);
            var v14 = new MetadataV14(v14Hex);

            var test1 = FindInV14(v14, "WonDeploy");
            //var test2 = FindInV14(v14, "LockIdentifier");

            // Constants
            var moduleName = "Slots";
            //var constantName = "BlockLength";

            var moduleFound = v14.RuntimeMetadataData.Modules.Value.Single(x => x.Name.Value.Equals(moduleName));
            //var constantFound = moduleFound.Constants.Value.Single(x => x.Name.Value.Equals(constantName));
            //var constant = v14.RuntimeMetadataData.Lookup.Value.First(x => x.Id.Value == constantFound.ConstantType.Value);

            // Events
            var eventName = "LeasePeriodOf<T>";
            var dico = (v14.RuntimeMetadataData.Lookup.Value.Single(x => x.Id.Value == moduleFound.Events.Value.ElemType.Value).Ty.TypeDef.Value2 as TypeDefVariant).TypeParam.Value.Single(x => x.Name.Value.Equals(eventName, StringComparison.CurrentCultureIgnoreCase));
            var x = 0;
        }

        private static void StoreV9(MetadataVersionCompact metadata, string metadataHex)
        {
            var currentMetadata = new MetadataV9(metadataHex);
            var modules = currentMetadata.RuntimeMetadataData.Modules.Value;

            foreach (var module in modules)
            {
                // Get all storage classes from events
                if (module.Events.OptionFlag)
                {
                    var vEvents = module.Events.Value.Value;
                    foreach (var vEvent in vEvents)
                    {
                        foreach (var args in vEvent.Args.Value.Select(y => y.Value))
                        {
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Event, module.Name.Value, vEvent.Name.Value, args));
                        }
                    }
                }

                var distinctStorageConstants = module.Constants.Value.Select(x => new { constName = x.Name.Value, constType = StorageClass.HarmonizeTypeName(x.ConstantType.Value) }).Distinct().ToList();

                foreach (var distinctStorageConstant in distinctStorageConstants)
                {
                    metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Constant, module.Name.Value, distinctStorageConstant.constName, distinctStorageConstant.constType));
                }

                if (module.Storage.OptionFlag)
                {
                    var distinctStorage = module.Storage.Value.Entries.Value.ToList();

                    foreach (var storage in distinctStorage)
                    {
                        if (storage.StorageType.Value == StorageType.TypeV9.Plain)
                        {
                            var storagePlain = storage.StorageType.Value2 as Str ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStoragePlain(storagePlain)));
                        }
                        else if (storage.StorageType.Value == StorageType.TypeV9.Map)
                        {
                            var storageMap = storage.StorageType.Value2 as StorageEntryTypeMapV9 ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageMap(storageMap)));
                        }
                        else if (storage.StorageType.Value == StorageType.TypeV9.DoubleMap)
                        {
                            var storageDoubleMap = storage.StorageType.Value2 as StorageEntryTypeDoubleMapV9 ?? throw new InvalidOperationException();
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageDoubleMap(storageDoubleMap)));
                        }
                    }
                }
            }
        }

        private static void StoreV10(MetadataVersionCompact metadata, string metadataHex)
        {
            var currentMetadata = new MetadataV10(metadataHex);
            var modules = currentMetadata.RuntimeMetadataData.Modules.Value;

            foreach (var module in modules)
            {
                // Get all storage classes from events
                if (module.Events.OptionFlag)
                {
                    var vEvents = module.Events.Value.Value;
                    foreach (var vEvent in vEvents)
                    {
                        foreach (var args in vEvent.Args.Value.Select(y => y.Value))
                        {
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Event, module.Name.Value, vEvent.Name.Value, args));
                        }
                    }
                }

                var distinctStorageConstants = module.Constants.Value.Select(x => new { constName = x.Name.Value, constType = StorageClass.HarmonizeTypeName(x.ConstantType.Value) }).Distinct().ToList();

                foreach (var distinctStorageConstant in distinctStorageConstants)
                {
                    metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Constant, module.Name.Value, distinctStorageConstant.constName, distinctStorageConstant.constType));
                }

                if (module.Storage.OptionFlag)
                {
                    var distinctStorage = module.Storage.Value.Entries.Value.ToList();

                    foreach (var storage in distinctStorage)
                    {
                        if (storage.StorageType.Value == StorageType.TypeV9.Plain)
                        {
                            var storagePlain = storage.StorageType.Value2 as Str ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStoragePlain(storagePlain)));
                        }
                        else if (storage.StorageType.Value == StorageType.TypeV9.Map)
                        {
                            var storageMap = storage.StorageType.Value2 as StorageEntryTypeMapV10 ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageMap(storageMap)));
                        }
                        else if (storage.StorageType.Value == StorageType.TypeV9.DoubleMap)
                        {
                            var storageDoubleMap = storage.StorageType.Value2 as StorageEntryTypeDoubleMapV10 ?? throw new InvalidOperationException();
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageDoubleMap(storageDoubleMap)));
                        }
                    }
                }
            }
        }

        private static void StoreV11(MetadataVersionCompact metadata, string metadataHex)
        {
            var currentMetadata = new MetadataV11(metadataHex);
            var modules = currentMetadata.RuntimeMetadataData.Modules.Value;

            foreach (var module in modules)
            {
                // Get all storage classes from events
                if (module.Events.OptionFlag)
                {
                    var vEvents = module.Events.Value.Value;
                    foreach (var vEvent in vEvents)
                    {
                        foreach (var args in vEvent.Args.Value.Select(y => y.Value))
                        {
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Event, module.Name.Value, vEvent.Name.Value, StorageClass.HarmonizeTypeName(args)));
                        }
                    }
                }

                var distinctStorageConstants = module.Constants.Value.Select(x => new { constName = x.Name.Value, constType = StorageClass.HarmonizeTypeName(x.ConstantType.Value) }).Distinct().ToList();

                foreach (var distinctStorageConstant in distinctStorageConstants)
                {
                    metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Constant, module.Name.Value, distinctStorageConstant.constName, distinctStorageConstant.constType));
                }

                if (module.Storage.OptionFlag)
                {
                    var distinctStorage = module.Storage.Value.Entries.Value.ToList();

                    foreach (var storage in distinctStorage)
                    {
                        if (storage.StorageType.Value == StorageType.TypeV9.Plain)
                        {
                            var storagePlain = storage.StorageType.Value2 as Str ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStoragePlain(storagePlain)));
                        }
                        else if (storage.StorageType.Value == StorageType.TypeV9.Map)
                        {
                            var storageMap = storage.StorageType.Value2 as StorageEntryTypeMapV11 ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageMap(storageMap)));
                        }
                        else if (storage.StorageType.Value == StorageType.TypeV9.DoubleMap)
                        {
                            var storageDoubleMap = storage.StorageType.Value2 as StorageEntryTypeDoubleMapV11 ?? throw new InvalidOperationException();
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageDoubleMap(storageDoubleMap)));
                        }
                    }
                }
            }
        }

        private static void StoreV12(MetadataVersionCompact metadata, string metadataHex)
        {
            var currentMetadata = new MetadataV12(metadataHex);
            var modules = currentMetadata.RuntimeMetadataData.Modules.Value;

            foreach (var module in modules)
            {
                // Get all storage classes from events
                if (module.Events.OptionFlag)
                {
                    var vEvents = module.Events.Value.Value;
                    foreach (var vEvent in vEvents)
                    {
                        foreach (var args in vEvent.Args.Value.Select(y => y.Value))
                        {
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Event, module.Name.Value, vEvent.Name.Value, StorageClass.HarmonizeTypeName(args)));
                        }
                    }
                }

                var distinctStorageConstants = module.Constants.Value.Select(x => new { constName = x.Name.Value, constType = StorageClass.HarmonizeTypeName(x.ConstantType.Value) }).Distinct().ToList();

                foreach (var distinctStorageConstant in distinctStorageConstants)
                {
                    metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Constant, module.Name.Value, distinctStorageConstant.constName, distinctStorageConstant.constType));
                }

                if (module.Storage.OptionFlag)
                {
                    var distinctStorage = module.Storage.Value.Entries.Value.ToList();

                    foreach (var storage in distinctStorage)
                    {
                        if (storage.StorageType.Value == StorageType.TypeV9.Plain)
                        {
                            var storagePlain = storage.StorageType.Value2 as Str ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStoragePlain(storagePlain)));
                        }
                        else if (storage.StorageType.Value == StorageType.TypeV9.Map)
                        {
                            var storageMap = storage.StorageType.Value2 as StorageEntryTypeMapV11 ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageMap(storageMap)));
                        }
                        else if (storage.StorageType.Value == StorageType.TypeV9.DoubleMap)
                        {
                            var storageDoubleMap = storage.StorageType.Value2 as StorageEntryTypeDoubleMapV11 ?? throw new InvalidOperationException();
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageDoubleMap(storageDoubleMap)));
                        }
                    }
                }
            }
        }

        private static void StoreV13(MetadataVersionCompact metadata, string metadataHex)
        {
            var currentMetadata = new MetadataV13(metadataHex);
            var modules = currentMetadata.RuntimeMetadataData.Modules.Value;

            foreach (var module in modules)
            {
                // Get all storage classes from events
                if (module.Events.OptionFlag)
                {
                    var vEvents = module.Events.Value.Value;
                    foreach (var vEvent in vEvents)
                    {
                        foreach (var args in vEvent.Args.Value.Select(y => y.Value))
                        {
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Event, module.Name.Value, vEvent.Name.Value, StorageClass.HarmonizeTypeName(args)));
                        }
                    }
                }

                var distinctStorageConstants = module.Constants.Value.Select(x => new { constName = x.Name.Value, constType = StorageClass.HarmonizeTypeName(x.ConstantType.Value) }).Distinct().ToList();

                foreach (var distinctStorageConstant in distinctStorageConstants)
                {
                    metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Constant, module.Name.Value, distinctStorageConstant.constName, distinctStorageConstant.constType));
                }

                if (module.Storage.OptionFlag)
                {
                    var distinctStorage = module.Storage.Value.Entries.Value.ToList();

                    foreach (var storage in distinctStorage)
                    {
                        if (storage.StorageType.Value == StorageType.Type.Plain)
                        {
                            var storagePlain = storage.StorageType.Value2 as Str ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStoragePlain(storagePlain)));
                        }
                        else if (storage.StorageType.Value == StorageType.Type.Map)
                        {
                            var storageMap = storage.StorageType.Value2 as StorageEntryTypeMapV11 ?? throw new InvalidOperationException();

                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageMap(storageMap)));
                        }
                        else if (storage.StorageType.Value == StorageType.Type.DoubleMap)
                        {
                            var storageDoubleMap = storage.StorageType.Value2 as StorageEntryTypeDoubleMapV11 ?? throw new InvalidOperationException();
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageDoubleMap(storageDoubleMap)));
                        }
                        else if (storage.StorageType.Value == StorageType.Type.NMap)
                        {
                            var storageNMap = storage.StorageType.Value2 as StorageEntryTypeNMapV13 ?? throw new InvalidOperationException();
                            metadata.StorageClasses.Add(new StorageClass(StorageClass.StorageType.Storage, module.Name.Value, storage.Name.Value, StorageClass.WriteStorageNMap(storageNMap)));
                        }
                    }
                }
            }
        }

        public string WriteType(uint typeId)
        {
            var detailType = MetadataNetApi.NodeMetadata.Types[typeId]; //GetPalletType(typeId);

            if (detailType is NodeTypeVariant detailVariant)
                return WriteNodeVariant(detailVariant);
            else if (detailType is NodeTypeCompact detailCompact)
                return WriteNodeCompact(detailCompact);
            else if (detailType is NodeTypePrimitive detailPrimitive)
                return WriteNodePrimitive(detailPrimitive);
            else if (detailType is NodeTypeComposite detailComposite)
                return WriteNodeComposite(detailComposite);
            else if (detailType is NodeTypeSequence detailSequence)
                return WriteNodeSequence(detailSequence);
            else if (detailType is NodeTypeTuple detailTuple)
                return WriteNodeTuple(detailTuple);
            else if (detailType is NodeTypeArray detailArray)
                return WriteNodeArray(detailArray);
            else
                throw new NotSupportedException("Type not supported yet..."); // BitSequence ??
        }

        public string WriteNodeVariant(NodeTypeVariant nodeType)
        {
            string display = string.Join(":", nodeType.Path);
            if (nodeType.TypeParams != null && nodeType.TypeParams.Length > 0)
            {
                display = $"{display}<{string.Join(",", nodeType.TypeParams.Where(p => p.TypeId != null).Select(p => WriteType((uint)p.TypeId)))}>";
            }

            return display;
        }

        public string WriteNodeCompact(NodeTypeCompact nodeType)
        {
            return $"{nodeType.TypeDef}<{WriteType(nodeType.TypeId)}>";
        }

        public string WriteNodePrimitive(NodeTypePrimitive nodeType)
        {
            return nodeType.Primitive.ToString();
        }

        public string WriteNodeComposite(NodeTypeComposite nodeType)
        {
            var display = string.Join(":", nodeType.Path);
            if (nodeType.TypeParams != null && nodeType.TypeParams.Length > 0)
            {
                display = $"{display}<{string.Join(",", nodeType.TypeParams.Select(x => x.Name))}>";
            }
            return display;
        }

        public string WriteNodeSequence(NodeTypeSequence nodeType)
        {
            return $"Vec<{WriteType(nodeType.TypeId)}>";
        }

        public string WriteNodeTuple(NodeTypeTuple nodeType)
        {
            return $"({string.Join(",", nodeType.TypeIds.Select(WriteType))})";
        }

        public string WriteNodeArray(NodeTypeArray nodeType)
        {
            return $"{WriteType(nodeType.TypeId)}[{nodeType.Length}]";
        }
    }
}
