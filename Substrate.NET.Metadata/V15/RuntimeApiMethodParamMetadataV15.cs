using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.V15
{
    public class RuntimeApiMethodParamMetadataV15 : BaseType, IMetadataName
    {
        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            TypeId = new TType();
            TypeId.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(TypeId.Encode());
            return result.ToArray();
        }

        public Str Name { get; private set; }
        public TType TypeId { get; private set; }
    }
}
