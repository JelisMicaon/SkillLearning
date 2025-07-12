using Microsoft.EntityFrameworkCore;
using SkillLearning.Application.Common.Interfaces;

namespace SkillLearning.Infrastructure.Persistence
{
    public class ApplicationReadDbContext(DbContextOptions<ApplicationReadDbContext> options) : DbContextBase(options), IReadDbContext
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Este DbContext é somente para leitura e não pode salvar alterações.");
        }
    }
}