using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Aenima.JsonNet.Tests
{
    [TestFixture]
    public class JsonNetEventSerializerTests
    {
        public class SerializerWorks : IEvent
        {
            public bool BecauseItsAwesome { get; set; }

            public SerializerWorks Inception { get; set; }
        }

        [Test]
        public void Serializes()
        {
            // arrange
            var expectedResult 
                = "{\"BecauseItsAwesome\":true,\"Inception\":{\"BecauseItsAwesome\":true}}";

            var input = new SerializerWorks
            {
                BecauseItsAwesome = true,
                Inception = new SerializerWorks {
                    BecauseItsAwesome = true
                }
            };

            var sut = new JsonNetEventSerializer();

            // act
            var result = sut.Serialize(input);

            // assert
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void Deserializes()
        {
            // arrange
            var expectedResult = new SerializerWorks
            {
                BecauseItsAwesome = true,
                Inception = new SerializerWorks {
                    BecauseItsAwesome = true
                }
            };

            var input = "{\"BecauseItsAwesome\":true,\"Inception\":{\"BecauseItsAwesome\":true}}";

            var sut = new JsonNetEventSerializer();
            // act
            var result = sut.Deserialize(input, expectedResult.GetType());

            // assert
            result.ShouldBeEquivalentTo(expectedResult);
        }

        [Test]
        public void Serializes_And_Deserializes_Dictionary()
        {
            // arrange
            var expectedResult = new Dictionary<string, object> {
                {"MyGuid"           , Guid.NewGuid() },
                {"MyEmptyGuid"      , Guid.Empty},
                {"Testing"          , true},
                {"Testing-string"   , "true"},
                {"event-clr-type"   , "Aenima.Dapper.Tests.DapperEventStoreTests+TestEventOne, Aenima.Dapper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"},
                {"aggregate-version", -1},
                {"kebas"            , null},
                {"kebas-empty"      , string.Empty}
            };

            var sut = new JsonNetEventSerializer();

            // act
            var json   = sut.Serialize(expectedResult);

            //json = "{ \"Testing\":true,\"event-id\":\"e83a33ec-ce52-9757-e1b9-39d22f3e4d73\",\"event-clr-type\":\"Aenima.Dapper.Tests.DapperEventStoreTests+TestEventOne, Aenima.Dapper.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"stream-id\":\"0587c6bd-d4f7-4e99-a2d4-823d13b89cd5\",\"aggregate-version\":-1,\"commit-id\":\"fb41a10d-01f9-42c2-9391-e07923c98527\"}";
            var result = sut.Deserialize(json, typeof(Dictionary<string, object>));

            // assert
            result.ShouldBeEquivalentTo(expectedResult);
        }
    }
}
