using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data.Common;

namespace SkillLearning.Infrastructure.Persistence
{
    public class QueryPerformanceInterceptor(
        ILogger<QueryPerformanceInterceptor> logger,
        IHttpContextAccessor httpContextAccessor,
        ConcurrentDictionary<DbCommand, Subsegment> subsegments) : DbCommandInterceptor
    {
        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            EndSubsegment(command, eventData.Exception);
            base.CommandFailed(command, eventData);
        }

        public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            EndSubsegment(command, eventData.Exception);
            return base.CommandFailedAsync(command, eventData, cancellationToken);
        }

        public override int NonQueryExecuted(
                    DbCommand command, CommandExecutedEventData eventData, int result)
        {
            EndSubsegment(command);
            LogIfSlow(command, eventData.Duration);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override ValueTask<int> NonQueryExecutedAsync(
                    DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            EndSubsegment(command);
            LogIfSlow(command, eventData.Duration);
            return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            StartSubsegment(command);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
                    DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            StartSubsegment(command);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override DbDataReader ReaderExecuted(
                    DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            EndSubsegment(command);
            LogIfSlow(command, eventData.Duration);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(
                    DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            EndSubsegment(command);
            LogIfSlow(command, eventData.Duration);
            return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            StartSubsegment(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            StartSubsegment(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
        {
            EndSubsegment(command);
            LogIfSlow(command, eventData.Duration);
            return base.ScalarExecuted(command, eventData, result);
        }

        public override ValueTask<object?> ScalarExecutedAsync(
                    DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
        {
            EndSubsegment(command);
            LogIfSlow(command, eventData.Duration);
            return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            StartSubsegment(command);
            return base.ScalarExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            StartSubsegment(command);
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        private void EndSubsegment(DbCommand command, Exception? exception = null)
        {
            if (subsegments.TryRemove(command, out var sub))
            {
                if (exception != null)
                    sub.AddException(exception);
                AWSXRayRecorder.Instance.EndSubsegment();
            }
        }

        private void LogIfSlow(DbCommand command, TimeSpan duration)
        {
            if (duration.TotalMilliseconds > 100)
            {
                logger.LogWarning("Slow DB Command ({Duration}ms): {CommandText}",
                    duration.TotalMilliseconds, command.CommandText);
            }
        }

        private void StartSubsegment(DbCommand command)
        {
            if (!AWSXRayRecorder.Instance.TraceContext.IsEntityPresent())
                return;

            AWSXRayRecorder.Instance.BeginSubsegment("EFCore DbCommand");
            if (AWSXRayRecorder.Instance.GetEntity() is Subsegment sub)
            {
                var commandText = command.CommandText;
                if (commandText.Length > 2000)
                {
                    commandText = commandText[..2000] + " [TRUNCATED]";
                }

                sub.AddMetadata("db.statement", commandText);
                sub.AddMetadata("db.type", "postgres");

                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    var userId = httpContext.User?.FindFirst("sub")?.Value ?? "anonymous";
                    sub.AddMetadata("user.id", userId);
                }

                subsegments[command] = sub;
            }
        }
    }
}