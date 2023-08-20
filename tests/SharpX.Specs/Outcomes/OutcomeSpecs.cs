#pragma warning disable 0618

using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using SharpX;
using SharpX.Extensions;
using SharpX.FsCheck;
using Xunit;
using Outcome = SharpX.Outcome;

namespace Outcomes;

public class OutcomeSpecs
{
    static Random _random = new CryptoRandom();

    [Property(Arbitrary = new[] { typeof(ArbitraryStringSeq) })]
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

    [Property(Arbitrary = new[] { typeof(ArbitraryString) })]
    public Property Shoud_build_Failure_with_string(string value)
    {
        Func<bool> property = () => {
            var outcome = Outcome.Failure(value);

            return OutcomeType.Failure == outcome.Tag &&
                   new Error(value, null) == outcome._error;
        };

        return property.When(value != default);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryString) })]
    public void Shoud_build_Failure_with_string_and_exception(string value)
    {
        var outcome = Outcome.Failure($"custom message {value}",
            new Exception($"exception message {value}"));

        outcome.Tag.Should().Be(OutcomeType.Failure);
        outcome._error.Should().Be(
            new Error($"custom message {value}", new Exception($"exception message {value}")));
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

    [Property(Arbitrary = new[] { typeof(ArbitraryString) })]
    public Property Result_of_type_Failure_with_different_error_strings_are_not_equal(string value)
    {
        Func<bool> property = () => {
            var result1 = Outcome.Failure(
                value, new Exception("here a trouble"));
            var result2 = Outcome.Failure(
                $"{value}{Strings.Generate(3)}", new Exception("here a trouble"));

            return false == result1.Equals(result2);
        };

        return property.When(value != default);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryString) })]
    public void Result_of_type_Failure_with_different_exceptions_are_not_equal(string value)
    {
        var result1 = Outcome.Failure(
            "something gone wrong", new Exception(value));
        var result2 = Outcome.Failure(
            "something gone wrong", new Exception($"{value}{Strings.Generate(3)}"));

        var outcome = result1.Equals(result2);

        outcome.Should().BeFalse();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryString) })]
    public Property Result_of_type_Failure_with_different_errors_are_not_equal(string value)
    {
        Func<bool> property = () => {
            var result1 = Outcome.Failure(
                value, new Exception(value));
            var result2 = Outcome.Failure(
                $"{value}{Strings.Generate(3)}", new Exception($"{value}{Strings.Generate(3)}"));

            return false == result1.Equals(result2);
        };

        return property.When(value != default);
    }

    [Fact]
    public void Should_match_Success()
    {
        var result = Outcome.Success;

        var outcome = result.MatchSuccess();

        outcome.Should().BeTrue();
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryString) })]
    public Property Should_match_Failure(string value)
    {
        Func<bool> property = () => {
            var result = Outcome.Failure(value, new Exception("here a trouble"));
            var outcome1 = result.MatchFailure(out Error outcome2);

            return true == outcome1 &&
                new Error(value, new Exception("here a trouble")) == outcome2;
        };

        return property.When(value != default);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryString) })]
    public Property Should_match_Failure_with_message_only(string value)
    {
        Func<bool> property = () => {
            var result = Outcome.Failure(value);

            var outcome1 = result.MatchFailure(out Error outcome2);

            return true == outcome1 && outcome2 == new Error(value, null);
        };

        return property.When(value != default);
    }
}
