using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;

namespace Substrate.NET.Metadata.V11
{
    public class RuntimeMetadataV11 : BaseType
    {
        public BaseVec<ModuleMetadataV11> Modules { get; private set; } = default!;
        public ExtrinsicMetadataV11 Extrinsic { get; private set; } = default!;

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

        public RuntimeMetadataV14 ToRuntimeMetadataV14()
        {
            var conversion = new ConversionBuilder(new List<PortableType>());

            var res = new RuntimeMetadataV14();

            res.Modules = new BaseVec<ModuleMetadataV14>(
                Modules.Value.Select(x => x.ToModuleMetadataV14(conversion))
                .ToArray());

            return res;
        }
    }
}
