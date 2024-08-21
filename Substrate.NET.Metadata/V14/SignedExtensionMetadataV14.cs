using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.V14
{
    public class SignedExtensionMetadataV14 : BaseType
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(SignedIdentifier.Encode());
            result.AddRange(SignedExtType.Encode());
            result.AddRange(AddSignedExtType.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            SignedIdentifier = new Str();
            SignedIdentifier.Decode(byteArray, ref p);

            SignedExtType = new TType();
            SignedExtType.Decode(byteArray, ref p);

            AddSignedExtType = new TType();
            AddSignedExtType.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str SignedIdentifier { get; private set; }
        public TType SignedExtType { get; private set; }
        public TType AddSignedExtType { get; private set; }
    }
}
