using Substrate.NET.Metadata.Base.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion
{
    /// <summary>
    /// Convert metadata < v14 to v14 format
    /// </summary>
    public interface IMetadataConversion
    {
        void AddToDictionnary(PortableRegistry lookup, string palletName);
    }
}
