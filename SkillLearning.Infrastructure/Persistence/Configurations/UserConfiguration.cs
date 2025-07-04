using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillLearning.Domain.Entities;
using SkillLearning.Domain.Enums;

namespace SkillLearning.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasConversion<string>();

            var adminUser = new User(
                id: Guid.Parse("c9d784a1-0b2a-4a2b-8a8f-2b0e7a1d4c3f"),
                username: "admin",
                email: "admin@skilllearning.com",
                passwordHash: "$2a$11$Wqtp1Vw7lRbF2r0wnp7B6OYIoH4LSN5/xWy/7E.MMSYgaKlFyT4tS", // admin123
                role: UserRole.Admin,
                createdAt: new DateTime(2023, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            );

            entity.HasData(adminUser);
        }
    }
}