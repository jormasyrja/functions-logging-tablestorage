using System;
using Microsoft.Azure.Cosmos.Table;

namespace JS.Extensions.Logging.TableStorage.Entities
{
    internal class LogTableEntity : TableEntity
    {
        public string Message { get; set; }
        public string Exception { get; set; }
        public string StackTrace { get; set; }
        public string CategoryName { get; set; }
        public string LogLevel { get; set; }

        public LogTableEntity(string partitionKey, string rowKey, string message, Exception exception, string categoryName, string logLevel) : base(partitionKey, rowKey)
        {
            Message = message;
            CategoryName = categoryName;
            LogLevel = logLevel;

            Exception = exception?.Message;
            StackTrace = exception?.StackTrace;
        }
    }
}
