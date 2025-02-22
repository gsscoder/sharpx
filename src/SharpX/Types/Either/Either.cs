
#pragma warning disable 8600, 8601, 8603, 8604, 8616, 8618
namespace SharpX;

public static class Either
{
    #region Value case constructors
    /// <summary>Builds the <c>Left</c> case of an <c>Either</c> value.</summary>
    public static Either<TLeft, TRight> Left<TLeft, TRight>(TLeft value) =>
        new Either<TLeft, TRight>(value);

    /// <summary>Builds the <c>Right</c> case of an <c>Either</c> value.</summary>
    public static Either<TLeft, TRight> Right<TLeft, TRight>(TRight value) =>
        new Either<TLeft, TRight>(value);
    #endregion

    #region Monad
    /// <summary>Inject a value into the <c>Either</c> type, returning Right case.</summary>
    public static Either<string, TRight> Return<TRight>(TRight value) =>
        Either.Right<string, TRight>(value);

    /// <summary>Monadic bind.</summary>
    public static Either<TLeft, TResult> Bind<TLeft, TRight, TResult>(
        Either<TLeft, TRight> either, Func<TRight, Either<TLeft, TResult>> func)
    {
        Guard.DisallowNull(nameof(either), either);
        Guard.DisallowNull(nameof(func), func);

        if (either.MatchRight(out TRight right)) {
            return func(right);
        }
        return Either.Left<TLeft, TResult>(either.GetLeft());
    }
    #endregion

    #region Functor
    /// <summary>Transforms a <c>Either</c> right value by using a specified mapping function.</summary>
    public static Either<TLeft, TResult> Map<TLeft, TRight, TResult>(Either<TLeft, TRight> either,
        Func<TRight, TResult> func)
    {
        Guard.DisallowNull(nameof(either), either);
        Guard.DisallowNull(nameof(func), func);

        if (either.MatchRight(out TRight right)) {
            return Either.Right<TLeft, TResult>(func(right));
        }
        return Either.Left<TLeft, TResult>(either.GetLeft());
    }
    #endregion

    #region Bifunctor
    /// <summary>Maps both parts of a Either type. Applies the first function if <c>Either</c>
    /// is <c>Left</c>. Otherwise applies the second function.</summary>
    public static Either<TLeft1, TRight1> Bimap<TLeft, TRight, TLeft1, TRight1>(Either<TLeft, TRight> either,
        Func<TLeft, TLeft1> mapLeft, Func<TRight, TRight1> mapRight)
    {
        Guard.DisallowNull(nameof(either), either);
        Guard.DisallowNull(nameof(mapLeft), mapLeft);
        Guard.DisallowNull(nameof(mapRight), mapRight);

        if (either.MatchRight(out TRight right)) {
            return Either.Right<TLeft1, TRight1>(mapRight(right));
        }
        return Either.Left<TLeft1, TRight1>(mapLeft(either.GetLeft()));
    }
    #endregion

    /// <summary>Fail with a message. Not part of mathematical definition of a monad.</summary>
    public static Either<string, TRight> Fail<TRight>(string message) => throw new Exception(message);

    /// <summary>Wraps a function, encapsulates any exception thrown within to a <c>Either</c>.</summary>
    public static Either<Exception, TRight> Try<TRight>(Func<TRight> func)
    {
        Guard.DisallowNull(nameof(func), func);

        try {
            return new Either<Exception, TRight>(func());
        }
        catch (Exception ex) {
            return new Either<Exception, TRight>(ex);
        }
    }

    /// <summary>Attempts to cast an object. Stores the cast value in <c>Right</c> if successful, otherwise
    /// stores the exception in <c>Left</c>.</summary>
    public static Either<Exception, TRight> Cast<TRight>(object obj) => Either.Try(() => (TRight)obj);

    /// <summary>Converts a <c>Just</c> value to a <c>Right</c> and a <c>Nothing</c> value to a
    /// <c>Left</c>.</summary>
    public static Either<TLeft, TRight> FromMaybe<TLeft, TRight>(Maybe<TRight> maybe, TLeft left)
    {
        Guard.DisallowNull(nameof(maybe), maybe);
        Guard.DisallowNull(nameof(left), left);

        if (maybe.Tag == MaybeType.Just) {
            return Either.Right<TLeft, TRight>(maybe.FromJust());
        }
        return Either.Left<TLeft, TRight>(left);
    }

    private static TLeft? GetLeft<TLeft, TRight>(this Either<TLeft, TRight> either) => either.FromLeft();
}
