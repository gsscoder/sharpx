#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using SharpX;
using SharpX.FsCheck;
using Xunit;

namespace Outcomes;

public class StringsSpecs
{
    static readonly Random _random = new CryptoRandom();

    [Theory]
    [InlineData('f', 0, "", "")]
    [InlineData('f', 1, "", "f")]
    [InlineData('f', 5, " ", "f f f f f")]
    public void Should_replicate(char value, int count, string separator, string expected)
    {
        var outcome = Strings.ReplicateChar(value, count, separator);

        outcome.Should().Be(expected);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryIntegerNegative) })]
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

    [Property(Arbitrary = new[] { typeof(ArbitraryIntegerPositive) })]
    public void Should_generate_a_random_string_of_given_length(int value)
    {
        var strings = new List<string>() { Strings.Generate(_random.Next(1, 60)) };

        var outcome = Strings.Generate(value);

        outcome.Should().NotBeNull().And.HaveLength(value);
        strings.Should().NotContain(outcome);

        strings.Add(outcome);
    }

    [Fact]
    public void Trying_to_set_allow_quotes_with_disallowed_special_chars_raises_ArgumentException()
    {
        Action action = () => Strings.Generate(1, new GenerateOptions()
        {
            AllowSpecialChars = false,
            AllowQuoteChars = true
        });

        action.Should().ThrowExactly<ArgumentException>()
            .WithMessage("Cannot allow quote chars when special chars are disallowed. (Parameter 'options')");
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryIntegerPositive) })]
    public void Should_generate_a_random_string_of_given_length_with_special_chars(int value)
    {
        var outcome = Strings.Generate(value + 10,
            new GenerateOptions { AllowSpecialChars = true, AllowQuoteChars = true });

        outcome.Should().NotBeNull().And.HaveLength(value + 10);
        outcome.Any(Strings.IsSpecialChar).Should().BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryIntegerPositive) })]
    public void Should_generate_a_random_string_of_given_length_with_special_chars_no_quotes(int value)
    {
        var outcome = Strings.Generate(value + 10,
            new GenerateOptions { AllowSpecialChars = true, AllowQuoteChars = false });

        outcome.Should().NotBeNull().And.HaveLength(value + 10);
        outcome.Any(c => c == '"' || c == '\'' || c == '`').Should().BeFalse();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryIntegerPositive) })]
    public void Should_generate_a_random_string_of_given_length_with_prefix(int value)
    {
        const string prefix = "prfx_";

        var outcome = Strings.Generate(value, null, prefix);

        outcome.Should().NotBeNull().And.HaveLength(value + prefix.Length);
        outcome.Should().StartWith(prefix);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData(" \n ")]
    [InlineData(" \r ")]
    [InlineData(" \t ")]
    public void Should_not_detect_empty_string_or_whitespace_as_special_character(string value)
    {
        var outcome = Strings.ContainsSpecialChar(value);

        outcome.Should().BeFalse();
    }

    [Theory]
    [InlineData("foobar/baz")]
    [InlineData("foo#?bar/baz")]
    [InlineData("fo!ob_ar|baz")]
    [InlineData("foob?ar^baz")]
    [InlineData("foo@bar_baz")]
    [InlineData("foobar[b]]az")]
    [InlineData("f00barbaz_")]
    public void Should_detect_strings_with_special_characters(string value)
    {
        var outcome = Strings.ContainsSpecialChar(value);

        outcome.Should().BeTrue();
    }

    [Theory]
    [InlineData("foobarbaz")]
    [InlineData("foo bar baz")]
    [InlineData("fòòbàrbàz")]
    [InlineData("f00b4rb4z")]
    public void Should_detect_strings_without_special_characters(string value)
    {
        var outcome = Strings.ContainsSpecialChar(value);

        outcome.Should().BeFalse();
    }

    [Theory]
    [InlineData("foo_bar_baz")]
    [InlineData("foo bar baz")]
    [InlineData("fòòbàrbàz")]
    [InlineData(" \n\r\t" )]
    [InlineData("f00-b4r-b4z")]
    public void Should_detect_strings_without_special_characters_except_excluded(string value)
    {
        var outcome = Strings.ContainsSpecialChar(value, '_', '-');

        outcome.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r")]
    [InlineData("\n")]
    [InlineData("\t \r \n")]
    public void Should_detect_empty_or_whitespace_string(string value)
    {
        var outcome = Strings.IsEmptyWhitespace(value);

        outcome.Should().BeTrue();
    }

    [Fact]
    public void Should_get_an_empty_string_when_start_index_and_length_are_zero()
    {
        var outcome = Strings.Substring(string.Empty, 0, 0, safe: true);

        outcome.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("foo bar")]
    [InlineData("foo bar baz")]
    public void Should_get_same_string_when_start_index_and_length_match_string_length(string value)
    {
        var outcome = Strings.Substring(value, 0, value.Length, safe: true);

        outcome.Should().Be(value);
    }

    [Theory]
    [InlineData("foo")]
    [InlineData("baar")]
    [InlineData("baaaz")]
    public void Should_get_a_substring_without_exception_even_when_length_exceeds_input_string_length(string value)
    {
        var input = Strings.Mangle(Strings.Replicate(value, _random.Next(1, 3), separator: "@"),
            times: 2, maxLength: 3);
        
        var outcome = Strings.Substring(input, 0, input.Length * _random.Next(2, 3), safe: true);

        outcome.Should().Be(input);
    }
}
