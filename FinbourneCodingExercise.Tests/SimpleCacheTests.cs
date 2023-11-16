namespace FinbourneCodingExercise.Tests
{
    [TestClass]
    public class SimpleCacheTests
    {
        [TestMethod]
        public void Add_NewItem_AddsNewItem()
        {
            var item = "sample";
            var key = "sampleKey";

            var cache = new SimpleCache();
            cache.Add(key, item);
            Assert.IsTrue(cache.Get<string>(key) == item);
        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(5)]
        public void Get_ExistingItem_ReturnsItem(int countLimit)
        {
            var cache = new SimpleCache(countLimit);
            var testData = CreateTestData(countLimit);
            var itemToCheckInfo = testData[countLimit / 2];

            var itemToCheck = (string)itemToCheckInfo.value;
            var itemToCheckId = itemToCheckInfo.id;

            foreach (var item in testData)
            {
                cache.Add(item.id, item.value);
            }

            var cachedItem = cache.Get<string>(itemToCheckId);
            Assert.IsTrue(cachedItem == itemToCheck);
        }

        [TestMethod]
        public void Get_OnRetrievingCacheOverflowedItem_ReturnsNull()
        {
            var cacheLimit = 3;
            var itemsCount = cacheLimit+1;

            var cache = new SimpleCache(cacheLimit);
            var testData = CreateTestData(itemsCount);

            foreach (var item in testData)
            {
                cache.Add(item.id, item.value);
            }

            var evictedItem = cache.Get<string>(testData[0].id);

            Assert.IsNull(evictedItem, "item expected to be dropped from the cache");
        }

        [TestMethod]
        public void Get_IncompatibleType_ThrowsInvalidCastException()
        {
            var key = "1";
            var cacheLimit = 3;
            var obj = "testObj";

            var cache = new SimpleCache(cacheLimit);
            cache.Add(key, obj);
            Assert.ThrowsException<InvalidCastException>(() => cache.Get<int>(key));
        }

        [TestMethod]
        public void Get_CompatibleType_ReturnsItem()
        {
            var key = "1";
            var cacheLimit = 3;
            var obj = new HashSet<object>(0);

            var cache = new SimpleCache(cacheLimit);
            cache.Add(key, obj);
            Assert.IsNotNull(cache.Get<IEnumerable<object>>(key));
        }

        [TestMethod]
        public void Evict_ExistingItem_RemovesItem()
        {
            var cache = CreateCacheWithTestData(3, 3);
            var itemToEvict = "X";
            var itemToEvictKey = "xKey";

            Assert.IsNull(cache.Get(itemToEvictKey));
            cache.Add(itemToEvictKey, itemToEvict);
            Assert.IsNotNull(cache.Get(itemToEvictKey));
            cache.Evict(itemToEvictKey);
            Assert.IsNull(cache.Get(itemToEvictKey));
        }

        private static (string id, object value)[] CreateTestData(int count)
        {
            var items = new (string id, object value)[count];

            for (var i = 0; i < items.Length; i++)
            {
                items[i] = ($"itemId#{i + 1}", $"itemContent#{i + 1}");
            }

            return items;
        }

        private static SimpleCache CreateCacheWithTestData(int cacheLimit, int testDataSize)
        {
            var cache = new SimpleCache(cacheLimit);
            var testData = CreateTestData(testDataSize);

            foreach (var item in testData)
            {
                cache.Add(item.id, item.value);
            }

            return cache;
        }
    }
}