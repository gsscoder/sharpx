using System;
using System.Linq;
using FluentAssertions;
using SharpX;
using Xunit;

namespace Tests
    {
    public class Request
    {
        public string Name { get; set; }
        public string EMail { get; set; }
    }

    public class Validation
    {
        public static Result<Request, string> ValidateInput(Request input)
        {
            if (input.Name == "") {
                return Result<Request, string>.FailWith("Name must not be blank");
            }
            if (input.EMail == "") {
                return Result<Request, string>.FailWith("Email must not be blank");
            }
            return Result<Request, string>.Succeed(input);

        }
    }

    public class SimpleValidation
    {
        [Fact]
        public void CanCreateSuccess()
        {
            var request = new Request { Name = "Giacomo", EMail = "mail@support.com" };
            var result = Validation.ValidateInput(request);
            request.Should().Be(result.SucceededWith());
        }
    }

    public class SimplePatternMatching
    {
        [Fact]
        public void CanMatchSuccess()
        {
            var request = new Request { Name = "Giacomo", EMail = "mail@support.com" };
            var result = Validation.ValidateInput(request);
            result.Match(
                (x, msgs) => { request.Should().Be(x); },
                msgs => { throw new Exception("wrong match case"); });
        }

        [Fact]
        public void CanMatchFailure()
        {
            var request = new Request { Name = "Giacomo", EMail = "" };
            var result = Validation.ValidateInput(request);
            result.Match(
                (x, msgs) => { throw new Exception("wrong match case"); },
                msgs => { "Email must not be blank".Should().Be(msgs.ElementAt(0)); });
        }
    }

    public class SimpleEitherPatternMatching
    {
        [Fact]
        public void CanMatchSuccess()
        {
            var request = new Request { Name = "Giacomo", EMail = "mail@support.com" };
            var result =
                Validation
                    .ValidateInput(request)
                    .Either(
                    (x, msgs) => x,
                    msgs => { throw new Exception("wrong match case"); });
            request.Should().Be(result);
        }

        [Fact]
        public void CanMatchFailure()
        {
            var request = new Request { Name = "Giacomo", EMail = "" };
            var result =
                Validation.ValidateInput(request)
                .Either(
                    (x, msgs) => { throw new Exception("wrong match case"); },
                    msgs => msgs.ElementAt(0));

            "Email must not be blank".Should().Be(result);
        }
    }
}
