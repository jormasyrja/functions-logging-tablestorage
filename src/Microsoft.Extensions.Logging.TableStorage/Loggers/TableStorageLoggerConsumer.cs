using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging.TableStorage.Entities;

namespace Microsoft.Extensions.Logging.TableStorage.Loggers
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
                var referenceTimestamp = DateTime.UtcNow;
                var bufferCount = 0;
                var batchOperation = new TableBatchOperation();

                while (!_logEventQueue.IsCompleted)
                {
                    var eventTaken = false;
                    LogTableEntity logEvent = null;
                    try
                    {
                        eventTaken = _logEventQueue.TryTake(out logEvent, TimeSpan.FromSeconds(1));
                    }
                    catch (ObjectDisposedException)
                    {
                        // can happen if the provider is disposed, no need to do anything since the system is probably going down anyway
                        break;
                    }

                    if (eventTaken)
                    {
                        bufferCount++;
                        batchOperation.Add(TableOperation.Insert(logEvent));
                    }

                    if (DateTime.UtcNow - referenceTimestamp >= _bufferTimeout || bufferCount >= _bufferSize)
                    {
                        if (bufferCount > 0)
                        {
                            if (!initialized)
                            {
                                await _cloudTable.CreateIfNotExistsAsync();
                                initialized = true;
                            }
                            await _cloudTable.ExecuteBatchAsync(batchOperation);
                            batchOperation = new TableBatchOperation();
                        }
                        referenceTimestamp = DateTime.UtcNow;
                        bufferCount = 0;
                    }
                }
            });
        }
    }
}
