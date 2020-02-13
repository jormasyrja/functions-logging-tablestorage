using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using JS.Extensions.Logging.TableStorage.Entities;
using JS.Extensions.Logging.TableStorage.Loggers;
using Microsoft.Azure.Cosmos.Table;
using NSubstitute;
using Xunit;

namespace JS.Extensions.Logging.TableStorage.Tests.Loggers
{
    public class TableStorageLoggerConsumerTests : IDisposable
    {
        private readonly BlockingCollection<LogTableEntity> _blockingCollection;
        private readonly CloudTable _cloudTableMock;

        public TableStorageLoggerConsumerTests()
        {
            _blockingCollection = new BlockingCollection<LogTableEntity>();
            _cloudTableMock = Substitute.For<CloudTable>(new Uri("https://www.test.com"), null);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(Constants.MaximumBufferSize)]
        public async Task Consume_BufferFull_FlushedToTableStorage(int bufferSize)
        {
            var consumer = new TableStorageLoggerConsumer(_blockingCollection, _cloudTableMock, 10000, bufferSize);
            var consumerTask = consumer.Start();
            for (int i = 0; i < bufferSize; i++)
            {
                _blockingCollection.Add(new LogTableEntity("", "", "", null, "", ""));
            }
            _blockingCollection.CompleteAdding();

            await Task.WhenAny(consumerTask, Task.Delay(5000));
            await _cloudTableMock.ReceivedWithAnyArgs().ExecuteBatchAsync(default);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Consume_MultiplePartitionKeys_OneCallForEachUniquePartitionKey(int items)
        {
            var consumer = new TableStorageLoggerConsumer(_blockingCollection, _cloudTableMock, 10000, items);
            var consumerTask = consumer.Start();
            for (int i = 0; i < items; i++)
            {
                _blockingCollection.Add(new LogTableEntity(Guid.NewGuid().ToString(), "", "", null, "", ""));
            }
            _blockingCollection.CompleteAdding();

            await Task.WhenAny(consumerTask, Task.Delay(5000));
            await _cloudTableMock.ReceivedWithAnyArgs(items).ExecuteBatchAsync(default);
        }

        public void Dispose()
        {
            _blockingCollection?.Dispose();
        }
    }
}
