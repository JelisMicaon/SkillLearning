using SkillLearning.Domain.Common;
using SkillLearning.Domain.Enums;

namespace SkillLearning.Domain.Entities
{
    public class User : EntityBase
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
    }
}