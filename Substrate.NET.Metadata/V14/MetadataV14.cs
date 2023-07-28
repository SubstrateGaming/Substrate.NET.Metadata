using Substrate.NET.Metadata.V13;
using Substrate.NET.Metadata.Base;

namespace Substrate.NET.Metadata.V14
{
    public class MetadataV14 : BaseMetadata<RuntimeMetadataV14>
    {
        public MetadataV14() : base()
        {
        }

        public MetadataV14(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V14;
        public override string TypeName() => nameof(MetadataV14);
    }
}
