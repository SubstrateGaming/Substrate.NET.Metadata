using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion.Internal
{
    internal class ConversionUtils
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
    }
}
