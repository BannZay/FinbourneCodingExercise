using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinbourneCodingExercise
{
    public class History<T> : IHistory<T> where T : class
    {
        /// provides fast access to <see cref="_usageHistory"/> nodes
        private readonly Dictionary<T, LinkedListNode<T>> _usageHistoryItems = new();

        /// <summary>
        /// Represents an order of items in which they were touched. The most recently used item goes to the tail.
        /// </summary>
        private readonly LinkedList<T> _usageHistory = new LinkedList<T>();

        public int Count => _usageHistory.Count;

        public T? LeastUsedItem => _usageHistory.First?.Value;

        public T? TheMostRecentlyUsedItem => _usageHistory.Last?.Value;

        /// <summary>
        /// Records item usage
        /// </summary>
        public void Record(T item)
        {
            if (_usageHistoryItems.TryGetValue(item, out var node))
            {
                _usageHistory.Remove(node);
                _usageHistory.AddLast(node);
            }
            else
            {
                node = _usageHistory.AddLast(item);
                _usageHistoryItems[item] = node;
            }
        }

        /// <summary>
        /// Removes item usage from the history
        /// </summary>
        public void Remove(T item)
        {
            var historyNode = _usageHistoryItems[item];

            _usageHistoryItems.Remove(item);
            _usageHistory.Remove(historyNode);
        }
    }
}
