using System.Diagnostics.CodeAnalysis;

namespace SkillLearning.Application.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class RedisSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}