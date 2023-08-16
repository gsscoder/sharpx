using System;
using System.Linq;
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
    [Property]
    public Property Generate_a_sequence_of_n_unique_items(int count)
    {
        Func<bool> property = () => {
            var outcome = Primitives.GenerateSeq<string>(() => Strings.Generate(9), count: count);
            
            return outcome.Count() == count &&
                   outcome.Distinct().Count() == count;
        };

        return property.When(count >= 0); 
    }

    [Property]
    public Property Generate_an_infinite_sequence_of_n_unique_items(int taken)
    {
        Func<bool> property = () => {
            var outcome = Primitives.GenerateSeq<string>(() => Strings.Generate(9), count: null)
                .Take(taken);

            return outcome.Count() == taken &&
                   outcome.Distinct().Count() == taken;
        };

        return property.When(taken >= 0);
    }
    #endregion
}
