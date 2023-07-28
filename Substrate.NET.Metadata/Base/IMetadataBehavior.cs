using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Base
{
    public interface IMetadataName
    {
        public Str Name { get; }
    }

    public interface IMetadataType
    {
        public TType ElemType { get; }
    }
}
