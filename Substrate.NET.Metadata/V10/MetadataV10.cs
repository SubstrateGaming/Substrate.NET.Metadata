using Substrate.NET.Metadata.Base;

namespace Substrate.NET.Metadata.V10
{
    public class MetadataV10 : BaseMetadata<RuntimeMetadataV10>
    {
        public MetadataV10() : base()
        {
        }

        public MetadataV10(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V10;

        public override string TypeName() => nameof(MetadataV10);
    }
}
