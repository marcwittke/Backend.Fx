namespace Backend.Fx.Extensions
{
    using System;
    using System.Collections.Generic;

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