using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;
using JS.Extensions.Logging.TableStorage.Entities;

namespace JS.Extensions.Logging.TableStorage.Loggers
{
    internal class TableStorageLoggerConsumer
    {
        private readonly BlockingCollection<LogTableEntity> _logEventQueue;
        private readonly TableClient _tableClient;
        private readonly TimeSpan _bufferTimeout;
        private readonly int _bufferSize;

        public TableStorageLoggerConsumer(BlockingCollection<LogTableEntity> logEventQueue, TableClient tableClient, int bufferTimeoutInSeconds, int bufferSize)
        {
            _logEventQueue = logEventQueue;
            _tableClient = tableClient;
            _bufferTimeout = TimeSpan.FromSeconds(bufferTimeoutInSeconds);
            _bufferSize = bufferSize;
        }

        public Task Start()
        {
            return Task.Factory.StartNew(async () =>
            {
                var initialized = false;
                // Now - reference = duration since last flush
                DateTime? referenceTimestamp = null;
                var bufferCount = 0;

                var takeTimeout = TimeSpan.FromMilliseconds(_bufferTimeout.TotalMilliseconds / 10);
                var entitiesToInsert = new List<LogTableEntity>();

                while (!_logEventQueue.IsCompleted)
                {
                    var eventTaken = false;
                    LogTableEntity logEvent = null;
                    try
                    {
                        eventTaken = _logEventQueue.TryTake(out logEvent, takeTimeout);
                    }
                    catch (ObjectDisposedException)
                    {
                        // can happen if the provider is disposed, no need to do anything since the system is probably going down anyway
                        break;
                    }

                    if (eventTaken)
                    {
                        referenceTimestamp = DateTime.UtcNow;
                        bufferCount++;

                        entitiesToInsert.Add(logEvent);
                    }

                    if (DateTime.UtcNow - referenceTimestamp < _bufferTimeout && bufferCount < _bufferSize)
                    {
                        continue;
                    }

                    if (!initialized)
                    {
                        await _tableClient.CreateIfNotExistsAsync();
                        initialized = true;
                    }

                    // All operations in a batch must have same partitionKey
                    var groupedByPartitionKey = entitiesToInsert
                        .GroupBy(entity => entity.PartitionKey);

                    foreach (var entities in groupedByPartitionKey)
                    {
                        var addEntitiesBatch = new List<TableTransactionAction>();
                        addEntitiesBatch.AddRange(entities.Select(entity => new TableTransactionAction(TableTransactionActionType.Add, entity)));

                        await _tableClient.SubmitTransactionAsync(addEntitiesBatch);
                    }
                    
                    referenceTimestamp = null;
                    bufferCount = 0;
                    entitiesToInsert.Clear();
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
