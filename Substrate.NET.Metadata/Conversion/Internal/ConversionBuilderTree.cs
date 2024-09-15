using Substrate.NET.Metadata.Base;
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
        public static string[] GenericValueToIgnore { get; set; } = ["T"];

        public static string HarmonizeTypeName(string className)
        {
            return className.Replace("T::", "");
        }

        internal static NodeBuilderType Build(NodeBuilderType nodeBuilderType)
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
                //return primitive;
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

            return nodeBuilderType;
        }

        /// <summary>
        /// Extract parameters from a class name
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        internal static List<string>? ExtractParameters(string className)
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

        /// <summary>
        /// Extract an array from a class name
        /// For example : [u8; 4] will build an NodeBuilderTypeArray with type u8 with a 4 length
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal static NodeBuilderTypeArray? ExtractArray(NodeBuilderType node)
        {
            string pattern = @"\[(.*);\s*(\d+)\]";
            Match match = Regex.Match(node.Adapted, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

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

        internal static NodeBuilderTypeTuple? ExtractTuple(NodeBuilderType node)
        {
            string pattern = @"\((.*)\)$";
            Match match = Regex.Match(node.Adapted, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

            if (match.Success)
            {
                var nodeTuple = new NodeBuilderTypeTuple(node.Adapted, node.Raw, node.PalletContext);

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

        /// <summary>
        /// Convert a primitive string to a NodeBuilderTypePrimitive
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal static NodeBuilderTypePrimitive? ExtractPrimitive(NodeBuilderType node)
        {
            object? res = null;
            if (Enum.TryParse(typeof(TypeDefPrimitive), node.Adapted, true, out res))
            {
                return new NodeBuilderTypePrimitive(node.Adapted, node.Raw, (TypeDefPrimitive)res, node.PalletContext);
            }

            return null;
        }

        /// <summary>
        /// Convert a class name to a generic class
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="MetadataConversionException"></exception>
        internal static NodeBuilderType? ExtractGeneric(NodeBuilderType node)
        {
            string pattern = @"([a-zA-Z:]*|)<(.*)>$";
            Match match = Regex.Match(node.Adapted, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

            if (match.Success)
            {
                var genericParameters = match.Groups[2].Value;

                // A valid pattern should have the same number of '<' and '>' and '<' should be before '>'
                if (!HaveValidParametersPattern(genericParameters))
                {
                    return null;
                }

                var typeDef = GetTypeDefFromString(match.Groups[1].Value);

                var binding = SearchV14.HardBinding(match.Groups[1].Value, node.PalletContext);
                var hasBeenHardBinded = binding != match.Groups[1].Value;

                NodeBuilderType result = typeDef switch
                {
                    TypeDefEnum.Sequence => new NodeBuilderTypeSequence(node.Adapted, node.Raw, node.PalletContext),
                    TypeDefEnum.Variant when match.Groups[1].Value == "Option" => new NodeBuilderTypeOption(node.Adapted, node.Raw, node.PalletContext),
                    TypeDefEnum.Variant => new NodeBuilderTypeVariant(node.Adapted, node.Raw, node.PalletContext),
                    TypeDefEnum.Composite => new NodeBuilderTypeComposite(binding, node.Adapted, node.PalletContext),
                    _ => throw new MetadataConversionException($"TypeDef {typeDef} is not handled")
                };

                    //if (result is NodeBuilderTypeComposite c && c.Raw != c.Adapted)
                if (result is NodeBuilderTypeComposite c && hasBeenHardBinded)
                {
                    return Build(new NodeBuilderTypeUndefined(c.Adapted, c.Raw, c.PalletContext));
                }
                else
                {
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

        private static TypeDefEnum GetTypeDefFromString(string genericType)
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
