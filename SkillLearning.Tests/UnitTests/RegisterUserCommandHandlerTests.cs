using FluentAssertions;
using MediatR;
using Moq;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.Commands;
using SkillLearning.Application.Features.Auth.Queries;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Events;

namespace SkillLearning.Tests.UnitTests
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly RegisterUserCommandHandler _handler;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public RegisterUserCommandHandlerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _cacheServiceMock = new Mock<ICacheService>();

            _handler = new RegisterUserCommandHandler(
                _mediatorMock.Object,
                _userRepositoryMock.Object,
                _eventPublisherMock.Object,
                _cacheServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenUserAlreadyExists()
        {
            var command = new RegisterUserCommand { Username = "testuser", Email = "test@example.com" };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CheckUserExistsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeFalse();
            _userRepositoryMock.Verify(r => r.AddUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrueAndCreateUser_WhenUserDoesNotExist()
        {
            var command = new RegisterUserCommand { Username = "newuser", Email = "new@example.com", Password = "password123" };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CheckUserExistsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Should().BeTrue();

            _userRepositoryMock.Verify(r => r.AddUserAsync(It.Is<User>(u => u.Username == command.Username)), Times.Once);
            _eventPublisherMock.Verify(p => p.PublishAsync(It.IsAny<UserRegisteredEvent>(), null), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()), Times.Exactly(3));
        }
    }
}