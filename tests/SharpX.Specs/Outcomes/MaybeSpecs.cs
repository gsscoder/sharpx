#pragma warning disable 0618

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using SharpX;
using SharpX.Extensions;
using SharpX.FsCheck;
using Xunit;

namespace Outcomes;

public class MaybeSpecs
{
    static Random _random = new CryptoRandom();

    [Property]
    public void Shoud_build_Just(NonZeroInt value)
    {
        var outcome = Maybe.Just(value.Get);

        outcome.IsJust().Should().BeTrue();
        outcome.FromJust().Should().Be(value.Get);
    }

    [Fact]
    public void Shoud_build_Nothing()
    {
        var outcome = Maybe.Nothing<int>();

        outcome.IsNothing().Should().BeTrue();
        outcome.Tag.Should().Be(MaybeType.Nothing);
    }

    [Fact]
    public void Should_throw_exception_when_building_a_Just_from_a_null_value()
    {
        Action action = () => Maybe.Just((object)null);

        action.Should().Throw<ArgumentException>();
    }

    [Property]
    public void Shoud_return_proper_maybe_with_a_value_type(IntWithMinMax value)
    {
        var outcome = Maybe.Return(value.Get);

        outcome.Should().NotBeNull();

        if (value.Get == default) {
            outcome.IsNothing().Should().BeTrue();
        }
        else {
            outcome.IsJust().Should().BeTrue();
            outcome.FromJust().Should().Be(value.Get);
        }
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public void Shoud_return_proper_maybe_with_a_reference_type(string value)
    {
        var outcome = Maybe.Return(value);

        outcome.Should().NotBeNull();

        if (value == null) {
            outcome.IsNothing().Should().BeTrue();
        }
        else {
            outcome.IsJust().Should().BeTrue();
            outcome.FromJust().Should().Be(value);
        }
    }


    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public void FromJust_should_unwrap_the_value_or_lazily_return_from_a_function(string value)
    {
        Func<string> func = () => "foo";

        var sut = Maybe.Return(value);

        var outcome = sut.FromJust(func);

        if (value == null) outcome.Should().NotBeNull().And.Be(func());
        else outcome.Should().NotBeNull().And.Be(value);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public void Should_return_a_singleton_sequence_with_Just_and_an_empty_with_Nothing(string value)
    {
        var sut = Maybe.Return(value);

        var outcome = sut.ToEnumerable();

        if (value == null) {
            outcome.Should().NotBeNull().And.BeEmpty();
        }
        else {
            outcome.Should().NotBeNullOrEmpty().And.HaveCount(1);
            outcome.ElementAt(0).Should().Be(value);
        }
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNullSeq) })]
    public void Should_return_Just_values_from_a_sequence(string[] values)
    {
        var maybes = from value in values select Maybe.Return(value);

        var outcome = maybes.Justs();

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCountLessOrEqualTo(values.Count())
            .And.ContainInOrder(from value in values where value != null select value);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNullSeq) })]
    public void Should_count_Nothing_values_of_a_sequence(string[] values)
    {
        var maybes = from value in values select Maybe.Return(values);

        var outcome = maybes.Nothings();

        outcome.Should().BeLessOrEqualTo(values.Count());
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNullSeq) })]
    public void Should_throw_out_Just_values_from_a_sequence(string[] values)
    {
        Func<string, Maybe<int>> readInt = value => {
            if (int.TryParse(value, out int result)) return Maybe.Just(result);
            return Maybe.Nothing<int>(); };

        var expected = from value in values where int.TryParse(value, out int _)
                       select int.Parse(value);

        var outcome = values.Map(readInt);

        outcome.Should().NotBeNullOrEmpty()
            .And.HaveCount(expected.Count())
            .And.ContainInOrder(expected);
    }


    [Property(Arbitrary = new[] { typeof(ArbitraryIntegerSeq) })]
    public void Should_match_Just_of_anonymous_tuple_type_with_lambda_function(int value)
    {
        var sut = Maybe.Just((value, value / 2));

        int? outcome1 = null;
        int? outcome2 = null;
        sut.Match(
            (value1, value2) => Unit.Do(() => { outcome1 = value1; outcome2 = value2; }),
            () => Unit.Default);
        outcome1.Should().NotBeNull().And.Be(value);
        outcome2.Should().NotBeNull().And.Be(value / 2);
    }

    [Property]
    public void Should_match_Just_of_anonymous_tuple_type(IntWithMinMax value)
    {
        var sut = Maybe.Just((value.Get, value.Get / 2));

        var outcome = sut.MatchJust(out int outcome1, out int outcome2);

        outcome.Should().BeTrue();
        outcome1.Should().Be(value.Get);
        outcome2.Should().Be(value.Get / 2);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public void Should_throw_out_and_map_a_Just_value_or_lazily_build_one_in_case_of_Nothing(string value)
    {
        Func<string> func = () => "foo";

        var sut = Maybe.Return(value);

        var outcome = sut.Map(v => v, func);

        if (value == null) outcome.Should().NotBeNull().And.Be(func());
        else outcome.Should().NotBeNull().And.Be(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Should_return_a_Just_when_Try_succeed_otherwise_Nothing(int value)
    {
        var number = new CryptoRandom().Next();
        var outcome = Maybe.Try(() => number / value);

        if (value == 0) outcome.Tag.Should().Be(MaybeType.Nothing);
        else outcome.Should().NotBeNull().And.Match<Maybe<int>>(x =>
            x.Tag == MaybeType.Just &&
            x._value == number);
    }

    [Fact]
    public void Should_return_false_when_a_Maybe_is_compared_to_null()
    {
        var sut = Maybe.Return("foo");

        var outcome = sut.Equals(null);

        outcome.Should().BeFalse();
    }

    [Fact]
    public void Nothing_wrapping_same_type_are_equals()
    {
        var sut1 = Maybe.Nothing<int>();
        var sut2 = Maybe.Nothing<int>();

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeTrue();
    }

    [Fact]
    public void Nothing_wrapping_same_type_compared_as_object_are_equals()
    {
        var sut1 = Maybe.Nothing<int>();
        object sut2 = Maybe.Nothing<int>();

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeTrue();
    }

    [Fact]
    public void Nothing_wrapping_different_type_are_different()
    {
        var sut1 = Maybe.Nothing<int>();
        var sut2 = Maybe.Nothing<string>();

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeFalse();
    }


    [Property]
    public void Maybe_of_different_type_are_not_equals(NonZeroInt value)
    {
        var sut1 = Maybe.Nothing<int>();
        var sut2 = Maybe.Return(value.Get);

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeFalse();
    }

    [Property]
    public void Maybe_of_different_type_compared_as_object_are_not_equals(NonZeroInt value)
    {
        var sut1 = Maybe.Nothing<int>();
        object sut2 = Maybe.Return(value);

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeFalse();
    }

    [Property]
    public void Maybe_with_different_values_are_not_equals(NonZeroInt value)
    {
        var sut1 = Maybe.Return(value);
        var otherValue = _random.Next();
        var sut2 = Maybe.Return(otherValue == value.Get ? otherValue / 2 : otherValue);

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeFalse();
    }

    [Property]
    public void Maybe_with_identical_values_are_equals(NonZeroInt value)
    {
        var sut1 = Maybe.Return(value.Get);
        var sut2 = Maybe.Return(value.Get);

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeTrue();
    }

    [Property]
    public Property Just_with_identical_value_has_same_hash_code(PositiveInt value, PositiveInt count)
    {
        Func<bool> property = () => {
            var outcome = from item in Enumerable.Repeat(Maybe.Just(value), count.Get)
                          select item.GetHashCode();

            return Maybe.Just(value).GetHashCode() == outcome.Distinct().Single();
        };

        return property.When(count.Get > 1);
    }

    [Property]
    public Property Nothing_of_identical_type_has_same_hash_code()
    {
        return (((string)null).ToMaybe().GetHashCode() == Maybe.Nothing<string>().GetHashCode()).ToProperty();
    }

    [Property]
    public void Maybe_with_identical_values_compared_as_object_are_equals(NonZeroInt value)
    {

        var sut1 = Maybe.Return(value);
        object sut2 = Maybe.Return(value);

        var outcome = sut1.Equals(sut2);

        outcome.Should().BeTrue();
    }


    [Fact]
    public void Just_wrapping_different_values_have_different_hash_codes()
    {
        var sut1 = Maybe.Just(_random.Next(0, 9));
        var sut2 = Maybe.Just(Strings.Generate(10));

        var outcome = sut1.GetHashCode() != sut2.GetHashCode();
        
        outcome.Should().BeTrue();
    }

    [Fact]
    public void Nothing_of_different_type_have_different_hash_codes()
    {
        var sut1 = Maybe.Nothing<int>();
        var sut2 = Maybe.Nothing<string>();

        var outcome = sut1.GetHashCode() != sut2.GetHashCode();
        
        outcome.Should().BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public void Do_method_consumes_the_value_only_on_Just(string value)
    {
            var evidence = false;
            var sut = Maybe.Return(value);

            sut.Do(_ => {
                evidence = true;
                return Unit.Default;
            });

            if (sut.IsNothing()) evidence.Should().BeFalse();
            else evidence.Should().BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public async Task DoAsync_method_consumes_the_value_only_on_Just(string value)
    {
        var evidence = false;
        var sut = Maybe.Return(value);

        await sut.DoAsync(_ => {
            evidence = true;
            return Task.FromResult(Unit.Default);
        });

        if (sut.IsNothing()) evidence.Should().BeFalse();
        else evidence.Should().BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public void Do_method_consumes_the_tuple_value_only_on_Just(string value)
    {
        var evidence = false;
        var sut = Maybe.Return((value, value));

        sut.Do(_ => {
            evidence = true;
            return Unit.Default;
        });

        if (sut.IsNothing()) evidence.Should().BeFalse();
        else evidence.Should().BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public async Task DoAsync_method_consumes_the_tuple_value_only_on_Just(string value)
    {
        var evidence = false;
        var sut = Maybe.Return((value, value));

        await sut.DoAsync(_ => {
            evidence = true;
            return Task.FromResult(Unit.Default);
        });

        if (sut.IsNothing()) evidence.Should().BeFalse();
        else evidence.Should().BeTrue();
    }           
}
