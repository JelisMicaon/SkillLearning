using Microsoft.EntityFrameworkCore;
using SkillLearning.Domain.Entities;

namespace SkillLearning.Application.Common.Interfaces
{
    public interface IReadDbContext
    {
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<User> Users { get; }
    }
}