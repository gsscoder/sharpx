namespace SharpX;

/// <summary>Represents the result of a failed computation.</summary>
public sealed class Bad<TSuccess, TMessage> : Result<TSuccess, TMessage>
{
    readonly IEnumerable<TMessage> _messages;

    public Bad(IEnumerable<TMessage> messages)
        : base(ResultType.Bad)
    {
        Guard.DisallowNull(nameof(messages), messages);

        _messages = messages;
    }

    public IEnumerable<TMessage> Messages => _messages;
}
