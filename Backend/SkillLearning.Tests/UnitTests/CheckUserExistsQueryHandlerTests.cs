using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.CheckUserExistsUseCase;
using SkillLearning.Domain.Entities;
using SkillLearning.Infrastructure.Persistence;

namespace SkillLearning.Tests.UnitTests.Auth
{
    public class CheckUserExistsQueryHandlerTests : IDisposable
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly CheckUserExistsQueryHandler _handler;
        private readonly ApplicationDbContext _readContext;
        private bool _disposed;

        public CheckUserExistsQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            _readContext = new ApplicationDbContext(options);
            _cacheServiceMock = new Mock<ICacheService>();

            _handler = new CheckUserExistsQueryHandler(_readContext, _cacheServiceMock.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenUserDoesNotExistAnywhere()
        {
            // Arrange
            var query = new CheckUserExistsQuery("nonexistent", "nonexistent@test.com");
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExistsInCacheByUsername()
        {
            // Arrange
            var query = new CheckUserExistsQuery("cacheduser", "any@email.com");
            var cacheKey = $"username:{query.Username}";
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(cacheKey)).ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExistsInRepository()
        {
            // Arrange
            var query = new CheckUserExistsQuery("dbuser", "db@test.com");

            var existingUser = new User("dbuser", "another@email.com", "password");
            _readContext.Users.Add(existingUser);

            await _readContext.SaveChangesAsync();

            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _readContext.Database.EnsureDeleted();
                _readContext.Dispose();
            }

            _disposed = true;
        }
    }
}