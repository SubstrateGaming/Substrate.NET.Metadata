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
        /// <summary>
        /// List all pallets
        /// </summary>
        public IEnumerable<T> AllModulesDiff { get; }

        /// <summary>
        /// List all unchanged pallets
        /// </summary>
        public IEnumerable<T> UnchangedModules { get; }

        /// <summary>
        /// List all pallets which have modification. But do not track 'TypeId' modifications
        /// </summary>
        public IEnumerable<T> ChangedModules { get; }

        /// <summary>
        /// List added pallets
        /// </summary>
        public IEnumerable<T> AddedModules { get; }

        /// <summary>
        /// List removed pallets
        /// </summary>
        public IEnumerable<T> RemovedModules { get; }
    }
}
