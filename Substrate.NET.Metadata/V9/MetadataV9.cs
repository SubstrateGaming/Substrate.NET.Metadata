using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.V14;

namespace Substrate.NET.Metadata.V9
{
    public class MetadataV9 : BaseMetadata<RuntimeMetadataV9>, IMetadataToV14
    {
        public MetadataV9() : base()
        {
        }

        public MetadataV9(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V9;

        public MetadataV14 ToMetadataV14()
        {
            var res = new MetadataV14();

            res.MetaDataInfo = MetaDataInfo;
            res.RuntimeMetadataData = RuntimeMetadataData.ToRuntimeMetadataV14();

            return res;
        }

        public override string TypeName() => nameof(MetadataV9);
    }
}
