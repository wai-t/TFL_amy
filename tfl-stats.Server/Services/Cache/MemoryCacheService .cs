using Microsoft.Extensions.Caching.Memory;

namespace tfl_stats.Server.Services.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult(default(T?));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            _memoryCache.Set(key, value, expiration);
            return Task.CompletedTask;
        }
    }

}
