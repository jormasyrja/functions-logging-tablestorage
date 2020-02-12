# Logger provider for Azure Table Storage

This project contains simple implementations of `ILoggerProvider` and `ILogger` interfaces.
- `TableStorageLoggerProvider`
- `TableStorageLogger`

## Getting Started

Using dependency injection provided in `Microsoft.Azure.Functions.Extensions`, add the provider in your startup code:
```cs
public override void Configure(IFunctionsHostBuilder builder)
{
    builder.Services.AddSingleton<ILoggerProvider, TableStorageLoggerProvider>(_ =>
	{
		var connectionString = <Storage account connection string>;
		var tableName = <logging table name>;

		var loggingConfiguration = new TableStorageLoggerConfiguration(connectionString, tableName);
		return new TableStorageLoggerProvider(loggingConfiguration);
	});
}
```

## Log levels
Set your desired logging levels in host.json : logging.

### Prerequisites
-  Azure Functions v3
- .NET Core 3.1
- `Microsoft.Azure.Functions.Extensions` for dependency injection

## License

MIT
