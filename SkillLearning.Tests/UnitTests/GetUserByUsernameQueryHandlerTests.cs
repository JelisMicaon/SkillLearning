using FluentAssertions;
using Moq;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Application.Features.Auth.Queries;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Enums;

namespace SkillLearning.Tests.UnitTests
{
    public class GetUserByUsernameQueryHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly GetUserByUsernameQueryHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public GetUserByUsernameQueryHandlerTests()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new GetUserByUsernameQueryHandler(_cacheServiceMock.Object, _userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserFromRepository_AndSetCache_WhenCacheIsMiss()
        {
            // Arrange
            var query = new GetUserByUsernameQuery("newuser");
            var userFromDb = new User(
                id: Guid.NewGuid(),
                username: "newuser",
                email: "new@test.com",
                passwordHash: "some_hash",
                role: UserRole.User,
                createdAt: DateTime.UtcNow
            );
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);
            _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(query.Username)).ReturnsAsync(userFromDb);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Username.Should().Be(userFromDb.Username);
            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(query.Username), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync($"user:{userFromDb.Id}", It.Is<UserDto>(dto => dto.Id == userFromDb.Id), It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserIsNotFoundInRepository()
        {
            // Arrange
            var query = new GetUserByUsernameQuery("notfounduser");
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);
            _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(query.Username)).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError(e => e.Message == "Usuário não encontrado.").Should().BeTrue();
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserFromCache_WhenCacheIsHit()
        {
            // Arrange
            var query = new GetUserByUsernameQuery("testuser");
            var userId = Guid.NewGuid();
            var cachedUserDto = new UserDto(userId, "testuser", "test@test.com", UserRole.User);
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>($"username:{query.Username}")).ReturnsAsync(userId);
            _cacheServiceMock.Setup(c => c.GetAsync<UserDto>($"user:{userId}")).ReturnsAsync(cachedUserDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(cachedUserDto);
            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldFetchFromDb_WhenUserIdInCacheButDtoIsNot()
        {
            // Arrange
            var query = new GetUserByUsernameQuery("testuser");
            var userId = Guid.NewGuid();
            var userFromDb = new User(
                id: userId,
                username: "testuser",
                email: "test@test.com",
                passwordHash: "hash",
                role: UserRole.User,
                createdAt: DateTime.UtcNow
            );
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>($"username:{query.Username}")).ReturnsAsync(userId);
            _cacheServiceMock.Setup(c => c.GetAsync<UserDto>($"user:{userId}")).ReturnsAsync((UserDto?)null);
            _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(query.Username)).ReturnsAsync(userFromDb);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Username.Should().Be(userFromDb.Username);
            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(query.Username), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync($"user:{userId}", It.Is<UserDto>(dto => dto.Id == userId), It.IsAny<TimeSpan?>()), Times.Once);
        }
    }
}