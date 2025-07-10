using FluentAssertions;
using SkillLearning.Domain.Entities;

namespace SkillLearning.Tests.UnitTests.Domain
{
    public class UserTests
    {
        [Fact]
        public void ChangeEmail_ShouldDoNothing_WhenEmailIsTheSame()
        {
            // Arrange
            var initialEmail = "same@email.com";
            var user = new User("testuser", initialEmail, "password123");

            // Act
            user.ChangeEmail(initialEmail);

            // Assert
            user.Email.Should().Be(initialEmail);
        }

        [Fact]
        public void GetActiveRefreshToken_ShouldReturnActiveToken_WhenOneExists()
        {
            // Arrange
            var user = new User("test", "test@test.com", "pass");
            var revokedToken = new RefreshToken(TimeSpan.FromDays(1));
            revokedToken.Revoke();
            var activeToken = new RefreshToken(TimeSpan.FromDays(1));

            user.AddRefreshToken(revokedToken);
            user.AddRefreshToken(activeToken);

            // Act
            var result = user.GetActiveRefreshToken();

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(activeToken);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("email-invalido")]
        public void ChangeEmail_ShouldThrowArgumentException_WhenEmailIsInvalid(string? invalidEmail)
        {
            // Arrange
            var user = new User("testuser", "initial@email.com", "password123");

            // Act
            var act = () => user.ChangeEmail(invalidEmail!);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_ShouldThrowArgumentException_WhenGivenNullOrWhitespacePassword(string? invalidPassword)
        {
            // Act
            var act = () => new User("testuser", "test@email.com", invalidPassword!);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void VerifyPassword_ShouldReturnFalse_WhenGivenNullOrWhitespacePassword(string? invalidPassword)
        {
            // Arrange
            var user = new User("testuser", "test@email.com", "correctPassword123");

            // Act
            var result = user.VerifyPassword(invalidPassword!);

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
    }
}