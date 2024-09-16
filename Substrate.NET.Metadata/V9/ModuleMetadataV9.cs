﻿using Newtonsoft.Json.Linq;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Compare;
using Substrate.NET.Metadata.Conversion;
using Substrate.NET.Metadata.Conversion.Internal;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.V9
{
    public class ModuleMetadataV9 : BaseType, IMetadataName
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

        internal ModuleMetadataV14 ToModuleMetadataV14(ConversionBuilder conversionBuilder, int index)
        {
            var result = new ModuleMetadataV14();
            result.Index = new U8((byte)index);
            result.Name = Name;
            result.Index = new U8((byte)index);

            conversionBuilder.CurrentPallet = Name.Value;

            // We do not do Calls conversion
            result.Calls = new BaseOpt<PalletCallMetadataV14>(null!);

            if (Events.OptionFlag)
            {
                result.Events = ToPalletEventV14(conversionBuilder);

                var variantField = new Field(
                    name: new BaseOpt<Str>(),
                    fieldTy: result.Events.Value.ElemType,
                    fieldTypeName: new BaseOpt<Str>(new Str($"{Name.Value}::Event<Runtime")),
                    docs: new BaseVec<Str>(new Str[0]));

                var eventVariant = new Variant(
                    name: Name,
                    variantFields: new BaseVec<Field>([variantField]),
                    index: new U8((byte)index),
                    docs: new BaseVec<Str>(new Str[0]));

                conversionBuilder.AddPalletEventBlockchainRuntimeEvent(eventVariant);
            }
            else
            {
                result.Events = new BaseOpt<PalletEventMetadataV14>(null!);
            }

            result.Errors = ToPalletErrorV14(conversionBuilder);

            result.Constants = new BaseVec<PalletConstantMetadataV14>(Constants.Value.Select(x => x.ToPalletConstantMetadataV14(conversionBuilder)).ToArray());


            if (Storage.OptionFlag)
            {
                result.Storage = new BaseOpt<PalletStorageMetadataV14>(this.Storage.Value.ToStorageMetadataV14(conversionBuilder));
            }
            else
            {
                result.Storage = new BaseOpt<PalletStorageMetadataV14>(null!);
            }

            return result;
        }

        private BaseOpt<PalletErrorMetadataV14> ToPalletErrorV14(ConversionBuilder conversionBuilder)
        {
            var errorsVariants = Errors.Value.Select((x, i) => x.ToVariant(conversionBuilder, i)).ToArray();
            var palletError = new PalletErrorMetadataV14();
            palletError.ElemType = TType.From(conversionBuilder.AddErrorRuntimeLookup(Name.Value, errorsVariants).Value);
            return new BaseOpt<PalletErrorMetadataV14>(palletError);
        }

        private BaseOpt<PalletEventMetadataV14> ToPalletEventV14(ConversionBuilder conversionBuilder)
        {
            var eventsVariants = Events.Value.Value.Select((x, i) => x.ToVariant(conversionBuilder, i)).ToArray();

            var palletEvent = new PalletEventMetadataV14();
            palletEvent.ElemType = TType.From(conversionBuilder.AddEventRuntimeLookup(Name.Value, eventsVariants).Value);

            return new BaseOpt<PalletEventMetadataV14>(palletEvent);
        }
    }
}
