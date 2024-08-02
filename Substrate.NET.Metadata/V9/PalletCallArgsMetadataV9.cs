using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V9
{
    public class PalletCallArgsMetadataV9 : BaseType, IMetadataName
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(CallType.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            CallType = new Str();
            CallType.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; set; }
        public Str CallType { get; set; }

        public void AddToDictionnary(PortableRegistry lookup, string palletName)
        {
            // Need to parse CallType to determine the type of the call and get or insert from the lookup
            
        }
    }
}
