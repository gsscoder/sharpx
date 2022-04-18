using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using SharpX;

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
}
