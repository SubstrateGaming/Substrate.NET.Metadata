using Substrate.NET.Metadata.Base;

namespace Substrate.NET.Metadata.V9
{
    public class MetadataV9 : BaseMetadata<RuntimeMetadataV9>
    {
        public MetadataV9() : base()
        {
        }

        public MetadataV9(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V9;

        public override string TypeName() => nameof(MetadataV9);
    }
}
