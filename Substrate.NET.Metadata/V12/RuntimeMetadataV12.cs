using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.V12
{
    public class RuntimeMetadataV12 : BaseType
    {
        public BaseVec<ModuleMetadataV12> Modules { get; private set; }
        public ExtrinsicMetadataV12 Extrinsic { get; private set; }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Modules = new BaseVec<ModuleMetadataV12>();
            Modules.Decode(byteArray, ref p);

            Extrinsic = new ExtrinsicMetadataV12();
            Extrinsic.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Modules.Encode());
            result.AddRange(Extrinsic.Encode());
            return result.ToArray();
        }

        internal RuntimeMetadataV14 ToRuntimeMetadataV14()
        {
            var conversion = new ConversionBuilder(new List<PortableType>());

            conversion.CreateUnknownType();
            conversion.CreateEventBlockchainRuntimeEvent();

            var res = new RuntimeMetadataV14();

            res.Modules = new BaseVec<ModuleMetadataV14>(
                Modules.Value.Select((x, i) => x.ToModuleMetadataV14(conversion, i))
                .ToArray());

            res.Extrinsic = Extrinsic.ToExtrinsicMetadataV14(conversion);

            conversion.CreateRuntime();
            res.Lookup = new PortableRegistry();
            res.Lookup.Create(conversion.PortableTypes.ToArray());

            return res;
        }
    }
}
