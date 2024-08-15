using Substrate.NET.Metadata.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Tests
{
    internal class TypeDefsTests
    {
        [Test]
        public void TType_FromUint_ShouldBeConverted()
        {
            var converted = TType.From(10u);

            Assert.That(converted, Is.Not.Null);
            Assert.That(converted.Value.Value, Is.EqualTo(new BigInteger(10)));
        }
    }
}
