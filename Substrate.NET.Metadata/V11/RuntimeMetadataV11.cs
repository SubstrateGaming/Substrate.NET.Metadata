using Substrate.NetApi.Model.Types.Base;

namespace Substrate.NET.Metadata.V11
{
    public class RuntimeMetadataV11 : BaseType
    {
        public BaseVec<ModuleMetadataV11> Modules { get; private set; }
        public ExtrinsicMetadataV11 Extrinsic { get; private set; }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Modules = new BaseVec<ModuleMetadataV11>();
            Modules.Decode(byteArray, ref p);

            Extrinsic = new ExtrinsicMetadataV11();
            Extrinsic.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }


    }
}
