using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging.TableStorage.Entities;
using Microsoft.Extensions.Logging.TableStorage.Loggers;
using NSubstitute;
using Xunit;

namespace Microsoft.Extensions.Logging.TableStorage.Tests.Loggers
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
            var consumer = new TableStorageLoggerConsumer(_blockingCollection, _cloudTableMock, int.MaxValue, bufferSize);
            for (int i = 0; i < bufferSize; i++)
            {
                _blockingCollection.Add(new LogTableEntity("", "", "", null, "", ""));
            }
            _blockingCollection.CompleteAdding();

            await Task.WhenAny(consumer.Start(), Task.Delay(5000));
            await _cloudTableMock.ReceivedWithAnyArgs().ExecuteBatchAsync(default);
        }

        public void Dispose()
        {
            _blockingCollection?.Dispose();
        }
    }
}
