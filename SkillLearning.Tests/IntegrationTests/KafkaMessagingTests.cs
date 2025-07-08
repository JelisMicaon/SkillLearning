using Confluent.Kafka;
using Confluent.Kafka.Admin;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Domain.Events;
using SkillLearning.Infrastructure.Services;
using System.Text.Json;
using Testcontainers.Kafka;

namespace SkillLearning.Tests.IntegrationTests
{
    public class KafkaMessagingTests : IAsyncLifetime
    {
        private readonly KafkaContainer _kafkaContainer = new KafkaBuilder()
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(KafkaBuilder.KafkaPort))
            .Build();

        private KafkaProducerService _producer = null!;
        private string _bootstrapServers = null!;

        public async Task InitializeAsync()
        {
            await _kafkaContainer.StartAsync();
            _bootstrapServers = _kafkaContainer.GetBootstrapAddress();

            var kafkaSettings = new KafkaSettings { BootstrapServers = _bootstrapServers };
            var options = Options.Create(kafkaSettings);

            _producer = new KafkaProducerService(options);
        }

        public Task DisposeAsync()
        {
            _producer.Dispose();
            return _kafkaContainer.StopAsync();
        }

        [Fact]
        public async Task PublishAndConsume_Should_SuccessfullyTransferEvent()
        {
            // Arrange
            var topic = $"test-topic-{Guid.NewGuid()}";
            var originalEvent = new UserRegisteredEvent(Guid.NewGuid(), "KafkaUser", "kafka@test.com", DateTime.UtcNow);

            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _bootstrapServers }).Build())
            {
                await adminClient.CreateTopicsAsync([
                    new TopicSpecification { Name = topic, ReplicationFactor = 1, NumPartitions = 1 }
                ]);
            }

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = $"test-consumer-group-{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            consumer.Subscribe(topic);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            // Act
            await _producer.PublishAsync(originalEvent, topic);

            var consumeResult = consumer.Consume(cts.Token);
            var receivedEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(consumeResult.Message.Value, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Assert
            receivedEvent.Should().NotBeNull();
            receivedEvent.Should().BeEquivalentTo(originalEvent, options => options.Excluding(e => e.RegisteredAt));
            receivedEvent!.RegisteredAt.Should().BeCloseTo(originalEvent.RegisteredAt, TimeSpan.FromSeconds(1));
        }
    }
}