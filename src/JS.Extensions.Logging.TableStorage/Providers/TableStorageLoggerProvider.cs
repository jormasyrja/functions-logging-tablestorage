﻿using System;
using System.Collections.Concurrent;
using Azure.Data.Tables;
using JS.Extensions.Logging.TableStorage.Entities;
using JS.Extensions.Logging.TableStorage.Loggers;
using Microsoft.Extensions.Logging;

namespace JS.Extensions.Logging.TableStorage.Providers
{
    public class TableStorageLoggerProvider : ILoggerProvider
    {
        private readonly TableStorageLoggerConfiguration _configuration;

        private readonly ConcurrentDictionary<string, ILogger> _loggers;
        private readonly BlockingCollection<LogTableEntity> _logEventQueue;
        /// <summary>
        /// Provides <see cref="ILogger"/> instances that log to Azure Table Storage
        /// </summary>
        public TableStorageLoggerProvider(TableStorageLoggerConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _loggers = new ConcurrentDictionary<string, ILogger>();
            _logEventQueue = new BlockingCollection<LogTableEntity>(Constants.MaximumBufferSize);

            var tableClient = new TableClient(_configuration.ConnectionString, _configuration.TableName);
            var consumer = new TableStorageLoggerConsumer(_logEventQueue, tableClient, _configuration.LogEventBufferTimeoutInSeconds, _configuration.LogEventBufferSize);
            consumer.Start();
        }

        public void Dispose()
        {
            _logEventQueue.CompleteAdding();
            _logEventQueue.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new TableStorageLoggerProducer(name, _configuration.PartitionKeyDateTimeFormat, _logEventQueue));
        }
    }
}
