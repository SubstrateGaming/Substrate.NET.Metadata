using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V9
{
    public class RuntimeMetadataV9 : BaseType, IMetadataConversion
    {
        public BaseVec<ModuleMetadataV9> Modules { get; private set; }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Modules = new BaseVec<ModuleMetadataV9>();
            Modules.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Modules.Encode());
            return result.ToArray();
        }

        public void AddToDictionnary(PortableRegistry lookup, string palletName)
        {
            
        }
    }
}
