using Microsoft.VisualBasic;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Substrate.NET.Metadata.Conversion.Internal.SearchV14;
using TypeDefExt = Substrate.NetApi.Model.Types.Base.BaseEnumExt<
    Substrate.NET.Metadata.Base.TypeDefEnum,
    Substrate.NET.Metadata.Base.TypeDefComposite,
    Substrate.NET.Metadata.Base.TypeDefVariant,
    Substrate.NET.Metadata.Base.TypeDefSequence,
    Substrate.NET.Metadata.Base.TypeDefArray,
    Substrate.NET.Metadata.Base.TypeDefTuple,
    Substrate.NetApi.Model.Types.Base.BaseEnum<Substrate.NET.Metadata.Base.TypeDefPrimitive>,
    Substrate.NET.Metadata.Base.TypeDefCompact,
    Substrate.NET.Metadata.Base.TypeDefBitSequence,
    Substrate.NetApi.Model.Types.Base.BaseVoid>;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    public class ConversionElementState
    {
        public ConversionElementState(string className, NodeBuilderType nodeBuilderType)
        {
            ClassName = className;
            NodeBuilderType = nodeBuilderType;
        }

        public string ClassName { get; set; }
        public NodeBuilderType NodeBuilderType { get; set; }
        public uint? IndexFoundInV14 { get; set; }
        public uint? IndexCreated { get; set; }

        public bool IsSuccessfullyMapped
        {
            get
            {
                return IndexFoundInV14 != null || IndexCreated != null;
            }
        }

        public override string ToString()
        {
            return $"{ClassName} (=> {NodeBuilderType.Adapted}) | V14 = {IndexFoundInV14} | CustomIndex = {IndexCreated} | Mapped = {IsSuccessfullyMapped}";
        }
    }

    public class ConversionBuilder
    {
        public List<PortableType> PortableTypes { get; init; }
        public List<ConversionElementState> ElementsState { get; init; }
        public string CurrentPallet { get; set; } = string.Empty;
        public uint? UnknowIndex { get; set; } = null;
        public const int START_INDEX = 1_000;

        public ConversionBuilder(List<PortableType> portableTypes)
        {
            PortableTypes = portableTypes;
            ElementsState = new List<ConversionElementState>();
        }

        /// <summary>
        /// Conversion that are not handle by the <see cref="ConversionBuilder"/> because it is not necessary or too complex
        /// </summary>
        public static List<string> UnhandleConversion = new List<string>()
        {
            "Vec<Option<Scheduled<<T as Trait>::Call, T::BlockNumber>>>", // v11 => Scheduler => v1 => Storage => Agenda
            "Vec<Option<Scheduled<<T as Trait>::Call, T::BlockNumber, T::PalletsOrigin, T::AccountId>>>", // v12 => Scheduler => v25 => Storage => Agenda
            "Vec<Option<Scheduled<<T as Config>::Call, T::BlockNumber, T::PalletsOrigin, T::AccountId>>>", // v12 => Scheduler => v27 => Storage => Agenda
            "ElectionResult<T::AccountId, BalanceOf<T>>", // V11 => Staking => v1 => Storage => QueuedElected
            "PhragmenScore", // V11 => Staking => v1 => Storage => QueuedScore
            "ElectionStatus<T::BlockNumber>", // V11 => Staking => v1 => Storage => EraElectionStatus
            "Vec<DeferredOffenceOf<T>>", // V11 => Offences => v1 => Storage => DeferredOffences
            "<T as Trait<I>>::Proposal", // V11 => Council => v1 => Storage => ProposalOf
            //"(BalanceOf<T>, Vec<T::AccountId>)", // V11 => ElectionsPhragmen => v1 => Storage => Voting ==> Easy to create
            "sp_std::marker::PhantomData<(AccountId, Event)>", // V11 => TechnicalMembership => v1 => Events => Dummy
            "Vec<UpwardMessage>", // V11 => Parachains => v1 => Storage => RelayDispatchQueue
            "IncludedBlocks<T>", // V11 => Attestations => v1 => Storage => RecentParaBlocks
            "BlockAttestations<T>", // V11 => Attestations => v1 => Storage => ParaBlockAttestations
            "WinningData<T>", // V11 => Slots => v1 => Storage => Winning
            "(LeasePeriodOf<T>, IncomingParachain<T::AccountId, T::Hash>)", // V11 => Slots => v1 => Storage => Onboarding
            "ElectionScore", // V12 => Staking => v25 => Storage => QueuedScore
            "(OpaqueCall, T::AccountId, BalanceOf<T>)", // V12 => Multisig => v25 => Storage => Calls
            "<T as Config<I>>::Proposal", // V12 => Council => v27 => Storage => ProposalOf
            "ProxyState<T::AccountId>", // V11 => Democracy => v1 => Storage => Proxy
            "NewBidder<AccountId>", // V11 => Slots => v1 => Events => WonDeploy
            "Bidder<T::AccountId>", // V11 => Slots => v1 => Storage => ReservedAmounts
        };

        public U32 GetNewIndex()
        {
            if (!PortableTypes.Any()) return new U32(START_INDEX);
            return new U32(Math.Max(START_INDEX, PortableTypes.Max(x => x.Id.Value) + 1));
        }

        public U32 BuildPortableTypes(string className)
        {
            className = className.Replace("\r", string.Empty).Replace("\n", string.Empty);

            if (UnhandleConversion.Contains(className)) 
                return new U32(UnknowIndex!.Value); // Ok this is bad

            var founded = GetPortableType(className);

            if(founded.searchResult == SearchResult.Founded)
                ElementsState[^1].IndexFoundInV14 = founded.portableType.Id.Value;
            else
                ElementsState[^1].IndexCreated = founded.portableType.Id.Value;

            return founded.portableType.Id;

            //if (founded != null)
            //{
            //    ElementsState[^1].IndexFoundInV14 = founded.Id.Value;
            //    return founded.Id;
            //}

            //var portableType = CreateNode(className, GetNewIndex());

            //ElementsState[^1].IndexCreated = portableType.Id.Value;
            //return portableType.Id;
            
        }

        /// <summary>
        /// Try to get the type from V14
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public (PortableType portableType, SearchResult searchResult) GetPortableType(string className)
        {
            var nodeRaw = new NodeBuilderTypeUndefined(className, CurrentPallet);

            // Node like events has do not have to be seached in V14
            if (nodeRaw.IndexHardBinding != null)
            {
                ElementsState.Add(new ConversionElementState(className, nodeRaw));
                return (LoopFromV14((int)nodeRaw.IndexHardBinding!), SearchResult.Founded);
            }

            var nodeType = ConversionBuilderTree.Build(nodeRaw);
            var index = SearchV14.SearchIndexByNode(nodeType, this);

            ElementsState.Add(new ConversionElementState(className, nodeType));

            return (LoopFromV14(index.index), index.searchResult);
        }

        public PortableType LoopFromV14(int index)
        {
            var alreadyInserted = PortableTypes.SingleOrDefault(x => x.Id.Value == index);
            if (alreadyInserted != null) return alreadyInserted;

            var portableType = SearchV14.FindTypeByIndex(index);

            PortableTypes.Add(portableType);
            switch(portableType.Ty.TypeDef.Value)
            {
                case TypeDefEnum.Composite:
                    var composite = (TypeDefComposite)portableType.Ty.TypeDef.Value2;

                    foreach (var field in composite.Fields.Value)
                    {
                        _ = LoopFromV14(field.FieldTy.Value);
                    }
                    break;
                case TypeDefEnum.Array:
                    var array = (TypeDefArray)portableType.Ty.TypeDef.Value2;
                    _ = LoopFromV14(array.ElemType.Value);
                    break;
                case TypeDefEnum.Variant:
                    var variants = (TypeDefVariant)portableType.Ty.TypeDef.Value2;
                    foreach(var variant in variants.TypeParam.Value)
                    {
                        foreach (var field in variant.VariantFields.Value) 
                        {
                            _ = LoopFromV14(field.FieldTy.Value);
                        }
                    }
                    break;
                case TypeDefEnum.Sequence:
                    var sequence = (TypeDefSequence)portableType.Ty.TypeDef.Value2;
                    _ = LoopFromV14(sequence.ElemType.Value);
                    break;
                case TypeDefEnum.Tuple:
                    var tuple = (TypeDefTuple)portableType.Ty.TypeDef.Value2;
                    foreach(var field in tuple.Fields.Value)
                    {
                        _ = LoopFromV14(field.Value);
                    }
                    break;
                case TypeDefEnum.Primitive: break;
                case TypeDefEnum.Compact:
                    var compact = (TypeDefCompact)portableType.Ty.TypeDef.Value2;
                    _ = LoopFromV14(compact.ElemType.Value);
                    break;
                case TypeDefEnum.BitSequence:
                    var bitSequence = (TypeDefBitSequence)portableType.Ty.TypeDef.Value2;
                    _ = LoopFromV14(bitSequence.BitOrderType.Value);
                    _ = LoopFromV14(bitSequence.BitStoreType.Value);
                    break;
                default:
                    throw new MetadataConversionException($"Unsupported {portableType.Ty.TypeDef.Value} TypeDef");
            }

            return portableType;
        }

        /// <summary>
        /// Create the node from scratch
        /// </summary>
        /// <param name="className"></param>
        /// <param name="currentMaxIndex"></param>
        /// <returns></returns>
        /// <exception cref="MetadataConversionException"></exception>
        public static PortableType CreateNode(string className, U32 currentMaxIndex)
        {

            var tpf = new TypePortableForm();

            //if (ExtractPrimitive(className) is TypeDefPrimitive prim)
            //{
            //    tpf.Path = new Base.Portable.Path();
            //    tpf.TypeParams = new BaseVec<TypeParameter>();
            //    tpf.TypeDef = new TypeDefExt();
            //    tpf.TypeDef.Create(TypeDefEnum.Primitive, new BaseEnum<TypeDefPrimitive>(prim));

            //    return new PortableType(currentMaxIndex, tpf);
            //}

            //var extractGeneric = ExtractGeneric(className);
            //if (extractGeneric is not null)
            //{
            //    var toFind = CustomMapping(HarmonizeTypeName(extractGeneric.Value.genericParameters[0]));
            //    var index = SearchV14.FindIndexByClass(toFind);
            //}

            throw new MetadataConversionException($"Unable to convert {className} to NodeType");
        }

        public static string HarmonizeTypeName(string className)
        {
            return className.Replace("T::", "").Replace("<T>", "");
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

        public static List<string>? ExtractParameters(string className)
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

        public static TypeDefPrimitive? ExtractPrimitive(string className)
        {
            object? res = null;
            if (Enum.TryParse(typeof(TypeDefPrimitive), className, true, out res))
            {
                return (TypeDefPrimitive)res;
            }

            return null;
        }

        public static (TypeDefEnum typeDefEnum, List<string> genericParameters)? ExtractGeneric(string className)
        {
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

        public static string CustomMapping(string className)
        {
            return className switch
            {
                "AccountId" => "AccountId32",
                "TaskAddress<BlockNumber>" => "TaskAddress<T::BlockNumber>",
                "Vec<IdentificationTuple>" => "Vec<IdentificationTuple<T>>",
                "Vec<AccountId>" => "Vec<T::AccountId>",
                "Vec<(AccountId, Balance)>" => "Vec<(<T as frame_system::Config>::AccountId, BalanceOf<T>)>",
                //"sp_std::marker::PhantomData<(AccountId, Event)>" => "BaseVoid", // Pas sur de moi
                "Timepoint<BlockNumber>" => "Timepoint<T::BlockNumber>",
                "limits::BlockWeights" => "BlockWeights",
                "limits::BlockLength" => "BlockLength",
                "TransactionPriority" => "u64",
                "& 'static[u8]" => "vec<u8>",
                "&[u8]" => "vec<u8>",
                "Moment" => "u64",
                "ModuleId" => "PalletId",
                "LeasePeriod" => "LeasePeriodOf<T>",
                "weights::ExtrinsicsWeight" => "PerDispatchClass",
                _ => className
            };
        }



        /// <summary>
        /// Create a pallet runtime events
        /// </summary>
        /// <param name="palletName"></param>
        /// <param name="variants"></param>
        /// <returns></returns>
        public U32 AddEventRuntimeLookup(string palletName, Variant[] variants)
            => AddRuntimeLookup("Event", palletName, variants);

        public U32 AddErrorRuntimeLookup(string palletName, Variant[] variants)
            => AddRuntimeLookup("Error", palletName, variants);

        /// <summary>
        /// Create a pallet runtime events / calls / errors / constants
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="palletName"></param>
        /// <param name="variants"></param>
        /// <returns></returns>
        private U32 AddRuntimeLookup(string objType, string palletName, Variant[] variants)
        {
            var path = new Base.Portable.Path();
            path.Create(new List<string>() { palletName, "pallet", objType }.Select(x => new Str(x)).ToArray());

            var typeParams = new BaseVec<TypeParameter>([ 
                new TypeParameter(new Str("T"), new BaseOpt<TType>())
            ]);

            var variant = new TypeDefVariant();
            variant.TypeParam = new BaseVec<Variant>(variants);
            var typeDef = new TypeDefExt();
            typeDef.Create(TypeDefEnum.Variant, variant);

            var docs = new BaseVec<Str>([new Str($"Event for the {palletName} pallet")]);

            var index = GetNewIndex();
            PortableTypes.Add(new PortableType(index, new TypePortableForm(
                path,
                typeParams,
                typeDef,
                docs
            )));

            return index;
        }

        public PalletEventMetadataV14 ConvertToEventV14(string eventName, string eventType)
        {
            var res = new PalletEventMetadataV14();

            res.ElemType = TType.From(BuildPortableTypes(eventType).Value);

            return res;
        }

        public uint GetStorageEventIndex() => SearchV14.HardIndexBinding("Vec<EventRecord<T::Event, T::Hash>>").Value;

        public void ClearEventBlockchainRuntimeEvent()
        {
            var portableType = LoopFromV14(SearchV14.PolkadotRuntimeEventIndex);
            var tdv = portableType.Ty.TypeDef.Value2 as TypeDefVariant;
            tdv.TypeParam = new BaseVec<Variant>(new Variant[0]);
        }
        public void AddPalletEventBlockchainRuntimeEvent(Variant variant)
        {
            var portableType = LoopFromV14(SearchV14.PolkadotRuntimeEventIndex);

            var tdv = portableType.Ty.TypeDef.Value2 as TypeDefVariant;

            var list = tdv.TypeParam.Value.ToList();
            list.Add(variant);

            tdv.TypeParam = new BaseVec<Variant>(list.ToArray());
        }

        #region Build node

        /// <summary>
        /// Composite class use for undefined mapping
        /// </summary>
        /// <returns></returns>
        public PortableType CreateUnknownNode()
        {
            var unknownNode = new TypeDefComposite();
            unknownNode.Fields = new BaseVec<Field>(new Field[0]);

            var pt = CreatePortableTypeFromNode(unknownNode);
            UnknowIndex = pt.Id.Value;

            return pt;
        }

        public PortableType CreatePortableTypeFromNode(BaseType node, List<string>? path = null, List<string>? docs = null)
        {
            
            var portableType = new PortableType();

            portableType.Id = GetNewIndex();
            portableType.Ty = new TypePortableForm();

            if(docs is not null)
                portableType.Ty.Docs = new BaseVec<Str>(docs.Select(x => new Str(x)).ToArray());
            else
                portableType.Ty.Docs = new BaseVec<Str>(new Str[0]);

            if(path is not null)
            {
                portableType.Ty.Path = new Base.Portable.Path();
                portableType.Ty.Path.Create(path.Select(x => new Str(x)).ToArray());
            }
            else
            {
                portableType.Ty.Path = new Base.Portable.Path();
                portableType.Ty.Path.Create(new Str[0]);
            }

            portableType.Ty.TypeParams = new BaseVec<TypeParameter>(new TypeParameter[0]);

            if (node is TypeDefTuple tdt)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Tuple, tdt);
            }

            if (node is TypeDefComposite tdc)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Composite, tdc);
            }

            if (node is TypeDefSequence tds)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Sequence, tds);
            }

            if (node is TypeDefArray tda)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Array, tda);
            }

            PortableTypes.Add(portableType);

            return portableType;
        }

        public TypeDefTuple BuildTuple(List<TType> types)
        {
            var node = new TypeDefTuple();

            node.Fields = new BaseVec<TType>(types.ToArray());

            return node;
        }

        public void CreateRuntime()
        {
            var emptyComposite = new TypeDefComposite();
            emptyComposite.Fields = new BaseVec<Field>(new Field[0]);

            CreatePortableTypeFromNode(node: emptyComposite, path:  new List<string>() { "conversion_runtime", "runtime" });
        }
        #endregion
    }
}
