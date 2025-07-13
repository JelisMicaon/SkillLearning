using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Application.Features.Auth.GetUserByUsernameUseCase;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Enums;
using SkillLearning.Infrastructure.Persistence;

namespace SkillLearning.Tests.UnitTests.Auth
{
    public class GetUserByUsernameQueryHandlerTests : IDisposable
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly GetUserByUsernameQueryHandler _handler;
        private readonly ApplicationDbContext _readContext;
        private bool _disposed;

        public GetUserByUsernameQueryHandlerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            _readContext = new ApplicationDbContext(options);
            _cacheServiceMock = new Mock<ICacheService>();

            _handler = new GetUserByUsernameQueryHandler(_cacheServiceMock.Object, _readContext);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserIsNotFoundInRepository()
        {
            // Arrange
            var query = new GetUserByUsernameQuery("notfounduser");
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError(e => e.Message == "Usuário não encontrado.").Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnUserFromCache_WhenCacheIsHit()
        {
            // Arrange
            var query = new GetUserByUsernameQuery("testuser");
            var userId = Guid.NewGuid();
            var cachedUserDto = new UserDto(userId, "testuser", "test@test.com", UserRole.User);
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(($"username:{query.Username}"))).ReturnsAsync(userId);
            _cacheServiceMock.Setup(c => c.GetAsync<UserDto>($"user:{userId}")).ReturnsAsync(cachedUserDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(cachedUserDto);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserFromRepository_AndSetCache_WhenCacheIsMiss()
        {
            // Arrange
            var query = new GetUserByUsernameQuery("newuser");
            var userToCreate = new User("newuser", "new@test.com", "password");

            _readContext.Users.Add(userToCreate);
            await _readContext.SaveChangesAsync();

            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Username.Should().Be(userToCreate.Username);
            _cacheServiceMock.Verify(c => c.SetAsync($"user:{userToCreate.Id}", It.IsAny<UserDto>(), It.IsAny<TimeSpan?>()), Times.Once);
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