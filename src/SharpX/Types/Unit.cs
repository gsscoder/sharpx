namespace SharpX;

/// <summary>The <c>Unit</c> type is a type that indicates the absence of a specific value.
/// THE type has only a single value, which acts as a placeholder when no other value
/// exists or is needed.</summary>
public readonly struct Unit : IComparable
{
    private static readonly Unit @default = new Unit();

    /// <summary>Returns the hash code for this <c>Unit</c>.</summary>
    public override int GetHashCode() => 0;

    /// <summary>Determines whether this instance and a specified object, which must also be a
    /// <c>Unit</c> object, have the same value.</summary>
    public override bool Equals(object? obj) => obj == null || obj is Unit;

    /// <summary>Compares always to equality.</summary>
    public int CompareTo(object? obj) => 0;

    /// <summary>Converts this instance to a string representation.</summary>
    public override string ToString() => "()";

    /// <summary><c>Unit</c> singleton instance.</summary>
    public static Unit Default { get { return @default; } }

    /// <summary>Returns <c>Unit</c> after executing a delegate.</summary>
    public static Unit Do(Action action)
    {
        Guard.DisallowNull(nameof(action), action);

        action();

        return @default;
    }

    /// <summary>Returns <c>Unit</c> after executing an async delegate.</summary
    public static Task<Unit> DoAsync(Func<Task> func)
    {
        Guard.DisallowNull(nameof(func), func);

        func().Wait();

        return Task.FromResult(@default);
    }
}
