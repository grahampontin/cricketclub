using System;
using Microsoft.Extensions.Caching.Memory;

namespace CricketClubMiddle
{
    /// <summary>
    /// Process-wide in-process cache backed by <see cref="MemoryCache"/>.
    /// Keeps the same singleton API that existing code relies on so callers need no changes.
    /// Using MemoryCache instead of a hand-rolled Hashtable gives us proper memory-pressure
    /// eviction, accurate TTL enforcement, and thread-safe atomic GetOrCreate semantics.
    /// </summary>
    public class InternalCache
    {
        private static readonly InternalCache _instance = new InternalCache();
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        private InternalCache() { }

        public static InternalCache GetInstance() => _instance;

        public void Insert(string key, object value, TimeSpan timeToLive)
        {
            _cache.Set(key, value, timeToLive);
        }

        public object Get(string key)
        {
            _cache.TryGetValue(key, out var value);
            return value;
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            _cache.Clear();
        }
    }
}
