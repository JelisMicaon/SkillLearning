using Amazon.XRay.Recorder.Core.Internal.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SkillLearning.Infrastructure.Persistence;
using SkillLearning.Tests.TestHelpers;
using System.Collections.Concurrent;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace SkillLearning.Tests.IntegrationTests
{
    public class QueryPerformanceInterceptorTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder().Build();
        private ApplicationWriteDbContext _context = null!;
        private ListLogger<QueryPerformanceInterceptor> _listLogger = null!;

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
            await _dbContainer.StopAsync();
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            _listLogger = new ListLogger<QueryPerformanceInterceptor>();
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var subsegmentsDictionary = new ConcurrentDictionary<DbCommand, Subsegment>();

            var interceptor = new QueryPerformanceInterceptor(_listLogger, httpContextAccessorMock.Object, subsegmentsDictionary);

            var options = new DbContextOptionsBuilder<ApplicationWriteDbContext>()
                .UseNpgsql(_dbContainer.GetConnectionString())
                .AddInterceptors(interceptor)
                .Options;

            _context = new ApplicationWriteDbContext(options);
            await _context.Database.EnsureCreatedAsync();
        }

        [Fact]
        public async Task Interceptor_ShouldLogWarning_WhenQueryIsSlow()
        {
            // Arrange
            var slowQuery = "SELECT pg_sleep(0.15)";

            // Act
            await _context.Database.ExecuteSqlRawAsync(slowQuery);

            // Assert
            _listLogger.Logs.Should().Contain(log =>
                log.LogLevel == Microsoft.Extensions.Logging.LogLevel.Warning &&
                log.Message.Contains("Slow DB Command")
            );
        }

        [Fact]
        public async Task Interceptor_ShouldNotLogWarning_WhenQueryIsFast()
        {
            // Arrange
            var fastQuery = "SELECT 1";
            _listLogger.Logs.Clear();

            // Act
            await _context.Database.ExecuteSqlRawAsync(fastQuery);

            // Assert
            _listLogger.Logs.Should().NotContain(log =>
                log.Message.Contains("Slow DB Command")
            );
        }
    }
}