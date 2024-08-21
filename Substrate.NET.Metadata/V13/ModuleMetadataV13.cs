using Substrate.NET.Metadata.V10;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V9;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Compare;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V13
{
    public class ModuleMetadataV13 : BaseType, IMetadataName
    {
        public Str Name { get; private set; }
        public BaseOpt<PalletStorageMetadataV13> Storage { get; private set; }
        public BaseOpt<BaseVec<PalletCallMetadataV13>> Calls { get; private set; }
        public BaseOpt<BaseVec<PalletEventMetadataV13>> Events { get; private set; }
        public BaseVec<PalletConstantMetadataV13> Constants { get; private set; }
        public BaseVec<PalletErrorMetadataV13> Errors { get; private set; }
        public U8 Index { get; private set; }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            Storage = new BaseOpt<PalletStorageMetadataV13>();
            Storage.Decode(byteArray, ref p);

            Calls = new BaseOpt<BaseVec<PalletCallMetadataV13>>();
            Calls.Decode(byteArray, ref p);

            Events = new BaseOpt<BaseVec<PalletEventMetadataV13>>();
            Events.Decode(byteArray, ref p);

            Constants = new BaseVec<PalletConstantMetadataV13>();
            Constants.Decode(byteArray, ref p);

            Errors = new BaseVec<PalletErrorMetadataV13>();
            Errors.Decode(byteArray, ref p);

            Index = new U8();
            Index.Decode(byteArray, ref p);

            TypeSize = p - start;
        }

        public override byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Name.Encode());
            result.AddRange(Storage.Encode());
            result.AddRange(Calls.Encode());
            result.AddRange(Events.Encode());
            result.AddRange(Constants.Encode());
            result.AddRange(Errors.Encode());
            result.AddRange(Index.Encode());
            return result.ToArray();
        }

        public MetadataDifferentialModulesV13 ToDifferentialModules(CompareStatus status)
        {
            return new MetadataDifferentialModulesV13()
            {
                ModuleName = Name.Value,
                CompareStatus = status,
                Calls = Calls.Value.Value.Select(y => (status, y)),
                Events = Events.Value.Value.Select(y => (status, y)),
                Constants = Constants.Value.Select(y => (status, y)),
                Errors = Errors.Value.Select(y => (status, y)),
                Storage = Storage.Value != null ?
                    Storage.Value.Entries.Value.Select(x => (Storage.Value.Prefix.Value, (status, x))) :
                    Enumerable.Empty<(string, (CompareStatus, StorageEntryMetadataV13))>(),
            };
        }
    }
}