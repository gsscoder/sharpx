using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using SharpX;
using System;
using System.Collections.Generic;
using Xunit;

public class EitherSpecs
{
    [Property(Arbitrary = new[] { typeof(ArbitraryListOfStrings) })]
    public void Shoud_build_Left(string[] values)
    {
        values.ForEach(value => {
            if (value == null) return; // Skip null values

            var outcome = Either.Left<string, int>(value);

            outcome.IsLeft().Should().BeTrue();
            outcome.FromLeft().Should().Be(value);
        });
    }

    [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
    public void Shoud_build_Right(int value)
    {
        if (value == default) return; // Skip default values

        var outcome = Either.Right<string, int>(value);

        outcome.IsRight().Should().BeTrue();
        outcome.FromRight().Should().Be(value);
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
