using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SharpX;

public class TrailSpecs
{
    [Fact]
    public void Should_collect_successes()
    {
        var ok1 = Result<int, string>.Succeed(1, "ok 1");
        var ok2 = Result<int, string>.Succeed(2);
        var ok3 = Result<int, string>.Succeed(2, "ok 3");
        var sut = Trial.Collect(new Result<int, string>[] { ok1, ok2, ok3 });

        var outcome = sut as Ok<IEnumerable<int>, string>;

        outcome.Should().NotBeNull();
        outcome.Success.Should().NotBeNullOrEmpty().And
            .ContainInOrder(1,2);
        outcome.Messages.Should().NotBeNullOrEmpty().And
            .ContainInOrder("ok 1", "ok 3");
    }

    [Fact]
    public void Should_propagate_fails()
    {
        var ok1 = Result<int, string>.Succeed(1, "ok 1");
        var ok2 = Result<int, string>.Succeed(2);
        var fail1 = Result<int, string>.FailWith("non ok 1");
        var fail2 = Result<int, string>.FailWith("non ok 2");
        var sut = Trial.Collect(new Result<int, string>[] { ok1, ok2, fail1, fail2 });

        var outcome = sut as Bad<IEnumerable<int>, string>;

        outcome.Should().NotBeNull();
        outcome.Messages.Should().NotBeNullOrEmpty().And
            .ContainInOrder("ok 1", "non ok 1", "non ok 2");
    }
}
