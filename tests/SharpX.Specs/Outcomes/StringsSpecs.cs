using System;
using System.Collections.Generic;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using SharpX;
using Xunit;

namespace Outcomes
{
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
    }
}
