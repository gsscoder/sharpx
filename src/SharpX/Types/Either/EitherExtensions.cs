
namespace SharpX;

public static class EitherExtensions
{
    #region LINQ operators
    /// <summary>Map operation compatible with LINQ.</summary>
    public static Either<TLeft, TResult> Select<TLeft, TRight, TResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, TResult> selector) => Either.Map(either, selector);

    /// <summary>Map operation compatible with LINQ.</summary>
    public static Either<TLeft, TResult> SelectMany<TLeft, TRight, TResult>(this Either<TLeft, TRight> result,
        Func<TRight, Either<TLeft, TResult>> func) => Either.Bind(result, func);
    #endregion

    #region Alternative match extensions
    public static Unit Match<TLeft, TRight>(this Either<TLeft, TRight> either,
        Func<TLeft, Unit> onLeft, Func<TRight, Unit> onRight)
    {
        Guard.DisallowNull(nameof(either), either);
        Guard.DisallowNull(nameof(onLeft), onLeft);
        Guard.DisallowNull(nameof(onRight), onRight);

        return either.MatchRight(out TRight? right) switch {
            true => onRight(right!),
            _ =>    onLeft(either.FromLeft())
        };
    }
    #endregion

    /// <summary>Equivalent to monadic <c>Return</c> operation. Builds a <c>Right</c> value
    /// by default.</summary>
    public static Either<string, TRight> ToEither<TRight>(this TRight value) => Either.Return<TRight>(value);

    /// <summary>Equivalent to monadic Bind.</summary>
    public static Either<TLeft, TResult> Bind<TLeft, TRight, TResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, Either<TLeft, TResult>> func) => Either.Bind(either, func);

    /// <summary>Equivalent to monadic Map.</summary>
    public static Either<TLeft, TResult> Map<TLeft, TRight, TResult>(
        this Either<TLeft, TRight> either,
        Func<TRight, TResult> func) => Either.Map(either, func);

    /// <summary>Eviqualent to monadic Bimap.</summary>
    public static Either<TLeft1, TRight1> Bimap<TLeft, TRight, TLeft1, TRight1>(
        this Either<TLeft, TRight> either,
        Func<TLeft, TLeft1> mapLeft,
        Func<TRight, TRight1> mapRight) => Either.Bimap(either, mapLeft, mapRight);

    /// <summary>Returns <c>true</c> if it is in form of <c>Left</c>.</summary>
    public static bool IsLeft<TLeft, TRight>(this Either<TLeft, TRight> either) =>
        either.Tag == EitherType.Left;

    /// <summary>Returns <c>true</c> if it is in form of <c>Right</c>.</summary>
    public static bool IsRight<TLeft, TRight>(this Either<TLeft, TRight> either) =>
        either.Tag == EitherType.Right;

    /// <summary>Extracts the element out of <c>Left</c> and returns a default value (or <c>noneValue</c>
    /// when given) if it is in form of <c>Right</c>.</summary>
    public static TLeft FromLeft<TLeft, TRight>(this Either<TLeft, TRight> either,
        TLeft? noneValue = default) => either.MatchLeft(out TLeft? value) ? value! : noneValue!;

    /// <summary>Extracts the element out of <c>Left</c> and throws an exception if it is form of
    /// <c>Right</c>.</summary>
    public static TLeft FromLeftOrFail<TLeft, TRight>(this Either<TLeft, TRight> either,
        Exception? exceptionToThrow = null)
    {
        Guard.DisallowNull(nameof(either), either);

        if (either.MatchLeft(out TLeft? value)) {
            return value!;
        }
        throw exceptionToThrow ?? new Exception("The value is empty.");
    }

    /// <summary>Extracts the element out of <c>Left</c> and returns a default (or <c>noneValue</c>
    /// when given) value if it is in form of<c>Right</c>.</summary>
    public static TRight? FromRight<TLeft, TRight>(this Either<TLeft, TRight> either,
        TRight? noneValue = default) => either.MatchRight(out TRight? value) ? value : noneValue;

    /// <summary>Extracts the element out of <c>Left</c> and throws an exception if it is form of
    /// <c>Right</c>.</summary>
    public static TRight? FromRightOrFail<TLeft, TRight>(this Either<TLeft, TRight> either,
        Exception? exceptionToThrow = null)
    {
        Guard.DisallowNull(nameof(either), either);

        if (either.MatchRight(out TRight? value)) {
            return value;
        }
        throw exceptionToThrow ?? new Exception("The value is empty.");
    }

    #region Sequences
    /// <summary>Extracts from a sequence of <c>Either</c> all the <c>Left</c> elements. All the
    /// <c>Left</c> elements are extracted in order.</summary>
    public static IEnumerable<TLeft> Lefts<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> source)
    {
        Guard.DisallowNull(nameof(source), source);

        return _(); IEnumerable<TLeft> _()
        {
            foreach (var either in source) {
                if (either.Tag == EitherType.Left) yield return either.FromLeft();
            }
        }
    }

    /// <summary>Extracts from a sequence of <c>Either</c> all the <c>Right</c> elements. All the
    /// <c>Rights</c> elements are extracted in order.</summary>
    public static IEnumerable<TRight> Rights<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> source)
    {
        Guard.DisallowNull(nameof(source), source);

        return _(); IEnumerable<TRight> _()
        {
            foreach (var either in source) {
                if (either.Tag == EitherType.Right) yield return either.FromRight()!;
            }
        }
    }       

    /// <summary>Partitions a sequence of <c>Either</c> into two sequences. All the <c>Left</c>
    /// elements are extracted, in order, to the first component of the pair. Similarly the <c>Right</c>
    /// elements are extracted to the second component of the pair.</summary>
    public static (IEnumerable<TLeft>, IEnumerable<TRight>) Partition<TLeft, TRight>(
        this IEnumerable<Either<TLeft, TRight>> source)
    {
        Guard.DisallowNull(nameof(source), source);

        var lefts = new List<TLeft>();
        var rights = new List<TRight>();

        foreach (var either in source) {
            if (either.Tag == EitherType.Left) lefts.Add(either.FromLeft());
            else rights.Add(either.FromRight()!);
        }
        return (lefts, rights);
    }
    #endregion
}
