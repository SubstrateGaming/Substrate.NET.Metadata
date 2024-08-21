using Newtonsoft.Json.Linq;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Compare;
using Substrate.NET.Metadata.Conversion;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V9
{
    public class ModuleMetadataV9 : BaseType, IMetadataName, IMetadataConversion
    {
        public Str Name { get; private set; }
        public BaseOpt<PalletStorageMetadataV9> Storage { get; private set; }
        public BaseOpt<BaseVec<PalletCallMetadataV9>> Calls { get; private set; }
        public BaseOpt<BaseVec<PalletEventMetadataV9>> Events { get; private set; }
        public BaseVec<PalletConstantMetadataV9> Constants { get; private set; }
        public BaseVec<PalletErrorMetadataV9> Errors { get; private set; }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;

            Name = new Str();
            Name.Decode(byteArray, ref p);

            Storage = new BaseOpt<PalletStorageMetadataV9>();
            Storage.Decode(byteArray, ref p);

            Calls = new BaseOpt<BaseVec<PalletCallMetadataV9>>();
            Calls.Decode(byteArray, ref p);

            Events = new BaseOpt<BaseVec<PalletEventMetadataV9>>();
            Events.Decode(byteArray, ref p);

            Constants = new BaseVec<PalletConstantMetadataV9>();
            Constants.Decode(byteArray, ref p);

            Errors = new BaseVec<PalletErrorMetadataV9>();
            Errors.Decode(byteArray, ref p);

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
            return result.ToArray();
        }

        public MetadataDifferentialModulesV9 ToDifferentialModules(CompareStatus status)
        {
            return new MetadataDifferentialModulesV9()
            {
                ModuleName = Name.Value,
                CompareStatus = status,
                Calls = Calls.Value.Value.Select(y => (status, y)),
                Events = Events.Value.Value.Select(y => (status, y)),
                Constants = Constants.Value.Select(y => (status, y)),
                Errors = Errors.Value.Select(y => (status, y)),
                Storage = Storage.Value != null ?
                    Storage.Value.Entries.Value.Select(x => (Storage.Value.Prefix.Value, (status, x))) :
                    Enumerable.Empty<(string, (CompareStatus, StorageEntryMetadataV9))>(),
            };
        }

        public void AddToDictionnary(PortableRegistry lookup, string palletName)
        {
            var newId = new U32(lookup.Value.Any() ? lookup.Value.Max(x => x.Id.Value) + 1 : 0);
            var path = new Base.Portable.Path();
            path.Create(new List<Str>() { new Str($"pallet_{palletName.ToLower()}"), new Str($"Pallet"), new Str($"Call") }.ToArray());

            BaseVec<Str> Docs = new BaseVec<Str>(new List<Str>() { new Str("Contains one variant") }.ToArray());
            var typeDef = new BaseEnumExt<TypeDefEnum, TypeDefComposite, TypeDefVariant, TypeDefSequence, TypeDefArray, TypeDefTuple, BaseEnum<TypeDefPrimitive>, TypeDefCompact, TypeDefBitSequence, BaseVoid>();

            BaseVec<Variant> typeParam = new BaseVec<Variant>(new List<Variant>().ToArray());
            var typeDefVariant = new TypeDefVariant();

            //typeDefVariant.Create();
            //typeDef.Create(TypeDefEnum.Variant, );

            var typePortableForm = new TypePortableForm(path, new BaseVec<TypeParameter>(), null, Docs);

            var portableType = new PortableType(newId, typePortableForm);
            lookup.Value.ToList().Add(portableType);
        }
    }
}
