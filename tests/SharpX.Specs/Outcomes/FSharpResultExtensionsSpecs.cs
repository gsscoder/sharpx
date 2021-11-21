using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.FSharp.Core;
using SharpX;
using SharpX.Extensions;
using Xunit;

namespace Outcomes
{
    public class FSharpResultExtensionsSpecs
    {
        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_match_a_result(int value)
        {
            int? expected = null;
            var sut = FSharpResult<int, string>.NewOk(value);

            sut.Match(
                matched => expected = value,
                _ => { throw new InvalidOperationException(); }
            );

            expected.Should().Be(value);
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_match_an_error(int value)
        {
            string error = null;
            var sut = FSharpResult<int, string>.NewError("bad result");

            sut.Match(
                _ => { throw new InvalidOperationException(); },
                message => { error = message; }
            );

            error.Should().Be("bad result");
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_map_a_value(int value)
        {
            var sut = FSharpResult<int, string>.NewOk(value);

            Func<int, double> func = x => x / 0.5;
            var outcome = sut.Map(func);

            outcome.IsOk.Should().BeTrue();
            outcome.ResultValue.Should().Be(func(value));
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_keep_error_when_mapping_a_value_of_a_fail(int value)
        {
            var sut = FSharpResult<int, string>.NewError("bad result");

            var outcome = sut.Map(x => x / 0.5);

            outcome.IsOk.Should().BeFalse();
            outcome.ResultValue.Should().Be(default);
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_bind_a_value(int value)
        {
            var sut = FSharpResult<int, string>.NewOk(value);

            Func<int, FSharpResult<double, string>> func =
                x => FSharpResult<double, string>.NewOk(x / 0.5);
            var outcome = sut.Bind(func);

            outcome.IsOk.Should().BeTrue();
            outcome.ResultValue.Should().Be(func(value).ResultValue);
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_keep_error_when_binding_a_value_of_fail(int value)
        {
            var sut = FSharpResult<int, string>.NewError("bad result");

            var outcome = sut.Bind(x => FSharpResult<double, string>.NewOk(x / 0.5));

            outcome.IsOk.Should().BeFalse();
            outcome.ResultValue.Should().Be(default);
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_return_a_value(int value)
        {
            var sut = FSharpResult<int, string>.NewOk(value);

            var outcome = sut.ReturnOrFail();

            outcome.Should().Be(value);
        }

        [Fact]
        public void Should_throws_exception_on_a_fail()
        {
            var sut = FSharpResult<int, string>.NewError("bad result");

            Action action = () => sut.ReturnOrFail();

            action.Should().ThrowExactly<Exception>()
                .WithMessage("bad result");
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_return_and_map_a_value(int value)
        {
            var sut = FSharpResult<int, string>.NewOk(value);

            Func<int, double> func = x => x / 0.5;
            var outcome = sut.Return(func, 0);

            outcome.Should().Be(func(value));
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_return_alternate_value_on_a_file(int value)
        {
            var sut = FSharpResult<int, string>.NewError("bad result");

            Func<int, double> func = x => x / 0.5;
            var outcome = sut.Return(func, 0);

            outcome.Should().Be(0);
        }

        [Property(Arbitrary = new[] { typeof(ArbitraryIntegers) })]
        public void Should_build_a_Maybe_Just_from_a_success(int value)
        {
            var sut = FSharpResult<int, string>.NewOk(value);

            var outcome = sut.ToMaybe();

            outcome.IsJust().Should().BeTrue();
            outcome.FromJust().Should().Be(value);
        }

        [Fact] 
        public void Should_build_a_Maybe_Nothing_from_a_fail()
        {
            var sut = FSharpResult<int, string>.NewError("bad result");

            var outcome = sut.ToMaybe();

            outcome.IsNothing().Should().BeTrue();
        }
    }
}
