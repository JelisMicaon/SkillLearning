namespace SkillLearning.Domain.Events
{
    public record UserRegisteredEvent(Guid UserId, string Username, string Email, DateTime RegisteredAt);
}