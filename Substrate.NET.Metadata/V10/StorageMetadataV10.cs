using Substrate.NET.Metadata.V9;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V10
{
    public class PalletStorageMetadataV10 : BaseType
    {
        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Prefix = new Str();
            Prefix.Decode(byteArray, ref p);

            Entries = new BaseVec<StorageEntryMetadataV10>();
            Entries.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Prefix { get; private set; }
        public BaseVec<StorageEntryMetadataV10> Entries { get; private set; }
    }
}