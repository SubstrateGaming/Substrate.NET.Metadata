using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Substrate.NetApi.Model.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types.Primitive;
using Substrate.NetApi.Model.Types.Base;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    public class NodeType
    {
        public U32 Id { get; set; }

        public Base.Portable.Path Path { get; set; }

        public NodeTypeParam[] TypeParams { get; set; } = new NodeTypeParam[0];

        public TypeDefEnum TypeDef { get; set; }

        public BaseVec<Str> Docs { get; set; }
    }

    public class NodeTypeParam
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint? TypeId { get; set; }
    }

    public class NodeTypePrimitive : NodeType
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TypeDefPrimitive Primitive { get; set; }
    }

    public class NodeTypeComposite : NodeType
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public NodeTypeField[] TypeFields { get; set; }
    }

    public class NodeTypeField
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TypeName { get; set; }

        public uint TypeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Docs { get; set; }
    }

    public class NodeTypeArray : NodeType
    {
        public uint Length { get; set; }

        public uint TypeId { get; set; }
    }

    public class NodeTypeSequence : NodeType
    {
        public uint TypeId { get; set; }
    }

    public class NodeTypeCompact : NodeType
    {
        public uint TypeId { get; set; }
    }

    public class NodeTypeTuple : NodeType
    {
        public uint[] TypeIds { get; set; }
    }

    public class NodeTypeBitSequence : NodeType
    {
        public uint TypeIdStore { get; set; }
        public uint TypeIdOrder { get; set; }
    }

    public class NodeTypeVariant : NodeType
    {
        public TypeVariant[] Variants { get; set; }
    }

    public class TypeVariant
    {
        public string Name { get; set; }
        public NodeTypeField[] TypeFields { get; set; }

        public int Index { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Docs { get; set; }
    }

    public class NodeMetadataV14
    {
        public Dictionary<uint, NodeType> Types { get; set; }
        public Dictionary<uint, PalletModule> Modules { get; set; }
        public ExtrinsicMetadata Extrinsic { get; set; }
        public uint TypeId { get; set; }
    }

    public class SignedExtensionMetadata
    {
        public string SignedIdentifier { get; set; }
        public uint SignedExtType { get; set; }
        public uint AddSignedExtType { get; set; }
    }

    public class ExtrinsicMetadata
    {
        public uint TypeId { get; set; }
        public int Version { get; set; }
        public SignedExtensionMetadata[] SignedExtensions { get; set; }
    }

    public class PalletConstant
    {
        public string Name { get; set; }
        public uint TypeId { get; set; }
        public byte[] Value { get; set; }
        public string[] Docs { get; set; }
    }

    public class PalletStorage
    {
        public string Prefix { get; set; }
        public Entry[] Entries { get; set; }
    }

    public class Entry
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Storage.Modifier Modifier { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Storage.Type StorageType { get; set; }

        public (uint, TypeMap) TypeMap { get; set; }
        public byte[] Default { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Docs { get; set; }
    }

    public class TypeMap
    {
        [JsonProperty("Hashers", ItemConverterType = typeof(StringEnumConverter))]
        public Storage.Hasher[] Hashers { get; set; }

        public uint Key { get; set; }
        public uint Value { get; set; }
    }

    public class PalletModule
    {
        public string Name { get; set; }
        public PalletStorage Storage { get; set; }
        public PalletCalls Calls { get; set; }
        public PalletEvents Events { get; set; }
        public PalletConstant[] Constants { get; set; }
        public PalletErrors Errors { get; set; }
        public uint Index { get; set; }
    }

    public class PalletCalls
    {
        public uint TypeId { get; set; }
    }

    public class PalletEvents
    {
        public uint TypeId { get; set; }
    }

    public class PalletErrors
    {
        public uint TypeId { get; set; }
    }
}
