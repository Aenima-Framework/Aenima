using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Aenima.EventStore;
using Aenima.System.Extensions;

namespace Aenima.Exceptions
{
    [Serializable]
    public class StreamConcurrencyException : Exception
    {
        private const string ErrorMessageTemplate 
            = "Expected stream '{0}' v{1}, but found v{2}! Actual stream is {3} versions {4}.";

        public readonly string StreamId;
        public readonly int ExpectedVersion;
        public readonly int ActualVersion;
        public readonly IReadOnlyCollection<StreamEvent> DeltaEvents;

        public StreamConcurrencyException(
            string streamId,
            int expectedVersion,
            int actualVersion,
            IReadOnlyCollection<StreamEvent> deltaEvents = null)
            : base(GenerateMessage(streamId, expectedVersion, actualVersion))
        {
            this.StreamId        = streamId;
            this.ExpectedVersion = expectedVersion;
            this.ActualVersion   = actualVersion;
            this.DeltaEvents     = deltaEvents;
        }

        protected StreamConcurrencyException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        { }

        private static string GenerateMessage(
            string streamId,
            int expectedVersion,
            int actualVersion)
        {
            var deltaVersion = actualVersion - expectedVersion;

            return ErrorMessageTemplate.FormatWith(
                streamId,
                expectedVersion,
                actualVersion,
                Math.Abs(deltaVersion),
                deltaVersion > 0 ? "ahead" : "behind");
        }
    }
}