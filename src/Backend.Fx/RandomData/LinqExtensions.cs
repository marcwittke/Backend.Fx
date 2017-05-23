namespace Backend.Fx.RandomData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class LinqExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var sourceAsArray = source.ToArray();

            for (var index = 0; index < sourceAsArray.Length; index++)
            {
                var randomIndex = TestRandom.Instance.Next(index + 1);
                var tmp = sourceAsArray[randomIndex];
                sourceAsArray[randomIndex] = sourceAsArray[index];
                sourceAsArray[index] = tmp;
            }

            return sourceAsArray;
        }

        public static T Random<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var sourceArray = source as T[] ?? source.ToArray();
            if (sourceArray.Length == 0)
            {
                throw new ArgumentException(string.Format("The enumerable of {0} does not contain any items, therefore no random item can be returned.", typeof(T).Name), nameof(source));
            }
            return sourceArray.ElementAt(TestRandom.Instance.Next(sourceArray.Length));
        }
    }
}