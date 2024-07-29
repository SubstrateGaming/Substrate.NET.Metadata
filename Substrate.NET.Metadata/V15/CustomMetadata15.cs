using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.V15
{
    public class CustomMetadata15 : BaseType
    {
        public BaseVec<BaseTuple<Str, CustomValueMetadata15>> Map { get; private set; } = default!;

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            Map = new BaseVec<BaseTuple<Str, CustomValueMetadata15>>();

            Map.Decode(byteArray, ref p);
            var bytesLength = p - start;
            TypeSize = bytesLength;
            Bytes = new byte[bytesLength];
            Array.Copy(byteArray, start, Bytes, 0, bytesLength);
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Map.Encode());
            return result.ToArray();
        }
    }

    public class CustomValueMetadata15 : BaseType
    {
        public TType CustomType { get; private set; } = default!;
        public BaseVec<U8> Value { get; private set; } = default!;

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(CustomType.Encode());
            result.AddRange(Value.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            CustomType = new TType();
            CustomType.Decode(byteArray, ref p);

            Value = new BaseVec<U8>();
            Value.Decode(byteArray, ref p);
        }
    }
}
