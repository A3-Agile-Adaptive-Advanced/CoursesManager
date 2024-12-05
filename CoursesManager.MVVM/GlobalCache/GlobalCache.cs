using CoursesManager.MVVM.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

public class GlobalCache
{
    private int _capacity;
    private int _InitialCapacity;
    private int _permanentItemCount;
    private readonly ConcurrentDictionary<string, LinkedListNode<CacheItem>> _cacheMap;
    private readonly ConcurrentDictionary<string, long> _usageOrder;
    private readonly object _lock = new object();

    private static readonly Lazy<GlobalCache> _instance = new Lazy<GlobalCache>(() => new GlobalCache(10));

    private GlobalCache(int capacity)
    {
        _capacity = capacity;
        _InitialCapacity = capacity;
        _permanentItemCount = 0;
        _cacheMap = new ConcurrentDictionary<string, LinkedListNode<CacheItem>>();
        _usageOrder = new ConcurrentDictionary<string, long>();
    }

    public static GlobalCache Instance => _instance.Value;

    public object Get(string key)
    {
        if (!_cacheMap.ContainsKey(key))
            throw new KeyNotFoundException();

        _usageOrder[key] = DateTime.UtcNow.Ticks;

        return _cacheMap[key].Value.Value;
    }

    public void Put(string key, object value, bool isPermanent)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        if (_cacheMap.TryGetValue(key, out var existingNode) && existingNode != null)
        {
            if (existingNode.Value.IsPermanent)
                throw new CantBeOverwrittenException($"The item with key '{key}' is permanent and cannot be overwritten.");

            existingNode.Value = new CacheItem(key, value, existingNode.Value.IsPermanent);
        }
        else
        {
            EnsureCapacity();
            _cacheMap[key] = new LinkedListNode<CacheItem>(new CacheItem(key, value, isPermanent));
        }

        // Update usage timestamp
        _usageOrder[key] = DateTime.UtcNow.Ticks;
    }


    private void EnsureCapacity()
    {
        if (_cacheMap.Count < _capacity)
            return; // No action needed if there's room

        if (_permanentItemCount == _capacity)
        {
            IncreaseCapacity(); // All items are permanent; increase capacity
        }
        else
        {
            EvictNonPermanentItem(); // Evict the least recently used non-permanent item
        }
    }


    private void IncreaseCapacity()
    {
        _capacity += 5;
    }

    private void DecreaseCapacity()
    {
        if (_capacity > _InitialCapacity)
        {
            _capacity--;
        }
    }

    private void EvictNonPermanentItem()
    {
        var leastUsedKey = _usageOrder.OrderBy(kvp => kvp.Value).FirstOrDefault().Key;
        if (!string.IsNullOrEmpty(leastUsedKey))
        {
            _cacheMap.TryRemove(leastUsedKey, out _);
            _usageOrder.TryRemove(leastUsedKey, out _);
        }
    }
    // Method is not used but implemented non the less for future possibilities.
    // Whenever an item needs to 'survive' longer than the expected lifecycle but
    // still should be cleaned up at some point this method is there to do so.
    public void RemovePermanentItem(string key)
    {
        lock (_lock)
        {
            if (_cacheMap.ContainsKey(key))
            {
                var node = _cacheMap[key];
                if (node.Value.IsPermanent)
                {
                    _permanentItemCount--;
                    _cacheMap.TryRemove(key, out _);
                    _usageOrder.TryRemove(key, out _);

                    // If the cache was full with perm items the cache has grown in size. this method ensures capacity never exceeds the applications needs.
                    DecreaseCapacity();
                }
            }
        }
    }

    private class CacheItem
    {
        public string Key { get; }
        public object Value { get; set; }
        public bool IsPermanent { get; }

        public CacheItem(string key, object value, bool isPermanent)
        {
            Key = key;
            Value = value;
            IsPermanent = isPermanent;

            // Automatically adjust the permanent item count
            if (isPermanent)
            {
                // Update the permanent item count directly in GlobalCache
                GlobalCache.Instance._permanentItemCount++;
            }
        }
    }
    public int getCapactiy()
    {
        return _capacity;
    }


#if DEBUG
    /// <summary>
    /// Clears the cache for unit testing purposes.
    /// </summary>
    public void Clear()
    {
        _cacheMap.Clear();
        _usageOrder.Clear();

    }

    // This allows you to create a custom cache instance for testing purposes in DEBUG builds
    private static int _testCapacity = 10;

    // A method for unit tests to set the custom capacity
    public static void SetTestCapacity(int capacity)
    {
        _testCapacity = capacity;
    }

    // Factory method for creating a cache with custom capacity in debug mode so you dont need to enter the default amount for each test.
    public static GlobalCache CreateForTesting()
    {
        return new GlobalCache(_testCapacity);
    }

#endif
}
