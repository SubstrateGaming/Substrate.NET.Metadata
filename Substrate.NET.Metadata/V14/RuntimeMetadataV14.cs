using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NetApi.Model.Types.Base;

namespace Substrate.NET.Metadata.V14
{
    public class RuntimeMetadataV14 : BaseType
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Lookup.Encode());
            result.AddRange(Modules.Encode());
            result.AddRange(Extrinsic.Encode());
            result.AddRange(TypeId.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Lookup = new PortableRegistry();
            Lookup.Decode(byteArray, ref p);

            Modules = new BaseVec<ModuleMetadataV14>();
            Modules.Decode(byteArray, ref p);

            Extrinsic = new ExtrinsicMetadataV14();
            Extrinsic.Decode(byteArray, ref p);

            TypeId = new TType();
            TypeId.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public PortableRegistry Lookup { get; internal set; }

        public BaseVec<ModuleMetadataV14> Modules { get; internal set; }

        public ExtrinsicMetadataV14 Extrinsic { get; internal set; }

        public TType TypeId { get; internal set; }
    }
}
