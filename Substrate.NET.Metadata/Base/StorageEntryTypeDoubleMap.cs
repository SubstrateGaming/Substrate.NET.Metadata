using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Substrate.NET.Metadata.StorageType;

namespace Substrate.NET.Metadata.Base
{
    /// <summary>
    /// Represent a "Double Map" storage with generic Hasher
    /// </summary>
    /// <typeparam name="THasher"></typeparam>
    public class StorageEntryTypeDoubleMap<THasher> : BaseType
        where THasher : Enum
    {
        public BaseEnum<THasher> Key1Hasher { get; private set; }
        public BaseEnum<THasher> Key2Hasher { get; private set; }

        public Str Key1 { get; private set; }
        public Str Key2 { get; private set; }

        public Str Value { get; private set; }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Key1Hasher.Encode());
            result.AddRange(Key1.Encode());
            result.AddRange(Key2.Encode());
            result.AddRange(Value.Encode());
            result.AddRange(Key2Hasher.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            int num = p;

            Key1Hasher = new BaseEnum<THasher>();
            Key1Hasher.Decode(byteArray, ref p);

            Key1 = new Str();
            Key1.Decode(byteArray, ref p);

            Key2 = new Str();
            Key2.Decode(byteArray, ref p);

            Value = new Str();
            Value.Decode(byteArray, ref p);

            Key2Hasher = new BaseEnum<THasher>();
            Key2Hasher.Decode(byteArray, ref p);

            TypeSize = p - num;
        }

        public StorageEntryTypeMapV14 ToStorageEntryTypeMapV14(ConversionBuilder conversionBuilder)
        {
            var result = new StorageEntryTypeMapV14();

            result.Hashers = new BaseVec<BaseEnum<Hasher>>(
            [
                new BaseEnum<Hasher>((Hasher)Enum.Parse(typeof(THasher), Key1Hasher.Value.ToString())),
                new BaseEnum<Hasher>((Hasher)Enum.Parse(typeof(THasher), Key2Hasher.Value.ToString())),
            ]);

            result.Key = TType.From(conversionBuilder.BuildPortableTypes($"({Key1.Value}, {Key2.Value})").Value);
            result.Value = TType.From(conversionBuilder.BuildPortableTypes(Value.Value).Value);

            return result;
        }
    }
}
