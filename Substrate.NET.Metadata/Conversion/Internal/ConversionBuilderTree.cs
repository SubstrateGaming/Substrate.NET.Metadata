﻿using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NetApi.Model.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    public static class ConversionBuilderTree
    {
        public static string HarmonizeTypeName(string className)
        {
            return className.Replace("T::", "");
        }

        public static NodeBuilderType Build(NodeBuilderType nodeBuilderType)
        {
            if (ExtractTuple(nodeBuilderType) is NodeBuilderTypeTuple tuples &&
                nodeBuilderType is NodeBuilderTypeUndefined)
            {
                nodeBuilderType = tuples;
            }

            var extractGeneric = ExtractGeneric(nodeBuilderType);

            if (extractGeneric is not null && nodeBuilderType is NodeBuilderTypeUndefined)
            {
                nodeBuilderType = extractGeneric!;
            }

            if (ExtractArray(nodeBuilderType) is NodeBuilderTypeArray array && nodeBuilderType is NodeBuilderTypeUndefined)
            {
                nodeBuilderType = array;
            }

            if (ExtractPrimitive(nodeBuilderType) is NodeBuilderTypePrimitive primitive && nodeBuilderType is NodeBuilderTypeUndefined)
            {
                nodeBuilderType = primitive!;
                return primitive;
            }

            if (nodeBuilderType is NodeBuilderTypeUndefined)
            {
                var content = HarmonizeTypeName(nodeBuilderType.Adapted);
                var hardBinding = SearchV14.HardBinding(content, nodeBuilderType.PalletContext);

                //if(!hardBinding.Equals(nodeBuilderType.Raw, StringComparison.CurrentCultureIgnoreCase))
                if (!string.IsNullOrEmpty(hardBinding) && hardBinding != content)
                {
                    nodeBuilderType = Build(new NodeBuilderTypeUndefined(hardBinding, nodeBuilderType.Raw, nodeBuilderType.PalletContext));
                }
                else
                {
                    nodeBuilderType = new NodeBuilderTypeComposite(
                        SearchV14.HardBinding(content, nodeBuilderType.PalletContext), nodeBuilderType.Raw, nodeBuilderType.PalletContext);
                }

            }

            if (nodeBuilderType.Children.Count > 0)
            {
                for (int i = 0; i < nodeBuilderType.Children.Count; i++)
                {
                    nodeBuilderType.Children[i] = Build(nodeBuilderType.Children[i]);
                }
            }

            //return new NodeBuilderTypeComposite(className);
            return nodeBuilderType;



            //if (ExtractRustGeneric(className) is string param)
            //{
            //    res.Add(param);
            //    return ExtractDeeper(res);
            //}

            //res.Add(HarmonizeTypeName(className));
            //return res;
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

            if (splitted.Any(x => x.Contains("("))) return null;

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

        //public static List<NodeBuilderTypeUndefined>? ExtractParameters(string className)
        //{
        //    List<NodeBuilderTypeUndefined> result = new();

        //    var splitted = className.Split(new[] { "," }, StringSplitOptions.None).Select(x => x.Trim()).ToList();
        //    result.AddRange(splitted.Select(x => new NodeBuilderTypeUndefined(x)));

        //    if (splitted.Count > 1)
        //    {
        //        List<int> indexBracketOpen = new();
        //        List<int> indexBracketClose = new();
        //        int lastIndexRemoved = 0;

        //        for (int i = 0; i < splitted.Count; i++)
        //        {
        //            var diff = splitted[i].Count(i => i == '<') - splitted[i].Count(i => i == '>');

        //            if (diff > 0)
        //            {
        //                Enumerable.Range(0, diff).ToList().ForEach(x => indexBracketOpen.Add(i));
        //            }

        //            if (diff < 0)
        //            {
        //                Enumerable.Range(0, Math.Abs(diff)).ToList().ForEach(x => indexBracketClose.Add(i));

        //                if (indexBracketClose.Count == indexBracketOpen.Count)
        //                {
        //                    var start = indexBracketOpen.First();
        //                    var end = indexBracketClose.Last();

        //                    var sub = string.Join(", ", splitted.GetRange(start, end - start + 1));

        //                    //result.RemoveRange(start - lastIndexRemoved, end - start + 1 - lastIndexRemoved);
        //                    Enumerable.Range(start, end - start + 1).ToList().ForEach(x => result.Remove(new NodeBuilderTypeUndefined(splitted[x])));
        //                    result.Insert(start - lastIndexRemoved, new NodeBuilderTypeUndefined(sub));

        //                    lastIndexRemoved = end;
        //                    indexBracketOpen.Clear();
        //                    indexBracketClose.Clear();
        //                }
        //            }
        //        }

        //        // At the end, let's check if something has changed
        //        return result.First() == new NodeBuilderTypeUndefined(className) ? null : result;
        //    }

        //    return null;
        //}

        private static NodeBuilderTypeArray? ExtractArray(NodeBuilderType node)
        {
            string pattern = @"\[(.*);\s*(\d+)\]";
            Match match = Regex.Match(node.Adapted, pattern);

            if (match.Success)
            {
                //match.Groups[1].Value -> array size
                var array = new NodeBuilderTypeArray(
                    node.Adapted,
                    node.Raw,
                    int.Parse(match.Groups[2].Value),
                    node.PalletContext);

                array.Children.Add(new NodeBuilderTypeUndefined(match.Groups[1].Value, node.PalletContext));

                return array;
            }

            return null;
        }

        private static NodeBuilderTypeTuple? ExtractTuple(NodeBuilderType node)
        {
            string pattern = @"\((.*)\)$";
            Match match = Regex.Match(node.Adapted, pattern);

            if (match.Success)
            {
                var nodeTuple = new NodeBuilderTypeTuple(node.Adapted, node.PalletContext);

                if (ExtractParameters(match.Groups[1].Value) is List<string> parameters)
                {
                    foreach (var param in parameters)
                    {
                        nodeTuple.Children.Add(new NodeBuilderTypeUndefined(param, node.PalletContext));
                    }
                }

                return nodeTuple;
            }

            return null;
        }

        //private static List<string> ExtractDeeper(NodeBuilderType nodeBuilderType, string content)
        //{
        //    //if (res.Count > 1)
        //    //{
        //    for (int i = 0; i < res.Count; i++)
        //    {
        //        var r = Build(res[i]);
        //        if (r.Count > 1)
        //        {
        //            res.Remove(res[i]);
        //            res.AddRange(r);
        //            i = -1;
        //        }
        //        else
        //        {
        //            res[i] = r[0];
        //        }
        //    }
        //    //}

        //    return res;
        //}

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

        public static NodeBuilderTypePrimitive? ExtractPrimitive(NodeBuilderType node)
        {
            object? res = null;
            if (Enum.TryParse(typeof(TypeDefPrimitive), node.Adapted, true, out res))
            {
                return new NodeBuilderTypePrimitive(node.Adapted, (TypeDefPrimitive)res, node.PalletContext);
            }

            return null;
        }

        public static NodeBuilderType? ExtractGeneric(NodeBuilderType node)
        {
            string pattern = @"([a-zA-Z:]*|)<(.*)>$";
            Match match = Regex.Match(node.Adapted, pattern);

            if (match.Success)
            {
                var genericParameters = match.Groups[2].Value;

                // A valid pattern should have the same number of '<' and '>' and '<' should be before '>'
                if (!HaveValidParametersPattern(genericParameters))
                {
                    return null;
                }

                var typeDef = GetTypeDefFromString(match.Groups[1].Value);

                NodeBuilderType result = typeDef switch
                {
                    TypeDefEnum.Sequence => new NodeBuilderTypeSequence(node.Adapted, node.PalletContext),
                    TypeDefEnum.Variant when match.Groups[1].Value == "Option" => new NodeBuilderTypeOption(node.Adapted, node.Raw, node.PalletContext),
                    TypeDefEnum.Variant => new NodeBuilderTypeVariant(node.Adapted, node.PalletContext),
                    TypeDefEnum.Composite => new NodeBuilderTypeComposite(
                        SearchV14.HardBinding(match.Groups[1].Value, node.PalletContext),
                        node.Adapted, node.PalletContext),
                    _ => throw new MetadataConversionException($"TypeDef {typeDef} is not handled")
                };

                if (!GenericValueToIgnore.Contains(genericParameters))
                {
                    if (ExtractParameters(genericParameters) is List<string> parameters)
                    {
                        foreach (var param in parameters)
                        {
                            result.Children.Add(new NodeBuilderTypeUndefined(param, node.PalletContext));
                        }
                    }
                    else
                    {
                        result.Children.Add(new NodeBuilderTypeUndefined(genericParameters, node.PalletContext));
                    }
                }

                return result;
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
                "Option" => TypeDefEnum.Variant,
                _ => TypeDefEnum.Composite
            };
        }
    }
}
