using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Aenima.System.Extensions;

namespace Aenima.System
{
    public static class Guard
    {
        [DebuggerStepThrough]
        public static void SmallerThan(Expression<Func<int>> reference, int threshold)
        {
            if(reference.Compile()() > threshold) return;

            throw new ArgumentException(
                ErrorMessages.ArgumentCannotBeSmallerThan.FormatWith(reference.GetParameterName(), threshold));
        }

        [DebuggerStepThrough]
        public static void SmallerThan(Expression<Func<long>> reference, long threshold)
        {
            if(reference.Compile()() > threshold) return;

            throw new ArgumentException(
                ErrorMessages.ArgumentCannotBeSmallerThan.FormatWith(reference.GetParameterName(), threshold));
        }

        [DebuggerStepThrough]
        public static void LargerThan(Expression<Func<int>> reference, int threshold)
        {
            if(reference.Compile()() < threshold) return;

            throw new ArgumentException(
                ErrorMessages.ArgumentCannotBeLargerThan.FormatWith(reference.GetParameterName(), threshold));
        }

        [DebuggerStepThrough]
        public static void LargerThan(Expression<Func<long>> reference, long threshold)
        {
            if(reference.Compile()() < threshold) return;

            throw new ArgumentException(
                ErrorMessages.ArgumentCannotBeLargerThan.FormatWith(reference.GetParameterName(), threshold));
        }

        #region . Null .

        [DebuggerStepThrough]
        public static void Null(Expression<Func<object>> reference)
        {
            Validate<object, ArgumentNullException>(
                reference,
                parameter => parameter == null,
                ErrorMessages.ArgumentCannotBeNull);
        }

        #endregion

        #region . NullOrDefault .

        [DebuggerStepThrough]
        public static void NullOrDefault<T>(Expression<Func<T>> reference)
        {
            Validate<T, ArgumentNullException>(
                reference,
                parameter => parameter.IsDefault(),
                ErrorMessages.ArgumentCannotBeNullOrDefault);
        }

        #endregion

        #region . NullOrEmpty .

        [DebuggerStepThrough]
        public static void NullOrEmpty(Expression<Func<string>> reference)
        {
            Validate<string, ArgumentNullException>(
                reference,
                parameter => parameter.IsNullOrEmpty(),
                ErrorMessages.ArgumentCannotBeNullOrEmpty);
        }

        [DebuggerStepThrough]
        public static void NullOrEmpty<T>(Expression<Func<IEnumerable<T>>> reference)
        {
            Validate<IEnumerable<T>, ArgumentNullException>(
                reference,
                parameter => parameter.IsNullOrEmpty(),
                ErrorMessages.ArgumentCannotBeNullOrEmpty);
        }

        #endregion

        #region . NullOrWhiteSpace .

        [DebuggerStepThrough]
        public static void NullOrWhiteSpace(Expression<Func<string>> reference)
        {
            Validate<string, ArgumentNullException>(
                reference,
                parameter => parameter.IsNullOrWhiteSpace(),
                ErrorMessages.ArgumentCannotBeNullOrWhitespace);
        }

        #endregion

        private static void Validate<TParameter, TException>(
            Expression<Func<TParameter>> reference,
            Predicate<TParameter> predicate,
            string message,
            Func<string, string, TException> exceptionFunc = null)
            where TException : Exception
        {
            if(!predicate(reference.Compile()())) return;

            var parameterName = reference.GetParameterName();

            var exception =
                exceptionFunc == null
                    ? (TException)Activator.CreateInstance(
                        typeof(TException),
                        parameterName,
                        message.FormatWith(parameterName))
                    : exceptionFunc(parameterName, message.FormatWith(parameterName));

            throw exception;
        }

        internal static class ErrorMessages
        {
            internal const string ArgumentCannotBeNull             = "Parameter '{0}' cannot be null.";
            internal const string ArgumentCannotBeNullOrDefault    = "Parameter '{0}' cannot be null or default.";
            internal const string ArgumentCannotBeNullOrEmpty      = "Parameter '{0}' cannot be null or empty.";
            internal const string ArgumentCannotBeNullOrWhitespace = "Parameter '{0}' cannot be null or whitespace.";     
            internal const string ArgumentCannotBeLargerThan       = "Parameter '{0}' cannot be larger than {1}.";
            internal const string ArgumentCannotBeSmallerThan      = "Parameter '{0}' cannot be smaller than {1}";
        }

    }
}
