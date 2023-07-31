using Substrate.NET.Metadata.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Compare.Base
{
    public interface IMetadataDiffBase<out T>
        where T : IMetadataDifferentialModules
    {
        public IEnumerable<T> AllModulesDiff { get; }

        public IEnumerable<T> UnchangedModules { get; }

        public IEnumerable<T> ChangedModules { get; }

        public IEnumerable<T> AddedModules { get; }

        public IEnumerable<T> RemovedModules { get; }
    }
}
