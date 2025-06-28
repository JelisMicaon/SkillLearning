namespace SkillLearning.Domain.Events
{
    public record UserLoginEvent(Guid UserId, string Username, string Email, DateTime Timestamp, string IpAddress, string UserAgent);
}