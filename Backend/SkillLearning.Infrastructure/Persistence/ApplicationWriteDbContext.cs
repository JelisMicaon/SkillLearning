using Microsoft.EntityFrameworkCore;
using SkillLearning.Application.Common.Interfaces;

namespace SkillLearning.Infrastructure.Persistence
{
    public class ApplicationWriteDbContext(DbContextOptions<ApplicationWriteDbContext> options) : DbContextBase(options), IUnitOfWork
    {
    }
}