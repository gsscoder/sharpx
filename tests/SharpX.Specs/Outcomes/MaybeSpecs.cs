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
using Xunit.Abstractions;
using static FsCheck.ResultContainer;

namespace Outcomes;

public class MaybeSpecs
{
    static Random _random = new CryptoRandom();

    [Property(Arbitrary = new[] { typeof(ArbitraryValue) })]
    public Property A_non_default_value_is_wrapped_into_a_Just(object value)
    {
        Func<bool> property = () => value.ToMaybe() == Maybe.Just(value);

        return property.When(value != default);
    }

    [Property(MaxTest=1)]
    public Property A_default_value_is_converted_into_a_Nothing()
    {
        return (((string)null).ToMaybe() == Maybe.Nothing<string>()).ToProperty();
    }

    [Property(MaxTest=1)]
    public Property Build_a_Just_from_a_null_value_throws_ArgumentException()
    {
        return FsCheck.FSharp.Prop.Throws<ArgumentException, Maybe<string>>(
            new Lazy<Maybe<string>>(() => Maybe.Just((string)null)));
    }

    [Property]
    public Property Build_correct_maybe_with_a_value_type(IntWithMinMax value)
    {
        var outcome = Maybe.Return(value.Get);

        return (value switch
        {
            var v when v.Get == default => outcome.Tag == MaybeType.Nothing,
            _ => outcome.Tag == MaybeType.Just && outcome.FromJust() == value.Get,
        }).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public Property Build_correct_maybe_with_a_reference_type(string value)
    {
        var outcome = Maybe.Return(value);

        return (value switch
        {
            var v when v == default => outcome.Tag == MaybeType.Nothing,
            _ => outcome.Tag == MaybeType.Just && outcome.FromJust() == value,
        }).ToProperty();
    }


    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public Property FromJust_unwraps_the_value_or_lazily_returns_from_a_function(string value)
    {
        Func<string> func = () => "foo";

        var sut = Maybe.Return(value);

        var outcome = sut.FromJust(func);

        return (value switch
        {
            var v when v == default => outcome == func(),
            _ => outcome == value,
        }).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public Property ToEnumerable_returns_a_singleton_sequence_with_Just_and_an_empty_with_Nothing(string value)
    {
        var sut = Maybe.Return(value);

        var outcome = sut.ToEnumerable();

        return (value switch
        {
            var v when v == default => outcome.Count() == 0,
            _ => outcome.ElementAt(0) == value,
        }).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNullSeq) })]
    public Property Justs_method_extracts_Just_values_from_a_sequence(string[] values)
    {
        var maybes = from value in values select Maybe.Return(value);

        return (from value in values where value != default select value).SequenceEqual(maybes.Justs()).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNullSeq) })]
    public Property Nothings_method_counts_Nothing_values_of_a_sequence(string[] values)
    {
        var maybes = from value in values select Maybe.Return(values);

        return (values.Length == maybes.Nothings()).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNullSeq) })]
    public Property Map_throws_out_Just_values_from_a_sequence(string[] values)
    {
        Func<string, Maybe<int>> readInt = value => {
            if (int.TryParse(value, out int result)) return Maybe.Just(result);
            return Maybe.Nothing<int>(); };

        var expected = from value in values where int.TryParse(value, out int _)
                       select int.Parse(value);

        var outcome = values.Map(readInt);

        return expected.SequenceEqual(outcome).ToProperty();    
    }


    [Property]
    public Property Match_Just_of_anonymous_tuple_type_with_lambda_function(IntWithMinMax value1, IntWithMinMax value2)
    {
        var sut = Maybe.Just((value1.Get, value2.Get));

        int? outcome1 = null;
        int? outcome2 = null;
        sut.Match(
            (value1, value2) => Unit.Do(() => { outcome1 = value1; outcome2 = value2; }),
            () => Unit.Default);
        
        return (value1.Get == outcome1.Value && value2.Get == outcome2.Value).ToProperty();
    }

    [Property]
    public Property Match_Just_of_anonymous_tuple_type(IntWithMinMax value1, IntWithMinMax value2)
    {
        var sut = Maybe.Just((value1.Get, value2.Get));

        var outcome = sut.MatchJust(out int outcome1, out int outcome2);

        return (outcome && value1.Get == outcome1 && value2.Get == outcome2).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public Property Map_throws_out_and_map_a_Just_value_or_lazily_build_one_in_case_of_Nothing(string value)
    {
        Func<string> func = () => "foo";

        var sut = Maybe.Return(value);

        var outcome = sut.Map(v => v, func);

        return (value switch
        {
            var v when v == default => func() == outcome,
            _ => value == outcome,
        }).ToProperty();    
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void Should_return_a_Just_when_Try_succeed_otherwise_Nothing(int value)
    {
        var number = Primitives.GenerateSeq<int>(count: 1).Single();
        var outcome = Maybe.Try(() => number / value);

        if (value == 0) outcome.Tag.Should().Be(MaybeType.Nothing);
        else outcome.Should().NotBeNull().And.Match<Maybe<int>>(x =>
            x.Tag == MaybeType.Just &&
            x._value == number);
    }

    [Property(MaxTest=1)]
    public Property Return_false_when_a_Maybe_is_compared_to_null()
    {
        var sut = Maybe.Return("foo");

        return (!sut.Equals(null)).ToProperty();
    }

    [Property(MaxTest=1)]
    public Property Nothing_values_wrapping_same_type_are_equals()
    {
        var sut1 = Maybe.Nothing<int>();
        var sut2 = Maybe.Nothing<int>();

        return sut1.Equals(sut2).ToProperty();
    }

    [Property(MaxTest=1)]
    public Property Nothing_values_wrapping_same_type_compared_as_object_are_equals()
    {
        var sut1 = Maybe.Nothing<int>();
        object sut2 = Maybe.Nothing<int>();

        return sut1.Equals(sut2).ToProperty();
    }

    [Property(MaxTest=1)]
    public Property Nothing_wrapping_different_type_are_different()
    {
        var sut1 = Maybe.Nothing<int>();
        var sut2 = Maybe.Nothing<string>();

        return (sut1.Equals(sut2) == false).ToProperty();
    }


    [Property(MaxTest=1)]
    public Property Maybe_of_different_type_are_not_equals(NonZeroInt value)
    {
        var sut1 = Maybe.Nothing<int>();
        var sut2 = Maybe.Return(value.Get);

        return (sut1.Equals(sut2) == false).ToProperty();
    }

    [Property]
    public Property Maybe_of_different_type_compared_as_object_are_not_equals(NonZeroInt value)
    {
        var sut1 = Maybe.Nothing<int>();
        object sut2 = Maybe.Return(value);

        return (sut1.Equals(sut2) == false).ToProperty();
    }

    [Property]
    public Property Maybe_with_different_values_are_not_equals(NonZeroInt value)
    {
        var sut1 = Maybe.Return(value);
        var otherValue = _random.Next();
        var sut2 = Maybe.Return(otherValue == value.Get ? otherValue / 2 : otherValue);

        return (sut1.Equals(sut2) == false).ToProperty();
    }

    [Property]
    public Property Maybe_with_identical_values_are_equals(NonZeroInt value)
    {
        var sut1 = Maybe.Return(value.Get);
        var sut2 = Maybe.Return(value.Get);

        return sut1.Equals(sut2).ToProperty();
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


    [Property(MaxTest=1)]
    public Property Just_wrapping_different_values_have_different_hash_codes()
    {
        var sut1 = Maybe.Just(_random.Next(0, 9));
        var sut2 = Maybe.Just(Strings.Generate(10));

        return (sut1.GetHashCode() != sut2.GetHashCode()).ToProperty();
    }

    [Property(MaxTest=1)]
    public Property Nothing_of_different_type_have_different_hash_codes()
    {
        var sut1 = Maybe.Nothing<int>();
        var sut2 = Maybe.Nothing<string>();

        return (sut1.GetHashCode() != sut2.GetHashCode()).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public Property Do_method_consumes_the_value_only_on_Just(string value)
    {
        var evidence = false;
        var sut = Maybe.Return(value);

        sut.Do(_ => {
            evidence = true;
            return Unit.Default;
        });

        if (sut.IsNothing()) evidence.Should().BeFalse();
        else evidence.Should().BeTrue();

        return (sut.Tag == MaybeType.Nothing
            ? evidence == false : evidence == true).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public async Task<Property> DoAsync_method_consumes_the_value_only_on_Just(string value)
    {
        var evidence = false;
        var sut = Maybe.Return(value);

        await sut.DoAsync(_ => {
            evidence = true;
            return Task.FromResult(Unit.Default);
        });

        return (sut.Tag == MaybeType.Nothing
            ? evidence == false : evidence == true).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public Property Do_method_consumes_the_tuple_value_only_on_Just(string value)
    {
        var evidence = false;
        var sut = Maybe.Return((value, value));

        sut.Do(_ => {
            evidence = true;
            return Unit.Default;
        });

        return (sut.Tag == MaybeType.Nothing
            ? evidence == false : evidence == true).ToProperty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryStringNull) })]
    public async Task<Property> DoAsync_method_consumes_the_tuple_value_only_on_Just(string value)
    {
        var evidence = false;
        var sut = Maybe.Return((value, value));

        await sut.DoAsync(_ => {
            evidence = true;
            return Task.FromResult(Unit.Default);
        });

        return (sut.Tag == MaybeType.Nothing
            ? evidence == false : evidence == true).ToProperty();
    }           
}
