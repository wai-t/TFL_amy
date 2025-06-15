using Microsoft.Extensions.Caching.Memory;

namespace tfl_stats.Server.Services.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly SemaphoreSlim _preloadLock = new(1, 1);

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T?> GetAsync<T>(string key, CacheLoader<T> loader)
        {
            await _preloadLock.WaitAsync();

            try
            {
                if (_memoryCache.TryGetValue(key, out T? value))
                {
                    return value;
                }

                var data = await loader(key);
                await SetAsync(key, data, TimeSpan.FromDays(1));
                return data;
            }
            finally
            {
                _preloadLock.Release();
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            _memoryCache.Set(key, value, expiration);
            return Task.CompletedTask;
        }
    }

}