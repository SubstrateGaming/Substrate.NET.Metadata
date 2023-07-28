using System;

namespace Substrate.NET.Metadata.Exceptions
{
    public class MetadataException : Exception
    {
        public MetadataException()
        {
        }

        public MetadataException(string message)
            : base(message)
        {
        }

        public MetadataException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
