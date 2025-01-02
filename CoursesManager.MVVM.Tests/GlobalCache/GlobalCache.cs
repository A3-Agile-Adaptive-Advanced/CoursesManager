using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CoursesManager.MVVM.Exceptions;
using NUnit.Framework;

namespace GlobalCacheExample
{
    [TestFixture]
    public class GlobalCacheTests
    {
        // Runs before each test to ensure cache is reset.
        [SetUp]
        public void Setup()
        {
            GlobalCache.SetTestCapacity(10); // Set a default capacity.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();
        }

        #region Add and Retrieve
        [Test]
        public void Test_Add_And_Get()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: false);
            var result = cache.Get("key1");

            // Assert.
            Assert.That(result, Is.EqualTo("value1"));
        }

        [Test]
        public void Test_Throws_KeyNotFoundException_When_Item_Not_Found()
        {
            // Arrange.
            GlobalCache.SetTestCapacity(10);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act & Assert.
            Assert.Throws<KeyNotFoundException>(() => cache.Get("non_existing_key"));
        }
        #endregion

        #region Eviction and Capacity

        [Test]
        public void Test_Deleting_Permanent_Item_To_Decrease_Capacity()
        {
            // Arrange
            GlobalCache.SetTestCapacity(2);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", true);
            cache.Put("key2", "value2", true);
            cache.Put("key3", "value3", true);
            int increasedCapacity = cache.GetCapacity();
            cache.RemovePermanentItem("key3");
            int newCapacity = cache.GetCapacity();

            // Assert: Capacity should be 3.
            Assert.That(increasedCapacity > newCapacity);
            Assert.That(newCapacity == 3);
        }
        [Test]
        public void Test_Capacity_Decreases_When_Same_As_InitialCapacity()
        {
            // Arrange
            int intialCapacity = 2;
            GlobalCache.SetTestCapacity(intialCapacity);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", true);
            cache.Put("key2", "value2", true);
            cache.Put("key3", "value3", true);
            cache.RemovePermanentItem("key1");
            cache.RemovePermanentItem("key2");
            cache.RemovePermanentItem("key3"); // this would cause the capacity to become 1 without the comparison of the initial capacity against current capacity
            int newCapacity = cache.GetCapacity();

            // Assert: Capacity should be equal to initial capacity
            Assert.That(intialCapacity == newCapacity);
        }
        public void Test_Evict_LRU_When_Capacity_Is_Exceeded()
        {
            // Arrange
            GlobalCache.SetTestCapacity(2);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", false);
            cache.Put("key2", "value2", false);
            cache.Put("key3", "value3", false); // This should trigger eviction


            // Assert: Key3 should be evicted as it's the least recently used
            Assert.Throws<KeyNotFoundException>(() => cache.Get("key1"));
            Assert.That(cache.Get("key2"), Is.EqualTo("value2"));
            Assert.That(cache.Get("key3"), Is.EqualTo("value3"));

        }

        [Test]
        public void Test_Do_Not_Evict_Permanent_Items()
        {
            // Arrange
            GlobalCache.SetTestCapacity(2);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: false);
            cache.Put("key2", "value2", isPermanent: true);  // Permanent, should not be evicted
            cache.Put("key3", "value3", isPermanent: false); // Should evict key1


            // Assert: Key1 should be evicted, Key2 should remain
            Assert.Throws<KeyNotFoundException>(() => cache.Get("key1"));
            Assert.That(cache.Get("key2"), Is.EqualTo("value2"));
            Assert.That(cache.Get("key3"), Is.EqualTo("value3"));
        }

        [Test]
        public void Test_Cache_Overflow_When_Capacity_Is_Exceeded()
        {
            // Arrange
            GlobalCache.SetTestCapacity(3);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: false);
            cache.Put("key2", "value2", isPermanent: false);
            cache.Put("key3", "value3", isPermanent: false);
            cache.Put("key4", "value4", isPermanent: false);  // This should evict key1

            // Assert: Key1 should be evicted, keys 2-4 should remain
            Assert.Throws<KeyNotFoundException>(() => cache.Get("key1"));
            Assert.That(cache.Get("key2"), Is.EqualTo("value2"));
            Assert.That(cache.Get("key3"), Is.EqualTo("value3"));
            Assert.That(cache.Get("key4"), Is.EqualTo("value4"));
        }
        #endregion

        #region Overwrite and Updates
        [Test]
        public void Test_Overwrite_Existing_Item()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act.
            cache.Put("key1", "value1", false);
            cache.Put("key1", "value2", false); // Overwrite.

            // Assert.
            var result = cache.Get("key1") as string;
            Assert.That(result, Is.EqualTo("value2"));
        }
        [Test]
        public void Test_Concurrent_Access_100_Threads()
        {
            // Arrange
            GlobalCache.SetTestCapacity(100);
            var globalCache = GlobalCache.CreateForTesting();
            globalCache.Clear();

            int threadCount = 100;
            var tasks = new List<Task>();

            // Act & Assert
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                tasks.Add(Task.Run(() =>
                {
                    string key = $"Key{threadIndex}";
                    globalCache.Put(key, $"Value{threadIndex}", false);
                    var value = globalCache.Get(key);
                    Assert.That($"Value{threadIndex}", Is.EqualTo(value));
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }
        [Test]
        public void Test_Concurrent_Access_Should_Maintain_Correct_Cache_State()
        {
            // Arrange
            GlobalCache.SetTestCapacity(100);
            var globalCache = GlobalCache.CreateForTesting();
            globalCache.Clear();

            int threadCount = 100;
            var tasks = new List<Task>();
            var keys = new List<string>();

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                string key = $"Key{threadIndex}";
                keys.Add(key);

                tasks.Add(Task.Run(() =>
                {
                    globalCache.Put(key, $"Value{threadIndex}", false);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            //Assert
            foreach (var key in keys)
            {
                var value = globalCache.Get(key);
                Assert.That($"Value{key.Replace("Key", "")}", Is.EqualTo(value));
            }
        }
        [Test]
        public void Test_Concurrent_Update_Same_Permanent_Value()
        {
            // Arrange
            GlobalCache.SetTestCapacity(1);
            var globalCache = GlobalCache.CreateForTesting();
            globalCache.Clear();

            string key = "SharedKey";
            string initialValue = "Value0";
            
            int threadCount = 100;
            var tasks = new List<Task>();
            var writtenValues = new ConcurrentBag<string>();

            // Act
            globalCache.Put(key, initialValue, true);

            for (int i = 0; i < threadCount; i++)
            {
                int currentValue = i;
                tasks.Add(Task.Run(() =>
                {
                    string value = $"Value{currentValue}";
                    globalCache.Put(key, value, true);
                    writtenValues.Add(value);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            
            //Assert
            string finalValue = (string)globalCache.Get(key);
            Assert.That(writtenValues.Contains(finalValue), $"The final value '{finalValue}' should match one of the written values.");
            Assert.That(writtenValues.Count, Is.EqualTo(threadCount), $"Expected {threadCount} updates, but found {writtenValues.Count}.");
        }
        #endregion

        #region Edge Cases and Error Handling
        [Test]
        public void Test_Get_Non_Existing_Item()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act.
            cache.Put("key1", "value1", isPermanent: false);
            cache.Clear();  // Making sure the item does no longer exist in the cache.

            // Assert.
            Assert.Throws<KeyNotFoundException>(() => cache.Get("key1"));
        }

        [Test]
        public void Test_Add_Null_Key_Should_Throw_ArgumentNullException()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act & Assert.
            Assert.Throws<ArgumentNullException>(() => cache.Put(null, "value1", isPermanent: false));
        }

        [Test]
        public void Test_Add_Null_Value_Should_Throw_ArgumentNullException()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act & Assert.
            Assert.Throws<ArgumentNullException>(() => cache.Put("key1", null, isPermanent: false));
        }

        [Test]
        public void Test_Update_Permanent_SingleItem_With_Different_Type()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act.
            cache.Put("key1", "value1", true);

            // Assert.
            Assert.Throws<CantBeOverwrittenException>(() => cache.Put("key1", 1, true));
        }
        [Test]
        public void Test_Update_Permanent_SingleItem_With_Same_Type()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act.
            cache.Put("key1", "value1", true);
            cache.Put("key1", "value2", true); // Overwrite.

            // Assert.
            var result = cache.Get("key1") as string;
            Assert.That(result, Is.EqualTo("value2"));
        }
        [Test]
        public void Test_Update_Permanent_Collection_With_Same_Type()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();
            var myCollection1 = new ObservableCollection<string>
            {
                "String 1",
                "String 2",
                "String 3"
            };
            var myCollection2 = new ObservableCollection<string>
            {
                "String 4",
                "String 5",
                "String 6"
            };

            // Act.
            cache.Put("key1", myCollection1, true);
            cache.Put("key1", myCollection2, true); // Overwrite.

            // Assert.
            var result = cache.Get("key1") as ObservableCollection<string>;
            Assert.That(result, Is.EqualTo(myCollection2));
        }
        [Test]
        public void Test_Update_Permanent_Collection_With_Different_Type()
        {
            // Arrange.
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();
            var myCollection1 = new ObservableCollection<string>
            {
                "String 1",
                "String 2",
                "String 3"
            };
            var myCollection2 = new ObservableCollection<long>
            {
                1,
                2,
                3
            };

            // Act.
            cache.Put("key1", myCollection1, true);

            // Assert.
            Assert.Throws<CantBeOverwrittenException>(() => cache.Put("key1", myCollection2, true));
        }
        #endregion

        #region Capacity Management
        [Test]
        public void Test_Set_Capacity_And_Add_Exceeding_Items()
        {
            // Arrange
            GlobalCache.SetTestCapacity(3);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: false);
            cache.Put("key2", "value2", isPermanent: false);
            cache.Put("key3", "value3", isPermanent: false);
            cache.Put("key4", "value4", isPermanent: false);  // This should evict the least recently used

            // Assert
            Assert.Throws<KeyNotFoundException>(() => cache.Get("key1"));
            Assert.That(cache.Get("key2"), Is.EqualTo("value2"));
            Assert.That(cache.Get("key3"), Is.EqualTo("value3"));
            Assert.That(cache.Get("key4"), Is.EqualTo("value4"));
        }
        #endregion
    }
}
