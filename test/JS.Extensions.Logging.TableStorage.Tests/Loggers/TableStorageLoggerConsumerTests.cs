using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Azure.Data.Tables;
using JS.Extensions.Logging.TableStorage.Entities;
using JS.Extensions.Logging.TableStorage.Loggers;
using NSubstitute;
using Xunit;

namespace JS.Extensions.Logging.TableStorage.Tests.Loggers
{
    public class TableStorageLoggerConsumerTests : IDisposable
    {
        private readonly BlockingCollection<LogTableEntity> _blockingCollection;
        private readonly TableClient _tableClientMock;

        public TableStorageLoggerConsumerTests()
        {
            _blockingCollection = new BlockingCollection<LogTableEntity>();
            _tableClientMock = Substitute.For<TableClient>("UseDevelopmentStorage=true", "Test");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(Constants.MaximumBufferSize)]
        public async Task Consume_BufferFull_FlushedToTableStorage(int bufferSize)
        {
            var consumer = new TableStorageLoggerConsumer(_blockingCollection, _tableClientMock, 10000, bufferSize);
            var consumerTask = consumer.Start();
            for (int i = 0; i < bufferSize; i++)
            {
                _blockingCollection.Add(new LogTableEntity("", "", "", null, "", ""));
            }
            _blockingCollection.CompleteAdding();

            await Task.WhenAny(consumerTask, Task.Delay(5000));
            await _tableClientMock.ReceivedWithAnyArgs().SubmitTransactionAsync(default);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Consume_MultiplePartitionKeys_OneCallForEachUniquePartitionKey(int items)
        {
            var consumer = new TableStorageLoggerConsumer(_blockingCollection, _tableClientMock, 10000, items);
            var consumerTask = consumer.Start();
            for (int i = 0; i < items; i++)
            {
                _blockingCollection.Add(new LogTableEntity(Guid.NewGuid().ToString(), "", "", null, "", ""));
            }
            _blockingCollection.CompleteAdding();

            await Task.WhenAny(consumerTask, Task.Delay(5000));
            await _tableClientMock.ReceivedWithAnyArgs(items).SubmitTransactionAsync(default);
        }

        public void Dispose()
        {
            _blockingCollection?.Dispose();
        }
    }
}
