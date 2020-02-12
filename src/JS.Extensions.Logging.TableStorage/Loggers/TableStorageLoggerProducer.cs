using System;
using System.Collections.Concurrent;
using JS.Extensions.Logging.TableStorage.Entities;
using Microsoft.Extensions.Logging;

namespace JS.Extensions.Logging.TableStorage.Loggers
{
    internal class TableStorageLoggerProducer : ILogger
    {
        private readonly string _partitionKeyFormat;
        private readonly BlockingCollection<LogTableEntity> _logEventQueue;
        private readonly string _categoryName;

        public TableStorageLoggerProducer(string categoryName, string partitionKeyFormat, BlockingCollection<LogTableEntity> logEventQueue)
        {
            _categoryName = categoryName;
            _partitionKeyFormat = partitionKeyFormat;
            _logEventQueue = logEventQueue;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var partitionKey = DateTime.UtcNow.ToString(_partitionKeyFormat);
            var logTableEntry = new LogTableEntity(partitionKey, Guid.NewGuid().ToString(), formatter.Invoke(state, exception), exception, _categoryName, logLevel.ToString());

            try
            {
                _logEventQueue.Add(logTableEntry);
            }
            catch (ObjectDisposedException)
            {
                // can happen if the provider is disposed, no need to do anything since the system is probably going down anyway
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
