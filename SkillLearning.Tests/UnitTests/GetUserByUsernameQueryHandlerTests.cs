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
        public async Task Handle_ShouldFetchFromDb_WhenUserIdInCacheButDtoIsNot()
        {
            var query = new GetUserByUsernameQuery("testuser");
            var userId = Guid.NewGuid();
            var userFromDb = new User { Id = userId, Username = "testuser", Email = "test@test.com", PasswordHash = "hash" };

            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>($"username:{query.Username}")).ReturnsAsync(userId);
            _cacheServiceMock.Setup(c => c.GetAsync<UserDto>($"user:{userId}")).ReturnsAsync((UserDto?)null);
            _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(query.Username)).ReturnsAsync(userFromDb);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(query.Username), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync($"user:{userId}", It.IsAny<UserDto>(), It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenUserIsNotFoundInRepository()
        {
            var query = new GetUserByUsernameQuery("notfounduser");

            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);
            _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(query.Username)).ReturnsAsync((User?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().BeNull();
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserFromCache_WhenCacheIsHit()
        {
            var query = new GetUserByUsernameQuery("testuser");
            var userId = Guid.NewGuid();
            var cachedUserDto = new UserDto(userId, "testuser", "test@test.com", "hash", UserRole.User);

            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>($"username:{query.Username}")).ReturnsAsync(userId);
            _cacheServiceMock.Setup(c => c.GetAsync<UserDto>($"user:{userId}")).ReturnsAsync(cachedUserDto);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().BeEquivalentTo(cachedUserDto);
            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnUserFromRepository_AndSetCache_WhenCacheIsMiss()
        {
            var query = new GetUserByUsernameQuery("newuser");
            var userFromDb = new User
            {
                Id = Guid.NewGuid(),
                Username = "newuser",
                Email = "new@test.com",
                PasswordHash = "hash",
                Role = UserRole.User
            };

            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>($"username:{query.Username}")).ReturnsAsync((Guid?)null);
            _userRepositoryMock.Setup(r => r.GetUserByUsernameAsync(query.Username)).ReturnsAsync(userFromDb);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Username.Should().Be(userFromDb.Username);

            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync(query.Username), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync($"user:{userFromDb.Id}", It.IsAny<UserDto>(), It.IsAny<TimeSpan?>()), Times.Once);
        }
    }
}