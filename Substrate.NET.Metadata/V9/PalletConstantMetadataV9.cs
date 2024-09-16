using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Diagnostics;
using System.Net.Mime;

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
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(ConstantType.Encode());
            result.AddRange(Value.Encode());
            result.AddRange(Documentation.Encode());
            return result.ToArray();
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

        internal PalletConstantMetadataV14 ToPalletConstantMetadataV14(ConversionBuilder conversionBuilder)
        {
            var result = new PalletConstantMetadataV14();

            result.Name = this.Name;
            result.ConstantValue = this.Value;
            result.ConstantType = TType.From(conversionBuilder.BuildPortableTypes(this.ConstantType.Value).Value);
            result.Documentation = this.Documentation;

            return result;
        }
    }
}
