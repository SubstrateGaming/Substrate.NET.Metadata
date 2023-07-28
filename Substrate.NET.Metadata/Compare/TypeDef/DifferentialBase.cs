using Substrate.NET.Metadata.Base;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Compare.TypeDef
{
    public class DifferentialBase<T>
        where T : IType, new()
    {
        public IEnumerable<(CompareStatus, T)> Elems { get; set; } = Enumerable.Empty<(CompareStatus, T)>();

        public bool HasChanges() => Elems.Any();
    }
}
