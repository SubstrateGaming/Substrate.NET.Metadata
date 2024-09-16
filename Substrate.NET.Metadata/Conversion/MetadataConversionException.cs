namespace Substrate.NET.Metadata.Conversion
{
    public class MetadataConversionException : Exception
    {
        public MetadataConversionException() { }

        public MetadataConversionException(string message)
            : base(message)
        {
        }

        public MetadataConversionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
