using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SkillLearning.Infrastructure.Persistence
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationWriteDbContext>
    {
        public ApplicationWriteDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "SkillLearning.Api"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationWriteDbContext>();
            var connectionString = config.GetConnectionString("Default");
            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationWriteDbContext(optionsBuilder.Options);
        }
    }
}