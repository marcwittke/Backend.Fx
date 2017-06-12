namespace Backend.Fx.RandomData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class LinqExtensions
    {
        /// <summary>
        /// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            T[] sourceAsArray = source as T[] ?? source.ToArray();

            int n = sourceAsArray.Length;
            while (n > 1)
            {
                int k = TestRandom.Instance.Next(n--);
                T temp = sourceAsArray[n];
                sourceAsArray[n] = sourceAsArray[k];
                sourceAsArray[k] = temp;
            }

            return sourceAsArray;
        }

        public static T Random<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var sourceQueryable = source as IQueryable<T>;
            if (sourceQueryable != null)
            {
                int count = sourceQueryable.Count();
                return sourceQueryable.Skip(TestRandom.Next(count - 1)).First();
            }
            
            var sourceArray = source as T[] ?? source.ToArray();
            if (sourceArray.Length == 0)
            {
                throw new ArgumentException(string.Format("The enumerable of {0} does not contain any items, therefore no random item can be returned.", typeof(T).Name), nameof(source));
            }
            return sourceArray.ElementAt(TestRandom.Next(sourceArray.Length));
        }
    }
}