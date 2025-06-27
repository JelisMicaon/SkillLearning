namespace SkillLearning.Application.Common.Models
{
    public class AuthResultDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}