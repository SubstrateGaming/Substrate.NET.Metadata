using Substrate.NET.Metadata.Base.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    internal interface ICustomNodeBuilder
    {
        bool IsVersionValid(uint version);
        PortableType Build(ConversionBuilder conversionBuilder);
    }
}
