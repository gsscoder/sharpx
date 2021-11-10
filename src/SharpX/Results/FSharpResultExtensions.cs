using System;
using System.Runtime.CompilerServices;
using Microsoft.FSharp.Core;

namespace SharpX
{
    public static class FSharpResultExtensions
    {
        /// <summary>Allows pattern matching on <c>FSharpResult</c>.</summary>
        public static void Match<T, TError>(this FSharpResult<T, TError> result,
            Action<T> onOk,
            Action<TError> onError)
        {
            if (onOk == null) throw new ArgumentNullException(nameof(onOk));
            if (onError == null) throw new ArgumentNullException(nameof(onError));

            if (result.IsOk) {
                onOk(result.ResultValue);
                return;
            }
            onError(result.ErrorValue);
        }

        /// <summary>Allows pattern matching on <c>FSharpResult</c>.</summary>
        public static TResult Either<T, TError, TResult>(this FSharpResult<T, TError> result,
            Func<T, TResult> onOk,
            Func<TError, TResult> onError)
        {
            if (onOk == null) throw new ArgumentNullException(nameof(onOk));
            if (onError == null) throw new ArgumentNullException(nameof(onError));

            return Either(onOk, onError, result);
        }

        /// <summary>Lifts a <c>Func</c> into an <c>FSharpResult</c> and applies it on the given
        /// result.</summary>
        public static FSharpResult<TResult, TError> Map<T, TError, TResult>(
            this FSharpResult<T, TError> result,
            Func<T, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            return Lift(func, result);
        }

        /// <summary>If the wrapped function is a success and the given result is a success the
        /// function is applied on the value. Otherwise the exisiting error is returned.</summary>
        public static FSharpResult<T, TError> Bind<TValue, T, TError>(
                this FSharpResult<TValue, TError> result,
                Func<TValue, FSharpResult<T, TError>> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            return Bind(func, result);
        }

        /// <summary>If the given result is a success the wrapped value will be returned. Otherwise
        /// the function throws an exception with the string representation of the error.</summary>
        public static T ReturnOrFail<T, TError>(this FSharpResult<T, TError> result)
        {
            Func<TError, T> raiseExn = err => throw new Exception(err.ToString());
        
            return Either(value => value, raiseExn, result);
        }

        /// <summary>Unwraps a value applying a function o returns another value on fail.</summary>
        public static TResult Return<T, TError, TResult>(
            this FSharpResult<T, TError> result,
            Func<T, TResult> func, TResult noneValue) => Either(func, value => noneValue, result);
        
        /// <summary>Builds a <c>Maybe</c> discarding error type.</summary>
        public static Maybe<T> ToMaybe<T, TError>(this FSharpResult<T, TError> result) =>
            result.IsOk ? Maybe.Just<T>(result.ResultValue) : Maybe.Nothing<T>();

        public static Either<TError, T> ToEither<T, TError>(this FSharpResult<T, TError> result) =>
            result.IsOk
                ? SharpX.Either.Right<TError, T>(result.ResultValue)
                : SharpX.Either.Left<TError, T>(result.ErrorValue);

        /// <summary>Takes a result and maps it with okFunc if it is a success, otherwise it maps it with
        /// errorFunc.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult Either<T, TError, TResult>(
            Func<T, TResult> okFunc,
            Func<TError, TResult> errorFunc,
            FSharpResult<T, TError> result)
        {
            if (okFunc == null) throw new ArgumentNullException(nameof(okFunc));
            if (errorFunc == null) throw new ArgumentNullException(nameof(errorFunc));

            if (result.IsOk) {
                return okFunc(result.ResultValue);
            }
            return errorFunc(result.ErrorValue);
        }

        /// <summary>If the result is a success it executes the given function on the value. Otherwise the
        /// exisiting error is returned.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FSharpResult<T, TError> Bind<TValue, T, TError>(
            Func<TValue, FSharpResult<T, TError>> func,
            FSharpResult<TValue, TError> result)
        {
                if (func == null) throw new ArgumentNullException(nameof(func));

                Func<TValue, FSharpResult<T, TError>> okFunc =
                    value => func(value);
                Func<TError, FSharpResult<T, TError>> errorFunc =
                    error => FSharpResult<T, TError>.NewError(error);
                return Either(okFunc, errorFunc, result);
        }

        /// <summary>If the wrapped function is a success and the given result is a success the function is
        /// applied on the value.  Otherwise the exisiting error is returned.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FSharpResult<T, TError> Apply<TValue, T, TError>(
            FSharpResult<Func<TValue, T>, TError> wrappedFunc,
            FSharpResult<TValue, TError> result
        )
        {
            if (wrappedFunc.IsOk && result.IsOk) {
                return FSharpResult<T, TError>.NewOk(
                    wrappedFunc.ResultValue(result.ResultValue));
            }
            return FSharpResult<T, TError>.NewError(result.ErrorValue);
        }

        /// <summary>Lifts a function into a result container and applies it on the given result.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FSharpResult<T, TError> Lift<TValue, T, TError>(
            Func<TValue, T> func,
            FSharpResult<TValue, TError> result) =>
            Apply(FSharpResult<Func<TValue, T>, TError>.NewOk(func), result);
    }
}
