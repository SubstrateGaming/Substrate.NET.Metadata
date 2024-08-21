using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using static Substrate.NET.Metadata.StorageType;

namespace Substrate.NET.Metadata.Base
{
    /// <summary>
    /// Represent a "Map" storage with generic Hasher
    /// </summary>
    /// <typeparam name="THasher"></typeparam>
    public abstract class StorageEntryTypeMap<THasher> : BaseType
        where THasher : Enum
    {
        public BaseEnum<THasher> Hasher { get; private set; } = default!;

        public Str Key { get; private set; } = default!;

        public Str Value { get; private set; } = default!;
        public Bool Linked { get; private set; } = default;

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Hasher.Encode());
            result.AddRange(Key.Encode());
            result.AddRange(Value.Encode());
            result.AddRange(Linked.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            int num = p;

            Hasher = new BaseEnum<THasher>();
            Hasher.Decode(byteArray, ref p);

            Key = new Str();
            Key.Decode(byteArray, ref p);

            Value = new Str();
            Value.Decode(byteArray, ref p);

            Linked = new Bool();
            Linked.Decode(byteArray, ref p);

            TypeSize = p - num;
        }

        public StorageEntryTypeMapV14 ToStorageEntryTypeMapV14(ConversionBuilder conversionBuilder)
        {
            var result = new StorageEntryTypeMapV14();

            result.Hashers = new BaseVec<BaseEnum<Hasher>>(
            [
                new BaseEnum<Hasher>((Hasher)Enum.Parse(typeof(Hasher), Hasher.Value.ToString()))
            ]);

            result.Key = TType.From(conversionBuilder.BuildLookup(Key.Value).Value);
            result.Value = TType.From(conversionBuilder.BuildLookup(Value.Value).Value);

            return result;
        }
    }
}
