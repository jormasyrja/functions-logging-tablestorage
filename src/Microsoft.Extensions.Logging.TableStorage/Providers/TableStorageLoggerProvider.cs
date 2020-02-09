using System;
using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging.TableStorage.Loggers;

namespace Microsoft.Extensions.Logging.TableStorage.Providers
{
    public class TableStorageLoggerProvider : ILoggerProvider
    {
        private readonly TableStorageLoggerConfiguration _configuration;
        private readonly CloudTable _loggingTableReference;
        private readonly ConcurrentDictionary<string, ILogger> _loggers;
        /// <summary>
        /// Provides <see cref="ILogger"/> instances that log to Azure Table Storage
        /// </summary>
        public TableStorageLoggerProvider(TableStorageLoggerConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggers = new ConcurrentDictionary<string, ILogger>();

            var cloudAccount = CloudStorageAccount.Parse(configuration.ConnectionString);
            var cloudTableClient = cloudAccount.CreateCloudTableClient();

            _loggingTableReference = cloudTableClient.GetTableReference(configuration.TableName);
            _loggingTableReference.CreateIfNotExists();
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new TableStorageLogger(name, _loggingTableReference, _configuration.RollOver));
        }
    }
}
