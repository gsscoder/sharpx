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
    [Property(Arbitrary = new[] { typeof(ArbitraryValue) })]
    public Property Converting_a_value_to_enumerable_is_equivalent_to_a_single_element_array(object value)
    {
        Func<bool> property = () => (new[] { value }).SequenceEqual(Primitives.ToEnumerable(value));

        return property.When(value != default);
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

    [Property]
    public Property Generate_a_sequence_of_n_unique_item_using_of_type_int(int count) =>
        Generate_a_sequence_of_n_unique_item_using_of_type<int>(count);

    [Property]
    public Property Generate_a_sequence_of_n_unique_item_using_of_type_double(int count) =>
        Generate_a_sequence_of_n_unique_item_using_of_type<double>(count);

    [Property]
    public Property Generate_a_sequence_of_n_unique_item_using_of_type_string(int count) =>
        Generate_a_sequence_of_n_unique_item_using_of_type<string>(count);

    Property Generate_a_sequence_of_n_unique_item_using_of_type<T>(int count)
    {

        Func<bool> property = () => {
            var outcome = Primitives.GenerateSeq<T>(count: count);

            var correct = outcome.Count() == count && outcome.Distinct().Count() == count;
            return correct;
        };

        return property.When(count >= 0);
    }
    #endregion

    #region IsNumber
    // NOTE: cannot use Theory for 'real' generic method test
    [Fact]
    public void Should_return_true_for_numeric_types_and_false_for_any_other_type_or_null()
    {
        Primitives.IsNumber((object)null).Should().BeFalse();
        Primitives.IsNumber(default(byte)).Should().BeTrue();
        Primitives.IsNumber(default(short)).Should().BeTrue();
        Primitives.IsNumber(default(ushort)).Should().BeTrue();
        Primitives.IsNumber(default(int)).Should().BeTrue();
        Primitives.IsNumber(default(uint)).Should().BeTrue();
        Primitives.IsNumber(default(long)).Should().BeTrue();
        Primitives.IsNumber(default(ulong)).Should().BeTrue();
        Primitives.IsNumber(default(float)).Should().BeTrue();
        Primitives.IsNumber(default(double)).Should().BeTrue();
        Primitives.IsNumber(default(decimal)).Should().BeTrue();
        Primitives.IsNumber("1234567890").Should().BeFalse();
        Primitives.IsNumber(new object()).Should().BeFalse();
        Primitives.IsNumber(new { empty = "" }).Should().BeFalse();
    }
    #endregion

    #region SafeInvoke
    [Fact]
    public void SafeInvoke_swallows_an_exception()
    {
        Exception raised = null;
        try {
            Primitives.SafeInvoke(() => throw new Exception());
        }
        catch (Exception ex) {
            raised = ex;
        }

        raised.Should().BeNull();
    }
    #endregion
}
