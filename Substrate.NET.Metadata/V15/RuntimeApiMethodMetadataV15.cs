using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.V15
{
    public class RuntimeApiMethodMetadataV15 : BaseType, IMetadataName
    {
        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            Inputs = new BaseVec<RuntimeApiMethodParamMetadataV15>();
            Inputs.Decode(byteArray, ref p);

            Output = new TType();
            Output.Decode(byteArray, ref p);

            Docs = new BaseVec<Str>();
            Docs.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(Inputs.Encode());
            result.AddRange(Output.Encode());
            result.AddRange(Docs.Encode());
            return result.ToArray();
        }

        public Str Name { get; private set; }
        public BaseVec<RuntimeApiMethodParamMetadataV15> Inputs { get; private set; }
        public TType Output { get; private set; }
        public BaseVec<Str> Docs { get; private set; }
    }
}
