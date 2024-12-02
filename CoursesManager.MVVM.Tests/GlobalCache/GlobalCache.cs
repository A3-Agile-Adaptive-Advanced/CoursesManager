using System;
using System.Collections.Generic;
using System.Diagnostics;
using CoursesManager.MVVM.Exceptions;
using NUnit.Framework;

namespace GlobalCacheExample
{
    [TestFixture]
    public class GlobalCacheTests
    {
        // Runs before each test to ensure cache is reset
        [SetUp]
        public void Setup()
        {
            GlobalCache.SetTestCapacity(10); // Set a default capacity
            var cache = GlobalCache.CreateForTesting();
            cache.Clear(); // Ensure cache is cleared before every test
        }

        #region Add and Retrieve
        [Test]
        public void Test_Add_And_Get()
        {
            // Arrange
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: false);
            var result = cache.Get("key1");

            // Assert
            Assert.That(result, Is.EqualTo("value1"));
        }

        [Test]
        public void Test_Throws_KeyNotFoundException_When_Item_Not_Found()
        {
            // Arrange
            GlobalCache.SetTestCapacity(10);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => cache.Get("non_existing_key"));
        }
        #endregion

        #region Eviction and Capacity
        [Test]
        public void Test_Evict_LRU_When_Capacity_Is_Exceeded()
        {
            // Arrange
            GlobalCache.SetTestCapacity(2);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: false);
            cache.Put("key2", "value2", isPermanent: false);
            cache.Put("key3", "value3", isPermanent: false); // This should trigger eviction


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
            // Arrange
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: false);
            cache.Put("key1", "value2", isPermanent: false); // Overwrite

            // Assert
            var result = cache.Get("key1");
            Assert.That(result, Is.EqualTo("value2"));
        }

        [Test]
        public void Test_Overwrite_Permanent_Item_Should_Fail()
        {
            // Arrange
            GlobalCache.SetTestCapacity(1);
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: true);
            // Attempt to overwrite a permanent item (should not overwrite)

            // Assert: the CantBeOverwrittenException should be thrown
            Assert.Throws<CantBeOverwrittenException>(() => cache.Put("key1", "value2", isPermanent: false));

        }
        #endregion

        #region Edge Cases and Error Handling
        [Test]
        public void Test_Clear_Cache()
        {
            // Arrange
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act
            cache.Put("key1", "value1", isPermanent: false);
            cache.Clear();  // Clear the cache

            // Assert
            Assert.Throws<KeyNotFoundException>(() => cache.Get("key1"));
        }

        [Test]
        public void Test_Add_Null_Key_Should_Throw_ArgumentNullException()
        {
            // Arrange
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => cache.Put(null, "value1", isPermanent: false));
        }

        [Test]
        public void Test_Add_Null_Value_Should_Throw_ArgumentNullException()
        {
            // Arrange
            var cache = GlobalCache.CreateForTesting();
            cache.Clear();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => cache.Put("key1", null, isPermanent: false));
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
