<h1 valign="center"><img src="./Assets/cosmosdb.svg"/>EF Core Cosmos DB Identity Provider</h1>

[![.NET 6 Build-Test](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/dotnet.yml) [![CodeQL](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/codeql-analysis.yml)
[![Unit Tests](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/unittests.yml/badge.svg)](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/unittests.yml)

This is a **Cosmos DB** implementation of an Identity provider for .NET 6 that uses the [EF Core Azure Cosmos DB Provider](https://docs.microsoft.com/en-us/ef/core/providers/cosmos/?tabs=dotnet-core-cli).

This project was forked from [Piero De Tomi](https://github.com/pierodetomi's) excellent project: [efcore-identity-cosmos](https://github.com/pierodetomi/efcore-identity-cosmos). If you are using .Net 5, it is highly recommended using that project instead of this one.

# Installation (NuGet)

To add this provider to your own Asp.Net 6 web project, add the following NuGet package:

```shell
PM> Install-Package AspNetCore.Identity.CosmosDb
```

# Integration Steps

## Before Starting

The following instructions show how to install the Cosmos DB identity provider. To continue please have the following ready:

- An Azure Cosmos DB account created - either the serverless or dedicated instance. You do not have to create a database yet.
- A SendGrid API Key if you are using the IEmailProvider used in these instructions

Note: This provider requires too many containers to use the free version of Cosmos DB.  The serverless instance is very economical
and is a good option to start with. See [documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/throughput-serverless) to help choose which is best for you.

## Application Configuration "Secrets"

Three secrets need to be created for this example:

- SendGridApiKey (The API key for your SendGrid account)
- CosmosIdentityDbName (The name of the database you want to use)
- ConnectionStrings:ApplicationDbContextConnection (The connection string for your Cosmos account)

And if you want the provider to automatically setup the database and required containers, use this setting:

- SetupCosmosDb

Here is an example of how to set the secrets in a `secrets.json` file that would be used with Visual Studio:

```json
{
  "SendGridApiKey": "YOUR SENDGRID API KEY",
  "SetupCosmosDb": "true", // Importat: Remove this variable after first run to improve startup performance.
  "CosmosIdentityDbName": "YourDabatabaseName",
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "THE CONNECTION STRING TO YOUR COSMOS ACCOUNT"
  }
}
```

## Modify Program.cs or Startup.cs File

After the "secrets" have been set, the next task is to modify your project's startup file.  For Asp.net
6 and higher that might be the `Project.cs` file. For other projects it might be your `Startup.cs.`


```csharp
public class MyDbContext : CosmosIdentityDbContext<IdentityUser>
{
  public MyDbContext(DbContextOptions dbContextOptions, IOptions<OperationalStoreOptions> options)
    : base(dbContextOptions, options) { }
}
```

Later in your development you'll likely add some entities to your application: you'll update the DbContext class adding the `DbSet<T>` properties and overriding the `OnModelCreating()` method for entity mappings:


```csharp
public class MyDbContext : CosmosIdentityDbContext<IdentityUser>
{
  public DbSet<SampleEntity> SampleEntities { get; set; }

  public DbSet<OtherSampleEntity> OtherSampleEntities { get; set; }

  public MyDbContext(DbContextOptions dbContextOptions, IOptions<OperationalStoreOptions> options)
    : base(dbContextOptions, options) { }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    // DO NOT REMOVE THIS LINE. If you do, your context won't work as expected.
    base.OnModelCreating(builder);
    
    // TODO: Add your own fluent mappings
  }
}
```

As specified in the above code snippet, when overriding the `OnModelCreating()` method it is **crucial** to not remove the `base.OnModelCreating(builder)` call: if you do so, the identity configuration mappings won't be applied and the application won't work properly.

## Configurations in Startup.cs File

## Remove the Default Identity Provider

Remove the line where the default/current identity provider is added/configured.

If you just created a new project, this line should be something like:

```csharp
services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
  .AddEntityFrameworkStores<ApplicationDbContext>();
```

## Remove Default DbContext Configuration

Remove the line where the SQL DbContext is configured.

It should be something like:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
  options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
```

## Add Cosmos DB Identity Provider

Now add the Cosmos DB provider:

```csharp
services.AddCosmosIdentity<MyDbContext, IdentityUser, IdentityRole>(
  // Auth provider standard configuration (e.g.: account confirmation, password requirements, etc.)
  options => ...,
  options => options.UseCosmos(
      "your_cosmos_db_URL",
      "your_cosmos_db_key",
      databaseName: "your_db"
  ),

  // If true, AddDefaultTokenProviders() method will be called on the IdentityBuilder instance
  addDefaultTokenProviders: false   
);
```

# This Provider & Identity UI

This provider is also **compatible** with Identity UI.

You can either use the default Identity UI (e.g.: in `Startup.cs` there's a call to `AddDefaultUI()` method) or use the [scaffolded](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-5.0&tabs=visual-studio) version of Identity UI. Both scenarios should work out of the box without having to do anything else.

Finally you can also use your own implementation of Identity UI, as long as you use the Identity services (e.g.: `UserManager` and `SignInManager`).

# Available Services

This library registers in the service collection a basic Cosmos DB repository implementation, that you can resolve in your constructors requiring the `IRepository` interface.

An example:

```csharp
public class MyClass {
  private readonly IRepository _repo;

  public MyClass(IRepository repo) {
    _repo = repo;
  }

  // ... Use the _repo instance methods to query the database
}
```

## Available IRepository methods

Just for your information, here is a summary of the available methods in the IRepository interface:

- `Table<TEntity>()`
- `GetById<TEntity>(string id)`
- `TryFindOne<TEntity>(Expression<Func<TEntity, bool>> predicate)`
- `Find<TEntity>(Expression<Func<TEntity, bool>> predicate)`
- `Add<TEntity>(TEntity entity)`
- `Update<TEntity>(TEntity entity)`
- `DeleteById<TEntity>(string id)`
- `Delete<TEntity>(TEntity entity)`
- `Delete<TEntity>(Expression<Func<TEntity, bool>> predicate)`
- `SaveChangesAsync()`

# Changelog

## v1.0.6

- Introduced support for `IUserLoginStore<TUser>` in User Store

## v1.0.5

- Introduced support for `IUserPhoneNumberStore<TUser>` in User Store

## v1.0.4

- Introduced support for `IUserEmailStore<TUser>` in User Store

## v2.0.0-alpha

- Forked from source repository [pierodetomi/efcore-identity-cosmos](https://github.com/pierodetomi/efcore-identity-cosmos).
- Refactored for .Net 6 LTS.
- Added `UserStore`, `RoleStore`, `UserManager` and `RoleManager` unit tests.
- Namespace changed to one more generic: `AspNetCore.Identity.CosmosDb`
- Implemented `IUserLockoutStore` interface for `UserStore`

## v2.0.1.0

- Added example web project
