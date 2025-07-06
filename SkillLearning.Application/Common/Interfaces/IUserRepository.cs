using SkillLearning.Domain.Entities;

namespace SkillLearning.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        void AddUser(User user);

        Task<bool> DoesUserExistAsync(string username, string email);

        Task<User?> GetUserByIdAsync(Guid userId);

        void AddRefreshToken(RefreshToken refreshToken);

        Task<User?> GetUserByUsernameAsync(string username);
    }
}