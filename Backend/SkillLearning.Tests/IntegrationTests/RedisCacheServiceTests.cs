using FluentAssertions;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using SkillLearning.Infrastructure.Services;
using Testcontainers.Redis;

namespace SkillLearning.Tests.IntegrationTests
{
    public class RedisCacheServiceTests : IAsyncLifetime
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder().Build();
        private RedisCacheService _cacheService = null!;

        public async Task DisposeAsync()
            => await _redisContainer.StopAsync();

        [Fact]
        public async Task GetAsync_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            // Arrange
            var key = "non-existent-key";

            // Act
            var retrievedData = await _cacheService.GetAsync<TestData>(key);

            // Assert
            retrievedData.Should().BeNull();
        }

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();

            var cacheOptions = new RedisCacheOptions
            {
                Configuration = _redisContainer.GetConnectionString()
            };
            var distributedCache = new RedisCache(cacheOptions);

            _cacheService = new RedisCacheService(distributedCache);
        }

        private record TestData(int Id, string Name);

        [Fact]
        public async Task RemoveAsync_ShouldDeleteItem_FromCache()
        {
            // Arrange
            var key = "item-to-be-removed";
            var dataToCache = new TestData(2, "Item Temporário");
            await _cacheService.SetAsync(key, dataToCache, TimeSpan.FromMinutes(1));

            // Act
            await _cacheService.RemoveAsync(key);
            var retrievedData = await _cacheService.GetAsync<TestData>(key);

            // Assert
            retrievedData.Should().BeNull();
        }

        [Fact]
        public async Task SetAsync_And_GetAsync_Should_CorrectlyStoreAndRetrieveItem()
        {
            // Arrange
            var key = "my-test-key";
            var dataToCache = new TestData(1, "Mago Supremo");

            // Act
            await _cacheService.SetAsync(key, dataToCache, TimeSpan.FromMinutes(1));
            var retrievedData = await _cacheService.GetAsync<TestData>(key);

            // Assert
            retrievedData.Should().NotBeNull();
            retrievedData.Should().BeEquivalentTo(dataToCache);
        }
    }
}