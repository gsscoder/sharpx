#pragma warning disable 0618

using System;
using System.Collections.Generic;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using SharpX;
using SharpX.FsCheck;
using Xunit;

namespace Outcomes;

public class EitherSpecs
{
    [Property(Arbitrary = new[] { typeof(ArbitraryValue) })]
    public Property A_non_default_value_is_wrapped_into_a_Left(object leftValue)
    {
        Func<bool> property = () => new Either<object, int>(leftValue).Equals(Either.Left<object, int>(leftValue));

        return property.When(leftValue != default);
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryValue) })]
    public Property A_non_default_value_is_wrapped_into_a_Right(object rightValue)
    {
        Func<bool> property = () => new Either<int, object>(rightValue).Equals(Either.Right<int, object>(rightValue));

        return property.When(rightValue != default);
    }

    [Fact]
    public void Trying_to_get_a_value_from_Left_with_FromLeftOrFail_raises_Exception_in_case_of_Right()
    {
        var sut = Either.Right<string, int>(new CryptoRandom().Next());

        Action action = () => sut.FromLeftOrFail();

        action.Should().ThrowExactly<Exception>()
            .WithMessage("The value is empty.");
    }

    [Fact]
    public void Trying_to_get_a_value_from_Right_with_FromRightOrFail_raises_Exception_in_case_of_Left()
    {
        var sut = Either.Left<string, int>("bad result");

        Action action = () => sut.FromRightOrFail();

        action.Should().ThrowExactly<Exception>()
            .WithMessage("The value is empty.");
    }

    [Fact]
    public void Shoud_partition_lefts_from_rights()
    {
        var eithers = new List<Either<string, int>>()
            {
                Either.Left<string, int>("foo"),
                Either.Right<string, int>(3),
                Either.Left<string, int>("bar"),
                Either.Right<string, int>(7),
                Either.Left<string, int>("baz"),
            };

        var outcome = eithers.Partition();

        outcome.Should().NotBeNull();
        outcome.Item1.Should().NotBeNullOrEmpty()
            .And.HaveCount(3)
            .And.ContainInOrder(eithers.Lefts());
        outcome.Item2.Should().NotBeNullOrEmpty()
            .And.HaveCount(2)
            .And.ContainInOrder(eithers.Rights());
    }
}
