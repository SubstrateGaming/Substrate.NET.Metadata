using Substrate.NET.Metadata.Conversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Tests.Conversion
{
    internal class ConversionUtilsTests
    {
        [Test]
        public void EndsWithArray_ShouldReturnTrue()
        {
            var array1 = new[] { 1, 2, 3, 4, 5 };
            var array2 = new[] { 4, 5 };

            var result = ConversionUtils.EndsWithArray(array1, array2);

            Assert.That(result);
        }

        [Test]
        public void EndsWithArray_ShouldReturnFalse()
        {
            var array1 = new[] { 1, 2, 3, 4, 5 };
            var array2 = new[] { 4, 6 };

            var result = ConversionUtils.EndsWithArray(array1, array2);

            Assert.That(result, Is.False);
        }

        [Test]
        public void AreArraysIdentical_WhenEquals_ShouldReturnTrue()
        {
            var array1 = new[] { 1, 2, 3, 4, 5 };
            var array2 = new[] { 1, 2, 3, 4, 5 };

            var result = ConversionUtils.AreArraysIdentical(array1, array2);

            Assert.That(result);
        }

        [Test]
        public void AreArraysIdentical_WhenNotSameSize_ShouldReturnFalse()
        {
            var array1 = new[] { 1, 2, 3, 4, 5 };
            var array2 = new[] { 1, 2, 3, 4 };

            var result = ConversionUtils.AreArraysIdentical(array1, array2);

            Assert.That(result, Is.False);
        }

        [Test]
        public void AreArraysIdentical_WhenDifferent_ShouldReturnFalse()
        {
            var array1 = new[] { 1, 2, 3, 4, 5 };
            var array2 = new[] { 1, 2, 3, 4, 6 };

            var result = ConversionUtils.AreArraysIdentical(array1, array2);

            Assert.That(result, Is.False);
        }
    }
}
