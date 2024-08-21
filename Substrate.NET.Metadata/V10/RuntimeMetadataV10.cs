using Substrate.NET.Metadata.V9;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi.Model.Types.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;

namespace Substrate.NET.Metadata.V10
{
    public class RuntimeMetadataV10 : BaseType
    {
        public BaseVec<ModuleMetadataV10> Modules { get; private set; }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Modules = new BaseVec<ModuleMetadataV10>();
            Modules.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Modules.Encode());
            return result.ToArray();
        }
    }
}
