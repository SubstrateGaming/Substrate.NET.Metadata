using Substrate.NET.Metadata.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Compare.Base
{
    public interface IMetadataDifferentialModules
    {
        public string ModuleName { get; }
        public CompareStatus CompareStatus { get; }
    }
}
