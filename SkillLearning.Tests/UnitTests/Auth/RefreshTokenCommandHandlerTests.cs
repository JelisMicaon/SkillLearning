using FluentAssertions;
using Moq;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Enums;
using System.Security.Claims;

namespace SkillLearning.Tests.UnitTests.Auth
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly RefreshTokenCommandHandler _handler;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public RefreshTokenCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authServiceMock = new Mock<IAuthService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new RefreshTokenCommandHandler(_userRepositoryMock.Object, _authServiceMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenAccessTokenIsInvalid()
        {
            // Arrange
            var command = new RefreshTokenCommand("invalidAccessToken", "anyRefreshToken");
            _authServiceMock.Setup(s => s.GetPrincipalFromExpiredToken(command.AccessToken)).Returns((ClaimsPrincipal?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError<AuthenticationError>(e => e.Message == "Token de acesso inválido ou malformado.").Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenRefreshTokenIsInvalidOrInactive()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var user = new User(userId, "test", "test@test.com", "pass", UserRole.User, DateTime.UtcNow);
            var command = new RefreshTokenCommand("expiredAccessToken", "invalidOrMissingToken");

            _authServiceMock.Setup(s => s.GetPrincipalFromExpiredToken(command.AccessToken)).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError<AuthenticationError>(e => e.Message == "Refresh token inválido ou expirado.").Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserFromTokenNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var command = new RefreshTokenCommand("validAccessToken", "anyRefreshToken");

            _authServiceMock.Setup(s => s.GetPrincipalFromExpiredToken(command.AccessToken)).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError<AuthenticationError>(e => e.Message == "Usuário associado ao token não encontrado.").Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }));

            var activeRefreshToken = new RefreshToken(TimeSpan.FromDays(1));
            var userWithToken = new User(userId, "test", "test@test.com", "hash", UserRole.User, DateTime.UtcNow);
            userWithToken.AddRefreshToken(activeRefreshToken);

            var command = new RefreshTokenCommand("expiredAccessToken", activeRefreshToken.Token);

            _authServiceMock.Setup(s => s.GetPrincipalFromExpiredToken(command.AccessToken)).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(userWithToken);
            _authServiceMock.Setup(s => s.GetUserClaims(It.IsAny<UserDto>())).ReturnsAsync(new List<Claim>());
            _authServiceMock.Setup(s => s.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>(), It.IsAny<string>())).Returns("newAccessToken");

            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue("a lógica do handler não deveria falhar com os mocks corretos");
            result.Value.AccessToken.Should().Be("newAccessToken");
            result.Value.RefreshToken.Should().NotBe(activeRefreshToken.Token);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            activeRefreshToken.IsRevoked.Should().BeTrue();
        }
    }
}