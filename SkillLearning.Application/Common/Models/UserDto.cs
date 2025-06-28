using SkillLearning.Domain.Enums;

namespace SkillLearning.Application.Common.Models
{
    public record UserDto(Guid Id, string Username, string Email, string PasswordHash, UserRole Role);
}