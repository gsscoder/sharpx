using System;
using System.Linq;
using FluentAssertions;
using SharpX;
using Xunit;

namespace Outcomes
{
    public class Result
    {
        [Fact]
        public void Should_fail_when_Try_catches_an_exception()
        {
            var exn = new Exception("Hello World");
            var result = Result<object, object>.Try(() => { throw exn; });
            exn.Should().Be(result.FailedWith().First());
        }

        [Fact]
        public void Should_succeed_when_Try_completes_without_an_exception()
        {
            var result = Result<string, object>.Try(() => "hello world");
            "hello world".Should().Be(result.SucceededWith());
        }
    }
}
