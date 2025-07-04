using FluentAssertions;
using Moq;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Enums;
using SkillLearning.Domain.Events;

namespace SkillLearning.Tests.UnitTests
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly RegisterUserCommandHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public RegisterUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _cacheServiceMock = new Mock<ICacheService>();

            _handler = new RegisterUserCommandHandler(
                _userRepositoryMock.Object,
                _eventPublisherMock.Object,
                _cacheServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenUserAlreadyExists()
        {
            // Arrange
            var command = new RegisterUserCommand { Username = "testuser", Email = "test@example.com", Password = "password" };

            _userRepositoryMock
                .Setup(r => r.DoesUserExistAsync(command.Username, command.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserRegisteredEvent>(), null), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrueAndCreateUser_WhenUserDoesNotExist()
        {
            // Arrange
            var command = new RegisterUserCommand { Username = "newuser", Email = "new@example.com", Password = "password123" };

            _userRepositoryMock
                .Setup(r => r.DoesUserExistAsync(command.Username, command.Email))
                .ReturnsAsync(false);

            User? addedUser = null;
            _userRepositoryMock
                .Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .Callback<User>(user => addedUser = user)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();

            _userRepositoryMock.Verify(r => r.AddUserAsync(It.Is<User>(u => u.Username == command.Username)), Times.Once);

            addedUser.Should().NotBeNull();
            addedUser!.Username.Should().Be(command.Username);
            addedUser.Email.Should().Be(command.Email);
            addedUser.Role.Should().Be(UserRole.User);

            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserRegisteredEvent>(), null), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()), Times.Exactly(3));
        }
    }
}