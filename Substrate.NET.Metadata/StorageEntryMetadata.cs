using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Substrate.NetApi.Model.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata
{
    /// <summary>
    /// Based on <see cref="Substrate.NetApi.Model.Meta.Entry"/>
    /// </summary>
    public abstract class StorageEntryMetadata
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
}
