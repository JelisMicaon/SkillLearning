using Confluent.Kafka;
using Microsoft.Extensions.Options;
using SkillLearning.Application.Common.Configuration;
using SkillLearning.Application.Common.Interfaces;
using System.Text.Json;

namespace SkillLearning.Infrastructure.Services
{
    public class KafkaConsumerService<TKey, TValue> : IKafkaConsumerService<TKey, TValue>
    {
        private readonly IConsumer<TKey, string> _consumer;
        private CancellationTokenSource? _consumeCancellationTokenSource;
        private bool _disposed;

        public KafkaConsumerService(IOptions<KafkaSettings> kafkaSettings)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers,
                GroupId = $"skilllearning-email-sender-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                SecurityProtocol = SecurityProtocol.Plaintext
            };

            _consumer = new ConsumerBuilder<TKey, string>(config)
                .SetKeyDeserializer(KafkaConsumerService<TKey, TValue>.CreateKeyDeserializer())
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();
        }

        private static IDeserializer<TKey> CreateKeyDeserializer()
        {
            if (typeof(TKey) == typeof(Null))
            {
                return (IDeserializer<TKey>)Deserializers.Null;
            }
            else if (typeof(TKey) == typeof(string))
            {
                return (IDeserializer<TKey>)Deserializers.Utf8;
            }
            throw new NotSupportedException($"Key deserializer for type {typeof(TKey).Name} is not supported.");
        }

        public async Task StartConsuming(string topic, Func<TValue, Task> handler, CancellationToken cancellationToken)
        {
            if (_consumeCancellationTokenSource != null && !_consumeCancellationTokenSource.IsCancellationRequested)
                return;

            _consumeCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _consumer.Subscribe(topic);

            try
            {
                while (!_consumeCancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

                        if (consumeResult == null || consumeResult.IsPartitionEOF)
                            continue;

                        var deserializedValue = JsonSerializer.Deserialize<TValue>(consumeResult.Message.Value, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                        if (deserializedValue is not null)
                            await handler(deserializedValue);

                        _consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        await Task.Delay(5000, _consumeCancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("❌ Erro inesperado no consumo:");
                        Console.WriteLine(ex);
                        await Task.Delay(1000);
                    }
                }
            }
            finally
            {
                _consumer.Unsubscribe();
            }
        }

        public void StopConsuming()
        {
            _consumeCancellationTokenSource?.Cancel();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _consumer.Close();
                    _consumer.Dispose();
                    _consumeCancellationTokenSource?.Dispose();
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