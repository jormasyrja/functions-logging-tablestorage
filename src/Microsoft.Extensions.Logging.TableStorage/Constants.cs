namespace Microsoft.Extensions.Logging.TableStorage
{
    internal static class Constants
    {
        internal const string DefaultPartitionKeyDateTimeFormat = "yyyyMMdd";
        internal const int DefaultBufferSize = 10;
        internal const int MaximumBufferSize = 1000;
        internal const int DefaultBufferTimeoutInSeconds = 10;
    }
}
