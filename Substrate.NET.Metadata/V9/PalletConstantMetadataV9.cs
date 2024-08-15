using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Diagnostics;

namespace Substrate.NET.Metadata.V9
{
    public class PalletConstantMetadataV9 : BaseType, IMetadataName
    {
        public Str Name { get; set; }

        public Str ConstantType { get; set; }

        public ByteGetter Value { get; set; }

        public BaseVec<Str> Documentation { get; set; }

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            int num = p;
            Name = new Str();
            Name.Decode(byteArray, ref p);
            ConstantType = new Str();
            ConstantType.Decode(byteArray, ref p);
            Value = new ByteGetter();
            Value.Decode(byteArray, ref p);
            Documentation = new BaseVec<Str>();
            Documentation.Decode(byteArray, ref p);
            TypeSize = p - num;
        }

        public PalletConstantMetadataV14 ToPalletConstantMetadataV14(PortableRegistry lookup)
        {
            var result = new PalletConstantMetadataV14();

            result.Name = this.Name;
            result.ConstantValue = this.Value;
            result.ConstantType = ConversionV14Helper.AddToLookup(lookup, this.ConstantType.Value);
            result.Documentation = this.Documentation;

            return result;
        }
    }
}
