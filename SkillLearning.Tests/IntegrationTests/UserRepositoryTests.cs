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

        private ApplicationWriteDbContext _context = null!;
        private UserRepository _userRepository = null!;

        [Fact]
        public async Task AddRefreshToken_ShouldPersistToken_AssociatedWithUser()
        {
            // Arrange
            var user = new User("user_with_token", "token@test.com", "password");
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();

            var refreshToken = new RefreshToken(TimeSpan.FromDays(7));
            refreshToken.UserId = user.Id;

            // Act
            _userRepository.AddRefreshToken(refreshToken);
            await _context.SaveChangesAsync();

            // Assert
            var userFromDb = await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstAsync(u => u.Id == user.Id);

            userFromDb.RefreshTokens.Should().NotBeEmpty();
            userFromDb.RefreshTokens.Should().Contain(rt => rt.Token == refreshToken.Token);
        }

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

        [Fact]
        public async Task DoesUserExistAsync_ShouldReturnTrue_WhenEmailExists()
        {
            // Arrange
            var existingUser = new User("existing_user", "existing@test.com", "password");
            await _context.Users.AddAsync(existingUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.DoesUserExistAsync("non_existent_user", "existing@test.com");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnCorrectUser_WhenUserExists()
        {
            // Arrange
            var newUser = new User("test_user_by_id", "byid@test.com", "password");
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserByIdAsync(newUser.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(newUser.Id);
            result.Username.Should().Be("test_user_by_id");
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldReturnCorrectUser_WhenUserExists()
        {
            // Arrange
            var newUser = new User("test_user_by_username", "byusername@test.com", "password");
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserByUsernameAsync(newUser.Username);

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be(newUser.Username);
        }

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();

            var options = new DbContextOptionsBuilder<ApplicationWriteDbContext>()
                .UseNpgsql(_dbContainer.GetConnectionString())
                .Options;

            _context = new ApplicationWriteDbContext(options);
            await _context.Database.EnsureCreatedAsync();

            _userRepository = new UserRepository(_context);
        }

        [Fact]
        public async Task IsEmailInUseAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
        {
            // Arrange
            var email = "non_existing@email.com";

            // Act
            var result = await _userRepository.IsEmailInUseAsync(email);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsEmailInUseAsync_ShouldReturnTrue_WhenEmailExists()
        {
            // Arrange
            var email = "existing@email.com";
            var newUser = new User("test_user_email", email, "password");
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.IsEmailInUseAsync(email);

            // Assert
            result.Should().BeTrue();
        }
    }
}