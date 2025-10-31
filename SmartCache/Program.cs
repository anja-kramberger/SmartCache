namespace SmartCache
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cache = new SmartCache<int, string>(3);
            cache.Add(1, "One");
            cache.Add(2, "Two");
            cache.Add(3, "Three");
            cache.Add(4, "Four");

            var top = cache.GetMostFrequentlyAccessed(3);
            foreach (var item in top)
            {
                Console.WriteLine($"{item.Key}: {item.Value}, access count: {item.AccessCount}");
            }
        }
    }
}
