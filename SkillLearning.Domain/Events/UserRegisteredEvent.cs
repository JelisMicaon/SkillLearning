namespace SkillLearning.Domain.Events
{
    public class UserRegisteredEvent
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime RegisteredAt { get; set; }

        public UserRegisteredEvent(Guid userId, string username, string email)
        {
            UserId = userId;
            Username = username;
            Email = email;
            RegisteredAt = DateTime.UtcNow;
        }
    }
}