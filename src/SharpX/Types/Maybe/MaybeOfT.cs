
#pragma warning disable 8601, 8602, 8603, 8619
using System.Text;

namespace SharpX;

/// <summary>The <c>Maybe</c> type models an optional value. A value of type <c>Maybe</c> either
/// contains a value (represented as <c>Just</c> a), or it is empty (represented as
/// <c>Nothing</c>).</summary>
public readonly struct Maybe<T> : IEquatable<Maybe<T>>
{
#if DEBUG
    internal readonly T? _value;
#else
    private readonly T? _value;
#endif
    private readonly MaybeType _tag;

    internal Maybe(T value)
    {
        _value = value;
        _tag = MaybeType.Just;
    }

    /// <summary>Type discriminator.</summary>
    public readonly MaybeType Tag { get => _tag; }

    /// <summary>Determines whether this instance and another specified <c>Maybe</c> object have the same value.</summary>
    public override bool Equals(object? other)
    {
        if (other is null) return false;
        if (other is not Maybe<T> maybe) return false;

        var casted = (Maybe<T>)other;

        return Equals(casted);
    }

    /// <summary>Determines whether this instance and another specified <c>Maybe</c> object have the same value.</summary>
    public bool Equals(Maybe<T> other) =>
        other.Tag != MaybeType.Just || _value.Equals(other._value);

    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);

    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !left.Equals(right);

    /// <summary>Serves as the default hash function.</summary>
    public override int GetHashCode()
    {
        unchecked {
            var hashCode = 2;
            hashCode = hashCode * 3 * typeof(Maybe<T>).GetHashCode();
            if (Tag == MaybeType.Just)
                hashCode = hashCode * 3 + _value.GetHashCode();
            return hashCode;
        }
    }

    public override string ToString() =>
        Tag switch {
            MaybeType.Just => new StringBuilder("Just(")
                                    .Append(_value)
                                    .Append(")")
                                    .ToString(),
            _ => "<Nothing>"
        };

    #region Basic match methods
    /// <summary>Matches a value returning <c>true</c> and value itself via an output
    /// parameter.</summary>
    public bool MatchJust(out T value)
    {
        value = Tag == MaybeType.Just ? _value : default;
        return Tag == MaybeType.Just;
    }

    /// <summary>Matches an empty value returning <c>true</c>.</summary>
    public bool MatchNothing() => Tag == MaybeType.Nothing;
    #endregion
}
