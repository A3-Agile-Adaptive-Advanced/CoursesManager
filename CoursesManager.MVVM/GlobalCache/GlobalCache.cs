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

    public object? Get(string key)
    {
        if (!_cacheMap.ContainsKey(key))
            throw new KeyNotFoundException();

        _usageOrder[key] = DateTime.Now.Ticks;

        return _cacheMap[key].Value.Value;
    }

    // When an item is permanent the only thing possible is to update its contents, here we check if the contents match whats already in cache. example:
    // when the cache contains either a string or collection of strings (List<string>), you can update with a string or collections of strings.
    // but not with a long or collection of longs (List<long>)
    public void Put(string key, object value, bool isPermanent)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        if (_cacheMap.TryGetValue(key, out var existingNode) && existingNode != null)
        {
            if (existingNode.Value.IsPermanent)
            {
                if (!ShouldUpdateValue(existingNode.Value.Value, value))
                {
                    throw new CantBeOverwrittenException($"The item with key '{key}' is permanent and cannot be overwritten.");
                }
                existingNode.Value.Value = value;
            }
            existingNode.Value = new CacheItem(key, value, existingNode.Value.IsPermanent);
        }
        else
        {
            EnsureCapacity();
            _cacheMap[key] = new LinkedListNode<CacheItem>(new CacheItem(key, value, isPermanent));
        }

        _usageOrder[key] = DateTime.Now.Ticks;
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
        _capacity += _InitialCapacity * 2;
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

    // helpermethod to make sure that a permanent item can be updated but not overwritten by another (new/different) type.
    private bool ShouldUpdateValue(object existingValue, object newValue)
    {
        if (existingValue is System.Collections.IEnumerable existingCollection && newValue is System.Collections.IEnumerable newCollection)
        {
            return CollectionsAreEqual(existingCollection, newCollection);
        }
        else
        {
            return Equals(existingValue, newValue);
        }
    }


    private bool CollectionsAreEqual(System.Collections.IEnumerable collection1, System.Collections.IEnumerable collection2)
    {
        if (ReferenceEquals(collection1, collection2)) return true;

        if (collection1 == null || collection2 == null) return false;

        return (collection1.GetType() == collection2.GetType());

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
    public int GetCapacity()
    {
        return _capacity;
    }


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

}
