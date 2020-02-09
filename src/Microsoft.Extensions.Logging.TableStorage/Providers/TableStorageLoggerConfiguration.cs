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
        /// Defines when Table Storage rows' partition keys roll over, based on UTC timestamp at the time of logging.
        /// Defaults to <see cref="PartitionKeyRollOver.Day"/>
        /// </summary>
        public PartitionKeyRollOver RollOver { get; set; }

        /// <summary>
        /// Configuration object for <see cref="TableStorageLoggerProvider"/>.
        /// </summary>
        /// <param name="connectionString">Storage account connection string</param>
        /// <param name="tableName">Table name for inserting log rows</param>
        /// <param name="partitionKeyRollOver">Rollover type for partitionKey, default is <see cref="PartitionKeyRollOver.Day"/>, which means that log rows are separated into one partition per day</param>
        public TableStorageLoggerConfiguration(string connectionString, string tableName, PartitionKeyRollOver partitionKeyRollOver = PartitionKeyRollOver.Day)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));

            RollOver = partitionKeyRollOver;
        }
    }

    /// <summary>
    /// Defines when Table Storage rows' partition keys roll over
    /// </summary>
    public enum PartitionKeyRollOver
    {
        /// <summary>
        /// yyyyMMdd, e.g. 20201231 (December 12th 2020)
        /// </summary>
        Day,
        /// <summary>
        /// yyyyMMdd-HH, e.g. 20201231-23 (December 12th 2020, 23:00)
        /// </summary>
        Hour
    }
}
