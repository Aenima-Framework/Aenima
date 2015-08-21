using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Aenima.EventStore;
using Aenima.Jil;
using Aenima.JsonNet;
using Aenima.Logging;
using Aenima.ServiceStack;
using Aenima.System;
using Aenima.System.Extensions;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace Aenima.Dapper.Tests
{
    [TestFixture]
    public class DapperEventStoreTests
    {
        private static readonly AutoFake AutoFake = new AutoFake();
        private static readonly Fixture AutoFixture = new DefaultFakeItEasyFixture();

        private static readonly string ConnectionString =
            @"Data Source=.;
            Initial Catalog=Aenima;
            Integrated Security=True;
            MultipleActiveResultSets=True;";

        [SetUp]
        public void SetUp()
        {
            AutoFake.Provide(new DapperEventStoreSettings(ConnectionString));
            //AutoFake.Provide<ISerializer>(new JsonNetSerializer());
            //AutoFake.Provide<ISerializer>(new JilSerializer());
            AutoFake.Provide<ISerializer>(new ServiceStackSerializer());
            AutoFake.Provide<IEventDispatcher>(new NullEventDispatcher());

            Log.Customize(type => new Fake<ILog>().FakedObject);
        }

        private async Task Transactionally(Func<Task> action)
        {
            using(var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled)) {
                await action();
            }
        }

        [Test]
        public async Task AppendStream_CreatesStreamAndAppendsEvents_WhenStreamDoesNotExist()
        {
            // arrange
            var streamId = AutoFixture.Create<string>();

            var expectedStreamEvents = AutoFixture
                .Build<StreamEvent>()
                .FromFactory(() => new StreamEvent(
                    AutoFixture.Create<TestEventOne>(), 
                    new Dictionary<string, string> { { "Testing", "true" } }))
                .CreateMany(1)
                .ToList();

            var eventVersion     = 0;
            var fallbackCommitId = Guid.NewGuid();

            foreach(var se in expectedStreamEvents) {
                se.EnrichMetadata(streamId, eventVersion++, fallbackCommitId);
            }

            var sut = AutoFake.Resolve<DapperEventStore>();

            await sut.Initialize();

            // act
            await sut.AppendStream(streamId, -1, expectedStreamEvents);
            // assert
            var result = await sut.ReadStream(streamId, 0, expectedStreamEvents.Count);

            result.Events.ShouldBeEquivalentTo(expectedStreamEvents);

            //await Transactionally(async () => {
            //    // act
            //    await sut.AppendStream(streamId, -1, expectedStreamEvents);
            //    // assert
            //    var result = await sut.ReadStream(streamId, 0, expectedStreamEvents.Count);

            //    result.Events.ShouldBeEquivalentTo(expectedStreamEvents);
            //});
        }

        //[TestCase(0, 3)]
        [TestCase(0, 10)]
        public async Task ReadStream_ReturnsEvents(int fromVersion, int eventCount)
        {
            //// arrange
            //var streamId = AutoFixture.Create<string>();
            //var events   = new List<StreamEvent>();

            //for(var i = 0; i < eventCount + fromVersion; i++) {
            //    events.Add(new StreamEvent(
            //        AutoFixture.Create<Guid>(),
            //        AutoFixture.Create<string>(),
            //        AutoFixture.Create<string>(),
            //        AutoFixture.Create<string>(),
            //        AutoFixture.Create<DateTime>(),
            //        streamId,
            //        i));
            //}

            //var expectedPage = AutoFixture
            //    .Build<StreamEventsPage>()
            //    .With(page => page.FromVersion, 0)
            //    .With(page => page.NextVersion, -1)
            //    .With(page => page.LastVersion, eventCount-1)
            //    .With(page => page.IsEndOfStream, true)
            //    .With(page => page.Events, events.AsReadOnly())
            //    .Create();

            //var sut = new DapperEventStore(new DapperEventStoreSettings(ConnectionString));

            //await sut.AppendStream(
            //    expectedPage.StreamId, 
            //    -1, 
            //    expectedPage.Events.Select(se => new NewStreamEvent(se.Id, se.Type, se.Data, se.Metadata)).ToList());

            //// act
            //var result = await sut.ReadStream(expectedPage.StreamId, expectedPage.FromVersion, eventCount);

            //// assert
            //result.ShouldBeEquivalentTo(expectedPage);
        }

        [Serializable]
        public class TestEventOne : IEvent
        {
            public int SortOrder { get; set; }

            public TestEventTwo Kebas { get; set; }
        }

        [Serializable]
        public class TestEventTwo : IEvent
        {
            public TestEventTwo(string serialKey, DateTime anotherProperty)
            {
                SerialKey = serialKey;
                AnotherProperty = anotherProperty;
            }

            public string SerialKey { get; private set; }
            public DateTime AnotherProperty { get; private set; }
        }


        //[TestCase(1, 1000)]
        //[TestCase(50, 10)]
        //public async Task Load_ReturnsFullEventStream_WhenVersionIsZero(int expectedVersion, int eventsPerVersion)
        //{
        //    // arrange
        //    var sut = new DapperEventStore(
        //        new DapperEventStoreSettings(ConnectionString));

        //    await sut.Initialize();

        //    var events =
        //        from version in Enumerable.Range(expectedVersion, eventsPerVersion * expectedVersion)
        //        select new TestEventOne { SortOrder = version } as IDomainEvent;

        //    var expectedEventStream = EventStream.Create(
        //        sourceId: AutoFixture.Create<string>(),
        //        sourceType: typeof(TestId),
        //        version: expectedVersion,
        //        events: events.ToList());

        //    for(var v = 0; v < expectedVersion; v++)
        //    {
        //        await sut.Append(
        //            expectedEventStream.SourceId,
        //            typeof(TestId),
        //            v,
        //            expectedEventStream.Events
        //                .Skip(v * eventsPerVersion)
        //                .Take(eventsPerVersion)
        //                .ToList());
        //    }

        //    // act
        //    var result = await sut.Load<TestId>(
        //        expectedEventStream.SourceId,
        //        0,
        //        long.MaxValue);

        //    // assert
        //    result.ShouldBeEquivalentTo(expectedEventStream);
        //}

        ////[TestCase(1, 1, 250)]
        ////[TestCase(500, 3, 400)]
        ////[TestCase(2000, 50, 500)]
        ////[TestCase(4000, 100, 750)]
        ////public async Task Load_ReturnsFullEventStream_FastEnough(int expectedVersion, int eventsPerVersion, int maxDuration)
        ////{
        ////    // arrange
        ////    var sut = new DapperEventStore(
        ////        new JsonNetSerializer(),
        ////        new DapperEventStoreSettings(ConnectionString, "EventStream_" + DateTime.UtcNow.ToFileTime()));

        ////    await sut.Initialize();

        ////    var eventsQuery =
        ////        from version in Enumerable.Range(expectedVersion, eventsPerVersion * expectedVersion)
        ////        select new TestEventOne { SortOrder = version } as IDomainEvent;

        ////    var expectedEventStream = EventStream.Create(
        ////        sourceId: AutoFixture.Create<string>(),
        ////        sourceType: typeof(TestId),
        ////        version: expectedVersion,
        ////        events: eventsQuery.ToList());

        ////    for(var v = 0; v < expectedVersion; v++)
        ////    {
        ////        await sut.Append(
        ////            expectedEventStream.SourceId,
        ////            typeof(TestId),
        ////            v,
        ////            expectedEventStream.Events
        ////                .Skip(v * eventsPerVersion)
        ////                .Take(eventsPerVersion)
        ////                .ToList());
        ////    }

        ////    // act & assert
        ////    sut.ExecutionTimeOf(store => store
        ////        .Load<TestId>(expectedEventStream.SourceId, 0, long.MaxValue).Wait())
        ////        .ShouldNotExceed(maxDuration.Milliseconds());
        ////}

        //[TestCase(true, TestName = "Using Binary Serializer.")]
        //[TestCase(false, TestName = "Using JsonNet Serializer.")]
        //public async Task Append_InsertsStreamRecord_BySerializerType(
        //{
        //    // arrange
        //    var expectedEventStream = EventStream.Create(
        //        sourceId: AutoFixture.Create<string>(),
        //        sourceType: typeof(TestId),
        //        version: 1,
        //        events: AutoFixture
        //            .CreateMany<TestEventOne>(2)
        //            .Cast<IDomainEvent>()
        //            .Concat(AutoFixture.CreateMany<TestEventTwo>(2))
        //            .ToList());

        //    var serializer = binary
        //        ? new BinarySerializer() as ISerializer
        //        : new JsonNetSerializer();

        //    var tableName = "EventStream_" + serializer.GetType().Name;

        //    var sut = new DapperEventStore(
        //        serializer,
        //        new DapperEventStoreSettings(ConnectionString, tableName));

        //    await sut.Initialize();

        //    // act
        //    await sut.Append<TestId>(expectedEventStream.SourceId, expectedEventStream.Revision - 1, expectedEventStream.Events);

        //    // assert
        //    var result = await sut.Load<TestId>(expectedEventStream.SourceId, expectedEventStream.Revision - 1, long.MaxValue);

        //    result.ShouldBeEquivalentTo(expectedEventStream);

        //    //var temp = await sut.LoadAll(
        //    //    global::System.Data.SqlTypes.SqlDateTime.MinValue.Value, 
        //    //    global::System.Data.SqlTypes.SqlDateTime.MaxValue.Value, typeof(TestId));

        //}

        //[Test]
        //public async Task Append_ThrowsOptimisticConcurrencyException_WhenEventStreamVersionAlreadyExists()
        //{
        //    // arrange
        //    var sut = new DapperEventStore(
        //        new JsonNetSerializer(),
        //        new DapperEventStoreSettings(ConnectionString));

        //    await sut.Initialize();

        //    var version = 0L;
        //    var sourceId = AutoFixture.Create<string>();
        //    var events = AutoFixture
        //        .CreateMany<TestEventOne>()
        //        .Cast<IDomainEvent>()
        //        .ToList();

        //    Func<Task> appendFunc = () => sut.Append(sourceId, typeof(TestId), version, events);

        //    await appendFunc();

        //    // act & assert
        //    var ex = appendFunc.ShouldThrow<OptimisticConcurrencyException>().Which;
        //    ex.ExpectedVersion.ShouldBeEquivalentTo(version);
        //    ex.ActualVersion.ShouldBeEquivalentTo(version + 1);
        //    ex.StreamId.ShouldBeEquivalentTo(sourceId);
        //    ex.ActualEvents.ShouldBeEquivalentTo(events);
        //}


        ////[Serializable]
        ////[DataContract]
        ////[KnownType(typeof(TestEventOne))]
        //public class TestEventOne : IEvent
        //{
        //    //[DataMember]
        //    public int SortOrder { get; set; }
        //}

        ////[Serializable]
        ////[DataContract]
        ////[KnownType(typeof(TestEventTwo))]
        //public class TestEventTwo : IEvent
        //{
        //    //[DataMember]
        //    public string Key { get; set; }
        //    //[DataMember]
        //    public DateTime AnotherProperty { get; set; }
        //}
    }
}

