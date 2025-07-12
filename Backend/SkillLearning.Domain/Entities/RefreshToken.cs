using SkillLearning.Domain.Common;
using System.Security.Cryptography;

namespace SkillLearning.Domain.Entities
{
    public class RefreshToken : EntityBase
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        private RefreshToken()
        { }

        public RefreshToken(TimeSpan lifetime)
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            Token = Convert.ToBase64String(randomNumber);

            ExpiresAt = DateTime.UtcNow.Add(lifetime);
        }

        public void Revoke()
        {
            if (IsRevoked)
                return;

            RevokedAt = DateTime.UtcNow;
        }
    }
}