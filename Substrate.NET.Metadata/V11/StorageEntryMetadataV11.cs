﻿using Substrate.NET.Metadata;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V11
{
    public class StorageEntryMetadataV11 : BaseType, IMetadataName
    {
        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            StorageModifier = new BaseEnum<StorageType.ModifierV9>();
            StorageModifier.Decode(byteArray, ref p);

            StorageType = new BaseEnumExt<StorageType.TypeV9, Str, StorageEntryTypeMapV11, StorageEntryTypeDoubleMapV11>();
            StorageType.Decode(byteArray, ref p);

            StorageDefault = new ByteGetter();
            StorageDefault.Decode(byteArray, ref p);

            Documentation = new BaseVec<Str>();
            Documentation.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; private set; }
        public BaseEnum<StorageType.ModifierV9> StorageModifier { get; private set; }
        public BaseEnumExt<StorageType.TypeV9, Str, StorageEntryTypeMapV11, StorageEntryTypeDoubleMapV11> StorageType { get; private set; }
        public ByteGetter StorageDefault { get; private set; }
        public BaseVec<Str> Documentation { get; private set; }
    }
}