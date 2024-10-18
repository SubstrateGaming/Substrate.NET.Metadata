using Substrate.NET.Metadata.Base.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    /// <summary>
    /// This class is used to build a custom node from previous versions that does not fit the current v14 node.
    /// </summary>
    internal class CustomNodeBuilder
    {
        internal List<ICustomNodeBuilder> _customNodes = new List<ICustomNodeBuilder>();

        public void Add(CustomCompositeBuilder customCompositeBuilder)
        {
            _customNodes.Add(customCompositeBuilder);
        }

        public CustomCompositeBuilder CreateCustomComposite()
        {
            var composite = new CustomCompositeBuilder();

            _customNodes.Add(composite);
            return composite;
        }

        public IEnumerable<PortableType> Build(ConversionBuilder conversionBuilder)
        {
            var nodes = new List<PortableType>();

            foreach (var customNode in _customNodes)
            {
                nodes.Add(customNode.Build(conversionBuilder));
            }

            return nodes;
        }
    }
}
