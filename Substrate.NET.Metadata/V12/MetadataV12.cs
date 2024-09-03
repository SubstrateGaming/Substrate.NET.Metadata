using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.V14;

namespace Substrate.NET.Metadata.V12
{
    public class MetadataV12 : BaseMetadata<RuntimeMetadataV12>, IMetadataToV14
    {
        public MetadataV12() : base()
        {
        }

        public MetadataV12(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V12;

        public MetadataV14 ToMetadataV14()
        {
            var res = new MetadataV14();

            res.MetaDataInfo = MetaDataInfo;
            res.RuntimeMetadataData = RuntimeMetadataData.ToRuntimeMetadataV14();

            return res;
        }

        public override string TypeName() => nameof(MetadataV12);
    }
}
