using FluentAssertions;
using SkillLearning.Domain.Entities;

namespace SkillLearning.Tests.UnitTests.Domain
{
    public class RefreshTokenTests
    {
        [Fact]
        public void IsActive_ShouldBeFalse_WhenTokenIsExpired()
        {
            // Arrange
            var token = new RefreshToken(TimeSpan.FromMinutes(-1));

            // Assert
            token.IsActive.Should().BeFalse();
            token.IsExpired.Should().BeTrue();
        }

        [Fact]
        public void IsActive_ShouldBeFalse_WhenTokenIsRevoked()
        {
            // Arrange
            var token = new RefreshToken(TimeSpan.FromDays(7));

            // Act
            token.Revoke();

            // Assert
            token.IsActive.Should().BeFalse();
            token.IsRevoked.Should().BeTrue();
        }

        [Fact]
        public async Task Revoke_ShouldDoNothing_WhenAlreadyRevoked()
        {
            // Arrange
            var token = new RefreshToken(TimeSpan.FromDays(1));
            token.Revoke();
            var firstRevokedDate = token.RevokedAt;

            // Act
            await Task.Delay(10);
            token.Revoke();
            var secondRevokedDate = token.RevokedAt;

            // Assert
            secondRevokedDate.Should().Be(firstRevokedDate);
        }

        [Fact]
        public void IsActive_ShouldBeTrue_ForNewToken()
        {
            // Arrange
            var token = new RefreshToken(TimeSpan.FromDays(7));

            // Assert
            token.IsActive.Should().BeTrue();
            token.IsExpired.Should().BeFalse();
            token.IsRevoked.Should().BeFalse();
        }
    }
}