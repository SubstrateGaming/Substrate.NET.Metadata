using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Base
{
    /// <summary>
    /// Represent a "NMap" storage with generic Hasher
    /// </summary>
    /// <typeparam name="THasher"></typeparam>
    [ExcludeFromCodeCoverage] // Should never happened. Here just for consistency
    public class StorageEntryTypeNMap<THasher> : BaseType
        where THasher : Enum
    {
        public BaseVec<BaseEnum<THasher>> Hashers { get; private set; } = default!;

        public BaseVec<Str> KeyVec { get; private set; } = default!;

        public Str Value { get; private set; } = default!;

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Hashers.Encode());
            result.AddRange(KeyVec.Encode());
            result.AddRange(Value.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            int num = p;

            Hashers = new BaseVec<BaseEnum<THasher>>();
            Hashers.Decode(byteArray, ref p);

            KeyVec = new BaseVec<Str>();
            KeyVec.Decode(byteArray, ref p);

            Value = new Str();
            Value.Decode(byteArray, ref p);

            TypeSize = p - num;
        }
    }
}
