using SkillLearning.Domain.Common;
using SkillLearning.Domain.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

namespace SkillLearning.Domain.Entities
{
    public class User : EntityBase
    {
        public User(string username, string email, string plainTextPassword)
        {
            Username = username;
            Email = email;
            SetPassword(plainTextPassword);
            Role = UserRole.User;
        }

        internal User(Guid id, string username, string email, string passwordHash, UserRole role, DateTime createdAt)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = role;
            CreatedAt = createdAt;
        }

        [ExcludeFromCodeCoverage]
        private User()
        { }

        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public UserRole Role { get; private set; } = UserRole.User;
        public string Username { get; private set; } = string.Empty;

        public void ChangeEmail(string newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail))
                throw new ArgumentException("O e-mail não pode ser nulo ou vazio.", nameof(newEmail));

            var normalizedEmail = newEmail.Trim().ToLowerInvariant();

            try
            {
                var _ = new MailAddress(normalizedEmail);
            }
            catch (FormatException)
            {
                throw new ArgumentException("O formato do e-mail é inválido.", nameof(newEmail));
            }

            if (Email == normalizedEmail)
                return;

            Email = normalizedEmail;
        }

        public bool VerifyPassword(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
                return false;

            return BCrypt.Net.BCrypt.Verify(plainTextPassword, this.PasswordHash);
        }

        private void SetPassword(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
                throw new ArgumentException("Password cannot be empty.", nameof(plainTextPassword));

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
        }
    }
}