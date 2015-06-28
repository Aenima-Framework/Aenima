using System;
using System.Runtime.Serialization;
using Aenima.System.Extensions;

namespace Aenima.Exceptions
{
    [Serializable]
    public class AggregateConcurrencyException<TAggregate> : Exception where TAggregate : IAggregate
    {
        private const string ErrorMessageTemplate = "Expected {0} '{1}' v{2}, but found v{3}!";
        public readonly string AggregateId;
        public readonly int ExpectedVersion;
        public readonly int ActualVersion;
        public readonly Type AggregateType;

        public AggregateConcurrencyException(
            string aggregateId,
            int expectedVersion,
            int actualVersion)
            : base(ErrorMessageTemplate.FormatWith(
                typeof(TAggregate).Name,
                aggregateId,
                expectedVersion,
                actualVersion))
        {
            this.AggregateId     = aggregateId;
            this.ExpectedVersion = expectedVersion;
            this.ActualVersion   = actualVersion;
            this.AggregateType   = typeof(TAggregate);
        }

        protected AggregateConcurrencyException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}