using System;
using System.Runtime.Serialization;
using Aenima.System.Extensions;

namespace Aenima.Exceptions
{
    [Serializable]
    public class StreamNotFoundException : Exception
    {
        private const string ErrorMessageTemplate = "Expected stream '{0}', but found nothing!";
        public readonly string StreamId;

        public StreamNotFoundException(string streamId)
            : base(ErrorMessageTemplate.FormatWith(streamId))
        {
            StreamId = streamId;
        }

        protected StreamNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}