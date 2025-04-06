using System.Runtime.CompilerServices;

namespace SharpX;

/// <summary>Provides static methods for manipulating <c>Maybe</c>.</summary>
public static class Maybe
{
    #region Value case constructors
    /// <summary>Builds the empty case of <c>Maybe</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> Nothing<T>() => new();

    /// <summary>Builds the case when <c>Maybe</c> contains a value.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> Just<T>(T value)
    {
        Guard.DisallowNull(nameof(value), value);
        
        return new Maybe<T>(value);
    }
    #endregion

    #region Monad
    /// <summary>Injects a value into the monadic <c>Maybe</c> type.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> Return<T>(T? value) => Equals(value, default(T)) ? Nothing<T>() : Just(value!);

    /// <summary>Sequentially compose two actions, passing any value produced by the first as
    /// an argument to the second.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T2> Bind<T1, T2>(Maybe<T1> maybe, Func<T1?, Maybe<T2>> onJust)
    {
        Guard.DisallowNull(nameof(maybe), maybe);
        Guard.DisallowNull(nameof(onJust), onJust);

        return maybe.MatchJust(out T1? value) ? onJust(value) : Nothing<T2>();
    }
    #endregion

    #region Functor
    /// <summary>Transforms a <c>Maybe</c> value by using a specified mapping function.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T2> Map<T1, T2>(Maybe<T1> maybe, Func<T1, T2> onJust)
    {
        Guard.DisallowNull(nameof(maybe), maybe);
        Guard.DisallowNull(nameof(onJust), onJust);

        return maybe.MatchJust(out T1? value) ? Just(onJust(value!)) : Nothing<T2>();
    }
    #endregion

    /// <summary>If both <c>Maybe</c> values contain a value, it merges them into a <c>Maybe</c>
    /// with a tupled value. </summary>
    public static Maybe<(T1, T2)> Merge<T1, T2>(Maybe<T1> first, Maybe<T2> second)
    {
        Guard.DisallowNull(nameof(first), first);
        Guard.DisallowNull(nameof(second), second);

        T2? value2 = default;
        return (first.MatchJust(out T1? value1) &&
                second.MatchJust(out value2)) switch {
            true => Just((value1!, value2!)),
            _ => Nothing<(T1, T2)>()
        };
    }

    /// <summary>Executes the given function on a <c>Just</c> success or returns a <c>Nothing</c>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Maybe<T> Try<T>(Func<T> func)
    {
        Guard.DisallowNull(nameof(func), func);

        try {
            return Just(func());
        }
        catch {
            return Nothing<T>();
        }
    }

    /// <summary>Maps <c>Either</c> right value to <c>Just</c>, otherwise returns
    /// <c>Nothing</c>.</summary>
    public static Maybe<TRight> FromEither<TLeft, TRight>(Either<TLeft, TRight> either)
    {
        Guard.DisallowNull(nameof(either), either);

        return (either.Tag == EitherType.Right) switch
        {
            true => Just(either.FromRight()!),
            _ => Nothing<TRight>()
        };
    }
}
