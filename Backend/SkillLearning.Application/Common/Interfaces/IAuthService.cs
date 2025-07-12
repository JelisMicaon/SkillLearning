using SkillLearning.Application.Common.Models;
using System.Security.Claims;

namespace SkillLearning.Application.Common.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(IEnumerable<Claim> claims, string issuer);

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        Task<List<Claim>> GetUserClaims(UserDto user);
    }
}