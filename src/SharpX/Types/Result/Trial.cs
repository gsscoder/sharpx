
using System.Runtime.CompilerServices;

namespace SharpX;

public static class Trial
{
    /// <summary>Wraps a value in a Success.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> Ok<TSuccess, TMessage>(TSuccess value) =>
        new Ok<TSuccess, TMessage>(value, Enumerable.Empty<TMessage>());

    /// <summary>Wraps a value in a Success.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> Pass<TSuccess, TMessage>(TSuccess value) =>
        new Ok<TSuccess, TMessage>(value, Enumerable.Empty<TMessage>());

    /// <summary>Wraps a value in a Success and adds a message.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> Warn<TSuccess, TMessage>(
        TMessage message, TSuccess value)
    {
        Guard.DisallowNull(nameof(message), message);
        Guard.DisallowNull(nameof(value), value);

        return new Ok<TSuccess, TMessage>(value, new[] { message });
    }

    /// <summary>Wraps a message in a Failure.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> Fail<TSuccess, TMessage>(TMessage message)
    {
        Guard.DisallowNull(nameof(message), message);

        return new Bad<TSuccess, TMessage>(new[] { message });
    }

    /// <summary>Returns true if the result was not successful.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Failed<TSuccess, TMessage>(Result<TSuccess, TMessage> result)
    {
        Guard.DisallowNull(nameof(result), result);

        return result.Tag == ResultType.Bad;
    }

    /// <summary>Takes a Result and maps it with successFunc if it is a Success otherwise it maps
    /// it with failureFunc.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult Either<TSuccess, TMessage, TResult>(
        Result<TSuccess, TMessage> result,
        Func<TSuccess, IEnumerable<TMessage>, TResult> successFunc,
        Func<IEnumerable<TMessage>, TResult> failureFunc)
    {
        Guard.DisallowNull(nameof(result), result);
        Guard.DisallowNull(nameof(successFunc), successFunc);
        Guard.DisallowNull(nameof(failureFunc), failureFunc);

        if (result is Ok<TSuccess, TMessage> ok) {
            return successFunc(ok.Success, ok.Messages);
        }
        var bad = (Bad<TSuccess, TMessage>)result;
        return failureFunc(bad.Messages);
    }

    /// <summary>If the given result is a Success the wrapped value will be returned. Otherwise
    /// the function throws an exception with Failure message of the result.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSuccess ReturnOrFail<TSuccess, TMessage>(Result<TSuccess, TMessage> result)
    {
        Guard.DisallowNull(nameof(result), result);

        return Either(result, (succ, _) => succ, msgs =>
            throw new Exception(
                        string.Join(
                        Environment.NewLine, msgs.Select(m => m.ToString())))
            );
    }

    /// <summary>Appends the given messages with the messages in the given result.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> MergeMessages<TSuccess, TMessage>(
        Result<TSuccess, TMessage> result,
        IEnumerable<TMessage> messages)
    {
        Guard.DisallowNull(nameof(result), result);
        Guard.DisallowNull(nameof(messages), messages);

        return Either<TSuccess, TMessage, Result<TSuccess, TMessage>>(result,
            (succ, msgs) => new Ok<TSuccess, TMessage>(succ, messages.Concat(msgs)),
            errors => new Bad<TSuccess, TMessage>(errors.Concat(messages)));
    }

    /// <summary>If the result is a Success it executes the given function on the value.
    /// Otherwise the exisiting failure is propagated.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> Bind<TValue, TSuccess, TMessage>(
        Result<TValue, TMessage> result,
        Func<TValue, Result<TSuccess, TMessage>> func)
    {
        Guard.DisallowNull(nameof(result), result);
        Guard.DisallowNull(nameof(func), func);

        return Either<TValue, TMessage, Result<TSuccess, TMessage>>(result,
            (succ, msgs) => MergeMessages(func(succ), msgs),
            messages => new Bad<TSuccess, TMessage>(messages));
    }

    /// <summary>Flattens a nested result given the Failure types are equal.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> Flatten<TSuccess, TMessage>(
        Result<Result<TSuccess, TMessage>, TMessage> result) => Bind(result, x => x);

    /// <summary>If the wrapped function is a success and the given result is a success the function
    /// is applied on the value. Otherwise the exisiting error messages are propagated.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> Apply<TValue, TSuccess, TMessage>(
        Result<TValue, TMessage> result,
        Result<Func<TValue, TSuccess>, TMessage> wrappedFunction)
    {
        Guard.DisallowNull(nameof(result), result);
        Guard.DisallowNull(nameof(wrappedFunction), wrappedFunction);

        if (wrappedFunction.Tag == ResultType.Ok && result.Tag == ResultType.Ok) {
            var ok1 = (Ok<Func<TValue, TSuccess>, TMessage>)wrappedFunction;
            var ok2 = (Ok<TValue, TMessage>)result;

            return new Ok<TSuccess, TMessage>(
                ok1.Success(ok2.Success), ok1.Messages.Concat(ok2.Messages));
        }
        if (wrappedFunction.Tag == ResultType.Bad && result.Tag == ResultType.Ok) {
            return new Bad<TSuccess, TMessage>(((Bad<TValue, TMessage>)result).Messages);
        }
        if (wrappedFunction.Tag == ResultType.Ok && result.Tag == ResultType.Bad) {
            return new Bad<TSuccess, TMessage>(
                ((Bad<TValue, TMessage>)result).Messages);
        }

        var bad1 = (Bad<Func<TValue, TSuccess>, TMessage>)wrappedFunction;
        var bad2 = (Bad<TValue, TMessage>)result;
        return new Bad<TSuccess, TMessage>(bad1.Messages.Concat(bad2.Messages));
    }

    /// <summary>Lifts a function into a Result container and applies it on the given
    /// result.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess, TMessage> Lift<TValue, TSuccess, TMessage>(
        Func<TValue, TSuccess> func,
        Result<TValue, TMessage> result) => Apply(result, Ok<Func<TValue, TSuccess>, TMessage>(func));

    /// <summary>Promotes a function to a monad/applicative, scanning the monadic/applicative
    /// arguments from left to right.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TSuccess1, TMessage1> Lift2<TSuccess, TMessage, TSuccess1, TMessage1>(
        Func<TSuccess, Func<TMessage, TSuccess1>> func,
        Result<TSuccess, TMessage1> first,
        Result<TMessage, TMessage1> second) => Apply(second, Lift(func, first));

    /// <summary>Collects a sequence of Results and accumulates their values. If the sequence
    /// contains an error the error will be propagated.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<IEnumerable<TSuccess>, TMessage> Collect<TSuccess, TMessage>(
        IEnumerable<Result<TSuccess, TMessage>> results)
    {
        Guard.DisallowNull(nameof(results), results);

        return Lift(Enumerable.Reverse,
            results.Aggregate<Result<TSuccess, TMessage>, Result<IEnumerable<TSuccess>, TMessage>, Result<IEnumerable<TSuccess>, TMessage>>(
            new Ok<IEnumerable<TSuccess>, TMessage>(Enumerable.Empty<TSuccess>(), Enumerable.Empty<TMessage>()),
            (result, next) =>
            {
                if (result.Tag == ResultType.Ok && next.Tag == ResultType.Ok) {
                    var ok1 = (Ok<IEnumerable<TSuccess>, TMessage>)result;
                    var ok2 = (Ok<TSuccess, TMessage>)next;
                    return
                        new Ok<IEnumerable<TSuccess>, TMessage>(
                                Enumerable.Empty<TSuccess>().Concat(new[] { ok2.Success }).Concat(ok1.Success),
                                ok1.Messages.Concat(ok2.Messages));
                }
                if (result.Tag == ResultType.Ok && next.Tag == ResultType.Bad) {
                    return new Bad<IEnumerable<TSuccess>, TMessage>(
                        ((Ok<IEnumerable<TSuccess>, TMessage>)result).Messages.Concat(
                            ((Bad<TSuccess, TMessage>)next).Messages));
                }
                if (result.Tag == ResultType.Bad && next.Tag == ResultType.Ok) {
                    return new Bad<IEnumerable<TSuccess>, TMessage>(
                        ((Bad<IEnumerable<TSuccess>, TMessage>)result).Messages.Concat(
                            ((Ok<TSuccess, TMessage>)next).Messages));
                }
                var bad1 = (Bad<IEnumerable<TSuccess>, TMessage>)result;
                var bad2 = (Bad<TSuccess, TMessage>)next;
                return new Bad<IEnumerable<TSuccess>, TMessage>(bad1.Messages.Concat(bad2.Messages));
            }, x => x));
    }
}
