using Ardalis.GuardClauses;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi;

namespace Substrate.NET.Metadata
{
    public class CheckRuntimeMetadata
    {
        public MetaDataInfo MetaDataInfo { get; private set; }

        public CheckRuntimeMetadata(string hexMetadata)
        {
            Guard.Against.NullOrEmpty(hexMetadata);

            var bytes = Utils.HexToByteArray(hexMetadata);

            int p = 0;
            MetaDataInfo = new MetaDataInfo();
            MetaDataInfo.Decode(bytes, ref p);
        }
    }
}
