using SkillLearning.Domain.Entities;
using System.Security.Claims;

namespace SkillLearning.Application.Common.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(IEnumerable<Claim> claims, string issuer);

        Task<List<Claim>> GetUserClaims(User user);
    }
}