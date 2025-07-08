using Microsoft.EntityFrameworkCore;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Domain.Entities;

namespace SkillLearning.Infrastructure.Persistence.Repositories
{
    public class UserRepository(ApplicationWriteDbContext context) : IUserRepository
    {
        public void AddRefreshToken(RefreshToken refreshToken)
            => context.RefreshTokens.Add(refreshToken);

        public void AddUser(User user)
            => context.Users.Add(user);

        public async Task<bool> DoesUserExistAsync(string username, string email)
            => await context.Users.AsNoTracking().AnyAsync(u => u.Username == username || u.Email == email);

        public async Task<User?> GetUserByIdAsync(Guid userId)
            => await context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<User?> GetUserByUsernameAsync(string username)
            => await context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Username == username);

        public async Task<bool> IsEmailInUseAsync(string email)
            => await context.Users.AsNoTracking().AnyAsync(u => u.Email == email);
    }
}