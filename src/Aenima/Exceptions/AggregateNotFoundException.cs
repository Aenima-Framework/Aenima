using System;
using System.Runtime.Serialization;
using Aenima.System.Extensions;

namespace Aenima.Exceptions
{
    [Serializable]
    public class AggregateNotFoundException<TAggregate> : Exception where TAggregate : IAggregate
    {
        private const string ErrorMessageTemplate = "Expected {0} '{1}', but found nothing!";
        public readonly string AggregateId;
        public readonly Type AggregateType;

        public AggregateNotFoundException(
            string aggregateId,
            StreamNotFoundException innerException)
            : base(ErrorMessageTemplate.FormatWith(
                typeof(TAggregate).Name,
                aggregateId),
                innerException)
        {
            this.AggregateId   = aggregateId;
            this.AggregateType = typeof(TAggregate);
        }

        protected AggregateNotFoundException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) {}
    }
}