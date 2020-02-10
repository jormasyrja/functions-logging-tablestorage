# Logger provider for Azure Table Storage

This project contains simple implementations of `ILoggerProvider` and `ILogger` interfaces.
- `TableStorageLoggingProvider`
- `TableStorageLogger`

## Getting Started

Using dependency injection provided in `Microsoft.Azure.Functions.Extensions`, add the provider in your startup code:
```cs
public override void Configure(IFunctionsHostBuilder builder)
{
    builder.Services.AddSingleton<ILoggerProvider, TableStorageLoggerProvider>( _ => {
        var connectionString = <your Storage account connection string>
        var tableName = <name of table to insert log rows>
        
        var loggingConfiguration = new TableStorageLoggingConfiguration(connectionString, tablename);
        return new TableStorageLoggingProvider(loggingConfiguration);
    });
}
```

### Prerequisites
- Azure Functions v2/v3
- `Microsoft.Azure.Functions.Extensions` for dependency injection

## License

MIT
