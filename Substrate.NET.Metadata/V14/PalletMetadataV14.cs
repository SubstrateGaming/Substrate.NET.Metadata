using Substrate.NET.Metadata;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Collections.Generic;
using static Substrate.NET.Metadata.StorageType;

namespace Substrate.NET.Metadata.V14
{
    public class PalletStorageMetadataV14 : BaseType
    {
        public PalletStorageMetadataV14() { }

        public PalletStorageMetadataV14(string prefix, IEnumerable<StorageEntryMetadataV14> entries)
        {
            Prefix = new Str(prefix);
            Entries = new BaseVec<StorageEntryMetadataV14>(entries.ToArray());
        }

        public Str Prefix { get; private set; } = default!;

        public BaseVec<StorageEntryMetadataV14> Entries { get; private set; } = default!;

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            int num = p;
            Prefix = new Str();
            Prefix.Decode(byteArray, ref p);
            Entries = new BaseVec<StorageEntryMetadataV14>();
            Entries.Decode(byteArray, ref p);
            TypeSize = p - num;
        }
    }

    public class StorageEntryMetadataV14 : BaseType, IMetadataName
    {
        public StorageEntryMetadataV14() { }

        public StorageEntryMetadataV14(Str name, BaseEnum<ModifierV9> storageModifier, BaseEnumExt<StorageType.Type, TType, StorageEntryTypeMapV14> storageType, ByteGetter storageDefault, BaseVec<Str> documentation)
        {
            Name = name;
            StorageModifier = storageModifier;
            StorageType = storageType;
            StorageDefault = storageDefault;
            Documentation = documentation;
        }

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            StorageModifier = new BaseEnum<ModifierV9>();
            StorageModifier.Decode(byteArray, ref p);

            StorageType = new BaseEnumExt<StorageType.Type, TType, StorageEntryTypeMapV14>();
            StorageType.Decode(byteArray, ref p);

            StorageDefault = new ByteGetter();
            StorageDefault.Decode(byteArray, ref p);

            Documentation = new BaseVec<Str>();
            Documentation.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; private set; } = default!;
        public BaseEnum<ModifierV9> StorageModifier { get; private set; } = default!;
        public BaseEnumExt<StorageType.Type, TType, StorageEntryTypeMapV14> StorageType { get; private set; } = default!;
        public ByteGetter StorageDefault { get; private set; } = default!;
        public BaseVec<Str> Documentation { get; private set; } = default!;
    }

    public class StorageEntryTypeMapV14 : BaseType
    {
        public StorageEntryTypeMapV14() { }

        public StorageEntryTypeMapV14(BaseVec<BaseEnum<Hasher>> hashers, TType key, TType value)
        {
            Hashers = hashers;
            Key = key;
            Value = value;
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Hashers.Encode());
            result.AddRange(Key.Encode());
            result.AddRange(Value.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Hashers = new BaseVec<BaseEnum<Hasher>>();
            Hashers.Decode(byteArray, ref p);

            Key = new TType();
            Key.Decode(byteArray, ref p);

            Value = new TType();
            Value.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public BaseVec<BaseEnum<Hasher>> Hashers { get; internal set; } = default!;
        public TType Key { get; internal set; } = default!;
        public TType Value { get; internal set; } = default!;
    }

    public class PalletCallMetadataV14 : BaseType, IMetadataName, IMetadataType
    {
        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            ElemType = new TType();
            ElemType.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public TType ElemType { get; private set; } = default!;
        public Str Name => new Str(ElemType.ToString());
    }

    public class PalletEventMetadataV14 : BaseType, IMetadataName, IMetadataType
    {
        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            ElemType = new TType();
            ElemType.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public TType ElemType { get; internal set; } = default!;

        public Str Name => new Str(ElemType.ToString());
    }

    public class PalletConstantMetadataV14 : BaseType, IMetadataName
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

            ConstantType = new TType();
            ConstantType.Decode(byteArray, ref p);

            ConstantValue = new ByteGetter();
            ConstantValue.Decode(byteArray, ref p);

            Documentation = new BaseVec<Str>();
            Documentation.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; internal set; } = default!;
        public TType ConstantType { get; internal set; } = default!;
        public ByteGetter ConstantValue { get; internal set; } = default!;
        public BaseVec<Str> Documentation { get; internal set; } = default!;
    }

    public class PalletErrorMetadataV14 : BaseType, IMetadataName, IMetadataType
    {
        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            ElemType = new TType();
            ElemType.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public TType ElemType { get; internal set; } = default!;
        public Str Name => new Str(ElemType.ToString());
    }
}
