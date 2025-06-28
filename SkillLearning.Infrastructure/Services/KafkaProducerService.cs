using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using System.Text.Json;

namespace SkillLearning.Infrastructure.Services
{
    public class KafkaProducerService : IEventPublisher
    {
        private readonly IProducer<Null, string> _producer;
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IOptions<KafkaSettings> kafkaSettings, ILogger<KafkaProducerService> logger)
        {
            _kafkaSettings = kafkaSettings.Value;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config)
                .SetErrorHandler((_, e) => _logger.LogError($"Kafka Producer Error: {e.Reason}"))
                .SetLogHandler((_, logMessage) => _logger.LogInformation($"Kafka Log: {logMessage.Message}"))
                .Build();
        }

        public async Task PublishAsync<TEvent>(TEvent @event, string? topic = null)
        {
            var eventTypeName = typeof(TEvent).Name;
            var defaultTopic = char.ToLowerInvariant(eventTypeName[0]) + eventTypeName.Substring(1);

            var targetTopic = topic ?? defaultTopic;

            var messageValue = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _logger.LogInformation($"Publishing event to Kafka topic '{targetTopic}': {messageValue}");

            try
            {
                var message = new Message<Null, string> { Value = messageValue };

                var deliveryResult = await _producer.ProduceAsync(targetTopic, message);

                _logger.LogInformation($"Event delivered to Kafka topic '{deliveryResult.Topic}', Partition: {deliveryResult.Partition.Value}, Offset: {deliveryResult.Offset.Value}");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError(e, $"Failed to deliver message to Kafka topic '{targetTopic}': {e.Error.Reason}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while publishing to Kafka topic '{targetTopic}'.");
                throw;
            }
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }
    }
}