using Microsoft.EntityFrameworkCore;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Domain.Entities;
using SkillLearning.Infrastructure.Persistence.Configurations;

namespace SkillLearning.Infrastructure.Persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork, IReadDbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        }
    }
}