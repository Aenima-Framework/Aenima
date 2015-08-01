using System;
using System.Runtime.Serialization;
using Aenima.System.Extensions;

namespace Aenima.Exceptions
{
    [Serializable]
    public class StreamDeletedException : Exception
    {
        private const string ErrorMessageTemplate
            = "Expected stream '{0}' v{1}, but it's deleted! Append new events to undelete it.";

        public readonly string StreamId;
        public readonly int Version;

        public StreamDeletedException(string streamId, int version)
            : base(ErrorMessageTemplate.FormatWith(streamId, version))
        {
            StreamId = streamId;
            Version  = version;
        }

        protected StreamDeletedException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}