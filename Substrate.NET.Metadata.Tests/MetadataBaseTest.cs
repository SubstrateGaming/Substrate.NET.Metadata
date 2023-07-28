using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Tests
{
    public abstract class MetadataBaseTest
    {
        protected string MockFiles = "\\Mocks\\";

        /// <summary>
        /// Read metadata from file
        /// </summary>
        /// <param name="metadataName">Metadata file name without extension</param>
        /// <returns></returns>
        protected string readMetadataFromFile(string metadataName)
        {
            return File.ReadAllText($"{AppContext.BaseDirectory}{MockFiles}{metadataName}.txt");
        }
    }
}
