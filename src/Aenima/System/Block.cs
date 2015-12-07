using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using Aenima.System.Extensions;

namespace Aenima.System
{
    public static class Block
    {
        [DebuggerStepThrough]
        public static void SmallerThan(Expression<Func<int>> reference, int threshold)
        {
            if(reference.Compile()() < threshold)
                throw new ArgumentException(ErrorMessages.ArgumentCannotBeSmallerThan.FormatWith(reference.GetParameterName(), threshold));
        }

        [DebuggerStepThrough]
        public static void SmallerThan(Expression<Func<long>> reference, long threshold)
        {
            if(reference.Compile()() < threshold)
                throw new ArgumentException(ErrorMessages.ArgumentCannotBeSmallerThan.FormatWith(reference.GetParameterName(), threshold));
        }

        [DebuggerStepThrough]
        public static void LargerThan(Expression<Func<int>> reference, int threshold)
        {
            if(reference.Compile()() > threshold)
                throw new ArgumentException(ErrorMessages.ArgumentCannotBeLargerThan.FormatWith(reference.GetParameterName(), threshold));
        }

        [DebuggerStepThrough]
        public static void LargerThan(Expression<Func<long>> reference, long threshold)
        {
            if(reference.Compile()() > threshold)
                throw new ArgumentException(ErrorMessages.ArgumentCannotBeLargerThan.FormatWith(reference.GetParameterName(), threshold));
        }

        [DebuggerStepThrough]
        public static void Null(Expression<Func<object>> reference)
        {
            if(reference.Compile()() == null)
                throw new ArgumentException(ErrorMessages.ArgumentCannotBeNull.FormatWith(reference.GetParameterName()));
        }

        [DebuggerStepThrough]
        public static void Default<T>(Expression<Func<T>> reference)
        {
            if(reference.Compile()().IsDefault())
                throw new ArgumentException(ErrorMessages.ArgumentCannotBeNullOrDefault.FormatWith(reference.GetParameterName()));
        }

 
        [DebuggerStepThrough]
        public static void NullOrEmpty<T>(Expression<Func<IEnumerable<T>>> reference)
        {
            if(reference.Compile()().IsNullOrEmpty())
                throw new ArgumentException(ErrorMessages.ArgumentCannotBeNullOrEmpty.FormatWith(reference.GetParameterName()));
        }

        [DebuggerStepThrough]
        public static void NullOrWhiteSpace(Expression<Func<string>> reference)
        {
            if(reference.Compile()().IsNullOrWhiteSpace())
                throw new ArgumentException(ErrorMessages.ArgumentCannotBeNullOrWhitespace.FormatWith(reference.GetParameterName()));
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
