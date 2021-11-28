using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SharpX
{
    public enum ResultType
    {
        Bad,
        Ok
    }

    /// <summary>Represents the result of a computation.</summary>
    public abstract class Result<TSuccess, TMessage>
    {
        readonly ResultType _tag;
        protected Result(ResultType tag) => _tag = tag;
        public ResultType Tag => _tag;

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
                    string.Join(Environment.NewLine, ok.Messages.Select(m => m.ToString()))));
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
                string.Join(Environment.NewLine, bad.Messages.Select(m => m.ToString()))));
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
}
