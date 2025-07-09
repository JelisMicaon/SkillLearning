using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
        public void GetPrincipalFromExpiredToken_ShouldReturnNull_WhenSignatureIsInvalid()
        {
            var wrongConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                        {"Jwt:Key", "ChaveErradaQueNaoVaiFuncionarParaValidacao"},
                        {"Jwt:Issuer", "TestIssuer"}
                })
                .Build();
            var wrongAuthService = new AuthService(wrongConfig);
            var tokenWithInvalidSignature = wrongAuthService.GenerateJwtToken(new List<Claim>(), "TestIssuer");

            // Act
            var principal = _authService.GetPrincipalFromExpiredToken(tokenWithInvalidSignature);

            // Assert
            principal.Should().BeNull();
        }

        [Fact]
        public void GetPrincipalFromExpiredToken_ShouldReturnPrincipal_WhenSignatureIsValid()
        {
            var tokenString = _authService.GenerateJwtToken(new List<Claim> { new("test", "claim") }, "TestIssuer");

            // Act
            var principal = _authService.GetPrincipalFromExpiredToken(tokenString);

            // Assert
            principal.Should().NotBeNull();
            principal.Claims.Should().Contain(c => c.Type == "test" && c.Value == "claim");
        }
    }
}