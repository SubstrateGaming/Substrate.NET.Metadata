using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.V14;

namespace Substrate.NET.Metadata.V10
{
    public class MetadataV10 : BaseMetadata<RuntimeMetadataV10>, IMetadataToV14
    {
        public MetadataV10() : base()
        {
        }

        public MetadataV10(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V10;

        public MetadataV14 ToMetadataV14()
        {
            var res = new MetadataV14();

            res.MetaDataInfo = MetaDataInfo;
            res.RuntimeMetadataData = RuntimeMetadataData.ToRuntimeMetadataV14();

            return res;
        }

        public override string TypeName() => nameof(MetadataV10);
    }
}
