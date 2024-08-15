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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    public class ConversionBuilder
    {
        public List<PortableType> PortableTypes { get; init; }

        public ConversionBuilder(List<PortableType> portableTypes)
        {
            PortableTypes = portableTypes;
        }

        //private PortableType[] Push(this PortableType[] pt, PortableType p)
        //{
        //    var list = pt.ToList();
        //    list.Add(p);

        //    return list.ToArray();
        //}

        public U32 GetNewIndex()
        {
            var max = PortableTypes.Max(x => x.Id);

            if (max is null) return new U32(0);
            return new U32(max.Value + 1);
        }

        public U32 BuildLookup(string className)
        {
            var portableType = GetNodeFromV14(className) ?? CreateNode(className, GetNewIndex());

            return portableType.Id;
        }

        /// <summary>
        /// Try to get the type from V14
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public PortableType? GetNodeFromV14(string className)
        {
            var nodeType = ConversionBuilderTree.Build(new NodeBuilderTypeUndefined(className));
            var index = SearchV14.FindIndexByNode(nodeType);

            if (index == null) return null;

            return LoopFromV14(index.Value);
        }

        public PortableType LoopFromV14(int index)
        {
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

            if (ExtractPrimitive(className) is TypeDefPrimitive prim)
            {
                tpf.Path = new Base.Portable.Path();
                tpf.TypeParams = new BaseVec<TypeParameter>();
                tpf.TypeDef = new TypeDefExt();
                tpf.TypeDef.Create(TypeDefEnum.Primitive, new BaseEnum<TypeDefPrimitive>(prim));

                return new PortableType(currentMaxIndex, tpf);
            }

            var extractGeneric = ExtractGeneric(className);
            if (extractGeneric is not null)
            {
                var toFind = CustomMapping(HarmonizeTypeName(extractGeneric.Value.genericParameters[0]));
                var index = SearchV14.FindIndexByClass(toFind);
            }

            throw new MetadataConversionException($"Unable to convert {className} to NodeType");
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

        //public static int? TryHardBinding(string className)
        //{
        //    return className switch
        //    {
        //        "u8" => 2,
        //        "u16" => 73,
        //        "u32" => 4,
        //        "u64" => 8,
        //        "u128" => 6,
        //        "bool" => 58,
        //        "Str" => 108,
        //        "Vec<WeightToFeeCoefficient<BalanceOf<T>>>" => 386,
        //        "LockIdentifier" => 125,
        //        "[u8;8]" => 125,
        //        _ => null
        //    };
        //}

        public U32 AddEventRuntimeLookup(string palletName, Variant[] variants)
        {
            var path = new Base.Portable.Path();
            path.Create(new List<string>() { palletName, "pallet", "Event" }.Select(x => new Str(x)).ToArray());

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

            res.ElemType = TType.From(BuildLookup(eventType).Value);

            return res;
        }

        public PalletConstantMetadataV14 ConvertToConstantV14(string constantName, string constantType, ByteGetter defaultValue)
        {
            var res = new PalletConstantMetadataV14();
            res.Name = new Str(constantName);
            res.ConstantValue = defaultValue;
            res.ConstantType = TType.From(BuildLookup(constantType).Value);

            return res;
        }

        public PalletErrorMetadataV14 ConvertToErrorV14(string errorName)
        {
            var res = new PalletErrorMetadataV14();
            res.ElemType = TType.From(BuildLookup(errorName).Value);

            return res;
        }
    }
}
