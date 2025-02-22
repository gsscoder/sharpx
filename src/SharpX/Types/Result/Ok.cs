namespace SharpX;

/// <summary>Represents the result of a successful computation.</summary>
public sealed class Ok<TSuccess, TMessage> : Result<TSuccess, TMessage>
{
    readonly TSuccess _success;
    readonly IEnumerable<TMessage> _messages;

    public Ok(TSuccess success, IEnumerable<TMessage> messages)
        : base(ResultType.Ok)
    {
        Guard.DisallowNull(nameof(success), success);
        Guard.DisallowNull(nameof(messages), messages);

        _success = success;
        _messages = messages;
    }

    public TSuccess Success => _success;

    public IEnumerable<TMessage> Messages => _messages;
}
