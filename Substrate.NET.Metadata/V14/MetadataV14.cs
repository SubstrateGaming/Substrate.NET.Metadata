using Substrate.NET.Metadata.V13;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Meta;
using Substrate.NetApi;
using Substrate.NetApi.Model.Types.Metadata.Base;
using Substrate.NetApi.Model.Types.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using TypeDefEnum = Substrate.NET.Metadata.Base.TypeDefEnum;
using TypeDefComposite = Substrate.NET.Metadata.Base.TypeDefComposite;

namespace Substrate.NET.Metadata.V14
{
    public class MetadataV14 : BaseMetadata<RuntimeMetadataV14>
    {
        public MetadataV14() : base()
        {
        }

        public MetadataV14(string hex) : base(hex)
        {
        }

        public override MetadataVersion Version => MetadataVersion.V14;
        public override string TypeName() => nameof(MetadataV14);

        /// <summary>
        /// Convert to <see cref="MetaData"/> to keep compatibility with Substrate.NetApi
        /// </summary>
        /// <returns></returns>
        public MetaData ToNetApiMetadata()
        {
            var metadata = new MetaData();
            metadata.Origin = "unknown";
            metadata.Magic = Utils.Bytes2HexString(MetaDataInfo.Magic.Bytes);
            metadata.Version = MetaDataInfo.Version;

            metadata.NodeMetadata = new NodeMetadataV14()
            {
                Types = CreateNodeTypeDict(RuntimeMetadataData.Lookup.Value),
                Modules = CreateModuleDict(RuntimeMetadataData.Modules.Value),
                Extrinsic = CreateExtrinsic(RuntimeMetadataData.Extrinsic),
            };

            return metadata;
        }

        /// <summary>
        /// Create Node Type Dictionary
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static Dictionary<uint, NodeType> CreateNodeTypeDict(PortableType[] types)
        {
            var result = new Dictionary<uint, NodeType>();

            foreach (var type in types)
            {
                var path = type.Ty.Path.Value.Length == 0 ? null : type.Ty.Path.Value.Select(p => p.Value).ToArray();
                var typeParams = type.Ty.TypeParams.Value.Length == 0 ? null : type.Ty.TypeParams.Value.Select(p =>
                {
                    return new NodeTypeParam()
                    {
                        Name = p.Name.Value,
                        TypeId = p.TypeParameterType.Value?.Value
                    };
                }).ToArray();

                var typeDefValue = (NetApi.Model.Types.Metadata.Base.TypeDefEnum)type.Ty.TypeDef.Value;
                var docs = type.Ty.Docs == null || type.Ty.Docs.Value.Length == 0 ? null : type.Ty.Docs.Value.Select(p => p.Value).ToArray();

                NodeType nodeType = null;
                switch (typeDefValue)
                {
                    case (NetApi.Model.Types.Metadata.Base.TypeDefEnum)TypeDefEnum.Composite:
                        {
                            var typeDef = (type.Ty.TypeDef.Value2 as TypeDefComposite).ToNetApi();
                            nodeType = new NodeTypeComposite()
                            {
                                Id = type.Id.Value,
                                Path = path,
                                TypeParams = typeParams,
                                TypeDef = typeDefValue,
                                TypeFields = typeDef.Fields.Value.Length == 0 ? null : typeDef.Fields.Value.Select(p =>
                                {
                                    var fDocs = p.Docs == null || p.Docs.Value.Length == 0 ? null : p.Docs.Value.Select(q => q.Value).ToArray();
                                    return new NodeTypeField()
                                    {
                                        Name = p.FieldName.Value?.Value,
                                        TypeName = p.FieldTypeName.Value?.Value,
                                        TypeId = p.FieldTy.Value,
                                        Docs = fDocs
                                    };
                                }).ToArray(),
                                Docs = docs
                            };
                        }
                        break;

                    case (NetApi.Model.Types.Metadata.Base.TypeDefEnum)TypeDefEnum.Variant:
                        {
                            var typeDef = (type.Ty.TypeDef.Value2 as Base.TypeDefVariant).ToNetApi();
                            nodeType = new NodeTypeVariant()
                            {
                                Id = type.Id.Value,
                                Path = path,
                                TypeParams = typeParams,
                                TypeDef = typeDefValue,
                                Variants = typeDef.TypeParam.Value.Length == 0 ? null : typeDef.TypeParam.Value.Select(p =>
                                {
                                    var vDocs = p.Docs == null || p.Docs.Value.Length == 0 ? null : p.Docs.Value.Select(q => q.Value).ToArray();
                                    return new TypeVariant()
                                    {
                                        Name = p.VariantName.Value,
                                        TypeFields = p.VariantFields.Value.Length == 0 ? null : p.VariantFields.Value.Select(q =>
                                        {
                                            var fDocs = q.Docs == null || q.Docs.Value.Length == 0 ? null : q.Docs.Value.Select(r => r.Value).ToArray();
                                            return new NodeTypeField()
                                            {
                                                Name = q.FieldName.Value?.Value,
                                                TypeName = q.FieldTypeName.Value?.Value,
                                                TypeId = q.FieldTy.Value,
                                                Docs = fDocs
                                            };
                                        }).ToArray(),
                                        Index = p.Index.Value,
                                        Docs = vDocs
                                    };
                                }).ToArray(),
                                Docs = docs
                            };
                        }
                        break;

                    case (NetApi.Model.Types.Metadata.Base.TypeDefEnum)TypeDefEnum.Sequence:
                        {
                            var typeDef = (type.Ty.TypeDef.Value2 as Base.TypeDefSequence).ToNetApi();
                            nodeType = new NodeTypeSequence()
                            {
                                Id = type.Id.Value,
                                Path = path,
                                TypeParams = typeParams,
                                TypeDef = typeDefValue,
                                TypeId = typeDef.TypeParam.Value,
                                Docs = docs
                            };
                        }
                        break;

                    case (NetApi.Model.Types.Metadata.Base.TypeDefEnum)TypeDefEnum.Array:
                        {
                            var typeDef = (type.Ty.TypeDef.Value2 as Base.TypeDefArray).ToNetApi();
                            nodeType = new NodeTypeArray()
                            {
                                Id = type.Id.Value,
                                Path = path,
                                TypeParams = typeParams,
                                TypeDef = typeDefValue,
                                TypeId = typeDef.TypeParam.Value,
                                Length = typeDef.Len.Value,
                                Docs = docs
                            };
                        }
                        break;

                    case (NetApi.Model.Types.Metadata.Base.TypeDefEnum)TypeDefEnum.Tuple:
                        {
                            var typeDef = (type.Ty.TypeDef.Value2 as Base.TypeDefTuple).ToNetApi();
                            nodeType = new NodeTypeTuple()
                            {
                                Id = type.Id.Value,
                                Path = path,
                                TypeParams = typeParams,
                                TypeDef = typeDefValue,
                                TypeIds = typeDef.Fields.Value.Select(p => (uint)p.Value).ToArray(),
                                Docs = docs
                            };
                        }
                        break;

                    case (NetApi.Model.Types.Metadata.Base.TypeDefEnum)TypeDefEnum.Primitive:
                        {
                            var typeDef = (Substrate.NetApi.Model.Types.Metadata.Base.TypeDefPrimitive)Enum.Parse(typeof(Base.TypeDefPrimitive), type.Ty.TypeDef.Value2.ToString());
                            nodeType = new NodeTypePrimitive()
                            {
                                Id = type.Id.Value,
                                Path = path,
                                TypeParams = typeParams,
                                TypeDef = typeDefValue,
                                Primitive = typeDef,
                                Docs = docs
                            };
                        }
                        break;

                    case (NetApi.Model.Types.Metadata.Base.TypeDefEnum)TypeDefEnum.Compact:
                        {
                            var typeDef = (type.Ty.TypeDef.Value2 as Base.TypeDefCompact).ToNetApi();
                            nodeType = new NodeTypeCompact()
                            {
                                Id = type.Id.Value,
                                Path = path,
                                TypeParams = typeParams,
                                TypeDef = typeDefValue,
                                TypeId = typeDef.TypeParam.Value,
                                Docs = docs
                            };
                        }
                        break;

                    case (NetApi.Model.Types.Metadata.Base.TypeDefEnum)TypeDefEnum.BitSequence:
                        {
                            var typeDef = (type.Ty.TypeDef.Value2 as Base.TypeDefBitSequence).ToNetApi();
                            nodeType = new NodeTypeBitSequence()
                            {
                                Id = type.Id.Value,
                                Path = path,
                                TypeParams = typeParams,
                                TypeDef = typeDefValue,
                                TypeIdOrder = typeDef.BitOrderType.Value,
                                TypeIdStore = typeDef.BitStoreType.Value,
                                Docs = docs
                            };
                        }
                        break;
                }

                if (nodeType != null)
                {
                    result.Add(nodeType.Id, nodeType);
                }
            }

            return result;
        }

        /// <summary>
        /// Create Module Dictionary
        /// </summary>
        /// <param name="modules"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Dictionary<uint, PalletModule> CreateModuleDict(ModuleMetadataV14[] modules)
        {
            var result = new Dictionary<uint, PalletModule>();

            foreach (var module in modules)
            {
                var palletModule = new PalletModule()
                {
                    Name = module.Name.Value,
                    Index = module.Index.Value,
                };
                result.Add(module.Index.Value, palletModule);

                if (module.Storage.OptionFlag)
                {
                    var storage = module.Storage.Value;
                    palletModule.Storage = new PalletStorage()
                    {
                        Prefix = storage.Prefix.Value,
                    };

                    palletModule.Storage.Entries = new Entry[storage.Entries.Value.Length];
                    for (int i = 0; i < storage.Entries.Value.Length; i++)
                    {
                        var entry = storage.Entries.Value[i];
                        palletModule.Storage.Entries[i] = new Entry()
                        {
                            Name = entry.Name.Value,
                            Modifier = (Storage.Modifier)entry.StorageModifier.Value,
                            StorageType = (Storage.Type)entry.StorageType.Value,
                            Default = entry.StorageDefault.Value.Select(p => p.Value).ToArray(),
                            Docs = entry.Documentation.Value.Select(p => p.Value).ToArray(),
                        };

                        switch (entry.StorageType.Value)
                        {
                            case (StorageType.Type)Storage.Type.Plain:
                                palletModule.Storage.Entries[i].TypeMap = (((Base.TType)entry.StorageType.Value2).Value, null);
                                break;

                            case (StorageType.Type)Storage.Type.Map:
                                var typeMap = (entry.StorageType.Value2 as StorageEntryTypeMapV14).ToStorageEntryTypeMap();
                                palletModule.Storage.Entries[i].TypeMap = (0, new TypeMap()
                                {
                                    Hashers = typeMap.Hashers.Value.Select(p => p.Value).ToArray(),
                                    Key = (uint)typeMap.Key.Value,
                                    Value = (uint)typeMap.Value.Value
                                });
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }
                }

                if (module.Calls.OptionFlag)
                {
                    var calls = module.Calls.Value;
                    palletModule.Calls = new PalletCalls()
                    {
                        TypeId = (uint)calls.ElemType.Value
                    };
                }

                if (module.Events.OptionFlag)
                {
                    var events = module.Events.Value;
                    palletModule.Events = new PalletEvents()
                    {
                        TypeId = (uint)events.ElemType.Value
                    };
                }

                var constants = module.Constants.Value;
                palletModule.Constants = new PalletConstant[constants.Length];
                for (int i = 0; i < constants.Length; i++)
                {
                    NetApi.Model.Types.Metadata.V14.PalletConstantMetadata constant = constants[i].ToPalletConstantNetApi();
                    palletModule.Constants[i] = new PalletConstant()
                    {
                        Name = constant.ConstantName.Value,
                        TypeId = (uint)constant.ConstantType.Value,
                        Value = constant.ConstantValue.Value.Select(p => p.Value).ToArray(),
                        Docs = constant.Docs.Value.Select(p => p.Value).ToArray()
                    };
                }

                if (module.Errors.OptionFlag)
                {
                    var errors = module.Errors.Value;
                    palletModule.Errors = new PalletErrors()
                    {
                        TypeId = (uint)errors.ElemType.Value
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// Create Extrinsic
        /// </summary>
        /// <param name="extrinsic"></param>
        /// <returns></returns>
        private static ExtrinsicMetadata CreateExtrinsic(ExtrinsicMetadataV14 extrinsic)
        {
            return new ExtrinsicMetadata()
            {
                //TypeId = (uint)extrinsic.ExtrinsicType.Value,
                //Version = (int)extrinsic.Version.Value,
                //SignedExtensions = extrinsic.SignedExtensions.Value.Select(p => new SignedExtensionMetadata()
                //{
                //    SignedIdentifier = p.SignedIdentifier.Value,
                //    SignedExtType = (uint)p.SignedExtType.Value,
                //    AddSignedExtType = (uint)p.AddSignedExtType.Value,
                //}).ToArray()
            };
        }
    }
}
