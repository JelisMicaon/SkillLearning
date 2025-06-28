using SkillLearning.Domain.Entities;

namespace SkillLearning.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);

        Task<User?> GetUserByUsernameAsync(string username);

        Task<bool> UserExistsByUsernameAsync(string username, string email);
    }
}