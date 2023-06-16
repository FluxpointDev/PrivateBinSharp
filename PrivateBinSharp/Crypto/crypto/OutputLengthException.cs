﻿using System.Runtime.Serialization;

namespace PrivateBinSharp.Crypto.crypto
{
    [Serializable]
    public class OutputLengthException
        : DataLengthException
    {
        public OutputLengthException()
            : base()
        {
        }

        public OutputLengthException(string message)
            : base(message)
        {
        }

        public OutputLengthException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected OutputLengthException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
