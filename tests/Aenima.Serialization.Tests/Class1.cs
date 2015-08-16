using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aenima.JsonNet;
using FluentAssertions;
using NUnit.Framework;

namespace Aenima.Serialization.Tests
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
                Inception = new SerializerWorks
                {
                    BecauseItsAwesome = true
                }
            };

            var sut = new JsonNetSerializer();

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
                Inception = new SerializerWorks
                {
                    BecauseItsAwesome = true
                }
            };

            var input = "{\"BecauseItsAwesome\":true,\"Inception\":{\"BecauseItsAwesome\":true}}";

            var sut = new JsonNetSerializer();
            // act
            var result = sut.Deserialize(input, expectedResult.GetType());

            // assert
            result.ShouldBeEquivalentTo(expectedResult);
        }
    }
}
