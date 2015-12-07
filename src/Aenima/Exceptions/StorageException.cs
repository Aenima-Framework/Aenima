using System;
using System.Runtime.Serialization;

namespace Aenima.Exceptions
{
    /// <summary>
    ///     Represents a general failure of the storage engine or persistence infrastructure.
    /// </summary>
    [Serializable]
    public class StorageException : Exception
    {
        private const string ErrorMessage = "Failed to execute operation!";

        public StorageException(Exception innerException)
            : base(ErrorMessage, innerException) {}

        protected StorageException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
    }
}