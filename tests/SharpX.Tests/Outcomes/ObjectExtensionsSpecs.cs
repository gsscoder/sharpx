using System;
using System.Reflection;
using FluentAssertions;
using FsCheck.Experimental;
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

        // NOTE: cannot use Theory for 'real' generic method test
        [Fact]
        public void Should_discards_anything_to_Unit()
        {
            ((byte)1).ToUnit().Should().Be(Unit.Default);
            2.ToUnit().Should().Be(Unit.Default);
            3.3.ToUnit().Should().Be(Unit.Default);
            "4".ToUnit().Should().Be(Unit.Default);
            typeof(ObjectExtensionsSpecs).ToUnit().Should().Be(Unit.Default);
            Assembly.GetCallingAssembly().ToUnit().Should().Be(Unit.Default);
        }
    }
}
