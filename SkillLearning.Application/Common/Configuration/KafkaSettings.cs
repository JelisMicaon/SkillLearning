using System.Diagnostics.CodeAnalysis;

namespace SkillLearning.Application.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
    }
}