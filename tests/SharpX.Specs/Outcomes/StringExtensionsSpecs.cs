using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SharpX;
using SharpX.Extensions;
using Xunit;

namespace Outcomes
{
    public class StringExtensionsSpecs
    {
        [Theory]
        [InlineData("foo", true)]
        [InlineData("0123456789", false)]
        [InlineData("foo01234", false)]
        [InlineData("foo.bar", false)]
        [InlineData("foo bar", false)]
        public void Should_detect_letter_characters(string value, bool expected)
        {
            var outcome = value.IsAlpha();
            
            outcome.Should().Be(expected);
        }

        [Theory]
        [InlineData("foo", true)]
        [InlineData("0123456789", true)]
        [InlineData("foo01234", true)]
        [InlineData("foo.bar", false)]
        [InlineData("foo bar", false)]
        public void Should_detect_alphanumeric_characters(string value, bool expected)
        {
            var outcome = value.IsAlphanumeric();
            
            outcome.Should().Be(expected);
        }

        [Theory]
        [InlineData("foo01234", false)]
        [InlineData("foo.bar", false)]
        [InlineData("foo bar", true)]
        [InlineData("foo\nbar", true)]
        [InlineData("foo\tbar", true)]
        public void Should_detect_whitespace_characters(string value, bool expected)
        {
            var outcome = value.IsWhitepace();
            
            outcome.Should().Be(expected);
        }

        #region Sanitize
        [Theory]
        [InlineData("foo bar@", "foo bar")]
        [InlineData("foo\tbar@", "foo bar")]
        [InlineData("foo-bar@", "foobar")]
        public void Should_sanitize_strings_normalizing_white_spaces(string value, string expected)
        {
            var outcome = value.Sanitize();
            
            outcome.Should().Be(expected);
        }

        [Theory]
        [InlineData("foo\nbar@", "foo\nbar")]
        [InlineData("foo\tbar@", "foo\tbar")]
        public void Should_sanitize_strings_without_normalizing_white_spaces(string value, string expected)
        {
            var outcome = value.Sanitize(normalizeWhiteSpace: false);
            
            outcome.Should().Be(expected);
        }
        #endregion

        [Theory]
        [InlineData("hello this is a test", new object[] {'!', "!!", 10}, "hello ! this !! is 10 a test")]
        public void Should_intersperse_values(string value, object[] values, string expected)
        {
            var outcome = value.Intersperse(values);
            
            outcome.Should().Be(expected);
        }

        [Theory]
        [InlineData("foo", 0, "", "")]
        [InlineData("foo", 1, "", "foo")]
        [InlineData("foo", 5, " ", "foo foo foo foo foo")]
        public void Should_replicate(string value, int count, string separator, string expected)
        {
            var outcome = value.Replicate(count, separator);
            
            outcome.Should().Be(expected);
        }

        #region Mangle
        [Theory]
        [InlineData("foo", 0, 0)]
        [InlineData("foo", 1, 1)]
        [InlineData("fooo", 3, 2)]
        [InlineData("foo bar", 3, 3)]
        public void Should_mangle(string value, int times, int maxLength)
        {
            int expectedMangleSize = times * maxLength;

            var outcome = value.Mangle(times, maxLength);

            outcome.Length.Should().Be(value.Length + expectedMangleSize);

            var mangleSize = (from @char in outcome.ToCharArray()
                            where !char.IsLetterOrDigit(@char) && !char.IsWhiteSpace(@char)
                            select @char).Count();

            mangleSize.Should().Be(expectedMangleSize);
        }

        [Fact]
        public void Mangle_same_string_length_throws_ArgumentException()
        {
            Action action = () => "foo".Mangle(3, 3);

            action.Should().ThrowExactly<ArgumentException>()
                .WithMessage("times");
        }

        [Fact]
        public void Mangle_beyond_string_length_throws_ArgumentException()
        {
            Action action = () => "foo bar baz".Mangle(100, 3);

            action.Should().ThrowExactly<ArgumentException>()
                .WithMessage("times");
        }
        #endregion

        [Theory]
        [InlineData("foo", "foo")]
        [InlineData("  foo  ", "foo")]
        [InlineData("  foo\t    bar\t\t      baz\t", "foo bar baz")]
        public void Should_normalize_white_spaces(string value, string expected)
        {
            var outcome  = value.NormalizeWhiteSpace();
            
            outcome.Should().Be(expected);;
        }

        [Theory]
        [InlineData("foo bar baz", 2, "foo bar baz")]
        [InlineData("fooo bar baz", 3, "fooo  ")]
        public void Should_strip_by_length(string value, int length, string expected)
        {
            var outcome = value.StripByLength(length);
            
            outcome.Should().Be(expected);
        }

        [Theory]
        [InlineData(
            new string[] {"foo bar baz", "fooo baar baaz"},
            new string[] {"foo", "bar", "baz", "fooo", "baar", "baaz"})]
        public void Should_flatten_a_string_sequence_into_words(
            IEnumerable<string> values, IEnumerable<string> expected)
        {
            var outcome = values.FlattenOnce();
            
            outcome.Should().BeEquivalentTo(expected);
        }
    }
}
