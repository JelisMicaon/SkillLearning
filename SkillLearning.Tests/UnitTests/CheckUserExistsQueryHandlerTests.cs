using FluentAssertions;
using Moq;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.Queries;

namespace SkillLearning.Tests.UnitTests
{
    public class CheckUserExistsQueryHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly CheckUserExistsQueryHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public CheckUserExistsQueryHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
            _handler = new CheckUserExistsQueryHandler(_userRepositoryMock.Object, _cacheServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenUserDoesNotExistAnywhere()
        {
            // Arrange
            var query = new CheckUserExistsQuery("nonexistent", "nonexistent@test.com");
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);
            _userRepositoryMock.Setup(r => r.DoesUserExistAsync(query.Username, query.Email)).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.DoesUserExistAsync(query.Username, query.Email), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExistsInCacheByEmail()
        {
            // Arrange
            var query = new CheckUserExistsQuery("newuser", "cached@test.com");
            var usernameCacheKey = $"username:{query.Username}";
            var emailCacheKey = $"email:{query.Email}";

            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(usernameCacheKey)).ReturnsAsync((Guid?)null);
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(emailCacheKey)).ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.DoesUserExistAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExistsInCacheByUsername()
        {
            // Arrange
            var query = new CheckUserExistsQuery("cacheduser", "cached@test.com");
            var cacheKey = $"username:{query.Username}";
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(cacheKey)).ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.DoesUserExistAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExistsInRepository()
        {
            // Arrange
            var query = new CheckUserExistsQuery("dbuser", "db@test.com");
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null);
            _userRepositoryMock.Setup(r => r.DoesUserExistAsync(query.Username, query.Email)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.DoesUserExistAsync(query.Username, query.Email), Times.Once);
        }
    }
}