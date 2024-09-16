using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata;
using System.Reflection;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V14;

namespace Substrate.NET.Metadata.V10
{
    public class StorageEntryMetadataV10 : BaseType, IMetadataName
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(StorageModifier.Encode());
            result.AddRange(StorageType.Encode());
            result.AddRange(StorageDefault.Encode());
            result.AddRange(Documentation.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            StorageModifier = new BaseEnum<StorageType.ModifierV9>();
            StorageModifier.Decode(byteArray, ref p);

            StorageType = new BaseEnumExt<StorageType.TypeV9, Str, StorageEntryTypeMapV10, StorageEntryTypeDoubleMapV10>();
            StorageType.Decode(byteArray, ref p);

            StorageDefault = new ByteGetter();
            StorageDefault.Decode(byteArray, ref p);

            Documentation = new BaseVec<Str>();
            Documentation.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; private set; } = default!;
        public BaseEnum<StorageType.ModifierV9> StorageModifier { get; private set; } = default!;
        public BaseEnumExt<StorageType.TypeV9, Str, StorageEntryTypeMapV10, StorageEntryTypeDoubleMapV10> StorageType { get; private set; } = default!;
        public ByteGetter StorageDefault { get; private set; } = default!;
        public BaseVec<Str> Documentation { get; private set; } = default!;

        internal StorageEntryMetadataV14 ToStorageEntryMetadataV14(ConversionBuilder conversionBuilder)
        {
            var mapStorageType = new BaseEnumExt<StorageType.Type, TType, StorageEntryTypeMapV14>();

            if (StorageType.Value == Metadata.StorageType.TypeV9.Plain)
            {
                var val = StorageType.Value2 as Str;
                var index = conversionBuilder.BuildPortableTypes(val!.Value);

                mapStorageType.Create(Metadata.StorageType.Type.Plain, TType.From(index.Value));
            }
            else if (StorageType.Value == Metadata.StorageType.TypeV9.Map)
            {
                var map = (StorageEntryTypeMapV11)StorageType.Value2;
                mapStorageType.Create(Metadata.StorageType.Type.Map, map.ToStorageEntryTypeMapV14(conversionBuilder));
            }
            else if (StorageType.Value == Metadata.StorageType.TypeV9.DoubleMap)
            {
                var map = (StorageEntryTypeDoubleMapV11)StorageType.Value2;
                mapStorageType.Create(Metadata.StorageType.Type.Map, map.ToStorageEntryTypeMapV14(conversionBuilder));

            }
            else
            {
                // NMap exists but should never happened
                throw new MetadataConversionException($"Error while converting StorageEntryMetadataV11. Received storage type {StorageType.Value} while should never happened");
            }

            var storageEntry = new StorageEntryMetadataV14(
                name: Name,
                storageModifier: StorageModifier,
                storageType: mapStorageType,
                storageDefault: StorageDefault,
                documentation: Documentation
            );

            return storageEntry;
        }
    }
}