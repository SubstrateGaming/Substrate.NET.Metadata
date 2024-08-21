using Newtonsoft.Json.Linq;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Net.Mime;

namespace Substrate.NET.Metadata.V9
{
    public class PalletErrorMetadataV9 : BaseType, IMetadataName
    {
        public Str Name { get; set; } = default!;
        public BaseVec<Str> Docs { get; set; } = default!;

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(Docs.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            int num = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            Docs = new BaseVec<Str>();
            Docs.Decode(byteArray, ref p);

            TypeSize = p - num;
        }

        public Variant ToVariant(ConversionBuilder conversionBuilder, int index)
        {
            var res = new Variant();

            res.Name = Name;
            res.Docs = Docs;
            res.Index = new U8((byte)index);
            res.VariantFields = new BaseVec<Field>(new Field[0]);

            return res;
        }
    }
}
