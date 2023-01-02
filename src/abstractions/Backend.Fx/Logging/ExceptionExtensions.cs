using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Backend.Fx.Logging
{
    [PublicAPI]
    [DebuggerStepThrough]
    public static class ExceptionExtensions
    {
        public static Exception GetInnerException(this Exception exception)
        {
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            return exception;
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue) where TSource : Exception
        {
            for (TSource current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : Exception
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        public static IEnumerable<string> GetaAllMessages(this Exception exception)
        {
            return exception.FromHierarchy(ex => ex.InnerException).Select(ex => ex.Message);
        }
    }
}