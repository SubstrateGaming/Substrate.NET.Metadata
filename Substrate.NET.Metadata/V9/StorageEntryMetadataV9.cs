﻿using Substrate.NET.Metadata;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V9
{
    public class StorageEntryMetadataV9 : BaseType, IMetadataName
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(Modifier.Encode());
            result.AddRange(StorageType.Encode());
            result.AddRange(Default.Encode());
            result.AddRange(Documentation.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            Modifier = new BaseEnum<StorageType.ModifierV9>();
            Modifier.Decode(byteArray, ref p);

            StorageType = new BaseEnumExt<StorageType.TypeV9, Str, StorageEntryTypeMapV9, StorageEntryTypeDoubleMapV9>();
            StorageType.Decode(byteArray, ref p);

            Default = new ByteGetter();
            Default.Decode(byteArray, ref p);

            Documentation = new BaseVec<Str>();
            Documentation.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; private set; }
        public BaseEnum<StorageType.ModifierV9> Modifier { get; private set; }
        public BaseEnumExt<StorageType.TypeV9, Str, StorageEntryTypeMapV9, StorageEntryTypeDoubleMapV9> StorageType { get; private set; }
        public ByteGetter Default { get; private set; }
        public BaseVec<Str> Documentation { get; private set; }
    }
}
