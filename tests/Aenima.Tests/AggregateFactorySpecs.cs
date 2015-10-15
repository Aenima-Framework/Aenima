using System;
using FluentAssertions;
using Xunit;

namespace Aenima.Tests
{
    public class AggregateFactorySpecs
    {
        [Fact]
        public void Creates_aggregate_not_using_state_without_restoring_events()
        {
            // arrange
            var expected = new AggregateWithoutState();

            // act
            var aggregate = AggregateFactory.Create<AggregateWithoutState>();

            // assert
            aggregate.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Creates_aggregate_not_using_state_restoring_events()
        {
            // arrange
            var events = new object[]
            {
                new AggregateCreated(Guid.NewGuid().ToString(),"Nomm nomm nomm")
            };

            var expected = new AggregateWithoutState();
            ((IAggregate) expected).Restore(events);

            // act
            var aggregate = AggregateFactory.Create<AggregateWithoutState>(events);

            // assert
            aggregate.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Creates_aggregate_using_state_without_restoring_events()
        {
            // arrange
            var expected = new AggregateWithState();

            // act
            var aggregate = AggregateFactory.Create<AggregateWithState>();

            // assert
            aggregate.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public void Creates_aggregate_using_state_restoring_events()
        {
            // arrange
            var events = new object[]
            {
                new AggregateCreated(Guid.NewGuid().ToString(),"Nomm nomm nomm")
            };

            var expected = new AggregateWithState();
            ((IAggregate)expected).Restore(events);

            // act
            var aggregate = AggregateFactory.Create<AggregateWithState>(events);

            // assert
            aggregate.ShouldBeEquivalentTo(expected);
        }

        public class AggregateWithoutState : Aggregate
        {
            public string Name { get; private set; }

            public void Create(string id, string name)
            {
                Apply(new AggregateCreated(id, name));
            }

            public void When(AggregateCreated domainEvent)
            {
                Id   = domainEvent.Id;
                Name = domainEvent.Name;
            }
        }

        public class AggregateWithState : Aggregate<AggregateState>
        {
            public void Create(string id, string name)
            {
                Apply(new AggregateCreated(id, name));
            }
        }

        public class AggregateState : State
        {
            public string Name { get; private set; }

            public void When(AggregateCreated domainEvent)
            {
                Id   = domainEvent.Id;
                Name = domainEvent.Name;
            }
        }

        public class AggregateCreated
        {        
            public string Id { get; }

            public string Name { get; }

            public AggregateCreated(string id, string name)
            {
                Id   = id;
                Name = name;
            }
        }
    }
}
