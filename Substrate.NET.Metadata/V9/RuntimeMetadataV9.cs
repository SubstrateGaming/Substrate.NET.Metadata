using Substrate.NetApi.Model.Types.Base;

namespace Substrate.NET.Metadata.V9
{
    public class RuntimeMetadataV9 : BaseType
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
            throw new NotImplementedException();
        }
    }
}
