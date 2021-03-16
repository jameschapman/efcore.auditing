# Entity Framework - Auditing

Simple library to create an audit of changes made via Entity Framework Core. Unfortunately other libraries didn't do what I needed so I wrote this this :(

## Installation

```
nuget install EfCore.Audit
```

## Usage

1. Add Auditing table to your DB Context

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	base.OnModelCreating(modelBuilder);
	modelBuilder.EnableAuditing();
}
```

2. Register Auditing in your service provider

```csharp
ServiceProvider = new ServiceCollection()
	.RegisterEntityFrameworkAuditing(ServiceLifetime.Singleton, options => {
		options.CurrentDateTime = () => CurrentDateTime;
		options.TransactionId = () => // Maybe the ASP request id goes here?
	})
   	.BuildServiceProvider();
```

You can optionally override the default `CurrentDateTime` and `TransactionId` options.

If you do not set the CurrentDateTime value it will default to `DateTime.UtcNow.`. `TransactionId` will default to a random GUID unless overridden. Each SaveChangesAsyncWithHistory call
will result in a new TransactionId being generated.

3. Save changes

```csharp
await _context.SaveChangesAsyncWithHistory("Test User");
```

```csharp
await _context.SaveChangesAsyncWithHistory("Test User", (context, items) =>
	{
		// do something here
	});
```

You can save Db Context changes via the `SaveChangesAsyncWithHistory` extension method. This method requires you to pass the name of the user you wish to associate to the change.
Optionally, you can also defined a callback action which accepts the current db context alongside a collection of changes, this allows for additional projections of data to be
created if required.

## Viewing Data

Audit data will be saved to the same persistance store as your DB context uses. It will create a table called 'Audit' that will contain the following information:

| Field         | Type     | Description                                                                                                                        |
| ------------- | -------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Id            | Guid     | Primary Key                                                                                                                        |
| RowId         | Json     | Contains Json representing the primary key of the changed record.                                                                  |
| TableName     | String   | The name of the table which the affected record resides                                                                            |
| Data          | Json     | Json detailing the information that has changed. This will in the format `{ FieldName: { OldValue: 'Value', NewValue: 'Value' } }` |
| Action        | String   | The action performed on the record, this will be one of Added, Updated, Deleted                                                    |
| CreateDate    | DateTime | DateTime stamp of when the audit record was created. Note, this can optionally be overridden in the AuditOptions configuration.    |
| CreatedBy     | String   | Who performed the change resulting in the audit being written                                                                      |
| TransactionId | String   | Unique identifier used to group audit entries spread across tables                                                                 |

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.