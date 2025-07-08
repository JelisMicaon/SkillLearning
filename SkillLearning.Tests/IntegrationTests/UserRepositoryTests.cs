using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SkillLearning.Domain.Entities;
using SkillLearning.Infrastructure.Persistence;
using SkillLearning.Infrastructure.Persistence.Repositories;
using Testcontainers.PostgreSql;

namespace SkillLearning.Tests.IntegrationTests
{
    public class UserRepositoryTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_pass")
            .Build();

        private ApplicationDbContext _context = null!;
        private UserRepository _userRepository = null!;

        [Fact]
        public async Task AddUser_ShouldSuccessfullyPersistUser_ToDatabase()
        {
            // Arrange
            var newUser = new User("jelis", "jelis@micaon.com", "strongPassword123");

            // Act
            _userRepository.AddUser(newUser);
            await _context.SaveChangesAsync();

            // Assert
            var userFromDb = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == newUser.Id);

            userFromDb.Should().NotBeNull();
            userFromDb!.Username.Should().Be("jelis");
        }

        public async Task DisposeAsync()
            => await _dbContainer.StopAsync();

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(_dbContainer.GetConnectionString()).Options;
            _context = new ApplicationDbContext(options);
            await _context.Database.MigrateAsync();
            _userRepository = new UserRepository(_context);
        }
    }
}