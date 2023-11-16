using System.Collections.Generic;

namespace FinbourneCodingExercise
{
    public interface ICache
    {
        /// <summary>
        /// The amount of items cache can hold at one period of time.
        /// </summary>
        int CountLimit { get; }

        /// <summary>
        /// Adds new item to the cache. Replaces the old item if key was already presented. Removes the least used item from the cache when there is no more free space.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="item"></param>
        void Add<T>(string key, T item);

        /// <summary>
        /// Retrieves item from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        T? Get<T>(string key);

        /// <summary>
        /// Removes item from the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if item existed and was just removed. Otherwise false</returns>
        bool Evict(string key);
    }

    public static class CacheExtensions
    { 
        public static object? Get(this ICache cache, string key)
        {
            return cache.Get<object>(key);
        }
    }
}