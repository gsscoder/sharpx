using System;
using System.Collections.Generic;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using SharpX;
using Xunit;

public class StringsSpecs
{
    static List<string> _strings = new List<string>() { Strings.Generate(new CryptoRandom().Next(1, 60)) };

    [Theory]
    [InlineData('f', 0, "", "")]
    [InlineData('f', 1, "", "f")]
    [InlineData('f', 5, " ", "f f f f f")]
    public void Should_replicate(char value, int count, string separator, string expected)
    {
        var outcome = Strings.ReplicateChar(value, count, separator);

        outcome.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryNegativeIntegers) })]
    public void Trying_to_generate_a_random_string_with_less_than_one_char_raises_ArgumentException(int value)
    {
        Action action = () => Strings.Generate(value);

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Should_generate_an_empty_string_when_length_is_zero()
    {
        var outcome = Strings.Generate(0);

        outcome.Should().NotBeNull().And.BeEmpty();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryPositiveIntegers) })]
    public void Should_generate_a_random_string_of_given_length(int value)
    {
        var outcome = Strings.Generate(value);

        outcome.Should().NotBeNull().And.HaveLength(value);
        _strings.Should().NotContain(outcome);

        _strings.Add(outcome);
    }
}
