using Confluent.Kafka;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using System.Text.Json;

namespace SkillLearning.Infrastructure.Services
{
    public class KafkaProducerService : IEventPublisher, IDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private bool _disposed;

        public KafkaProducerService(IOptions<KafkaSettings> kafkaSettings)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
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

            var message = new Message<Null, string> { Value = messageValue };

            await _producer.ProduceAsync(targetTopic, message);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _producer.Flush(TimeSpan.FromSeconds(10));
                    _producer.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}