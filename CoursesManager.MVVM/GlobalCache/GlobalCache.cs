using CoursesManager.MVVM.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;


/// <summary>
/// Class to store both permanent and non-permanent objects at runtime to be accessed throughout the whole application.
/// This class implements the LCU principle while using validation of the entered objects to make sure only that what should be removed will be removed.
/// The permanent items do not allow removal, but can be updated if the same type is entered into a permanent slot.
/// </summary>
public class GlobalCache
{
    #region Attributes
    private readonly int _initialCapacity;
    private readonly ConcurrentDictionary<string, CacheItem>_cacheMap;
    private readonly ConcurrentDictionary<string, long> _usageOrder;
    private readonly object _lock = new object();

    private int _capacity;
    private static int _permanentItemCount;
    #endregion

    private static readonly Lazy<GlobalCache> _instance = new (() => new GlobalCache(10));

    private GlobalCache(int capacity)
    {
        _capacity = capacity;
        _initialCapacity = capacity;
        _permanentItemCount = 0;
        _cacheMap = new ConcurrentDictionary<string, CacheItem>();
        _usageOrder = new ConcurrentDictionary<string, long>();
    }

    public static GlobalCache Instance => _instance.Value;

    #region Control methods
    public object? Get(string key)
    {
        if (!_cacheMap.TryGetValue(key, out var existingItem))
            throw new KeyNotFoundException();

        _usageOrder[key] = DateTime.Now.Ticks;

        return _cacheMap[key].Value;
    }

    // When an item is permanent the only thing possible is to update its contents, a check is done to see if the contents match what is already in cache.
    // example:
    // when the cache contains a string it can be updated with a string or when it contains a collection of strings (List<string>) it can be updated with a collections of strings.
    // but when trying to update a string with a long there will be a CantBeOverWrittenException thrown.
    public void Put(string key, object value, bool isPermanent)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        if (_cacheMap.TryGetValue(key, out var existingItem))
        {
            if ((existingItem.IsPermanent != isPermanent))
            {
                throw new CantBeOverwrittenException($"The item with key '{key}' is not a permanent item.");
            }
            Update(key, value);
        }
        else
        {
            EnsureCapacity();
            _cacheMap[key] = new CacheItem(key, value, isPermanent);
        }

        _usageOrder[key] = DateTime.Now.Ticks;
    }

    // Method is not used but implemented nonetheless for future possibilities.
    // Whenever an item needs to 'survive' longer than the expected lifecycle but
    // still should be cleaned up at some point this method is there to do so.
    public void RemovePermanentItem(string key)
    {
        lock (_lock)
        {
            if (_cacheMap.ContainsKey(key))
            {
                var node = _cacheMap[key];
                if (node.IsPermanent)
                {
                    _permanentItemCount--;
                    _cacheMap.TryRemove(key, out _);
                    _usageOrder.TryRemove(key, out _);

                    // If the cache was full of perm items the cache has grown in size. this method ensures capacity never exceeds the applications needs.
                    DecreaseCapacity();
                }
            }
        }
    }
    #endregion
    #region helper methods and CacheItem class


    private void Update(string key, object value)
    {
        if (_cacheMap.TryGetValue(key, out var existingItem))
        {
            if (existingItem.IsPermanent && ShouldNotUpdateValue(existingItem.Value, value))
            {
                throw new CantBeOverwrittenException($"The item with key '{key}' is of a different type and cannot be overwritten.");
            }


            existingItem.Value = value;
            _usageOrder[key] = DateTime.Now.Ticks;
        }
        else
        {
            throw new NullReferenceException($"The item with key '{key}' does not exist in the cache.");
        }

    }
    private void EnsureCapacity()
    {
        if (_cacheMap.Count < _capacity) return;
        if (_permanentItemCount == _capacity)
        {
            IncreaseCapacity();
        }
        else
        {
            EvictNonPermanentItem();
        }
    }

    private void IncreaseCapacity()
    {
        _capacity = _initialCapacity * 2;
    }

    private void DecreaseCapacity()
    {
        if (_capacity > _initialCapacity) _capacity--;
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

    // helper-method to make sure that a permanent item can be updated but not overwritten by another (new/different) type.
    private bool ShouldNotUpdateValue(object existingValue, object newValue)
    {
        if (existingValue is System.Collections.IEnumerable existingCollection && newValue is System.Collections.IEnumerable newCollection)
        {
            return CollectionsAreEqual(existingCollection, newCollection);
        }
        else
        {
            return !Equals(existingValue, newValue);
        }
    }


    private bool CollectionsAreEqual(System.Collections.IEnumerable collection1, System.Collections.IEnumerable collection2)
    {
        if (ReferenceEquals(collection1, collection2)) return false;

        return !(collection1.GetType() == collection2.GetType());

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

            if (isPermanent)
            {
                _permanentItemCount++;
            }
        }
    }
    #endregion
    #region Debug methods
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

    // Factory method for creating a cache with custom capacity in debug mode so you don't need to enter the default amount for each test.
    public static GlobalCache CreateForTesting()
    {
        return new GlobalCache(_testCapacity);
    }

    public int GetCapacity()
    {
        return _capacity;
        
    }
    #endregion

}
