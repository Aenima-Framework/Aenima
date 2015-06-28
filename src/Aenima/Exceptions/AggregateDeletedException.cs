using System;
using System.Runtime.Serialization;
using Aenima.System.Extensions;

namespace Aenima.Exceptions
{
    [Serializable]
    public class AggregateDeletedException<TAggregate> : Exception where TAggregate : IAggregate
    {
        private const string ErrorMessageTemplate = "Expected {0} '{1}' v{2}, but it's deleted!";
        public readonly string AggregateId;
        public readonly int Version;
        public readonly Type AggregateType;

        public AggregateDeletedException(
            string aggregateId,
            int version,
            StreamDeletedException innerException)
            : base(ErrorMessageTemplate.FormatWith(
                typeof(TAggregate).Name,
                aggregateId,
                version),
                innerException)
        {
            this.AggregateId   = aggregateId;
            this.Version       = version;
            this.AggregateType = typeof(TAggregate);
        }

        protected AggregateDeletedException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}