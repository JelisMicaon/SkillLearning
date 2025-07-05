using Microsoft.EntityFrameworkCore;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Domain.Entities;

namespace SkillLearning.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DoesUserExistAsync(string username, string email)
            => await _context.Users.AsNoTracking().AnyAsync(u => u.Username == username || u.Email == email);

        public async Task<User?> GetUserByIdAsync(Guid userId)
            => await _context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<User?> GetUserByUsernameAsync(string username)
            => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);

        public async Task UpdateUserAsync(User user)
            => await _context.SaveChangesAsync();
    }
}