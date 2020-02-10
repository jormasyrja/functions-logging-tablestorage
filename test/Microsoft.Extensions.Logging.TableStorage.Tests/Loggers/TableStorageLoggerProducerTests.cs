using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.TableStorage.Entities;
using Microsoft.Extensions.Logging.TableStorage.Loggers;
using Xunit;

namespace Microsoft.Extensions.Logging.TableStorage.Tests.Loggers
{
    public class TableStorageLoggerProducerTests : IDisposable
    {
        private readonly BlockingCollection<LogTableEntity> _blockingCollection;

        public TableStorageLoggerProducerTests()
        {
            _blockingCollection = new BlockingCollection<LogTableEntity>();
        }

        [Theory]
        [InlineData(LogLevel.Information,"TestCategory", "Test information", null)]
        [InlineData(LogLevel.Critical, "TestCategory", "Test critical", null)]
        [InlineData(LogLevel.Warning, "TestCategory", "Test warning", null)]
        [InlineData(LogLevel.Trace, "TestCategory", "Test trace", null)]
        [InlineData(LogLevel.Debug, "SomeOtherCategory", "Testing exception", "Something went wrong")]
        public void Log_EntityPushedToQueue(LogLevel logLevel, string category, string message, string exceptionMessage)
        {
            var logger = new TableStorageLoggerProducer(category, Constants.DefaultPartitionKeyDateTimeFormat, _blockingCollection);

            var exception = exceptionMessage != null ? new Exception(exceptionMessage) : null;
            logger.Log(logLevel, exception, message);

            var logEvent = _blockingCollection.Take();
            Assert.Equal(logLevel.ToString(), logEvent.LogLevel);
            Assert.Equal(category, logEvent.CategoryName);
            Assert.Equal(message, logEvent.Message);
            Assert.Equal(exceptionMessage, logEvent.Exception);
        }

        public void Dispose()
        {
            _blockingCollection?.Dispose();
        }
    }
}
