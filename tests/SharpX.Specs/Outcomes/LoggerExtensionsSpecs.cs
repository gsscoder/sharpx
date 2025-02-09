using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Sut = SharpX.Extensions.LoggerExtensions;

namespace Outcomes;

public class LoggerExtensionsSpecs
{
    private class EmptyObject { }

    [Theory]
    [InlineData("WarnWith")]
    [InlineData("FailWith")]
    [InlineData("PanicWith")]
    public void Methods_returns_a_reference_type(string methodName)
    {
        var logger = new Mock<ILogger>().Object;

        var outcome = methodName != "PanicWith"
            ? InvokeOnSut<EmptyObject>(methodName, logger, "Message {Data}", new EmptyObject(), new object[] { "data value" })
            : InvokeOnSut<EmptyObject>(methodName, logger, "Message {Data}", new EmptyObject(), new Exception(), new object[] { "data value" });

        outcome.Should().NotBeNull();
        outcome.GetType().Should().Be(typeof(EmptyObject));
    }

    [Theory]
    [InlineData("WarnWith")]
    [InlineData("FailWith")]
    [InlineData("PanicWith")]
    public void Methods_returns_a_value_type(string methodName)
    {
        var logger = new Mock<ILogger>().Object;

        var outcome = methodName != "PanicWith"
            ? InvokeOnSut<bool>(methodName, logger, "Message {Data}", false, new object[] { "data value" })
            : InvokeOnSut<bool>(methodName, logger, "Message {Data}", false, new Exception(), new object[] { "data value" });

        outcome.Should().NotBeNull();
        outcome.GetType().Should().Be(typeof(bool));
    }

    [Theory]
    [InlineData("WarnWith")]
    [InlineData("FailWith")]
    [InlineData("PanicWith")]
    public void Methods_raise_an_exception_when_passing_a_null_reference_type(string methodName)
    {
        var logger = new Mock<ILogger>().Object;

        Action test = () =>
        {
            _ = methodName != "PanicWith"
                ? InvokeOnSut<EmptyObject>(methodName, logger, "Message {Data}", (EmptyObject)null, new object[] { "data value" })
                : InvokeOnSut<EmptyObject>(methodName, logger, "Message {Data}", (EmptyObject)null, new Exception(), new object[] { "data value" });
        };

        test.Should().Throw<TargetInvocationException>()
            .WithInnerException<ArgumentNullException>().WithMessage("returnValue cannot be null. (Parameter 'returnValue')");
    }

    private static object InvokeOnSut<T>(string methodName, params object[] args)
    {
        var method = typeof(Sut).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(x => x.Name == methodName && x.IsGenericMethod);
        var generic = method.MakeGenericMethod(typeof(T));

        return generic.Invoke(null, args);
    }
}
