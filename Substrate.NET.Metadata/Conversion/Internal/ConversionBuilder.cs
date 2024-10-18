using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.V14;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static Substrate.NET.Metadata.Conversion.Internal.SearchV14;
using TypeDefExt = Substrate.NetApi.Model.Types.Base.BaseEnumExt<
    Substrate.NET.Metadata.Base.TypeDefEnum,
    Substrate.NET.Metadata.Base.TypeDefComposite,
    Substrate.NET.Metadata.Base.TypeDefVariant,
    Substrate.NET.Metadata.Base.TypeDefSequence,
    Substrate.NET.Metadata.Base.TypeDefArray,
    Substrate.NET.Metadata.Base.TypeDefTuple,
    Substrate.NetApi.Model.Types.Base.BaseEnum<Substrate.NET.Metadata.Base.TypeDefPrimitive>,
    Substrate.NET.Metadata.Base.TypeDefCompact,
    Substrate.NET.Metadata.Base.TypeDefBitSequence,
    Substrate.NetApi.Model.Types.Base.BaseVoid>;

[assembly: InternalsVisibleToAttribute("Substrate.NET.Metadata.Tests")]
[assembly: InternalsVisibleToAttribute("Substrate.NET.Metadata.Node.Tests")]
namespace Substrate.NET.Metadata.Conversion.Internal
{
    /// <summary>
    /// Class to keep track of converted types
    /// </summary>
    internal class ConversionElementState
    {
        public ConversionElementState(string className, NodeBuilderType nodeBuilderType)
        {
            ClassName = className;
            NodeBuilderType = nodeBuilderType;
        }

        /// <summary>
        /// The Rust struct name
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public NodeBuilderType NodeBuilderType { get; set; }

        /// <summary>
        /// Facultative index found in v14
        /// </summary>
        public uint? IndexFoundInV14 { get; set; }

        /// <summary>
        /// Facultative index of created node
        /// </summary>
        public uint? IndexCreated { get; set; }

        public bool IsSuccessfullyMapped
        {
            get
            {
                return IndexFoundInV14 != null || IndexCreated != null;
            }
        }

        public override string ToString()
        {
            return $"{ClassName} (=> {NodeBuilderType.Adapted}) | V14 = {IndexFoundInV14} | CustomIndex = {IndexCreated} | Mapped = {IsSuccessfullyMapped}";
        }
    }

    /// <summary>
    /// This class make the transition between metadatas prev v14 and v14
    /// It helps to build the list of PortableType
    /// </summary>
    internal class ConversionBuilder
    {
        public List<PortableType> PortableTypes { get; init; }
        public List<ConversionElementState> ElementsState { get; init; }
        public IDictionary<int, int> OverrideTypeMapping { get; init; }
        public string CurrentPallet { get; set; } = string.Empty;
        public uint? UnknowIndex { get; set; } = null;
        public uint? PolkadotRuntimeEventIndex { get; private set; } = null;

        public const int START_INDEX = 1_000;

        public ConversionBuilder(List<PortableType> portableTypes)
        {
            PortableTypes = portableTypes;
            ElementsState = new List<ConversionElementState>();
            OverrideTypeMapping = new Dictionary<int, int>();
        }

        /// <summary>
        /// Conversion that are not handle by the <see cref="ConversionBuilder"/> because it is not necessary or too complex
        /// </summary>
        public readonly List<string> NotHandledConversion =
        [
            "Vec<Option<Scheduled<<T as Trait>::Call, T::BlockNumber>>>", // v11 => Scheduler => v1 => Storage => Agenda
            "Vec<Option<Scheduled<<T as Trait>::Call, T::BlockNumber, T::PalletsOrigin, T::AccountId>>>", // v12 => Scheduler => v25 => Storage => Agenda
            "Vec<Option<Scheduled<<T as Config>::Call, T::BlockNumber, T::PalletsOrigin, T::AccountId>>>", // v12 => Scheduler => v27 => Storage => Agenda
            "ElectionResult<T::AccountId, BalanceOf<T>>", // V11 => Staking => v1 => Storage => QueuedElected
            "PhragmenScore", // V11 => Staking => v1 => Storage => QueuedScore
            "ElectionStatus<T::BlockNumber>", // V11 => Staking => v1 => Storage => EraElectionStatus
            "Vec<DeferredOffenceOf<T>>", // V11 => Offences => v1 => Storage => DeferredOffences
            "<T as Trait<I>>::Proposal", // V11 => Council => v1 => Storage => ProposalOf
            //"(BalanceOf<T>, Vec<T::AccountId>)", // V11 => ElectionsPhragmen => v1 => Storage => Voting ==> Easy to create
            "sp_std::marker::PhantomData<(AccountId, Event)>", // V11 => TechnicalMembership => v1 => Events => Dummy
            "Vec<UpwardMessage>", // V11 => Parachains => v1 => Storage => RelayDispatchQueue
            "IncludedBlocks<T>", // V11 => Attestations => v1 => Storage => RecentParaBlocks
            "BlockAttestations<T>", // V11 => Attestations => v1 => Storage => ParaBlockAttestations
            "WinningData<T>", // V11 => Slots => v1 => Storage => Winning
            "(LeasePeriodOf<T>, IncomingParachain<T::AccountId, T::Hash>)", // V11 => Slots => v1 => Storage => Onboarding
            "ElectionScore", // V12 => Staking => v25 => Storage => QueuedScore
            "(OpaqueCall, T::AccountId, BalanceOf<T>)", // V12 => Multisig => v25 => Storage => Calls
            "<T as Config<I>>::Proposal", // V12 => Council => v27 => Storage => ProposalOf
            "ProxyState<T::AccountId>", // V11 => Democracy => v1 => Storage => Proxy
            "NewBidder<AccountId>", // V11 => Slots => v1 => Events => WonDeploy
            "Bidder<T::AccountId>", // V11 => Slots => v1 => Storage => ReservedAmounts
            "Vec<DownwardMessage<T::AccountId>>", // V11 => Parachains => v14 => Storage => DownwardMessageQueue
            "AccountValidity", // V11 => Purchase => v16 => Event => ValidityUpdated // https://docs.rs/crate/polkadot-runtime-common/latest/source/src/purchase.rs
            "AccountStatus<BalanceOf<T>>", // V11 => Purchase => v16 => Storage => Accounts // https://docs.rs/crate/polkadot-runtime-common/latest/source/src/purchase.rs
        ];

        /// <summary>
        /// Return a new for the next PortableType
        /// </summary>
        /// <returns></returns>
        public U32 GetNewIndex()
        {
            if (PortableTypes.Count == 0) return new U32(START_INDEX);
            return new U32(Math.Max(START_INDEX, PortableTypes.Max(x => x.Id.Value) + 1));
        }

        /// <summary>
        /// Return the index of the type from a Rust struct name
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public U32 BuildPortableTypes(string className)
        {
            className = className.Replace("\r", string.Empty).Replace("\n", string.Empty);

            if (NotHandledConversion.Contains(className))
                return new U32(UnknowIndex!.Value); // Ok this is bad

            var founded = GetPortableType(className);

            if (founded.searchResult == SearchResult.Founded)
                ElementsState[^1].IndexFoundInV14 = founded.portableType.Id.Value;
            else
                ElementsState[^1].IndexCreated = founded.portableType.Id.Value;

            return founded.portableType.Id;
        }

        public (int index, SearchResult searchResult) FindIndexByClass(string className)
        {
            var nodeRaw = new NodeBuilderTypeUndefined(className, CurrentPallet);

            var nodeType = ConversionBuilderTree.Build(nodeRaw);
            var res =  SearchV14.SearchIndexByNode(nodeType, this);

            res.index = GetIndexOrMapped(res.index);

            ElementsState.Add(new ConversionElementState(className, nodeType));

            return res;
        }

        /// <summary>
        /// Try to get the type from V14
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public (PortableType portableType, SearchResult searchResult) GetPortableType(string className)
        {
            var nodeRaw = new NodeBuilderTypeUndefined(className, CurrentPallet);

            // Node like events has do not have to be seached in V14
            if (nodeRaw.IndexHardBinding != null)
            {
                ElementsState.Add(new ConversionElementState(className, nodeRaw));
                return (LoopFromV14((int)nodeRaw.IndexHardBinding!), SearchResult.Founded);
            }

            var index = FindIndexByClass(className);

            return (LoopFromV14(index.index), index.searchResult);
        }

        /// <summary>
        /// Check if the index has been override. If true return the override index, otherwise return self
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int GetIndexOrMapped(int index)
        {
            if(OverrideTypeMapping.TryGetValue(index, out var newIndex))
            {
                return newIndex;
            }

            return index;
        }

        public PortableType LoopFromV14(int index)
        {
            var alreadyInserted = PortableTypes.SingleOrDefault(x => x.Id.Value == index);
            if (alreadyInserted != null) return alreadyInserted;

            var portableType = SearchV14.FindTypeByIndex(index).Clone();
            
            PortableTypes.Add(portableType);
            switch (portableType.Ty.TypeDef.Value)
            {
                case TypeDefEnum.Composite:
                    var composite = (TypeDefComposite)portableType.Ty.TypeDef.Value2;

                    foreach (var field in composite.Fields.Value)
                    {
                        field.FieldTy.Value = GetIndexOrMapped(field.FieldTy.Value);
                        _ = LoopFromV14(field.FieldTy.Value);
                    }
                    break;
                case TypeDefEnum.Array:
                    var array = (TypeDefArray)portableType.Ty.TypeDef.Value2;

                    array.ElemType.Value = GetIndexOrMapped(array.ElemType.Value);
                    _ = LoopFromV14(array.ElemType.Value);
                    break;
                case TypeDefEnum.Variant:
                    var variants = (TypeDefVariant)portableType.Ty.TypeDef.Value2;
                    foreach (var variant in variants.TypeParam.Value)
                    {
                        foreach (var field in variant.VariantFields.Value)
                        {
                            field.FieldTy.Value = GetIndexOrMapped(field.FieldTy.Value);
                            _ = LoopFromV14(field.FieldTy.Value);
                        }
                    }
                    break;
                case TypeDefEnum.Sequence:
                    var sequence = (TypeDefSequence)portableType.Ty.TypeDef.Value2;

                    sequence.ElemType.Value = GetIndexOrMapped(sequence.ElemType.Value);
                    _ = LoopFromV14(sequence.ElemType.Value);
                    break;
                case TypeDefEnum.Tuple:
                    var tuple = (TypeDefTuple)portableType.Ty.TypeDef.Value2;
                    foreach (var field in tuple.Fields.Value)
                    {
                        field.Value = GetIndexOrMapped(field.Value);
                        _ = LoopFromV14(field.Value);
                    }
                    break;
                case TypeDefEnum.Primitive: break;
                case TypeDefEnum.Compact:
                    var compact = (TypeDefCompact)portableType.Ty.TypeDef.Value2;

                    compact.ElemType.Value = GetIndexOrMapped(compact.ElemType.Value);
                    _ = LoopFromV14(compact.ElemType.Value);
                    break;
                case TypeDefEnum.BitSequence:
                    var bitSequence = (TypeDefBitSequence)portableType.Ty.TypeDef.Value2;

                    bitSequence.BitOrderType.Value = GetIndexOrMapped(bitSequence.BitOrderType.Value);
                    bitSequence.BitStoreType.Value = GetIndexOrMapped(bitSequence.BitStoreType.Value);

                    _ = LoopFromV14(bitSequence.BitOrderType.Value);
                    _ = LoopFromV14(bitSequence.BitStoreType.Value);
                    break;
                default:
                    throw new MetadataConversionException($"Unsupported {portableType.Ty.TypeDef.Value} TypeDef");
            }

            return portableType;
        }

        public static string HarmonizeTypeName(string className)
        {
            return className
                .Replace("T::", "")
                .Replace("<T>", "");
        }

        /// <summary>
        /// Create a pallet runtime events
        /// </summary>
        /// <param name="palletName"></param>
        /// <param name="variants"></param>
        /// <returns></returns>
        public U32 AddEventRuntimeLookup(string palletName, Variant[] variants)
            => AddRuntimeLookup("Event", palletName, variants);

        public U32 AddErrorRuntimeLookup(string palletName, Variant[] variants)
            => AddRuntimeLookup("Error", palletName, variants);

        /// <summary>
        /// Create a pallet runtime events / calls / errors / constants
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="palletName"></param>
        /// <param name="variants"></param>
        /// <returns></returns>
        private U32 AddRuntimeLookup(string objType, string palletName, Variant[] variants)
        {
            var path = new Base.Portable.Path();

            var palletNameCasted = palletName.ToLower() switch
            {
                "system" => "frame_system",
                _ => "pallet_" + palletName.ToLower()
            };

            path.Create(new List<string>() { palletNameCasted, "pallet", objType }.Select(x => new Str(x)).ToArray());

            var typeParams = new BaseVec<TypeParameter>([
                new TypeParameter(new Str("T"), new BaseOpt<TType>())
            ]);

            var variant = new TypeDefVariant();
            variant.TypeParam = new BaseVec<Variant>(variants);
            var typeDef = new TypeDefExt();
            typeDef.Create(TypeDefEnum.Variant, variant);

            var docs = new BaseVec<Str>([new Str($"Event for the {palletName} pallet")]);

            var index = GetNewIndex();
            PortableTypes.Add(new PortableType(index, new TypePortableForm(
                path,
                typeParams,
                typeDef,
                docs
            )));

            return index;
        }

        public void CreateEventBlockchainRuntimeEvent()
        {
            var node = new TypeDefVariant();
            node.TypeParam = new BaseVec<Variant>(new Variant[0]);

            var eventsPortableType = CreatePortableTypeFromNode(node, ["polkadot_runtime", "Event"], null);

            PolkadotRuntimeEventIndex = eventsPortableType.Id.Value;

            // Keep track of the override
            OverrideTypeMapping.Add(FindIndexByClass("polkadot_runtime::Event")!.index, (int)PolkadotRuntimeEventIndex.Value);
        }

        public void AddPalletEventBlockchainRuntimeEvent(Variant variant)
        {
            var portableType = LoopFromV14((int)PolkadotRuntimeEventIndex!);

            var tdv = portableType.Ty.TypeDef.Value2 as TypeDefVariant;

            if (tdv is null)
                throw new MetadataConversionException("The type is not a variant");

            var list = tdv.TypeParam.Value.ToList();
            list.Add(variant);

            tdv.TypeParam = new BaseVec<Variant>(list.ToArray());
        }

        #region Build node

        /// <summary>
        /// Composite class use for undefined mapping
        /// </summary>
        /// <returns></returns>
        public PortableType CreateUnknownType()
        {
            var unknownType = new TypeDefComposite();
            unknownType.Fields = new BaseVec<Field>([]);

            var pt = CreatePortableTypeFromNode(unknownType, ["unknownType"]);
            UnknowIndex = pt.Id.Value;

            return pt;
        }

        public PortableType CreatePortableTypeFromNode(BaseType node, List<string>? path = null, List<string>? docs = null)
        {
            var portableType = new PortableType();

            portableType.Id = GetNewIndex();
            portableType.Ty = new TypePortableForm();

            if (docs is not null)
                portableType.Ty.Docs = new BaseVec<Str>(docs.Select(x => new Str(x)).ToArray());
            else
                portableType.Ty.Docs = new BaseVec<Str>(new Str[0]);

            if (path is not null)
            {
                portableType.Ty.Path = new Base.Portable.Path();
                portableType.Ty.Path.Create(path.Select(x => new Str(x)).ToArray());
            }
            else
            {
                portableType.Ty.Path = new Base.Portable.Path();
                portableType.Ty.Path.Create(new Str[0]);
            }

            portableType.Ty.TypeParams = new BaseVec<TypeParameter>([]);

            if (node is TypeDefTuple tdt)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Tuple, tdt);

                // Loop through the fields to add them to lookup. WARNING : Call LoopFromV14 could lead to an error if one of the children is not present in V14 (never happened until now, but could be possible I think)
                foreach(var field in tdt.Fields.Value)
                {
                    LoopFromV14((int)field.Value);
                }
                
            }
            else if (node is TypeDefComposite tdc)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Composite, tdc);

                // Loop through the fields to add them to lookup. WARNING : Call LoopFromV14 could lead to an error if one of the children is not present in V14 (never happened until now, but could be possible I think)
                foreach (var field in tdc.Fields.Value)
                {
                    LoopFromV14((int)field.FieldTy.Value);
                }
            } 
            else if (node is TypeDefSequence tds)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Sequence, tds);

                LoopFromV14((int)tds.ElemType.Value);
            } 
            else if (node is TypeDefArray tda)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Array, tda);

                LoopFromV14((int)tda.ElemType.Value);
            } 
            else if (node is TypeDefVariant tdv)
            {
                portableType.Ty.TypeDef = new TypeDefExt();
                portableType.Ty.TypeDef.Create(TypeDefEnum.Variant, tdv);

                foreach (var variant in tdv.TypeParam.Value.SelectMany(x => x.VariantFields.Value))
                {
                    LoopFromV14((int)variant.ElemType.Value);
                }
            }

            PortableTypes.Add(portableType);

            return portableType;
        }

        public TypeDefTuple BuildTuple(List<TType> types)
        {
            var node = new TypeDefTuple();

            node.Fields = new BaseVec<TType>(types.ToArray());

            return node;
        }

        public void CreateRuntime()
        {
            var emptyComposite = new TypeDefComposite();
            emptyComposite.Fields = new BaseVec<Field>(new Field[0]);

            CreatePortableTypeFromNode(node: emptyComposite, path: new List<string>() { "conversion_runtime", "runtime" });
        }
        #endregion
    }
}
