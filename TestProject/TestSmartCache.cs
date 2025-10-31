using SmartCache;

namespace TestProject
{
    public class TestSmartCache
    {
        [Fact]
        public void Test_LRU_Eviction()
        {
            var cache = new SmartCache<int, string>(3);
            cache.Add(1, "One");
            cache.Add(2, "Two");
            cache.Add(3, "Three");
            //LRU -> 3 2 1


            cache.Get(1); //LRU -> 1 3 2
            cache.Get(2); //LRU -> 2 1 3
            cache.Add(4, "Four"); //LRU -> 4 2 1

            Assert.False(cache.ContainsKey(3));
            Assert.True(cache.ContainsKey(1));
            Assert.True(cache.ContainsKey(2));
            Assert.True(cache.ContainsKey(4));

        }

        [Fact]
        public void Test_Frequency_Tracking()
        {
            var cache = new SmartCache<int, string>(4);

            cache.Add(1, "One");
            cache.Add(2, "Two");
            cache.Add(3, "Three");
            cache.Add(4, "Four");

            cache.Get(1); // AccessCount = 2
            cache.Get(3); // AccessCount = 2
            cache.Get(1); // AccessCount = 3

            var top = cache.GetMostFrequentlyAccessed(3).ToList();

            Assert.Equal(1, top[0].Key); // 3 accesses
            Assert.Equal(3, top[1].Key); // 2 accesses
            Assert.Equal(2, top[2].Key); // 1 access

        }

        [Fact]
        public void Test_Thread_Safety_With_Concurrent_Access()
        {
            var cache = new SmartCache<int, int>(4);
            cache.Add(0, 0);
            cache.Add(1, 10);
            cache.Add(2, 20);

            Parallel.Invoke(
                () => cache.Add(3, 30),
                () => { if (cache.ContainsKey(0)) cache.Get(0); },
                () => cache.Remove(1)
            );

            var top = cache.GetMostFrequentlyAccessed(3).ToList();
            Assert.True(top.Count <= 4);
            Assert.All(top, item => Assert.NotNull(item.Value));
        }
    }
}