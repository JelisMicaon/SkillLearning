using Microsoft.Extensions.Caching.Distributed;
using SkillLearning.Application.Common.Interfaces;
using System.Text.Json;

namespace SkillLearning.Infrastructure.Services
{
    public class RedisCacheService(IDistributedCache cache) : ICacheService
    {
        private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web);

        public async Task<T?> GetAsync<T>(string key)
        {
            var cached = await cache.GetStringAsync(key);
            return cached is null ? default : JsonSerializer.Deserialize<T>(cached, options);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
            };
            var json = JsonSerializer.Serialize(value, RedisCacheService.options);
            await cache.SetStringAsync(key, json, options);
        }

        public async Task RemoveAsync(string key)
        {
            await cache.RemoveAsync(key);
        }
    }
}