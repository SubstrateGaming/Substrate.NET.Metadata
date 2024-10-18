using Substrate.NetApi.Model.Types.Base;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion;

namespace Substrate.NET.Metadata.V10
{
    public class RuntimeMetadataV10 : BaseType
    {
        public BaseVec<ModuleMetadataV10> Modules { get; private set; } = default!;

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Modules = new BaseVec<ModuleMetadataV10>();
            Modules.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Modules.Encode());
            return result.ToArray();
        }

        internal RuntimeMetadataV14 ToRuntimeMetadataV14(uint? specVersion)
        {
            var conversion = new ConversionBuilder(new List<PortableType>());

            // Custom nodes created manually
            ManualNodes.All(specVersion).Build(conversion);

            conversion.CreateUnknownType();
            conversion.CreateEventBlockchainRuntimeEvent();

            var res = new RuntimeMetadataV14();

            res.Modules = new BaseVec<ModuleMetadataV14>(
                Modules.Value.Select((x, i) => x.ToModuleMetadataV14(conversion, i))
                .ToArray());

            conversion.CreateRuntime();

            res.Lookup = new PortableRegistry();
            res.Lookup.Create(conversion.PortableTypes.ToArray());

            return res;
        }
    }
}
