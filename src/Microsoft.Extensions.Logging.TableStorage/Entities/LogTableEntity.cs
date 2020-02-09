using Microsoft.Azure.Cosmos.Table;

namespace Microsoft.Extensions.Logging.TableStorage.Entities
{
    internal class LogTableEntity : TableEntity
    {
        public string LogEventName { get; set; }
        public string Message { get; set; }
        public string CategoryName { get; set; }
        public string LogLevel { get; set; }

        public LogTableEntity(string partitionKey, string id, string logEventName, string message, string categoryName, string logLevel) : base(partitionKey, id)
        {
            LogEventName = logEventName;
            Message = message;
            CategoryName = categoryName;
            LogLevel = logLevel;
        }
    }
}
