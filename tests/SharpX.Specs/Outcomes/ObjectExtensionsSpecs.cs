using System;
using FluentAssertions;
using SharpX;
using SharpX.Extensions;
using Xunit;

namespace Outcomes
{
    public class ObjectExtensionsSpecs
    {
        class FakeObject
        {
            public string StringValue { get; set; }
            public int IntValue { get; set; }
        }

        static readonly Random _random = new CryptoRandom();

        [Fact]
        public void Should_convert_anything_to_Unit()
        {
            new object().ToUnit().Should().Be(Unit.Default);
            _random.NextDouble().ToUnit().Should().Be(Unit.Default);
            Strings.Generate(_random.Next(3, 9)).ToUnit().Should().Be(Unit.Default);
            new FakeObject { StringValue = "foobarbaz", IntValue = 123 }.ToUnit().Should().Be(Unit.Default);
        }
    }
}
