using SkillLearning.Domain.Entities;

namespace SkillLearning.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);

        Task<User?> GetPasswordHashByUsernameAsync(string username);

        Task<User?> GetUserByEmailAsync(string email);

        Task<User?> GetUserByIdAsync(Guid id);

        Task<User?> GetUserByUsernameAsync(string username);

        Task<bool> UserExistsByUsernameAsync(string username);
    }
}