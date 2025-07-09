using Microsoft.Extensions.Logging;

namespace SkillLearning.Tests.TestHelpers
{
    public class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel LogLevel, string Message, Exception? Exception)> Logs { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            Logs.Add((logLevel, message, exception));
        }

        private class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            { }
        }
    }
}