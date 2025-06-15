namespace tfl_stats.Server.Services.Cache
{
    public delegate Task<T> CacheLoader<T>(string cacheKey);
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key, CacheLoader<T> loader);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
    }
}