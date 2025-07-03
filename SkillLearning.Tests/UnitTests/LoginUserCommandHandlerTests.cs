using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Common.Models;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Application.Features.Auth.Queries;
using SkillLearning.Domain.Enums;
using SkillLearning.Domain.Events;
using System.Security.Claims;

namespace SkillLearning.Tests.UnitTests
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly LoginUserCommandHandler _handler;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsOptionsMock;
        private readonly Mock<IMediator> _mediatorMock;

        public LoginUserCommandHandlerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _authServiceMock = new Mock<IAuthService>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _jwtSettingsOptionsMock = new Mock<IOptions<JwtSettings>>();

            var jwtSettings = new JwtSettings { Issuer = "TestIssuer" };
            _jwtSettingsOptionsMock.Setup(o => o.Value).Returns(jwtSettings);

            _handler = new LoginUserCommandHandler(
                _mediatorMock.Object,
                new Mock<IUserRepository>().Object,
                _authServiceMock.Object,
                _jwtSettingsOptionsMock.Object,
                _eventPublisherMock.Object,
                _httpContextAccessorMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldUseDefaultValues_WhenHttpContextIsNull()
        {
            var password = "correctpassword";
            var command = new LoginUserCommand { Username = "testuser", Password = password };
            var userDto = new UserDto(
                Guid.NewGuid(),
                "testuser",
                "test@test.com",
                BCrypt.Net.BCrypt.HashPassword(password),
                SkillLearning.Domain.Enums.UserRole.User
            );

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByUsernameQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(userDto);
            _authServiceMock.Setup(s => s.GetUserClaims(userDto)).ReturnsAsync(new List<Claim>());
            _authServiceMock.Setup(s => s.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>(), It.IsAny<string>())).Returns("token");

            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            UserLoginEvent? capturedEvent = null;
            _eventPublisherMock
                .Setup(p => p.PublishAsync(It.IsAny<UserLoginEvent>(), null))
                .Callback<UserLoginEvent, string>((evt, topic) => capturedEvent = evt);

            await _handler.Handle(command, CancellationToken.None);

            capturedEvent.Should().NotBeNull();
            capturedEvent!.IpAddress.Should().Be("Unknown");
            capturedEvent!.UserAgent.Should().Be("Unknown");
        }

        [Fact]
        public async Task Handle_ShouldReturnAuthResult_AndPublishEvent_WhenCredentialsAreValid()
        {
            var password = "correctpassword";
            var command = new LoginUserCommand { Username = "testuser", Password = password };
            var userDto = new UserDto(
                Guid.NewGuid(),
                "testuser",
                "test@test.com",
                BCrypt.Net.BCrypt.HashPassword(password),
                UserRole.User
            );
            var expectedToken = "um.jwt.token.valido";

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetUserByUsernameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);
            _authServiceMock
                .Setup(s => s.GetUserClaims(userDto))
                .ReturnsAsync(new List<Claim>());
            _authServiceMock
                .Setup(s => s.GenerateJwtToken(It.IsAny<IEnumerable<Claim>>(), It.IsAny<string>()))
                .Returns(expectedToken);

            var httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
            httpContext.Request.Headers["User-Agent"] = "Test Browser";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeOfType<AuthResultDto>();
            result!.Token.Should().Be(expectedToken);

            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserLoginEvent>(), null), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            var command = new LoginUserCommand { Username = "testuser", Password = "wrongpassword" };
            var userDto = new UserDto(
                Guid.NewGuid(),
                "testuser",
                "test@test.com",
                BCrypt.Net.BCrypt.HashPassword("correctpassword"),
                UserRole.User
            );

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetUserByUsernameQuery>(q => q.Username == "testuser"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userDto);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeNull();
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserLoginEvent>(), null), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenUserNotFound()
        {
            var command = new LoginUserCommand { Username = "unknown", Password = "password" };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetUserByUsernameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserDto?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}