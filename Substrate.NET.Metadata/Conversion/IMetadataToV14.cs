using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion
{
    public interface IMetadataToV14
    {
        /// <summary>
        /// Convert the metadata to V14 metadata
        /// This is use to keep the metadata in the same format and compatible with the Substrate.NET.Toolchain (https://github.com/SubstrateGaming/Substrate.NET.Toolchain)
        /// </summary>
        /// <returns></returns>
        MetadataV14 ToMetadataV14(uint? specVersion = null);
    }
}
