using Substrate.NetApi.Model.Types.Base;

namespace Substrate.NET.Metadata.Base
{
    public abstract class BaseMetadata<T> : BaseType, IMetaDataInfo
        where T : BaseType, new()
    {
        protected BaseMetadata()
        {
        }

        protected BaseMetadata(string hex)
        {
            Create(hex);
        }

        public abstract MetadataVersion Version { get; }

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            MetaDataInfo = new MetaDataInfo();
            MetaDataInfo.Decode(byteArray, ref p);

            RuntimeMetadataData = new T();
            RuntimeMetadataData.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public MetaDataInfo MetaDataInfo { get; private set; } = default!;
        public T RuntimeMetadataData { get; private set; } = default!;
    }
}
