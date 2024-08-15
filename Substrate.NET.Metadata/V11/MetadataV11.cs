using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.V14;

namespace Substrate.NET.Metadata.V11
{
    public class MetadataV11 : BaseMetadata<RuntimeMetadataV11>
    {
        public MetadataV11() : base()
        {
        }

        public MetadataV11(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V11;

        public override string TypeName() => nameof(MetadataV11);

        public MetadataV14 ToMetadataV14()
        {
            //var res = new MetadataV14()
            return null;
        }
    }
}
