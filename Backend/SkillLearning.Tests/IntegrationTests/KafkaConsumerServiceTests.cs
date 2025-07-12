using Confluent.Kafka;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Domain.Events;
using SkillLearning.Infrastructure.Services;
using Testcontainers.Kafka;

namespace SkillLearning.Tests.IntegrationTests
{
    public class KafkaConsumerServiceTests : IAsyncLifetime
    {
        private readonly KafkaContainer _kafkaContainer = new KafkaBuilder()
            .WithImage("confluentinc/cp-kafka:7.0.1")
            .Build();

        private KafkaConsumerService<Null, UserRegisteredEvent> _consumerService = null!;
        private KafkaProducerService _producerService = null!;

        public Task DisposeAsync()
        {
            _consumerService.Dispose();
            _producerService.Dispose();
            return _kafkaContainer.StopAsync();
        }

        public async Task InitializeAsync()
        {
            await _kafkaContainer.StartAsync();
            var bootstrapServers = _kafkaContainer.GetBootstrapAddress();

            var kafkaSettings = new KafkaSettings { BootstrapServers = bootstrapServers };
            var options = Options.Create(kafkaSettings);

            _consumerService = new KafkaConsumerService<Null, UserRegisteredEvent>(options);
            _producerService = new KafkaProducerService(options);
        }

        [Fact]
        public async Task StartConsuming_ShouldReceiveAndProcessMessage_AndThenStop()
        {
            // Arrange
            var topic = $"test-topic-{Guid.NewGuid()}";
            var originalEvent = new UserRegisteredEvent(Guid.NewGuid(), "KafkaUser", "kafka@test.com", DateTime.UtcNow);

            var tcs = new TaskCompletionSource<UserRegisteredEvent>();
            using var cts = new CancellationTokenSource();

            Task MessageHandler(UserRegisteredEvent receivedEvent)
            {
                tcs.SetResult(receivedEvent);
                cts.Cancel();
                return Task.CompletedTask;
            }

            await _producerService.PublishAsync(originalEvent, topic);

            // Act
            await _consumerService.StartConsuming(topic, MessageHandler, cts.Token);

            var receivedEvent = await tcs.Task;

            // Assert
            receivedEvent.Should().NotBeNull();
            receivedEvent.Should().BeEquivalentTo(originalEvent, options =>
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>()
            );
        }
    }
}