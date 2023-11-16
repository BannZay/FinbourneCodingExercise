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

    public delegate void CacheItemEvicted(string key, object? item);

    public class SimpleCache : ICache
    {
        public event CacheItemEvicted? OnItemEviction;

        // concurrentDictionary was not used since two collections needs to be updated for every operation, ReaderWriterLockSlim used instead.
        private readonly Dictionary<string, ItemHistory<object>> _cache 
            = new Dictionary<string, ItemHistory<object>>();

        /// <summary>
        /// Represents an order of items in which they were touched. The most recently used item goes to the tail.
        /// </summary>
        private readonly LinkedList<string> _usageHistory = new LinkedList<string>();

        /// <summary>
        /// Syncs access to <see cref="_cache"/> and <see cref="_usageHistory"/>
        /// </summary>
        private ReaderWriterLockSlim _sync = new ReaderWriterLockSlim(); 
        
        public int CountLimit { get; init; }

        public SimpleCache(int countLimit = 1000)
        {
            if (countLimit <= 0) throw new ArgumentOutOfRangeException("count", "must be greater than zero");

            CountLimit = countLimit;
        }

        public void Add<T>(string key, T item)
        {
            ItemHistory<object>? evictedItem = null;

            _sync.EnterWriteLock();

            try
            {
                if (_cache.TryGetValue(key, out var existingItem))
                {
                    TouchItem(existingItem.HistoryEntry);
                    existingItem.Value = item;
                }
                else
                {
                    var usageHistoryItem = _usageHistory.AddLast(key);
                    _cache.Add(key, new ItemHistory<object>(item, usageHistoryItem));
                }

                if (_cache.Count > CountLimit)
                {
                    EvictLeastUsedItem(true);
                }
            }
            finally { _sync.ExitWriteLock(); }

            if (evictedItem != null)
            {
                OnItemEviction?.Invoke(evictedItem.HistoryEntry.Value, evictedItem.Value);
            }
        }

        public T? Get<T>(string key)
        {
            ItemHistory<object>? historyItem = null;

            _sync.EnterUpgradeableReadLock();

            try
            {
                if (_cache.TryGetValue(key, out historyItem))
                {
                    TouchItem(historyItem.HistoryEntry);
                }
            }
            finally { _sync.ExitUpgradeableReadLock(); }

            var item = historyItem?.Value;

            if (item != null)
            {
                return (T)item;
            }
            else
            {
                return default;
            }
        }

        public bool Evict(string key)
        {
            ItemHistory<object>? evictedItem = null;

            _sync.EnterWriteLock();

            try
            {
                if (!_cache.Remove(key, out evictedItem))
                {
                    return false;
                }

                _usageHistory.Remove(evictedItem.HistoryEntry);
            }
            finally { _sync.ExitWriteLock(); }

            OnItemEviction?.Invoke(key, evictedItem);
            return true;
        }

        private ItemHistory<object>? EvictLeastUsedItem(bool isSyncedContext = false)
        {
            if (!isSyncedContext)
            {
                _sync.EnterWriteLock();
            }

            try
            {
                var leastUsedItem = _usageHistory.First!;
                _usageHistory.RemoveFirst();
                _cache.Remove(leastUsedItem!.Value, out var evictedItem);
                return evictedItem;
            }
            finally
            {
                if (!isSyncedContext)
                {
                    _sync.ExitWriteLock();
                }
            }
        }

        private void TouchItem(LinkedListNode<string> historyItem)
        {
            _sync.EnterWriteLock();

            try
            {
                _usageHistory.Remove(historyItem);
                _usageHistory.AddLast(historyItem);
            }
            finally { _sync.ExitWriteLock(); }
        }

        private class ItemHistory<T>
        {
            public ItemHistory(T? value, LinkedListNode<string> historyEntry)
            {
                Value = value ?? throw new ArgumentNullException(nameof(value)); ;
                HistoryEntry = historyEntry ?? throw new ArgumentNullException(nameof(historyEntry)); ;
            }

            public T? Value { get; set; }
            public LinkedListNode<string> HistoryEntry { get; init; }
        }
    }
}