using System;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.Base
{
    public class Variant : BaseType, IMetadataName
    {
        public Variant() { }

        public Variant(Str name, BaseVec<Field> variantFields, U8 index, BaseVec<Str> docs)
        {
            Name = name;
            VariantFields = variantFields;
            Index = index;
            Docs = docs;
        }

        public override string TypeName() => "Variant<T: Form = MetaForm>";

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(VariantFields.Encode());
            result.AddRange(Index.Encode());
            result.AddRange(Docs.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            VariantFields = new BaseVec<Field>();
            VariantFields.Decode(byteArray, ref p);

            Index = new U8();
            Index.Decode(byteArray, ref p);

            Docs = new BaseVec<Str>();
            Docs.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; internal set; }
        public BaseVec<Field> VariantFields { get; internal set; }
        public U8 Index { get; internal set; }
        public BaseVec<Str> Docs { get; internal set; }
    }
}