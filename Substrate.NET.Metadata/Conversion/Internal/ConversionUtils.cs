using Substrate.NET.Metadata.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    internal static class ConversionUtils
    {
        /// <summary>
        /// Check if the array1 ends with array2
        /// Example [1, 2, 3, 4, 5] ends with [4, 5] => true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        internal static bool EndsWithArray<T>(T[] array1, T[] array2)
        {
            if (array2.Length > array1.Length)
            {
                return false;
            }

            for (int i = 0; i < array2.Length; i++)
            {
                if (!array1[array1.Length - array2.Length + i].Equals(array2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if the array1 array2 are identical
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        internal static bool AreArraysIdentical<T>(T[] array1, T[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array2.Contains(array1[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Shortcut to check if the Option is None
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        internal static bool IsNone(this Variant variant)
        {
            return variant.Name.Value.Equals("None", StringComparison.CurrentCultureIgnoreCase) && variant.VariantFields.Value.Length == 0;
        }

        /// <summary>
        /// Shortcut to check if the Option is Some(index)
        /// </summary>
        /// <param name="variant"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static bool IsSome(this Variant variant, int index)
        {
            return variant.VariantFields.Value.Length == 1 && (int)variant.VariantFields.Value[0].FieldTy.Value == index;
        }
    }
}
