using FluentAssertions;
using SkillLearning.Domain.Entities;

namespace SkillLearning.Tests.UnitTests.Domain
{
    public class UserTests
    {
        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenGivenIncorrectPassword()
        {
            // Arrange
            var user = new User("testuser", "test@email.com", "correctPassword123");

            // Act
            var result = user.VerifyPassword("wrongPassword");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenGivenCorrectPassword()
        {
            // Arrange
            var user = new User("testuser", "test@email.com", "correctPassword123");

            // Act
            var result = user.VerifyPassword("correctPassword123");

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void VerifyPassword_ShouldReturnFalse_WhenGivenNullOrWhitespacePassword(string invalidPassword)
        {
            // Arrange
            var user = new User("testuser", "test@email.com", "correctPassword123");

            // Act
            var result = user.VerifyPassword(invalidPassword);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_ShouldThrowArgumentException_WhenGivenNullOrWhitespacePassword(string invalidPassword)
        {
            // Act
            var act = () => new User("testuser", "test@email.com", invalidPassword);

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}