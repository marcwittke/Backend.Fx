using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Backend.Fx.Util
{
    [PublicAPI]
    public static class EnumerableEx
    {
        public static void ForAll<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }
    }
}