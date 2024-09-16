using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.V10;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V13;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.V9;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Text.RegularExpressions;

namespace Substrate.NET.Metadata.Conversion.Internal
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

        public static string WriteStoragePlain(Str plain) => WriteStoragePlain(plain.Value);
        public static string WriteStoragePlain(string plain)
        {
            return ConversionBuilder.HarmonizeTypeName(plain.ToString());
        }

        public static string WriteStorageMap(string hasher, string key, string value, bool? linked)
        {
            return $"[Hasher = {ConversionBuilder.HarmonizeTypeName(hasher)} / Key = {ConversionBuilder.HarmonizeTypeName(key)} / Value = {ConversionBuilder.HarmonizeTypeName(value)}]";
        }

        public static string WriteStorageDoubleMap(string key1, string key1Hasher, string key2, string key2Hasher, string value)
        {
            return $"Key1 = {ConversionBuilder.HarmonizeTypeName(key1)} / Key1Hasher = {ConversionBuilder.HarmonizeTypeName(key1Hasher)} / Key2 = {ConversionBuilder.HarmonizeTypeName(key2)} / Key2Hasher = {ConversionBuilder.HarmonizeTypeName(key2Hasher)} / Value = {ConversionBuilder.HarmonizeTypeName(value)}";
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
                return ConversionBuilder.HarmonizeTypeName(typeDefPrimitive.Value.ToString());
            }
            else
            {
                if (p.Value[index].Ty.Path.Value.Count() == 0)
                {

                }
                else
                {
                    return ConversionBuilder.HarmonizeTypeName(p.Value[index].Ty.Path.Value.Last().Value);
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

        public enum StorageType
        {
            Call,
            Event,
            Storage,
            Constant,
            Error
        }
    }
}
