using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using SharpX;
using SharpX.FsCheck;
using Xunit;

namespace Outcomes;

public class PrimitivesSpecs
{
    [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
    public void Should_convert_a_value_to_enumerable(int value)
    {
        var outcome = Primitives.ToEnumerable(value);

        outcome.Should().NotBeEmpty()
            .And.HaveCount(1)
            .And.Contain(value);
    }

    [Fact]
    public void Should_return_false_when_chance_is_0_percent()
    {
        var outcome = Primitives.ChanceOf(0);

        outcome.Should().BeFalse();
    }

    [Fact]
    public void Should_return_true_when_chance_is_100_percent()
    {
        var outcome = Primitives.ChanceOf(100);

        outcome.Should().BeTrue();
    }
}
