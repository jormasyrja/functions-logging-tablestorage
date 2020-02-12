using JS.Extensions.Logging.TableStorage.Providers;
using Xunit;

namespace JS.Extensions.Logging.TableStorage.Tests.Providers
{
    public class TableStorageLoggerProviderTests
    {
        private readonly TableStorageLoggerConfiguration _minimalConfiguration;

        public TableStorageLoggerProviderTests()
        {
            _minimalConfiguration = new TableStorageLoggerConfiguration("UseDevelopmentStorage=true", "table");
        }

        [Fact]
        public void CreateLogger_WithMinimalConfiguration_LoggerCreated()
        {
            var provider = new TableStorageLoggerProvider(_minimalConfiguration);
            var logger = provider.CreateLogger("category");

            Assert.NotNull(logger);
        }
    }
}
