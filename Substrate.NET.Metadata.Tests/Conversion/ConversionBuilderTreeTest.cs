using Substrate.NET.Metadata.Base.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Tests.Conversion
{
    internal class ConversionBuilderTreeTest
    {
        private static PortableType[] InitPortableRegistry()
        {
            var portableRegistry = new PortableRegistry();
            portableRegistry.Create(new List<PortableType>().ToArray());
            var pt = portableRegistry.Value;
            Assert.That(pt.Length, Is.EqualTo(0));
            return pt;
        }

        [Test]
        public void Build_FromPrimitive_ShouldSucceed()
        {
            PortableType[] pt = InitPortableRegistry();

        }
    }
}
