using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aenima.EventStore;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Aenima.JsonNet.Tests
{
    [TestFixture]
    public class DomainEventSerializerTests
    {
        [Test]
        public void ReturnsValidNewStreamEvent_WithoutExtraHeaders()
        {
            // arrange
            var sut = new JsonNetEventSerializer();
            
            var domainEvent = new SomethingHappened { SomeoneDidIt = Guid.NewGuid().ToString() } as IEvent;

            domainEvent.SetMetadata(
                id              : Guid.NewGuid(), 
                raisedOn        : DateTime.UtcNow, 
                aggregateId     : Guid.NewGuid().ToString(), 
                aggregateVersion: 0);

            var eventHeaders = new Dictionary<string, object>()
            {
                { "Id"                , domainEvent.Id },
                { "AggregateId"       , domainEvent.AggregateId },
                { "AggregateVersion"  , domainEvent.AggregateVersion },
                { "RaisedOn"          , domainEvent.RaisedOn },
                { "ProcessId"         , domainEvent.ProcessId },
                { "DomainEventClrType", domainEvent.GetType().AssemblyQualifiedName },
            };

            var expectedResult = new NewStreamEvent(
                domainEvent.Id,
                "SomethingHappened",
                JsonConvert.SerializeObject(domainEvent, JsonNetEventSerializer.ToNewStreamEventSerializerSettings),
                JsonConvert.SerializeObject(eventHeaders, JsonNetEventSerializer.ToNewStreamEventSerializerSettings));

            // act
            var result = sut.ToNewStreamEvent(domainEvent);

            // assert
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void ReturnsValidNewStreamEvent_WithExtraHeaders()
        {
            // arrange
            var sut = new JsonNetEventSerializer();

            var domainEvent = new SomethingHappened { SomeoneDidIt = Guid.NewGuid().ToString() } as IDomainEvent;

            domainEvent.SetMetadata(
                id              : Guid.NewGuid(),
                raisedOn        : DateTime.UtcNow,
                aggregateId     : Guid.NewGuid().ToString(),
                aggregateVersion: 0);

            var extraHeaders = new Dictionary<string, object>()
            {
                { "CertainInfo", "Who cares?" },
                { "MoarInfo"   , "No one..."},
            };

            var eventHeaders = new Dictionary<string, object>()
            {
                { "Id"                , domainEvent.Id },
                { "AggregateId"       , domainEvent.AggregateId },
                { "AggregateVersion"  , domainEvent.AggregateVersion },
                { "RaisedOn"          , domainEvent.RaisedOn },
                { "ProcessId"         , domainEvent.ProcessId },
                { "DomainEventClrType", domainEvent.GetType().AssemblyQualifiedName },
                { "CertainInfo"       , "Who cares?" },
                { "MoarInfo"          , "No one..."},
            };

            var expectedResult = new NewStreamEvent(
                domainEvent.Id,
                "SomethingHappened",
                JsonConvert.SerializeObject(domainEvent, JsonNetEventSerializer.ToNewStreamEventSerializerSettings),
                JsonConvert.SerializeObject(eventHeaders, JsonNetEventSerializer.ToNewStreamEventSerializerSettings));

            // act
            var result = sut.ToNewStreamEvent(domainEvent, extraHeaders);

            // assert
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void ReturnsValidDomainEvent()
        {
            // arrange
            var sut = new JsonNetEventSerializer();

            var expectedResult = new SomethingHappened { SomeoneDidIt = Guid.NewGuid().ToString() } as IDomainEvent;

            expectedResult.SetMetadata(
                id              : Guid.NewGuid(),
                raisedOn        : DateTime.UtcNow,
                aggregateId     : Guid.NewGuid().ToString(),
                aggregateVersion: 5);

            var eventHeaders = new Dictionary<string, object>()
            {
                { "Id"                , expectedResult.Id },
                { "AggregateId"       , expectedResult.AggregateId },
                { "AggregateVersion"  , expectedResult.AggregateVersion },
                { "RaisedOn"          , expectedResult.RaisedOn },
                { "ProcessId"         , expectedResult.ProcessId },
                { "DomainEventClrType", expectedResult.GetType().AssemblyQualifiedName },
            };

            var newStreamEvent = new StreamEvent(
                id: expectedResult.Id,
                type: "SomethingHappened", 
                data: JsonConvert.SerializeObject(expectedResult, JsonNetEventSerializer.Settings), 
                metadata: JsonConvert.SerializeObject(eventHeaders, JsonNetEventSerializer.Settings), 
                storedOn: DateTime.MinValue,
                streamId: expectedResult.AggregateId, 
                streamVersion: expectedResult.AggregateVersion);

            // act
            var result = sut.FromStreamEvent(newStreamEvent);

            // assert
            result.ShouldBeEquivalentTo((SomethingHappened)expectedResult);
        }

        public class SomethingHappened : DomainEvent
        {
            public string SomeoneDidIt { get; set; }
        }
    }
}
