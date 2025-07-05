using SkillLearning.Domain.Entities;

namespace SkillLearning.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);

        Task<bool> DoesUserExistAsync(string username, string email);

        Task<User?> GetUserByIdAsync(Guid userId);

        Task<User?> GetUserByUsernameAsync(string username);

        Task UpdateUserAsync(User user);
    }
}