﻿using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NetApi.Model.Types.Base;

namespace Substrate.NET.Metadata.V15
{
    public class RuntimeMetadataV15 : BaseType
    {
        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Lookup = new PortableRegistry();
            Lookup.Decode(byteArray, ref p);

            Modules = new BaseVec<ModuleMetadataV15>();
            Modules.Decode(byteArray, ref p);

            Extrinsic = new ExtrinsicMetadataV15();
            Extrinsic.Decode(byteArray, ref p);

            TypeId = new TType();
            TypeId.Decode(byteArray, ref p);

            Apis = new BaseVec<RuntimeApiMetadataV15>();
            Apis.Decode(byteArray, ref p);

            OuterEnums = new OuterEnums15();
            OuterEnums.Decode(byteArray, ref p);

            Custom = new CustomMetadata15();
            Custom.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public PortableRegistry Lookup { get; private set; } = default!;

        public BaseVec<ModuleMetadataV15> Modules { get; private set; } = default!;

        public ExtrinsicMetadataV15 Extrinsic { get; private set; } = default!;

        public BaseVec<RuntimeApiMetadataV15> Apis { get; private set; } = default!;

        public TType TypeId { get; private set; } = default!;

        public OuterEnums15 OuterEnums { get; private set; } = default!;

        public CustomMetadata15 Custom { get; private set; } = default!;
    }
}
