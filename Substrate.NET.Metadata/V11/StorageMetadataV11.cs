using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V10;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V11
{
    public class PalletStorageMetadataV11 : BaseType
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

            Entries = new BaseVec<StorageEntryMetadataV11>();
            Entries.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Prefix { get; private set; } = default!;
        public BaseVec<StorageEntryMetadataV11> Entries { get; private set; } = default!;

        public PalletStorageMetadataV14 ToStorageMetadataV14(ConversionBuilder conversionBuilder)
        {
            var storage = new PalletStorageMetadataV14(
                prefix: Prefix.Value,
                entries: Entries.Value.Select(x => x.ToStorageEntryMetadataV14(conversionBuilder))
            );

            return storage;
        }
    }
}