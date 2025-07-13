using FluentAssertions;
using Moq;
using SkillLearning.Application.Common.Errors;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Users.UpdateUserEmailUseCase;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Enums;

namespace SkillLearning.Tests.UnitTests.Users
{
    public class UpdateUserEmailCommandHandlerTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly UpdateUserEmailCommandHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _iUnitOfWorkMock;

        public UpdateUserEmailCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
            _iUnitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new UpdateUserEmailCommandHandler(_userRepositoryMock.Object, _cacheServiceMock.Object, _iUnitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new UpdateUserEmailCommand(userId, "new.email@test.com");

            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Any(e => e.Message == "Usuário não encontrado.").Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ShouldReturnValidationFailure_WhenDomainThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new UpdateUserEmailCommand(userId, "email-invalido");
            var user = new User(userId, "testuser", "old.email@test.com", "hash", UserRole.User, DateTime.UtcNow);

            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.HasError<ValidationError>().Should().BeTrue();
            _iUnitOfWorkMock.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new UpdateUserEmailCommand(userId, "new.email@test.com");
            var user = new User(userId, "testuser", "old.email@test.com", "hash", UserRole.User, DateTime.UtcNow);

            _userRepositoryMock.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _iUnitOfWorkMock.Verify(r => r.SaveChangesAsync(CancellationToken.None), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveAsync($"user:{userId}"), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoveAsync("email:old.email@test.com"), Times.Once);
        }
    }
}