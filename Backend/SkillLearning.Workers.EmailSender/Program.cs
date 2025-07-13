using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.EventHandlersUseCase;
using SkillLearning.Infrastructure.Configuration;
using SkillLearning.Infrastructure.Services;
using SkillLearning.Workers.EmailSender.Services;

namespace SkillLearning.Workers.EmailSender
{
    public static class Program
    {
        public static async Task Main(string[] args) =>
            await CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment.EnvironmentName;

                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));
                    services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

                    services.AddSingleton(typeof(IKafkaConsumerService<,>), typeof(KafkaConsumerService<,>));
                    services.AddTransient<IEmailSender, Infrastructure.Services.EmailSender>();
                    services.AddTransient<LoginNotificationEventHandler>();
                    services.AddHostedService<LoginEventConsumerHostedService>();
                });
    }
}