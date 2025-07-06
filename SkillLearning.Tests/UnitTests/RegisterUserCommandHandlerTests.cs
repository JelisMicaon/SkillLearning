using FluentAssertions;
using Moq;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Domain.Events;

namespace SkillLearning.Tests.UnitTests
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly RegisterUserCommandHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _iUnitOfWorkMock;

        public RegisterUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _cacheServiceMock = new Mock<ICacheService>();
            _iUnitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new RegisterUserCommandHandler(
                _userRepositoryMock.Object,
                _eventPublisherMock.Object,
                _cacheServiceMock.Object,
                _iUnitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnFailedResult_WhenUserAlreadyExists()
        {
            // Arrange
            var command = new RegisterUserCommand { Username = "testuser", Email = "test@example.com", Password = "password" };

            _userRepositoryMock
                .Setup(r => r.DoesUserExistAsync(command.Username, command.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError(e => e.Message == "O nome de usuário ou e-mail já está em uso.").Should().BeTrue();

            _iUnitOfWorkMock.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessResult_WhenUserDoesNotExist()
        {
            // Arrange
            var command = new RegisterUserCommand { Username = "newuser", Email = "new@example.com", Password = "password123" };

            _userRepositoryMock
                .Setup(r => r.DoesUserExistAsync(command.Username, command.Email))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _iUnitOfWorkMock.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once);
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserRegisteredEvent>(), null), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()), Times.Exactly(3));
        }
    }
}