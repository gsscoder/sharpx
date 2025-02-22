#pragma warning disable 8602
namespace SharpX;

/// <summary>Represents the result of a computation.</summary>
public abstract class Result<TSuccess, TMessage>
{
    private readonly ResultType _tag;
    
    protected Result(ResultType tag)
    {
        _tag = tag;
    }

    public ResultType Tag { get => _tag; }

    /// <summary>Creates a Failure result with the given messages.</summary>
    public static Result<TSuccess, TMessage> FailWith(IEnumerable<TMessage> messages)
    {
        Guard.DisallowNull(nameof(messages), messages);

        return new Bad<TSuccess, TMessage>(messages);
    }

    /// <summary>Creates a Failure result with the given message.</summary>
    public static Result<TSuccess, TMessage> FailWith(TMessage message)
    {
        Guard.DisallowNull(nameof(message), message);

        return new Bad<TSuccess, TMessage>(new[] { message });
    }

    /// <summary>Creates a Success result with the given value.</summary>
    public static Result<TSuccess, TMessage> Succeed(TSuccess value)
    {
        Guard.DisallowNull(nameof(value), value);

        return new Ok<TSuccess, TMessage>(value, Enumerable.Empty<TMessage>());
    }

    /// <summary>Creates a Success result with the given value and the given message.</summary>
    public static Result<TSuccess, TMessage> Succeed(TSuccess value, TMessage message)
    {
        Guard.DisallowNull(nameof(value), value);
        Guard.DisallowNull(nameof(message), message);

        return new Ok<TSuccess, TMessage>(value, new[] { message });
    }

    /// <summary>Creates a Success result with the given value and the given messages.</summary>
    public static Result<TSuccess, TMessage> Succeed(TSuccess value, IEnumerable<TMessage> messages)
    {
        Guard.DisallowNull(nameof(value), value);
        Guard.DisallowNull(nameof(messages), messages);

        return new Ok<TSuccess, TMessage>(value, messages);
    }

    /// <summary>Executes the given function on a given success or captures the failure.</summary>
    public static Result<TSuccess, Exception> Try(Func<TSuccess> func)
    {
        Guard.DisallowNull(nameof(func), func);

        try {
            return new Ok<TSuccess, Exception>(
                    func(), Enumerable.Empty<Exception>());
        }
        catch (Exception ex) {
            return new Bad<TSuccess, Exception>(
                new[] { ex });
        }
    }

    public override string ToString()
    {
        switch (Tag) {
            default:
                var ok = (Ok<TSuccess, TMessage>)this;
                return string.Format(
                    "OK: {0} - {1}",
                    ok.Success,
                    string.Join(Environment.NewLine, ok.Messages.Select(v => v.ToString())));
            case ResultType.Bad:
                var bad = (Bad<TSuccess, TMessage>)this;
                return string.Format(
                    "Error: {0}",
                    string.Join(Environment.NewLine, bad.Messages.Select(v => v.ToString())));
        }
    }
}
