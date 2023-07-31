using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Compare;
using Substrate.NET.Metadata.V10;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V12;
using Substrate.NET.Metadata.V13;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.V9;

namespace Substrate.NET.Metadata.Service
{
    public interface IMetadataService
    {
        /// <summary>
        /// Get major version from metadata
        /// </summary>
        /// <param name="hexMetadata"></param>
        /// <returns></returns>
        MetadataVersion GetMetadataVersion(string hexMetadata);

        /// <summary>
        /// Check if metadatas have same major version
        /// </summary>
        /// <param name="hexMetadata1"></param>
        /// <param name="hexMetadata2"></param>
        /// <returns></returns>
        MetadataVersion EnsureMetadataVersion(string hexMetadata1, string hexMetadata2);

        /// <summary>
        /// Compare V9 metadata
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        MetadataDiffV9 MetadataCompareV9(MetadataV9 m1, MetadataV9 m2);

        /// <summary>
        /// Compare V10 metadata
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        MetadataDiffV10 MetadataCompareV10(MetadataV10 m1, MetadataV10 m2);

        /// <summary>
        /// Compare V11 metadata
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        MetadataDiffV11 MetadataCompareV11(MetadataV11 m1, MetadataV11 m2);

        /// <summary>
        /// Compare V12 metadata
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        MetadataDiffV12 MetadataCompareV12(MetadataV12 m1, MetadataV12 m2);

        /// <summary>
        /// Compare V13 metadata
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        MetadataDiffV13 MetadataCompareV13(MetadataV13 m1, MetadataV13 m2);

        /// <summary>
        /// Compare V14 metadata
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        MetadataDiffV14 MetadataCompareV14(MetadataV14 m1, MetadataV14 m2);
    }
}