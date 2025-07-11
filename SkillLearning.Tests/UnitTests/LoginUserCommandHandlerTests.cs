using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Events;
using System.Net;
using System.Security.Claims;

namespace SkillLearning.Tests.UnitTests
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IUnitOfWork> _iUnitOfWorkMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly LoginUserCommandHandler _handler;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsOptionsMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IActivityNotifier> _iActivityNotifier;

        public LoginUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authServiceMock = new Mock<IAuthService>();
            _iUnitOfWorkMock = new Mock<IUnitOfWork>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _jwtSettingsOptionsMock = new Mock<IOptions<JwtSettings>>();
            _iActivityNotifier = new Mock<IActivityNotifier>();

            var jwtSettings = new JwtSettings { Issuer = "TestIssuer", Key = "O8jLw5kXvPq9s2v8y/B?E(G+KbPeShVm" };
            _jwtSettingsOptionsMock.Setup(o => o.Value).Returns(jwtSettings);

            _handler = new LoginUserCommandHandler(
                _userRepositoryMock.Object,
                _authServiceMock.Object,
                _jwtSettingsOptionsMock.Object,
                _eventPublisherMock.Object,
                _httpContextAccessorMock.Object,
                _iUnitOfWorkMock.Object,
                _iActivityNotifier.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnAuthResult_WhenCredentialsAreValid()
        {
            // Arrange
            var password = "correctpassword";
            var command = new LoginUserCommand { Username = "testuser", Password = password };
            var expectedToken = "um.jwt.token.valido";
            var userEntity = new User("testuser", "test@test.com", password);

            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameAsync(command.Username))
                .ReturnsAsync(userEntity);

            _authServiceMock
                .Setup(s => s.GetUserClaims(It.Is<UserDto>(dto => dto.Username == command.Username)))
                .ReturnsAsync(new List<Claim>());

            _authServiceMock
                .Setup(s => s.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>(), It.IsAny<string>()))
                .Returns(expectedToken);

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
            httpContext.Request.Headers["User-Agent"] = "Test Browser";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.AccessToken.Should().Be(expectedToken);
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserLoginEvent>(), null), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserNotFound()
        {
            // Arrange
            var command = new LoginUserCommand { Username = "unknown", Password = "password" };

            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameAsync(command.Username))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError(e => e.Message == "Nome de usuário ou senha inválidos.").Should().BeTrue();
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserLoginEvent>(), null), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenPasswordIsIncorrect()
        {
            // Arrange
            var command = new LoginUserCommand { Username = "testuser", Password = "wrongpassword" };
            var userEntity = new User("testuser", "test@test.com", "correctpassword");

            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameAsync(command.Username))
                .ReturnsAsync(userEntity);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError(e => e.Message == "Nome de usuário ou senha inválidos.").Should().BeTrue();
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserLoginEvent>(), null), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldUseDefaultEventValues_WhenHttpContextIsNull()
        {
            // Arrange
            var password = "correctpassword";
            var command = new LoginUserCommand { Username = "testuser", Password = password };
            var userEntity = new User("testuser", "test@test.com", password);

            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameAsync(command.Username))
                .ReturnsAsync(userEntity);

            _authServiceMock
                .Setup(s => s.GetUserClaims(It.IsAny<UserDto>()))
                .ReturnsAsync(new List<Claim>());

            _authServiceMock
                .Setup(s => s.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>(), It.IsAny<string>()))
                .Returns("token");

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null!);

            UserLoginEvent? capturedEvent = null;
            _eventPublisherMock
                .Setup(p => p.PublishAsync(It.IsAny<UserLoginEvent>(), null))
                .Callback<UserLoginEvent, string>((evt, topic) => capturedEvent = evt);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedEvent.Should().NotBeNull();
            capturedEvent!.IpAddress.Should().Be("Unknown");

            capturedEvent!.UserAgent.Should().Be("Unknown");
        }
    }
}