using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SharpX.Extensions;

namespace SharpX
{
    public enum OutcomeType
    {
        Success,
        Failure
    }

    public struct Error : IEquatable<Error>
    {
        readonly Lazy<ExceptionEqualityComparer> _comparer => new Lazy<ExceptionEqualityComparer>(
            () => new ExceptionEqualityComparer());
        Exception _exception;
        public string Message { get; private set; }
        public Maybe<Exception> Exception => _exception.ToMaybe();

        internal Error(string message, Exception exception)
        {
            Guard.DisallowNull(nameof(message), message);

            Message = message;
            _exception = exception;
        }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (other is not Error error) return false;

            var casted = (Error)other;

            return Equals(casted);
        }

        public bool Equals(Error other) =>
            other.Message.Equals(Message)  &&
                    _comparer.Value.Equals(other._exception, _exception);

        public static bool operator ==(Error left, Error right) => left.Equals(right);

        public static bool operator !=(Error left, Error right) => !left.Equals(right);

        public override int GetHashCode() =>
            _exception == null
                ? Message.GetHashCode()
                : Message.GetHashCode() ^ _exception.GetHashCode();

        public override string ToString() => Exception.IsJust()
            ? new StringBuilder(capacity: 256)
                .AppendLine($"{Message}:")
                .AppendLine(Exception.FromJust().Format())
                .ToString()
            : Message;

        sealed class ExceptionEqualityComparer : IEqualityComparer<Exception>
        {
            public bool Equals(Exception first, Exception second)
            {
                if (first == null && second == null) return true;
                if (first == null || second == null) return false;
                if (first.GetType() != second.GetType()) return false;
                if (first.Message != second.Message) return false;
                if (first.InnerException != null) return Equals(first.InnerException, second.InnerException);
                return true;
            }

            public int GetHashCode(Exception exception)
            {
                var hash = exception.Message.GetHashCode();
                if (exception.InnerException != null) hash ^= exception.InnerException.Message.GetHashCode();
                return hash;
            }
        }
    }

    public struct Outcome : IEquatable<Outcome>
    {
        internal readonly Error _error;

        internal Outcome(Error error)
        {
            Tag = OutcomeType.Failure;
            _error = error;
        }

        public OutcomeType Tag { get; private set; }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            if (other is not Outcome outcome) return false;

            var casted = (Outcome)other;

            return Equals(casted);
        }

        public bool Equals(Outcome other) =>
            other.Tag != OutcomeType.Failure || _error.Equals(other._error);

        public static bool operator ==(Outcome left, Outcome right) => left.Equals(right);

        public static bool operator !=(Outcome left, Outcome right) => !left.Equals(right);

        public override int GetHashCode() =>
            Tag == OutcomeType.Success
                ? ToString().GetHashCode()
                : _error.GetHashCode();

        public override string ToString() =>
            Tag switch {
                OutcomeType.Success => "<Success>",
                _                  => _error.ToString()
            };

        #region Value case constructors
        public static Outcome Failure(string error)
        {
            Guard.DisallowNull(nameof(error), error);
            Guard.DisallowEmptyWhiteSpace(nameof(error), error);

            return new Outcome(new Error(error, null));
        }

        public static Outcome Failure(string error, Exception exception)
        {
            Guard.DisallowNull(nameof(error), error);
            Guard.DisallowEmptyWhiteSpace(nameof(error), error);
            Guard.DisallowNull(nameof(exception), exception);

            return new Outcome(new Error(error, exception));
        }

        public static Outcome Success => new Outcome();
        #endregion

        #region Basic match methods
        public bool MatchFailure(out Error error)
        {
            error = Tag == OutcomeType.Failure ? _error : default;
            return Tag == OutcomeType.Failure;
        }

        public bool MatchSuccess() => Tag == OutcomeType.Success;
        #endregion
    }

    public static class OutcomeExtensions
    {
        public static Unit Match(this Outcome outcome,
            Func<Unit> onSuccess, Func<Error, Unit> onFailure)
        {
            Guard.DisallowNull(nameof(outcome), outcome);
            Guard.DisallowNull(nameof(onSuccess), onSuccess);
            Guard.DisallowNull(nameof(onFailure), onFailure);

            return outcome.MatchFailure(out Error error) switch {
                true => onFailure(error),
                _    => onSuccess() 
            };
        }

        public static Unit Match(this Outcome outcome,
            Func<Unit> onSuccess, Func<string, Unit> onFailure)
        {
            Guard.DisallowNull(nameof(outcome), outcome);
            Guard.DisallowNull(nameof(onSuccess), onSuccess);
            Guard.DisallowNull(nameof(onFailure), onFailure);

            return outcome.MatchFailure(out Error error) switch {
                true => onFailure(error.Message),
                _    => onSuccess() 
            };
        }

        public static Unit Match(this Outcome outcome,
            Func<Unit> onSuccess, Func<Maybe<Exception>, Unit> onFailure)
        {
            Guard.DisallowNull(nameof(outcome), outcome);
            Guard.DisallowNull(nameof(onSuccess), onSuccess);
            Guard.DisallowNull(nameof(onFailure), onFailure);

            return outcome.MatchFailure(out Error error) switch {
                true => onFailure(error.Exception),
                _    => onSuccess() 
            };
        }

        public static Unit Match(this Outcome outcome,
            Func<Unit> onSuccess, Func<Exception, Unit> onFailure)
        {
            Guard.DisallowNull(nameof(outcome), outcome);
            Guard.DisallowNull(nameof(onSuccess), onSuccess);
            Guard.DisallowNull(nameof(onFailure), onFailure);

            return outcome.MatchFailure(out Error error) switch {
                true => onFailure(error.Exception.FromJust()),
                _    => onSuccess() 
            };
        }
    }
}
