using Confluent.Kafka;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.EventHandlersUseCase;
using SkillLearning.Domain.Events;

namespace SkillLearning.Workers.EmailSender.Services
{
    public class LoginEventConsumerHostedService(
        IKafkaConsumerService<Null, UserLoginEvent> kafkaConsumerService,
        LoginNotificationEventHandler loginNotificationEventHandler) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                await kafkaConsumerService.StartConsuming(
                    "userLoginEvent",
                    async (loginEvent) => await loginNotificationEventHandler.HandleLoginEventAsync(loginEvent),
                    cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            kafkaConsumerService.StopConsuming();
            return Task.CompletedTask;
        }
    }
}