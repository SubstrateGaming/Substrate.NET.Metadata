//using Substrate.NetApi.Model.Meta;
//using Substrate.NetApi;
//using Newtonsoft.Json.Converters;
//using Newtonsoft.Json;
//using Substrate.NetApi.Model.Types.Base;
//using Substrate.NET.Metadata.V11;
//using Substrate.NET.Metadata.V12;
//using Substrate.NET.Metadata.V13;
//using Substrate.NET.Metadata.V14;
//using Substrate.NET.Metadata.V15;
//using Substrate.NET.Metadata.V9;
//using Substrate.NET.Metadata.V10;
//using Substrate.NET.Metadata.Base;
//using Substrate.NetApi.Model.Types.Metadata.V14;

//namespace Substrate.NET.Metadata.Common
//{
//    /// <summary>
//    /// Reresents the a unique structure of metadata.
//    /// Build this same structure for all versions of metadata (v9 to v15)
//    /// 
//    /// Based on <see cref="NetApi.Model.Meta.MetaData"/>
//    /// </summary>
//    public class MetaData
//    {
//        public MetaData(IMetaDataInfo metadata, string origin = "unknown")
//        {
//            Origin = origin;
//            Magic = Utils.Bytes2HexString(metadata.MetaDataInfo.Magic.Bytes);
//            Version = metadata.MetaDataInfo.Version.Value;

//            // Ensure that the parameter is known
//            if (metadata is MetadataV9 metadataV9)
//            {
//                buildFromV9(metadataV9);
//            }
//            else if (metadata is MetadataV10 metadataV10)
//            {
//                buildFromV10(metadataV10);
//            }
//            if (metadata is MetadataV11 metadataV11)
//            {
//                buildFromV11(metadataV11);
//            }
//            else if (metadata is MetadataV12 metadataV12)
//            {
//                buildFromV12(metadataV12);
//            }
//            else if (metadata is MetadataV13 metadataV13)
//            {
//                buildFromV13(metadataV13);
//            }
//            else if (metadata is MetadataV14 metadataV14)
//            {
//                buildFromV14(metadataV14);
//            }
//            else if (metadata is MetadataV15 metadataV15)
//            {
//                buildFromV15(metadataV15);
//            }
//            else
//            {
//                throw new Exception("Unknown metadata type");
//            }

            
//        }

//        /// <summary>
//        /// Origin
//        /// </summary>
//        public string Origin { get; set; }

//        /// <summary>
//        /// Magic
//        /// </summary>
//        public string Magic { get; set; }

//        /// <summary>
//        /// Version
//        /// </summary>
//        public byte Version { get; set; }

//        public NodeMetadataV14 NodeMetadata { get; set; }

//        public string Serialize()
//        {
//            return JsonConvert.SerializeObject(this, new StringEnumConverter());
//        }

//        private void buildFromV9(MetadataV9 v9)
//        {
//            NodeMetadata = new NodeMetadataV14()
//            {
//                Types = CreateNodeTypeDict(v14.RuntimeMetadataData.Lookup.Value),
//                Modules = CreateModuleDict(v14.RuntimeMetadataData.Modules.Value),
//                Extrinsic = CreateExtrinsic(v14.RuntimeMetadataData.Extrinsic),
//                TypeId = (uint)v14.RuntimeMetadataData.TypeId.Value
//            };
//        }

//        private void buildFromV10(MetadataV10 v10)
//        {

//        }

//        private void buildFromV11(MetadataV11 v11)
//        {

//        }

//        private void buildFromV12(MetadataV12 v12)
//        {

//        }

//        private void buildFromV13(MetadataV13 v13)
//        {

//        }

//        private void buildFromV14(MetadataV14 v14)
//        {
//            Magic = Utils.Bytes2HexString(v14.MetaDataInfo.Magic.Bytes);
//            Version = v14.MetaDataInfo.Version.Value;
//            NodeMetadata = new NodeMetadataV14()
//            {
//                Types = CreateNodeTypeDict(v14.RuntimeMetadataData.Lookup.Value),
//                Modules = CreateModuleDict(v14.RuntimeMetadataData.Modules.Value),
//                Extrinsic = CreateExtrinsic(v14.RuntimeMetadataData.Extrinsic),
//                TypeId = (uint)v14.RuntimeMetadataData.TypeId.Value
//            };

//            /// <summary>
//            /// Create Node Type Dictionary
//            /// </summary>
//            /// <param name="types"></param>
//            /// <returns></returns>
//            static Dictionary<uint, NodeType> CreateNodeTypeDict(Base.Portable.PortableType[] types)
//            {
//                var result = new Dictionary<uint, NodeType>();

//                foreach (var type in types)
//                {
//                    var path = type.Ty.Path.Value.Length == 0 ? null : type.Ty.Path.Value.Select(p => p.Value).ToArray();
//                    var typeParams = type.Ty.TypeParams.Value.Length == 0 ? null : type.Ty.TypeParams.Value.Select(p =>
//                    {
//                        return new NodeTypeParam()
//                        {
//                            Name = p.TypeParameterName.Value,
//                            TypeId = p.TypeParameterType.Value?.Value
//                        };
//                    }).ToArray();
//                    var typeDefValue = type.Ty.TypeDef.Value;
//                    var docs = type.Ty.Docs == null || type.Ty.Docs.Value.Length == 0 ? null : type.Ty.Docs.Value.Select(p => p.Value).ToArray();

//                    NodeType nodeType = null;
//                    switch (typeDefValue)
//                    {
//                        case TypeDefEnum.Composite:
//                            {
//                                var typeDef = type.Ty.TypeDef.Value2 as TypeDefComposite;
//                                nodeType = new NodeTypeComposite()
//                                {
//                                    Id = type.Id.Value,
//                                    Path = path,
//                                    TypeParams = typeParams,
//                                    TypeDef = typeDefValue,
//                                    TypeFields = typeDef.Fields.Value.Length == 0 ? null : typeDef.Fields.Value.Select(p =>
//                                    {
//                                        var fDocs = p.Docs == null || p.Docs.Value.Length == 0 ? null : p.Docs.Value.Select(q => q.Value).ToArray();
//                                        return new NodeTypeField()
//                                        {
//                                            Name = p.FieldName.Value?.Value,
//                                            TypeName = p.FieldTypeName.Value?.Value,
//                                            TypeId = p.FieldTy.Value,
//                                            Docs = fDocs
//                                        };
//                                    }).ToArray(),
//                                    Docs = docs
//                                };
//                            }
//                            break;

//                        case TypeDefEnum.Variant:
//                            {
//                                var typeDef = type.Ty.TypeDef.Value2 as TypeDefVariant;
//                                nodeType = new NodeTypeVariant()
//                                {
//                                    Id = type.Id.Value,
//                                    Path = path,
//                                    TypeParams = typeParams,
//                                    TypeDef = typeDefValue,
//                                    Variants = typeDef.TypeParam.Value.Length == 0 ? null : typeDef.TypeParam.Value.Select(p =>
//                                    {
//                                        var vDocs = p.Docs == null || p.Docs.Value.Length == 0 ? null : p.Docs.Value.Select(q => q.Value).ToArray();
//                                        return new TypeVariant()
//                                        {
//                                            Name = p.VariantName.Value,
//                                            TypeFields = p.VariantFields.Value.Length == 0 ? null : p.VariantFields.Value.Select(q =>
//                                            {
//                                                var fDocs = q.Docs == null || q.Docs.Value.Length == 0 ? null : q.Docs.Value.Select(r => r.Value).ToArray();
//                                                return new NodeTypeField()
//                                                {
//                                                    Name = q.FieldName.Value?.Value,
//                                                    TypeName = q.FieldTypeName.Value?.Value,
//                                                    TypeId = q.FieldTy.Value,
//                                                    Docs = fDocs
//                                                };
//                                            }).ToArray(),
//                                            Index = p.Index.Value,
//                                            Docs = vDocs
//                                        };
//                                    }).ToArray(),
//                                    Docs = docs
//                                };
//                            }
//                            break;

//                        case TypeDefEnum.Sequence:
//                            {
//                                var typeDef = type.Ty.TypeDef.Value2 as TypeDefSequence;
//                                nodeType = new NodeTypeSequence()
//                                {
//                                    Id = type.Id.Value,
//                                    Path = path,
//                                    TypeParams = typeParams,
//                                    TypeDef = typeDefValue,
//                                    TypeId = typeDef.TypeParam.Value,
//                                    Docs = docs
//                                };
//                            }
//                            break;

//                        case TypeDefEnum.Array:
//                            {
//                                var typeDef = type.Ty.TypeDef.Value2 as TypeDefArray;
//                                nodeType = new NodeTypeArray()
//                                {
//                                    Id = type.Id.Value,
//                                    Path = path,
//                                    TypeParams = typeParams,
//                                    TypeDef = typeDefValue,
//                                    TypeId = typeDef.TypeParam.Value,
//                                    Length = typeDef.Len.Value,
//                                    Docs = docs
//                                };
//                            }
//                            break;

//                        case TypeDefEnum.Tuple:
//                            {
//                                var typeDef = type.Ty.TypeDef.Value2 as TypeDefTuple;
//                                nodeType = new NodeTypeTuple()
//                                {
//                                    Id = type.Id.Value,
//                                    Path = path,
//                                    TypeParams = typeParams,
//                                    TypeDef = typeDefValue,
//                                    TypeIds = typeDef.Fields.Value.Select(p => (uint)p.Value).ToArray(),
//                                    Docs = docs
//                                };
//                            }
//                            break;

//                        case TypeDefEnum.Primitive:
//                            {
//                                var typeDef = (TypeDefPrimitive)Enum.Parse(typeof(TypeDefPrimitive), type.Ty.TypeDef.Value2.ToString());
//                                nodeType = new NodeTypePrimitive()
//                                {
//                                    Id = type.Id.Value,
//                                    Path = path,
//                                    TypeParams = typeParams,
//                                    TypeDef = typeDefValue,
//                                    Primitive = typeDef,
//                                    Docs = docs
//                                };
//                            }
//                            break;

//                        case TypeDefEnum.Compact:
//                            {
//                                var typeDef = type.Ty.TypeDef.Value2 as TypeDefCompact;
//                                nodeType = new NodeTypeCompact()
//                                {
//                                    Id = type.Id.Value,
//                                    Path = path,
//                                    TypeParams = typeParams,
//                                    TypeDef = typeDefValue,
//                                    TypeId = typeDef.TypeParam.Value,
//                                    Docs = docs
//                                };
//                            }
//                            break;

//                        case TypeDefEnum.BitSequence:
//                            {
//                                var typeDef = type.Ty.TypeDef.Value2 as TypeDefBitSequence;
//                                nodeType = new NodeTypeBitSequence()
//                                {
//                                    Id = type.Id.Value,
//                                    Path = path,
//                                    TypeParams = typeParams,
//                                    TypeDef = typeDefValue,
//                                    TypeIdOrder = typeDef.BitOrderType.Value,
//                                    TypeIdStore = typeDef.BitStoreType.Value,
//                                    Docs = docs
//                                };
//                            }
//                            break;
//                    }

//                    if (nodeType != null)
//                    {
//                        result.Add(nodeType.Id, nodeType);
//                    }
//                }

//                return result;
//            }

//            /// <summary>
//            /// Create Module Dictionary
//            /// </summary>
//            /// <param name="modules"></param>
//            /// <returns></returns>
//            /// <exception cref="NotImplementedException"></exception>
//            static Dictionary<uint, PalletModule> CreateModuleDict(PalletMetadata[] modules)
//            {
//                var result = new Dictionary<uint, PalletModule>();

//                foreach (var module in modules)
//                {
//                    var palletModule = new PalletModule()
//                    {
//                        Name = module.PalletName.Value,
//                        Index = module.Index.Value,
//                    };
//                    result.Add(module.Index.Value, palletModule);

//                    if (module.PalletStorage.OptionFlag)
//                    {
//                        var storage = module.PalletStorage.Value;
//                        palletModule.Storage = new PalletStorage()
//                        {
//                            Prefix = storage.Prefix.Value,
//                        };

//                        palletModule.Storage.Entries = new Entry[storage.Entries.Value.Length];
//                        for (int i = 0; i < storage.Entries.Value.Length; i++)
//                        {
//                            var entry = storage.Entries.Value[i];
//                            palletModule.Storage.Entries[i] = new Entry()
//                            {
//                                Name = entry.StorageName.Value,
//                                Modifier = entry.StorageModifier.Value,
//                                StorageType = entry.StorageType.Value,
//                                Default = entry.StorageDefault.Value.Select(p => p.Value).ToArray(),
//                                Docs = entry.Documentation.Value.Select(p => p.Value).ToArray(),
//                            };

//                            switch (entry.StorageType.Value)
//                            {
//                                case Storage.Type.Plain:
//                                    palletModule.Storage.Entries[i].TypeMap = (((TType)entry.StorageType.Value2).Value, null);
//                                    break;

//                                case Storage.Type.Map:
//                                    var typeMap = ((StorageEntryTypeMap)entry.StorageType.Value2);
//                                    palletModule.Storage.Entries[i].TypeMap = (0, new TypeMap()
//                                    {
//                                        Hashers = typeMap.Hashers.Value.Select(p => p.Value).ToArray(),
//                                        Key = (uint)typeMap.Key.Value,
//                                        Value = (uint)typeMap.Value.Value
//                                    });
//                                    break;

//                                default:
//                                    throw new NotImplementedException();
//                            }
//                        }
//                    }

//                    if (module.PalletCalls.OptionFlag)
//                    {
//                        var calls = module.PalletCalls.Value;
//                        palletModule.Calls = new PalletCalls()
//                        {
//                            TypeId = (uint)calls.CallType.Value
//                        };
//                    }

//                    if (module.PalletEvents.OptionFlag)
//                    {
//                        var events = module.PalletEvents.Value;
//                        palletModule.Events = new PalletEvents()
//                        {
//                            TypeId = (uint)events.EventType.Value
//                        };
//                    }

//                    var constants = module.PalletConstants.Value;
//                    palletModule.Constants = new PalletConstant[constants.Length];
//                    for (int i = 0; i < constants.Length; i++)
//                    {
//                        PalletConstantMetadata constant = constants[i];
//                        palletModule.Constants[i] = new PalletConstant()
//                        {
//                            Name = constant.ConstantName.Value,
//                            TypeId = (uint)constant.ConstantType.Value,
//                            Value = constant.ConstantValue.Value.Select(p => p.Value).ToArray(),
//                            Docs = constant.Documentation.Value.Select(p => p.Value).ToArray()
//                        };
//                    }

//                    if (module.PalletErrors.OptionFlag)
//                    {
//                        var errors = module.PalletErrors.Value;
//                        palletModule.Errors = new PalletErrors()
//                        {
//                            TypeId = (uint)errors.ErrorType.Value
//                        };
//                    }
//                }

//                return result;
//            }

//            /// <summary>
//            /// Create Extrinsic
//            /// </summary>
//            /// <param name="extrinsic"></param>
//            /// <returns></returns>
//            static ExtrinsicMetadata CreateExtrinsic(ExtrinsicMetadataStruct extrinsic)
//            {
//                return new ExtrinsicMetadata()
//                {
//                    TypeId = (uint)extrinsic.ExtrinsicType.Value,
//                    Version = (int)extrinsic.Version.Value,
//                    SignedExtensions = extrinsic.SignedExtensions.Value.Select(p => new SignedExtensionMetadata()
//                    {
//                        SignedIdentifier = p.SignedIdentifier.Value,
//                        SignedExtType = (uint)p.SignedExtType.Value,
//                        AddSignedExtType = (uint)p.AddSignedExtType.Value,
//                    }).ToArray()
//                };
//            }
//        }

//        private void buildFromV15(MetadataV15 v15)
//        {

//        }
//    }
//}
