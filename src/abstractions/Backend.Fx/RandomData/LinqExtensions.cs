﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Backend.Fx.BuildingBlocks;
using JetBrains.Annotations;

namespace Backend.Fx.RandomData
{
    [PublicAPI]
    public static class LinqExtensions
    {
        /// <summary>
        ///     https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var sourceAsArray = source as T[] ?? source.ToArray();

            var n = sourceAsArray.Length;
            while (n > 1)
            {
                var k = TestRandom.Instance.Next(n--);
                (sourceAsArray[n], sourceAsArray[k]) = (sourceAsArray[k], sourceAsArray[n]);
            }

            return sourceAsArray;
        }

        public static T Random<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // ReSharper disable once PossibleMultipleEnumeration
            if (TryAsQueryable(source, out var sourceQueryable, out var count))
            {
                if (count == 0)
                    throw new ArgumentException(
                        $"The enumerable of {typeof(T).Name} does not contain any items, therefore no random item can be returned.", nameof(source));

                return sourceQueryable.Skip(TestRandom.Next(count - 1)).First();
            }

            // ReSharper disable once PossibleMultipleEnumeration
            var sourceArray = source as T[] ?? source.ToArray();
            if (sourceArray.Length == 0)
                throw new ArgumentException(
                    $"The enumerable of {typeof(T).Name} does not contain any items, therefore no random item can be returned.", nameof(source));
            return sourceArray.ElementAt(TestRandom.Next(sourceArray.Length));
        }

        public static T RandomOrDefault<T>(this IEnumerable<T> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            // ReSharper disable once PossibleMultipleEnumeration
            if (TryAsQueryable(source, out var sourceQueryable, out var count))
            {
                if (count == 0) return default;

                return sourceQueryable.Skip(TestRandom.Next(count - 1)).FirstOrDefault();
            }

            // ReSharper disable once PossibleMultipleEnumeration
            var sourceArray = source as T[] ?? source.ToArray();
            if (sourceArray.Length == 0) return default;
            return sourceArray.ElementAt(TestRandom.Next(sourceArray.Length));
        }

        private static bool TryAsQueryable<T>(this IEnumerable<T> source, out IQueryable<T> outQueryable, out int count)
        {
            outQueryable = null;
            count = 0;

            if (source is IQueryable<T> sourceQueryable)
            {
                count = sourceQueryable.Count();
                if (count == 0)
                {
                    outQueryable = Array.Empty<T>().AsQueryable();
                    return true;
                }

                PropertyInfo idProperty = typeof(T).GetProperty(nameof(Identified.Id), BindingFlags.Instance | BindingFlags.Public);
                if (idProperty != null) sourceQueryable = sourceQueryable.OrderBy(nameof(Identified.Id));

                outQueryable = sourceQueryable;
                return true;
            }

            return false;
        }
    }
}