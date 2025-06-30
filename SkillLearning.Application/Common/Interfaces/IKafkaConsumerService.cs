namespace SkillLearning.Application.Common.Interfaces
{
    public interface IKafkaConsumerService<TKey, TValue> : IDisposable
    {
        Task StartConsuming(string topic, Func<TValue, Task> handler, CancellationToken cancellationToken);

        void StopConsuming();
    }
}