using System.Linq;
using System.Security.Cryptography;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using SharpX;
using SharpX.FsCheck;
using Xunit;

namespace Outcomes;

public class PrimitivesSpecs
{
    [Property(Arbitrary = new[] { typeof(ArbitraryValueSeq) })]
    public Property Converting_a_value_to_enumerable_is_equivalent_to_a_single_element_array(object value)
    {
        return (new[] { value }).SequenceEqual(Primitives.ToEnumerable(value)).ToProperty();
    }

    #region ChanceOf
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
    #endregion

    #region GenerateSeq
    [Fact]
    public void Should_generate_an_empty_sequence_when_count_is_0()
    {
        var outcome = Primitives.GenerateSeq<string>(() => Strings.Generate(9), count: 0);

        outcome.Should().NotBeNull()
            .And.BeEmpty();
    }

    [Property]
    public void Should_generate_a_sequence_of_n_items(PositiveInt value)
    {
        var outcome = Primitives.GenerateSeq<string>(() => Strings.Generate(9), count: value.Get);

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCount(value.Get)
            .And.OnlyHaveUniqueItems();
    }

    [Fact]
    public void Should_generate_an_infinite_sequence_when_count_is_null()
    {
        var taken = RandomNumberGenerator.GetInt32(9, 100);

        var outcome = Primitives.GenerateSeq<string>(() => Strings.Generate(9), count: null)
            .Take(taken);

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCount(taken)
            .And.OnlyHaveUniqueItems();
    }
    #endregion
}
