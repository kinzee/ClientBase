using System;
using System.Runtime.Serialization;

namespace GFramework
{
    public class GFrameworkException : Exception
    {
        public GFrameworkException()
            : base()
        {
        }

        public GFrameworkException(string message)
            : base(message)
        {
        }

        public GFrameworkException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected GFrameworkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}