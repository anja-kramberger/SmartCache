using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCache
{
    public class SmartCache<TKey, TValue>
    {
        private class CacheItem
        {
            public TKey Key;
            public TValue Value;
            public int AccesCount { get; set; }

            public CacheItem(TKey key, TValue value)
            {
                Key = key;
                Value = value;
                AccesCount = 0;
            }
        }

        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> map = new Dictionary<TKey, LinkedListNode<CacheItem>>();
        private readonly int capacity;
        private readonly LinkedList<CacheItem> listLRU = new LinkedList<CacheItem>();
        private readonly object _lock = new object();

        public SmartCache(int _capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("Out of range!");
            capacity = _capacity;
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (map.TryGetValue(key, out var node))
                {
                    //update existing value
                    node.Value.Value = value;
                    node.Value.AccesCount++;
                    MoveToFront(node);
                    return;

                    //overwriting old data exception
                    //throw new InvalidOperationException($"{key} already exists in cache.");
                }

                if (map.Count >= capacity)
                {
                    RemovetLeastRecentlyUsed();
                }

                var item = new CacheItem(key, value);
                item.AccesCount++;
                var newItem = new LinkedListNode<CacheItem>(item);
                listLRU.AddFirst(newItem);
                map[key] = newItem;
            }
        }
        public bool Remove(TKey key)
        {
            lock (_lock)
            {
                if (map.TryGetValue(key, out var node))
                {
                    var value = node.Value.Value;
                    listLRU.Remove(node);
                    map.Remove(key);

                    Console.WriteLine($"Deleted key: {key}, value: {value}");
                    return true;
                }
                return false;
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_lock)
            {
                return map.ContainsKey(key);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                map.Clear();
                listLRU.Clear();
            }
        }

        public TValue Get(TKey key)
        {
            lock (_lock)
            {
                if (!map.TryGetValue(key, out var item))
                    throw new KeyNotFoundException();

                item.Value.AccesCount++;
                MoveToFront(item);
                return item.Value.Value;
            }
        }

        public IEnumerable<(TKey Key, TValue Value, int AccessCount)> GetMostFrequentlyAccessed(int count)
        {
            lock (_lock)
            {
                return map.Values.Select(s => s.Value).OrderByDescending(item => item.AccesCount)
                    .Take(count).Select(item => (item.Key, item.Value, item.AccesCount)).ToList();
            }
        }

        private void MoveToFront(LinkedListNode<CacheItem> item)
        {
            listLRU.Remove(item);
            listLRU.AddFirst(item);
        }

        private void RemovetLeastRecentlyUsed()
        {
            var lru = listLRU.Last;
            if (lru == null) return;

            listLRU.RemoveLast();
            map.Remove(lru.Value.Key);
        }
    }
}
