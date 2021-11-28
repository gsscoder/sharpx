using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using SharpX;
using SharpX.Extensions;
using Xunit;
using Outcome = SharpX.Outcome;

namespace Outcomes
{
    public class OutcomeSpecs
    {
        static Random _random = new CryptoRandom();

        [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
        public void Error_with_same_string_and_exception_are_equal(string[] values)
        {
            values.ForEach(value =>
            {
                if (value == null) return;  // Skip null values

                var outcome = new Error(
                    $"custom message {value}", new Exception($"exception message {value}")).Equals(
                            new Error(
                    $"custom message {value}", new Exception($"exception message {value}")));

                outcome.Should().BeTrue();
            });
        }

        [Fact]
        public void Shoud_build_Success()
        {
            var outcome = Outcome.Success;

            outcome.Tag.Should().Be(OutcomeType.Success);
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
        public void Shoud_build_Failure_with_string(string[] values)
        {
            values.ForEach(value =>
            {
                if (value == null) return; // Skip null values
                if (value.Trim().Length == 0) return; // Skip white spaces values

                var outcome = Outcome.Failure(value);

                outcome.Tag.Should().Be(OutcomeType.Failure);
                outcome._error.Should().Be(new Error(value, null));
            });
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
        public void Shoud_build_Failure_with_string_and_exception(string[] values)
        {
            values.ForEach(value =>
            {
                if (value == null) return;  // Skip null values

                var outcome = Outcome.Failure($"custom message {value}",
                    new Exception($"exception message {value}"));

                outcome.Tag.Should().Be(OutcomeType.Failure);
                outcome._error.Should().Be(
                    new Error($"custom message {value}", new Exception($"exception message {value}")));
            });
        }

        [Fact]
        public void Result_of_type_Success_are_equal()
        {
            var result1 = Outcome.Success;
            var result2 = Outcome.Success;
            
            var outcome = result1.Equals(result2);

            outcome.Should().BeTrue();
        }

        [Fact]
        public void Result_of_type_Failure_with_same_error_are_equal()
        {
            var result1 = Outcome.Failure("something gone wrong", new Exception("here a trouble"));
            var result2 = Outcome.Failure("something gone wrong", new Exception("here a trouble"));
            
            var outcome = result1.Equals(result2);

            outcome.Should().BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
        public void Result_of_type_Failure_with_different_error_strings_are_not_equal(string[] values)
        {
            values.ForEach(value =>
            {
                if (value == null) return;  // Skip null values
                if (value.Trim().Length == 0) return; // Skip white spaces values

                var result1 = Outcome.Failure(
                    value, new Exception("here a trouble"));
                var result2 = Outcome.Failure(
                    $"{value}{Strings.Generate(3)}", new Exception("here a trouble"));
            
                var outcome = result1.Equals(result2);

                outcome.Should().BeFalse();
            });
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
        public void Result_of_type_Failure_with_different_exceptions_are_not_equal(string[] values)
        {
            values.ForEach(value =>
            {
                if (value == null) return;  // Skip null values

                var result1 = Outcome.Failure(
                    "something gone wrong", new Exception(value));
                var result2 = Outcome.Failure(
                    "something gone wrong", new Exception($"{value}{Strings.Generate(3)}"));
            
                var outcome = result1.Equals(result2);

                outcome.Should().BeFalse();
            });
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
        public void Result_of_type_Failure_with_different_errors_are_not_equal(string[] values)
        {
            values.ForEach(value =>
            {
                if (value == null) return; // Skip null values
                if (value.Trim().Length == 0) return; // Skip white spaces values

                var result1 = Outcome.Failure(
                    value, new Exception(value));
                var result2 = Outcome.Failure(
                    $"{value}{Strings.Generate(3)}", new Exception($"{value}{Strings.Generate(3)}"));
            
                var outcome = result1.Equals(result2);

                outcome.Should().BeFalse();
            });
        }

        [Fact]
        public void Should_match_Success()
        {
            var result = Outcome.Success;

            var outcome = result.MatchSuccess();

            outcome.Should().BeTrue();
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
        public void Should_match_Failure(string[] values)
        {
            values.ForEach(value =>
            {
                if (value == null) return; // Skip null values
                if (value.Trim().Length == 0) return; // Skip white spaces values

                var result = Outcome.Failure(value, new Exception("here a trouble"));

                var outcome = result.MatchFailure(out Error outcome1);

                outcome.Should().BeTrue();
                outcome1.Should().Be(new Error(value, new Exception("here a trouble")));
            });
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
        public void Should_match_Failure_with_message_only(string[] values)
        {
            values.ForEach(value =>
            {
                if (value == null) return; // Skip null values
                if (value.Trim().Length == 0) return; // Skip white spaces values

                var result = Outcome.Failure(value);

                var outcome = result.MatchFailure(out Error outcome1);

                outcome.Should().BeTrue();
                outcome1.Should().Be(new Error(value, null));
            });
        } 
    }
}
