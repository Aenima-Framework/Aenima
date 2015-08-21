using System;
using System.Runtime.Serialization;
using Aenima.System.Extensions;

namespace Aenima.Exceptions
{
    [Serializable]
    public class StreamNotFoundException : Exception
    {
        public readonly string StreamId;

        public StreamNotFoundException(string streamId)
            : base($"Expected stream '{streamId}', but found nothing!")
        {
            StreamId = streamId;
        }

        protected StreamNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}