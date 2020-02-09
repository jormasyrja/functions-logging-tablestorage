using System;
using System.Globalization;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging.TableStorage.Entities;
using Microsoft.Extensions.Logging.TableStorage.Providers;

namespace Microsoft.Extensions.Logging.TableStorage.Loggers
{
    internal class TableStorageLogger : ILogger
    {
        private readonly PartitionKeyRollOver _rollOver;
        private readonly CloudTable _cloudTable;
        private readonly string _categoryName;

        public TableStorageLogger(string categoryName, CloudTable cloudTable, PartitionKeyRollOver rollOver)
        {
            _categoryName = categoryName;
            _cloudTable = cloudTable;
            _rollOver = rollOver;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var id = eventId.Id != default
                ? eventId.Id.ToString(CultureInfo.InvariantCulture)
                : Guid.NewGuid().ToString();

            var partitionKey = CreatePartitionKeyString(DateTime.UtcNow);
            var logTableEntry = new LogTableEntity(partitionKey, id, eventId.Name, formatter.Invoke(state, exception), _categoryName, logLevel.ToString());

            var tableOperation = TableOperation.Insert(logTableEntry);
            _cloudTable.Execute(tableOperation);
        }

        private string CreatePartitionKeyString(DateTime timestamp)
        {
            const string baseFormat = "yyyyMMdd";
            var format = _rollOver switch
            {
                PartitionKeyRollOver.Day => baseFormat,
                PartitionKeyRollOver.Hour => $"{baseFormat}-HH",
                _ => baseFormat
            };

            return timestamp.ToString(format);
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
