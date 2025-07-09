using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Domain.Entities;
using SkillLearning.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SkillLearning.Tests.IntegrationTests
{
    public class ReadDbContextTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("test_db_read")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();

        private ServiceProvider _serviceProvider = null!;

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationReadDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString())
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());

            _serviceProvider = services.BuildServiceProvider();

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationReadDbContext>();
            await context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _serviceProvider.DisposeAsync();
        }

        [Fact]
        public async Task SaveChangesAsync_OnReadOnlyContext_ShouldThrowInvalidOperationException()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();
            var readDbContext = scope.ServiceProvider.GetRequiredService<IReadDbContext>() as DbContext;

            readDbContext.Should().NotBeNull();
            readDbContext.Add(new User("test", "test@fail.com", "password"));

            // Act
            Func<Task> act = async () => await readDbContext.SaveChangesAsync();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Este DbContext é somente para leitura e não pode salvar alterações.");
        }
    }
}