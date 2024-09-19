using Substrate.NET.Metadata.V12;
using Substrate.NET.Metadata.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.V14;

namespace Substrate.NET.Metadata.V13
{
    public class MetadataV13 : BaseMetadata<RuntimeMetadataV13>, IMetadataToV14
    {
        public MetadataV13() : base()
        {
        }

        public MetadataV13(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V13;

        public MetadataV14 ToMetadataV14(uint? specVersion = null)
        {
            var res = new MetadataV14();

            res.MetaDataInfo = MetaDataInfo;
            res.RuntimeMetadataData = RuntimeMetadataData.ToRuntimeMetadataV14(specVersion);

            return res;
        }

        public override string TypeName() => nameof(MetadataV13);
    }
}
