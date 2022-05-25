using System;
using Azure;
using Azure.Data.Tables;

namespace JS.Extensions.Logging.TableStorage.Entities
{
    internal class LogTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public string StackTrace { get; set; }
        public string CategoryName { get; set; }
        public string LogLevel { get; set; }

        public LogTableEntity(string partitionKey, string rowKey, string message, Exception exception, string categoryName, string logLevel)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Message = message;
            CategoryName = categoryName;
            LogLevel = logLevel;

            Exception = exception?.Message;
            StackTrace = exception?.ToString();
        }
    }
}
