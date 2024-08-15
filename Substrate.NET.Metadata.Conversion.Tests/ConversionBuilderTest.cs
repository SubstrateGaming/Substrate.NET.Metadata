using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Tests
{
    internal class ConversionBuilderTest
    {
        [Test]
        [TestCase("Option<Vec<u8>>")]
        public void BuildTypeDictionnary_FromPrimitive_ShouldSucceed(string primitiveType)
        {

        }

        [Test]
        [TestCase("Option<Vec<u8>>")]
        public void BuildTypeDictionnary_FromTuple_ShouldSucceed(string primitiveType)
        {

        }
    }
}
