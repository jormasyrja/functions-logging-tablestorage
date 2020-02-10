using System;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos.Table;

namespace Microsoft.Extensions.Logging.TableStorage.Entities
{
    internal class LogTableEntity : TableEntity
    {
        public string Message { get; }
        public string Exception { get; }
        public string StackTrace { get; }
        public string CategoryName { get; }
        public string LogLevel { get; }

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
