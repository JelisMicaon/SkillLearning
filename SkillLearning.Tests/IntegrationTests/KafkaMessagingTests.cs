using Confluent.Kafka;
using FluentAssertions;
using SkillLearning.Domain.Events;
using System.Text.Json;
using Testcontainers.Kafka;

namespace SkillLearning.Tests.IntegrationTests
{
    public class KafkaMessagingTests : IAsyncLifetime
    {
        private readonly KafkaContainer _kafkaContainer = new KafkaBuilder()
            .WithImage("confluentinc/cp-kafka:7.0.1")
            .Build();

        private string _bootstrapServers = null!;
        private IProducer<Null, string> _producer = null!;

        public Task DisposeAsync()
        {
            _producer.Dispose();
            return _kafkaContainer.StopAsync();
        }

        public async Task InitializeAsync()
        {
            await _kafkaContainer.StartAsync();
            _bootstrapServers = _kafkaContainer.GetBootstrapAddress();

            var producerConfig = new ProducerConfig { BootstrapServers = _bootstrapServers };
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        [Fact]
        public async Task ProduceAndConsume_ShouldTransferMessageSuccessfully()
        {
            // Arrange
            var topic = $"test-topic-{Guid.NewGuid()}";
            var originalEvent = new UserRegisteredEvent(Guid.NewGuid(), "KafkaUser", "kafka@test.com", DateTime.UtcNow);
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var messageValue = JsonSerializer.Serialize(originalEvent, jsonOptions);

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = $"test-consumer-group-{Guid.NewGuid()}",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                SessionTimeoutMs = 10000
            };

            using var consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build();
            consumer.Subscribe(topic);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            // Act
            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = messageValue }, cts.Token);
            var consumeResult = consumer.Consume(cts.Token);

            // Assert
            consumeResult.Should().NotBeNull();
            var receivedEvent = JsonSerializer.Deserialize<UserRegisteredEvent>(consumeResult.Message.Value, jsonOptions);

            receivedEvent.Should().NotBeNull();
            receivedEvent.Should().BeEquivalentTo(originalEvent, options =>
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1))).WhenTypeIs<DateTime>()
            );
        }
    }
}