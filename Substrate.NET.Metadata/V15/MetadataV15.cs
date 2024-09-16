using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Substrate.NET.Metadata.V15
{
    [ExcludeFromCodeCoverage] // Exclude temporary because of incomplete implementation and lack of informations
    public class MetadataV15 : BaseMetadata<RuntimeMetadataV15>
    {
        public MetadataV15() : base()
        {
        }

        public MetadataV15(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V15;
        public override string TypeName() => nameof(MetadataV15);
    }
}
