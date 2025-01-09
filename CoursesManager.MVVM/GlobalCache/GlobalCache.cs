using CoursesManager.MVVM.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;


/// <summary>
/// Class to store both permanent and non-permanent objects at runtime to be accessed throughout the whole application.
/// This class implements the LCU principle while using validation of the entered objects to make sure only that what should be removed will be removed.
/// The permanent items do not allow removal, but can be updated if the same type is entered into a permanent slot.
/// This is the only place to change the size of the cache, in the static 'Lazy<GlobalCache>' the initial capacity is set to 10, if the application
/// ever needs to exceed this capacity then this is the place the change it from 10 to the required size.
/// Do note that this may impact performance, additional performance checks maybe required by a cache of significant bigger size.
/// Current statistics when testing with 100 concurrent calls to the put method is on average 3.5ms. When calling both get and put its 4ms.
/// </summary>
public class GlobalCache
{
    #region Attributes
    private readonly ConcurrentDictionary<string, CacheItem> _cacheMap;
    private readonly ConcurrentDictionary<string, long> _usageOrder;
    private readonly object _lock = new object();

    private int _capacity;
    private int _initialCapacity;
    private static int _permanentItemCount;
    #endregion

    private static readonly Lazy<GlobalCache> _instance = new(() => new GlobalCache(10));

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
            UpdateItem(key, value, existingItem);
        }
        else
        {
            EnsureCapacity();
            _cacheMap[key] = new CacheItem(value, isPermanent);
        }

        _usageOrder[key] = DateTime.Now.Ticks;
    }

    // Method is not used but implemented nonetheless for future possibilities.
    // Whenever an item needs to 'survive' longer than the expected lifecycle but
    // still should be cleaned up at some point this method is there to do so.
    // Should be turned off by default, but in current scenario turned on to run unittests.
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

    // when an item already exists this method is there to prevent duplication and checks if overwriting is allowed.
    private void UpdateItem(string key, object value, CacheItem existingItem)
    {
        lock (_lock)
        {
            if (existingItem.IsPermanent && ShouldNotUpdateValue(existingItem.Value, value))
            {
                throw new CantBeOverwrittenException(
                    $"The item with key '{key}' is of a different type and cannot be overwritten.");
            }

            existingItem.Value = value;
            _usageOrder[key] = DateTime.Now.Ticks;
        }
    }

    // This method will grant the cache the ability to grow according to its contents.
    // When the max capacity is reached and only permanent items are inside this will make it grow.
    // When a permanent item is removed this method will shrink it.
    // Reasoning here is that the initial capacity is set for non-permanent items.
    private void EnsureCapacity()
    {
        lock (_lock)
        {
            if (_cacheMap.Count < _capacity) return;
            if (_permanentItemCount == _capacity)
            {
                IncreaseCapacity();
            }
            else
            {
                EvictLruNonPermanentItem();
            }
        }
    }

    private void IncreaseCapacity()
    {
        _capacity += _initialCapacity;
    }

    private void DecreaseCapacity()
    {
        if (_capacity > _initialCapacity) _capacity--;
    }

    private void EvictLruNonPermanentItem()
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
        if (existingValue.GetType() != newValue.GetType())
        {
            return true;
        }

        if (existingValue is System.Collections.IEnumerable existingCollection && newValue is System.Collections.IEnumerable newCollection)
        {
            return CollectionsAreEqual(existingCollection, newCollection);
        }
        return !Equals(existingValue, newValue);
    }
    private bool CollectionsAreEqual(System.Collections.IEnumerable collection1, System.Collections.IEnumerable collection2)
    {
        if (ReferenceEquals(collection1, collection2)) return false;

        return !(collection1.GetType() == collection2.GetType());
    }

    // Private class to handle all possible objects inside the cache in a universal matter.
    private class CacheItem
    {
        public object Value { get; set; }
        public bool IsPermanent { get; }

        public CacheItem(object value, bool isPermanent)
        {
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
    // methods are internal, in AssemblyInfo access is granted to the unittests project "CoursesManager.MVVM.Tests"
    /// <summary>
    /// Clears the cache for unit testing purposes.
    /// </summary>
    internal void Clear()
    {
        _cacheMap.Clear();
        _usageOrder.Clear();
    }

    // This allows you to create a custom cache instance for testing purposes in DEBUG builds
    internal static int _testCapacity = 10;

    // A method for unit tests to set the custom capacity
    internal static void SetTestCapacity(int capacity)
    {
        _testCapacity = capacity;
    }

    // Factory method for creating a cache with custom capacity in debug mode so you don't need to enter the default amount for each test.
    internal static GlobalCache CreateForTesting()
    {
        return new GlobalCache(_testCapacity);
    }

    internal int GetCapacity()
    {
        return _capacity;

    }
    #endregion

}
