﻿using Substrate.NET.Metadata;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.V11;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V13
{
    public class StorageEntryMetadataV13 : BaseType, IMetadataName
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

            StorageType = new BaseEnumExt<StorageType.Type, Str, StorageEntryTypeMapV11, StorageEntryTypeDoubleMapV11, StorageEntryTypeNMapV13>();
            StorageType.Decode(byteArray, ref p);

            StorageDefault = new ByteGetter();
            StorageDefault.Decode(byteArray, ref p);

            Documentation = new BaseVec<Str>();
            Documentation.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; private set; }
        public BaseEnum<StorageType.ModifierV9> StorageModifier { get; private set; }
        public BaseEnumExt<StorageType.Type, Str, StorageEntryTypeMapV11, StorageEntryTypeDoubleMapV11, StorageEntryTypeNMapV13> StorageType { get; private set; }
        public ByteGetter StorageDefault { get; private set; }
        public BaseVec<Str> Documentation { get; private set; }
    }
}