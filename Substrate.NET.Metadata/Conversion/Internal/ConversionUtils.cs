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

        internal static bool IsNone(this Variant variant)
        {
            return variant.Name.Value.Equals("None", StringComparison.CurrentCultureIgnoreCase) && variant.VariantFields.Value.Length == 0;
        }

        internal static bool IsSome(this Variant variant, int index)
        {
            return variant.VariantFields.Value.Length == 1 && (int)variant.VariantFields.Value[0].FieldTy.Value == index;
        }
    }
}
