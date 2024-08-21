using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NetApi.Model.Types.Base;
using System.Linq;

namespace Substrate.NET.Metadata.V15
{
    public class RuntimeMetadataV15 : BaseType
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Types.Encode());
            result.AddRange(Modules.Encode());
            result.AddRange(Extrinsic.Encode());
            result.AddRange(TypeId.Encode());
            result.AddRange(Apis.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Types = new PortableRegistry();
            Types.Decode(byteArray, ref p);

            Modules = new BaseVec<ModuleMetadataV15>();
            Modules.Decode(byteArray, ref p);

            Extrinsic = new ExtrinsicMetadataV15();
            Extrinsic.Decode(byteArray, ref p);

            TypeId = new TType();
            TypeId.Decode(byteArray, ref p);

            Apis = new BaseVec<RuntimeApiMetadataV15>();
            Apis.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public PortableRegistry Types { get; private set; }

        public BaseVec<ModuleMetadataV15> Modules { get; private set; }

        public ExtrinsicMetadataV15 Extrinsic { get; private set; }

        public BaseVec<RuntimeApiMetadataV15> Apis { get; private set; }

        public TType TypeId { get; private set; }
    }
}
