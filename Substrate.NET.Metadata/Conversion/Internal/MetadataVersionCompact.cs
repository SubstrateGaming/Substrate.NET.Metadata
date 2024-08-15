using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    internal class MetadataVersionCompact
    {
        public MetadataVersionCompact(int majorVersion, int minorVersion, string blockHash)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            BlockHash = blockHash;
        }

        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public string BlockHash { get; set; }

        public List<StorageClass> StorageClasses { get; set; } = new List<StorageClass>();

        public List<string> Flatten()
        {
            var res = new List<string>();

            foreach (var storageClass in StorageClasses)
            {
                res.Add($"{MajorVersion}|{MinorVersion}|{storageClass.ToString()}");
            }

            return res;
        }

        public void AddOrInc(Dictionary<string, int> uniqueName, string name)
        {
            if (uniqueName.ContainsKey(name))
            {
                uniqueName[name] += 1;
            }
            else
            {
                uniqueName[name] = 1;
            }
        }

        public void GetDistinctStorageClasses(Dictionary<string, int> uniqueName)
        {
            var distinctClasses = StorageClasses.Select(x => x.ClassNameMapped).Distinct().ToList();

            for (int i = 0; i < distinctClasses.Count; i++)
            {
                var classes = ConversionBuilder.HarmonizeFullType(distinctClasses[i]);

                foreach (var c in classes)
                {
                    AddOrInc(uniqueName, c);
                }
            }
        }

        public override string ToString() => $"{MajorVersion}.{MinorVersion}.{BlockHash}";
    }
}
