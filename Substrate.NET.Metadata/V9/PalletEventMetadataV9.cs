using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V9
{
    public class PalletEventMetadataV9 : BaseType, IMetadataName
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(Args.Encode());
            result.AddRange(Docs.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            Args = new BaseVec<Str>();
            Args.Decode(byteArray, ref p);

            Docs = new BaseVec<Str>();
            Docs.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; set; }
        public BaseVec<Str> Args { get; set; }
        public BaseVec<Str> Docs { get; set; }

        public Variant ToVariant(ConversionBuilder conversionBuilder, int index)
        {
            var res = new Variant();

            res.Name = Name;
            res.Docs = Docs;
            res.Index = new U8((byte)index);

            var argsType = Args.Value.Select(x => new { Name = x, TypeId = conversionBuilder.BuildLookup(x.Value) });
            res.VariantFields = new BaseVec<Field>(
                argsType.Select(x => new Field(new BaseOpt<Str>(x.Name), TType.From(x.TypeId.Value), new BaseOpt<Str>(), new BaseVec<Str>(new Str[0]))
                ).ToArray());

            return res;
        }
    }
}
