using Confluent.Kafka;
using SkillLearning.Application.Common.Interfaces;
using SkillLearning.Application.Features.Auth.EventHandlers;
using SkillLearning.Domain.Events;

namespace SkillLearning.Workers.EmailSender.Services
{
    public class LoginEventConsumerHostedService : IHostedService
    {
        private readonly IKafkaConsumerService<Null, UserLoginEvent> _kafkaConsumerService;
        private readonly LoginNotificationEventHandler _loginNotificationEventHandler;

        public LoginEventConsumerHostedService(
            IKafkaConsumerService<Null, UserLoginEvent> kafkaConsumerService,
            LoginNotificationEventHandler loginNotificationEventHandler)
        {
            _kafkaConsumerService = kafkaConsumerService;
            _loginNotificationEventHandler = loginNotificationEventHandler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                await _kafkaConsumerService.StartConsuming(
                    "userLoginEvent",
                    async (loginEvent) => await _loginNotificationEventHandler.HandleLoginEventAsync(loginEvent),
                    cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _kafkaConsumerService.StopConsuming();
            return Task.CompletedTask;
        }
    }
}