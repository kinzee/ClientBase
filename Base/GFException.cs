using System;
using System.Runtime.Serialization;

namespace GF
{
    public class GFException : Exception
    {
        public GFException()
            : base()
        {
        }

        public GFException(string message)
            : base(message)
        {
        }

        public GFException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GFException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}