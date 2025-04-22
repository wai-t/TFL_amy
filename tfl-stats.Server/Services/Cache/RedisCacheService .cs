using Newtonsoft.Json;
using StackExchange.Redis;
namespace tfl_stats.Server.Services.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _redis;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _redis.StringGetAsync(key);
            if (value.IsNullOrEmpty) return default;

            return JsonConvert.DeserializeObject<T>(value!);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            var json = JsonConvert.SerializeObject(value);
            await _redis.StringSetAsync(key, json, expiration);
        }
    }

}
