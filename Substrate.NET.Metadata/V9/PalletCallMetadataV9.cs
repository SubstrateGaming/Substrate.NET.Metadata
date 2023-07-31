using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Diagnostics.CodeAnalysis;

namespace Substrate.NET.Metadata.V9
{
    public class PalletCallMetadataV9 : BaseType, IMetadataName
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
