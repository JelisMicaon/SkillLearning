using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SkillLearning.Application.Common.Models;
using SkillLearning.Domain.Enums;
using SkillLearning.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillLearning.Tests.UnitTests.Services
{
    public class AuthServiceTests
    {
        private readonly AuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
                {
                    {"Jwt:Key", "MinhaChaveSuperSecretaDeTestesComMaisDe32Bytes"},
                    {"Jwt:Issuer", "TestIssuer"},
                    {"Jwt:Audience", "TestAudience"}
                };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(_configuration);
        }

        [Fact]
        public void GenerateJwtToken_ShouldCreateToken_WithCorrectClaimsAndIssuer()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, "user-id-123"),
                new(JwtRegisteredClaimNames.Name, "Test User"),
                new(ClaimTypes.Role, "Admin")
            };
            var issuer = "TestIssuer";

            // Act
            var tokenString = _authService.GenerateJwtToken(claims, issuer);

            // Assert
            tokenString.Should().NotBeNullOrEmpty();
            var handler = new JwtSecurityTokenHandler();
            var decodedToken = handler.ReadJwtToken(tokenString);
            decodedToken.Issuer.Should().Be(issuer);
            decodedToken.Audiences.Should().Contain("TestAudience");
            decodedToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "user-id-123");
            decodedToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ShouldReturnPrincipal_WhenSignatureIsValid()
        {
            // Arrange
            var tokenString = _authService.GenerateJwtToken(new List<Claim> { new("test", "claim") }, "TestIssuer");

            // Act
            var principal = _authService.GetPrincipalFromExpiredToken(tokenString);

            // Assert
            principal.Should().NotBeNull();
            principal.Claims.Should().Contain(c => c.Type == "test" && c.Value == "claim");
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ShouldReturnNull_WhenSignatureIsInvalid()
        {
            // Arrange
            var wrongConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "ESTA_CHAVE_E_DIFERENTE_E_CAUSARA_FALHA_NA_VALIDACAO" },
                    { "Jwt:Issuer", "TestIssuer" },
                })
                .Build();

            var wrongAuthService = new AuthService(wrongConfig);
            var tokenWithInvalidSignature = wrongAuthService.GenerateJwtToken([], "TestIssuer");

            // Act
            var principal = _authService.GetPrincipalFromExpiredToken(tokenWithInvalidSignature);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public async Task GetUserClaims_ShouldReturnCorrectClaims_ForGivenUserDto()
        {
            // Arrange
            var userId = System.Guid.NewGuid();
            var userDto = new UserDto(userId, "testuser", "test@user.com", UserRole.User);

            // Act
            var claims = await _authService.GetUserClaims(userDto);

            // Assert
            claims.Should().NotBeNull();
            claims.Should().HaveCount(4);
            claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
            claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "testuser");
            claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@user.com");
            claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        }
    }
}