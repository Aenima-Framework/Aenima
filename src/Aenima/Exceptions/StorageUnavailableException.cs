using System;
using System.Runtime.Serialization;

namespace Aenima.Exceptions
{
    /// <summary>
    ///     Indicates that the underlying persistence medium is unreachable, unavailable or off line.
    /// </summary>
    [Serializable]
    public class StorageUnavailableException : Exception
    {
        private const string ErrorMessage = "Failed to connect to the data store!";

        public StorageUnavailableException(Exception innerException)
            : base(ErrorMessage, innerException) {}

        protected StorageUnavailableException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
    }
}