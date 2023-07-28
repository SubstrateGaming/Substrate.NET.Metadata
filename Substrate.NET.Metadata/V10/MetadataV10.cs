using Substrate.NET.Metadata.V9;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Metadata.V14;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
