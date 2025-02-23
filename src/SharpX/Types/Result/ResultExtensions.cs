using System.Runtime.CompilerServices;

namespace SharpX;

/// <summary>Extensions methods for easier usage.</summary>
public static class ResultExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <summary>Builds a Maybe type instance from a Result one.</summary>
    public static Maybe<TSuccess> ToMaybe<TSuccess, TMessage>(this Result<TSuccess, TMessage> result)
    {
        Guard.DisallowNull(nameof(result), result);

        return result.Tag == ResultType.Ok
                ? Maybe.Just(((Ok<TSuccess, TMessage>)result).Success)
                : Maybe.Nothing<TSuccess>();
    }

    /// <summary>Allows pattern matching on Results.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Match<TSuccess, TMessage>(this Result<TSuccess, TMessage> result,
        Action<TSuccess, IEnumerable<TMessage>> ifSuccess,
        Action<IEnumerable<TMessage>> ifFailure)
    {
        Guard.DisallowNull(nameof(result), result);
        Guard.DisallowNull(nameof(ifSuccess), ifSuccess);
        Guard.DisallowNull(nameof(ifFailure), ifFailure);

        if (result is Ok<TSuccess, TMessage> ok) {
            ifSuccess(ok.Success, ok.Messages);
            return;
        }
        var bad = (Bad<TSuccess, TMessage>)result;
        ifFailure(bad.Messages);
    }

    /// <summary>Allows pattern matching on Results.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Either<TSuccess, TMessage, TResult>(this Result<TSuccess, TMessage> result,
        Func<TSuccess, IEnumerable<TMessage>, TResult> ifSuccess,
        Func<IEnumerable<TMessage>, TResult> ifFailure) => Trial.Either(result, ifSuccess, ifFailure);

    /// <summary>Lifts a Func into a Result and applies it on the given result.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TMessage> Map<TSuccess, TMessage, TResult>(
        this Result<TSuccess, TMessage> result, Func<TSuccess, TResult> func) =>
        Trial.Lift(func, result);

    /// <summary>Collects a sequence of Results and accumulates their values. If the sequence
    /// contains an error the error will be propagated.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<IEnumerable<TSuccess>, TMessage> Collect<TSuccess, TMessage>(
        this IEnumerable<Result<TSuccess, TMessage>> values) => Trial.Collect(values);

    /// <summary>Collects a sequence of Results and accumulates their values. If the sequence
    /// contains an error the error will be propagated.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<IEnumerable<TSuccess>, TMessage> Flatten<TSuccess, TMessage>(
        this Result<IEnumerable<Result<TSuccess, TMessage>>, TMessage> result)
    {
        Guard.DisallowNull(nameof(result), result);

        if (result.Tag == ResultType.Ok) {
            var ok = (Ok<IEnumerable<Result<TSuccess, TMessage>>, TMessage>)result;
            var values = ok.Success;
            var result1 = Collect(values);
            if (result1.Tag == ResultType.Ok) {
                var ok1 = (Ok<IEnumerable<TSuccess>, TMessage>)result1;
                return new Ok<IEnumerable<TSuccess>, TMessage>(ok1.Success, ok1.Messages);
            }
            var bad1 = (Bad<IEnumerable<TSuccess>, TMessage>)result1;
            return new Bad<IEnumerable<TSuccess>, TMessage>(bad1.Messages);
        }
        var bad = (Bad<IEnumerable<Result<TSuccess, TMessage>>, TMessage>)result;
        return new Bad<IEnumerable<TSuccess>, TMessage>(bad.Messages);
    }

    /// <summary>If the result is a Success it executes the given Func on the value. Otherwise
    /// the exisiting failure is propagated.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TMessage> SelectMany<TSuccess, TMessage, TResult>(this Result<TSuccess, TMessage> result,
        Func<TSuccess, Result<TResult, TMessage>> func) => Trial.Bind(result, func);

    /// <summary>If the result is a Success it executes the given Func on the value. If the result
    /// of the Func is a Success it maps it using the given Func. Otherwise the exisiting failure
    /// is propagated.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TMessage> SelectMany<TSuccess, TMessage, TValue, TResult>(
        this Result<TSuccess, TMessage> result,
        Func<TSuccess, Result<TValue, TMessage>> func,
        Func<TSuccess, TValue, TResult> mapperFunc)
    {
        Guard.DisallowNull(nameof(result), result);
        Guard.DisallowNull(nameof(func), func);
        Guard.DisallowNull(nameof(mapperFunc), mapperFunc);

        Func<TSuccess, Func<TValue, TResult>> curriedMapper = suc => val => mapperFunc(suc, val);
        Func<
            Result<TSuccess, TMessage>,
            Result<TValue, TMessage>,
            Result<TResult, TMessage>
        > liftedMapper = (a, b) => Trial.Lift2(curriedMapper, a, b);
        var v = Trial.Bind(result, func);
        return liftedMapper(result, v);
    }

    /// <summary>Lifts a Func into a Result and applies it on the given result.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult, TMessage> Select<TSuccess, TMessage, TResult>(this Result<TSuccess, TMessage> result,
        Func<TSuccess, TResult> func) => Trial.Lift(func, result);

    /// <summary>Returns the error messages or fails if the result was a success.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TMessage> FailedWith<TSuccess, TMessage>(this Result<TSuccess, TMessage> result)
    {
        Guard.DisallowNull(nameof(result), result);

        if (result.Tag == ResultType.Ok) {
            var ok = (Ok<TSuccess, TMessage>)result;
            throw new Exception(
                string.Format("Result was a success: {0} - {1}",
                ok.Success,
                string.Join(Environment.NewLine, ok.Messages.Select(m => m!.ToString()))));
        }
        var bad = (Bad<TSuccess, TMessage>)result;
        return bad.Messages;
    }

    /// <summary>Returns the result or fails if the result was an error.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSuccess SucceededWith<TSuccess, TMessage>(this Result<TSuccess, TMessage> result)
    {
        Guard.DisallowNull(nameof(result), result);

        if (result.Tag == ResultType.Ok) {
            var ok = (Ok<TSuccess, TMessage>)result;
            return ok.Success;
        }
        var bad = (Bad<TSuccess, TMessage>)result;
        throw new Exception(
            string.Format("Result was an error: {0}",
            string.Join(Environment.NewLine, bad.Messages.Select(m => m!.ToString()))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    /// <summary>Returns messages in case of success, otherwise an empty sequence.</summary>
    public static IEnumerable<TMessage> SuccessMessages<TSuccess, TMessage>(this Result<TSuccess, TMessage> result)
    {
        Guard.DisallowNull(nameof(result), result);

        return result.Tag == ResultType.Ok
                ? ((Ok<TSuccess, TMessage>)result).Messages
                : Enumerable.Empty<TMessage>();
    }
}
