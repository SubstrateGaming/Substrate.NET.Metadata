using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Diagnostics.CodeAnalysis;

namespace Substrate.NET.Metadata.V9
{
    public class PalletCallMetadataV9 : BaseType, IMetadataName, IMetadataConversion
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

            Args = new BaseVec<PalletCallArgsMetadataV9>();
            Args.Decode(byteArray, ref p);

            Docs = new BaseVec<Str>();
            Docs.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Name { get; set; }
        public BaseVec<PalletCallArgsMetadataV9> Args { get; set; }
        public BaseVec<Str> Docs { get; set; }

        public void AddToDictionnary(PortableRegistry lookup, string palletName)
        {
            var variant = new Variant(
                name: Name,
                index: new U8(0), // todo change
                docs: Docs,
                variantFields: new BaseVec<Field>(
                    new List<Field>() {
                        new Field(
                            name: new BaseOpt<Str>(Name),
                            fieldTy: new TType(),
                            fieldTypeName: new BaseOpt<Str>(),
                            docs: new BaseVec<Str>()
                        )
                    }.ToArray()
                )
            );


            //var newId = new U32(lookup.Value.Any() ? lookup.Value.Max(x => x.Id.Value) + 1 : 0);
            //var path = new Base.Portable.Path();
            //path.Create(new List<Str>() { new Str($"pallet_{palletName.ToLower()}"), new Str($"Pallet"), new Str($"Call") }.ToArray());

            //var typePortableForm = new TypePortableForm(path, new BaseVec<TypeParameter>(), null, Docs);

            //var portableType = new PortableType(newId, typePortableForm);
            //lookup.Value.ToList().Add(portableType);
        }
    }

    public class PalletCallMetadataV9Comparer : IEqualityComparer<PalletCallMetadataV9>
    {
        public bool Equals(PalletCallMetadataV9? x, PalletCallMetadataV9? y)
        {
            return x?.Name == y?.Name;
        }

        public int GetHashCode([DisallowNull] PalletCallMetadataV9 obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
