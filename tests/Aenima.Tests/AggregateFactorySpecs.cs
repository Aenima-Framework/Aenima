using System;
using System.Threading;
using System.Threading.Tasks;
using Aenima.Autofac;
using Aenima.Data;
using Aenima.DependencyResolution;
using Autofac;
using Autofac.Extras.FakeItEasy;
using FakeItEasy;
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

    public class InProcQueryServiceSpecs
    {
        readonly AutoFake AutoFake = new AutoFake();

        [Fact]
        public async Task Runs_Query()
        {
            // arrange

            // create container builder
            var builder = new ContainerBuilder();

            builder
               .RegisterType<AutofacDependencyResolver>()
               .As<IDependencyResolver>()
               .InstancePerLifetimeScope();

            // register handlers
            builder
                .RegisterGenerics(typeof(IQueryHandler<,>))
                .PropertiesAutowired();

            // register query service
            builder
                .RegisterType<InProcQueryService>()
                .AsSelf()
                .SingleInstance();

            var container = builder.Build();

            var expected = 4;

            var queryService = container.Resolve<InProcQueryService>();

            // act
            var result = await queryService.Run(new SimpleQuery { ReturnValue = expected });

            // assert
            result.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public async Task Runs_query_and_returns_simple_type()
        {
            // arrange
            var expected = 4;

            var query = new SimpleQuery {
                ReturnValue = expected
            };

            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), expected.GetType());

            A.CallTo(() => AutoFake
                .Resolve<IDependencyResolver>()
                .Resolve(handlerType))
                .Returns(new SimpleQueryHandler());

            var queryService = AutoFake.Resolve<InProcQueryService>();

            // act
            var result = await queryService.Run(query);

            // assert
            result.ShouldBeEquivalentTo(expected);
        }

        [Fact]
        public async Task Runs_query_and_returns_complex_type()
        {
            // arrange
            var expected = new Complex { ReturnValue = 5 };

            var query = new ComplexQuery
            {
                ReturnValue = expected.ReturnValue
            };

            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), expected.GetType());

            A.CallTo(() => AutoFake
                .Resolve<IDependencyResolver>()
                .Resolve(handlerType))
                .Returns(new ComplexQueryHandler());

            var queryService = AutoFake.Resolve<InProcQueryService>();

            // act
            var result = await queryService.Run(query);

            // assert
            result.ShouldBeEquivalentTo(expected);
        }

        public class SimpleQuery : IQuery<int>
        {
            public int ReturnValue { get; set; }
        }

        public class SimpleQueryHandler : IQueryHandler<SimpleQuery, int>
        {
            public Task<int> Handle(SimpleQuery query, CancellationToken cancellationToken)
            {
                return Task.FromResult(query.ReturnValue);
            }
        }

        public class Complex
        {
            public int ReturnValue { get; set; }
        }
        public class ComplexQuery : IQuery<Complex>
        {
            public int ReturnValue { get; set; }
        }
        public class ComplexQueryHandler : IQueryHandler<ComplexQuery, Complex>
        {
            public Task<Complex> Handle(ComplexQuery query, CancellationToken cancellationToken)
            {
                return Task.FromResult(new Complex { ReturnValue = query.ReturnValue});
            }
        }
    }
}
