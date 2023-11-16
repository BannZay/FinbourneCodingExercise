using System.Collections.Concurrent;

namespace FinbourneCodingExercise
{
    // The best way to approach this issue in a real world would be to research the way it was done in the existing robust solution before implementing it.
    // But I did not do that intentionally, just because I considered it as a cheating for completion of a coding exercise.
    // Instead I tried my best "re-inventing" it from the scratch.

    // Almost every optimisation comes with a complication or two which decreases code maintanability.
    // A hardest challenge for me was to guess a perfect balance between performance and simplicity
    // of the solution which suits your particular needs.
    // Hope I havent crossed "too-complex to maintain" line trying to make it faster.

    public delegate void CacheItem(string key, object? item);

    public class SimpleCache : ICache
    {
        // concurrentDictionary was not used since multiple objects (cache and history) being updated for every operation. ReaderWriterLockSlim was used instead.
        private readonly Dictionary<string, object> _cache = new();
        private readonly IHistory<string> _itemsUsageHistory;

        /// <summary>
        /// Syncs access to <see cref="_cache"/> and <see cref="_itemsUsageHistory"/>
        /// </summary>
        private ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(); 
        
        public int CountLimit { get; init; }

        public int Count => _cache.Count;

        public event CacheItem? OnItemRemoved;

        public SimpleCache(IHistory<string> history, int countLimit = 1000)
        {
            if (countLimit <= 0) throw new ArgumentOutOfRangeException("count", "must be greater than zero");

            CountLimit = countLimit;
            _itemsUsageHistory = history ?? throw new ArgumentNullException(nameof(history));
        }

        public void Add<T>(string key, T item)
        {
            object? removedItem = null;

            _sync.EnterWriteLock();

            try
            {
                _cache[key] = item!;
                _itemsUsageHistory.Record(key);

                if (_cache.Count > CountLimit)
                {
                    removedItem = RemoveLeastUsedItemSynced();
                }
            }
            finally { _sync.ExitWriteLock(); }

            if (removedItem != null)
            {
                OnItemRemoved?.Invoke(key, removedItem);
            }
        }

        public T? Get<T>(string key)
        {
            _sync.EnterUpgradeableReadLock();

            try
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    _itemsUsageHistory.Record(key);
                    return (T)item;
                }
                else
                {
                    return default;
                }

            }
            finally { _sync.ExitUpgradeableReadLock(); }
        }

        public bool Evict(string key)
        {
            object? evictedItem = null;

            _sync.EnterWriteLock();

            try
            {
                if (!_cache.Remove(key, out evictedItem))
                {
                    return false;
                }

                _itemsUsageHistory.Remove(key);
            }
            finally { _sync.ExitWriteLock(); }

            OnItemRemoved?.Invoke(key, evictedItem);
            return true;
        }

        private object? RemoveLeastUsedItemSynced()
        {
            var leastUsedItem = _itemsUsageHistory.LeastUsedItem;

            if (leastUsedItem == null)
                throw new InvalidOperationException();

            _itemsUsageHistory.Remove(leastUsedItem);

            _cache.Remove(leastUsedItem, out var item);
            return item;
        }
    }
}