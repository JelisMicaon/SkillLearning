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
            var query = new CheckUserExistsQuery("nonexistent", "nonexistent@test.com");
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null); // Cache miss
            _userRepositoryMock.Setup(r => r.UserExistsByUsernameAsync(query.Username, query.Email)).ReturnsAsync(false); // Não encontrado no DB

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.UserExistsByUsernameAsync(query.Username, query.Email), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExistsInCacheByEmail()
        {
            var query = new CheckUserExistsQuery("newuser", "cached@test.com");
            var usernameCacheKey = $"username:{query.Username}";
            var emailCacheKey = $"email:{query.Email}";

            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(usernameCacheKey)).ReturnsAsync((Guid?)null); // Cache miss no username
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(emailCacheKey)).ReturnsAsync(Guid.NewGuid()); // Cache hit no email

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.UserExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExistsInCacheByUsername()
        {
            var query = new CheckUserExistsQuery("cacheduser", "cached@test.com");
            var cacheKey = $"username:{query.Username}";
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(cacheKey)).ReturnsAsync(Guid.NewGuid());

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.UserExistsByUsernameAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenUserExistsInRepository()
        {
            var query = new CheckUserExistsQuery("dbuser", "db@test.com");
            _cacheServiceMock.Setup(c => c.GetAsync<Guid?>(It.IsAny<string>())).ReturnsAsync((Guid?)null); // Cache miss em tudo
            _userRepositoryMock.Setup(r => r.UserExistsByUsernameAsync(query.Username, query.Email)).ReturnsAsync(true); // Encontrado no DB

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().BeTrue();
            _userRepositoryMock.Verify(r => r.UserExistsByUsernameAsync(query.Username, query.Email), Times.Once);
        }
    }
}