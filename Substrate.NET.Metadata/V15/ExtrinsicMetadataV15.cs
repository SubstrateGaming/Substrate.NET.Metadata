using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.V15
{
    public class ExtrinsicMetadataV15 : BaseType
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Version.Encode());
            result.AddRange(AddressType.Encode());
            result.AddRange(CallType.Encode());
            result.AddRange(SignatureType.Encode());
            result.AddRange(ExtraType.Encode());
            result.AddRange(SignedExtensions.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Version = new U8();
            Version.Decode(byteArray, ref p);

            AddressType = new TType();
            AddressType.Decode(byteArray, ref p);

            CallType = new TType();
            CallType.Decode(byteArray, ref p);

            SignatureType = new TType();
            SignatureType.Decode(byteArray, ref p);

            ExtraType = new TType();
            ExtraType.Decode(byteArray, ref p);

            SignedExtensions = new BaseVec<SignedExtensionMetadataV14>();
            SignedExtensions.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public U8 Version { get; private set; } = default!;
        public TType AddressType { get; private set; } = default!;
        public TType CallType { get; private set; } = default!;
        public TType SignatureType { get; private set; } = default!;
        public TType ExtraType { get; private set; } = default!;
        public BaseVec<SignedExtensionMetadataV14> SignedExtensions { get; private set; } = default!;
    }
}
