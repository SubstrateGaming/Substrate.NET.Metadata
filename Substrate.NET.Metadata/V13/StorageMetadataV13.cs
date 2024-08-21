using Substrate.NET.Metadata.V11;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V13
{
    public class PalletStorageMetadataV13 : BaseType
    {
        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Prefix.Encode());
            result.AddRange(Entries.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Prefix = new Str();
            Prefix.Decode(byteArray, ref p);

            Entries = new BaseVec<StorageEntryMetadataV13>();
            Entries.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Prefix { get; private set; }
        public BaseVec<StorageEntryMetadataV13> Entries { get; private set; }
    }
}