using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JS.Extensions.Logging.TableStorage.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace JS.Extensions.Logging.TableStorage.Loggers
{
    internal class TableStorageLoggerConsumer
    {
        private readonly BlockingCollection<LogTableEntity> _logEventQueue;
        private readonly CloudTable _cloudTable;
        private readonly TimeSpan _bufferTimeout;
        private readonly int _bufferSize;

        public TableStorageLoggerConsumer(BlockingCollection<LogTableEntity> logEventQueue, CloudTable cloudTable, int bufferTimeoutInSeconds, int bufferSize)
        {
            _logEventQueue = logEventQueue;
            _cloudTable = cloudTable;
            _bufferTimeout = TimeSpan.FromSeconds(bufferTimeoutInSeconds);
            _bufferSize = bufferSize;
        }

        public Task Start()
        {
            return Task.Run(async () =>
            {
                var initialized = false;
                // Now - reference = duration since last flush
                DateTime? referenceTimestamp = null;
                var bufferCount = 0;
                var operations = new List<TableOperation>();

                var takeTimeout = TimeSpan.FromMilliseconds(_bufferTimeout.TotalMilliseconds / 10);

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
                        operations.Add(TableOperation.Insert(logEvent));
                    }

                    if (DateTime.UtcNow - referenceTimestamp < _bufferTimeout && bufferCount < _bufferSize)
                    {
                        continue;
                    }

                    if (!initialized)
                    {
                        await _cloudTable.CreateIfNotExistsAsync();
                        initialized = true;
                    }

                    // All operations in a batch must have same partitionKey
                    var groupedByPartitionKey = operations
                        .GroupBy(operation => operation.Entity.PartitionKey);

                    foreach (var tableOperations in groupedByPartitionKey)
                    {
                        var batchOperation = new TableBatchOperation();
                        foreach (var operation in tableOperations)
                        {
                            batchOperation.Add(operation);
                        }
                        await _cloudTable.ExecuteBatchAsync(batchOperation);
                    }
                    
                    referenceTimestamp = null;
                    bufferCount = 0;
                    operations.Clear();
                }
            });
        }
    }
}
