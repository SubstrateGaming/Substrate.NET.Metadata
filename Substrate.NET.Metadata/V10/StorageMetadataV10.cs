using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.V9;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System.Xml.Linq;

namespace Substrate.NET.Metadata.V10
{
    public class PalletStorageMetadataV10 : BaseType
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

            Entries = new BaseVec<StorageEntryMetadataV10>();
            Entries.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public Str Prefix { get; private set; } = default!;
        public BaseVec<StorageEntryMetadataV10> Entries { get; private set; } = default!;

        internal PalletStorageMetadataV14 ToStorageMetadataV14(ConversionBuilder conversionBuilder)
        {
            var storage = new PalletStorageMetadataV14(
                prefix: Prefix.Value,
                entries: Entries.Value.Select(x => x.ToStorageEntryMetadataV14(conversionBuilder))
            );

            return storage;
        }
    }
}