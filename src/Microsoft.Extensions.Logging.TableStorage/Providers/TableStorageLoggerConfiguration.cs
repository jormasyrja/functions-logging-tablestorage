using System;

namespace Microsoft.Extensions.Logging.TableStorage.Providers
{
    public class TableStorageLoggerConfiguration
    {
        /// <summary>
        /// Storage Account connection string
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Table Storage table name used for logging. Will be created if it doesn't exist.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// <see cref="DateTime"/> format to use as the partition key for inserted log rows. Default is <see cref="Constants.DefaultPartitionKeyDateTimeFormat"/>
        /// </summary>
        public string PartitionKeyDateTimeFormat { get; }

        /// <summary>
        /// How long should the log buffer wait until flush (write to Table Storage), if buffer is not full
        /// </summary>
        public int LogEventBufferTimeoutInSeconds { get; }
        /// <summary>
        /// Number of events to store in buffer before force flushing
        /// </summary>
        public int LogEventBufferSize { get; }

        /// <summary>
        /// Configuration object for <see cref="TableStorageLoggerProvider"/>.
        /// </summary>
        /// <param name="connectionString">Storage account connection string</param>
        /// <param name="tableName">Table name for inserting log rows</param>
        /// <param name="partitionKeyDateTimeFormat"><see cref="DateTime"/> format to use as the partition key for inserted log rows. Default is <see cref="Constants.DefaultPartitionKeyDateTimeFormat"/></param>
        /// <param name="logEventBufferTimeoutInSeconds">How long should the log buffer wait until flush (write to Table Storage), if buffer is not full</param>
        /// <param name="logEventBufferSize">Number of events to store in buffer before force flushing</param>
        public TableStorageLoggerConfiguration(
            string connectionString,
            string tableName,
            string partitionKeyDateTimeFormat = Constants.DefaultPartitionKeyDateTimeFormat,
            int logEventBufferTimeoutInSeconds = Constants.DefaultBufferTimeoutInSeconds,
            int logEventBufferSize = Constants.DefaultBufferSize)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            PartitionKeyDateTimeFormat = partitionKeyDateTimeFormat ?? throw new ArgumentNullException(nameof(partitionKeyDateTimeFormat));

            if (logEventBufferTimeoutInSeconds < 1)
            {
                throw new ArgumentException($"{nameof(logEventBufferTimeoutInSeconds)} must be greater than zero");
            }
            LogEventBufferTimeoutInSeconds = logEventBufferTimeoutInSeconds;

            if (logEventBufferSize < 1 || logEventBufferSize > Constants.MaximumBufferSize)
            {
                throw new ArgumentException($"{nameof(logEventBufferSize)} must be greater than zero and less than {Constants.MaximumBufferSize}");
            }
            LogEventBufferSize = logEventBufferSize;
        }
    }
}
