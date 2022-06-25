![Logo](https://raw.githubusercontent.com/pierodetomi/efcore-identity-cosmos/main/PieroDeTomi.EntityFrameworkCore.Identity.Cosmos/_res/icons/nuget-icon.png)

# EF Core Cosmos DB Identity Provider
This is a **Cosmos DB** implementation of an Identity provider for .NET Core 5, using the official [EF Core Azure Cosmos DB Provider](https://docs.microsoft.com/en-us/ef/core/providers/cosmos/?tabs=dotnet-core-cli).

You can use this package to easily bootstrap an ASP.NET **Identity Server** backed by a CosmosDb database and EF Core, in place of SQL Server.

# Installation (NuGet)

```shell
PM> Install-Package PieroDeTomi.EntityFrameworkCore.Identity.Cosmos
```

# Integration Steps

## Project Requirements
The following steps assume that you have an ASP.NET Core 5 Web Application project that uses Identity and/or IdentityServer features.

## Cosmos DB Requirements
### Database
Just as with EF Core on SQL Server, you have to manually create a database in your Cosmos DB instance to be able to operate.

### Containers
Since **migrations are NOT supported when using EF Core on Cosmos DB**, you’ll have to manually create the following containers in your database:

| Container Name | Partition Key |
| --- | --- |
| Identity | /Id |
| Identity_DeviceFlowCodes | /SessionId |
| Identity_Logins | /ProviderKey |
| Identity_PersistedGrant | /Key |
| Identity_Tokens | /UserId |
| Identity_UserRoles | /UserId |
| Identity_Roles | /Id |

## DbContext
You have to create a DbContext that implements the provided `CosmosIdentityDbContext` type.

To start off you can create just an empty DbContext class that satisfies the above requirement:


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

## Update IdentityServer Configuration (If Applicable)
If your project is using **IdentityServer**, update the related configuration in order to use your new DbContext implementation:

```csharp
// Note that we're using MyDbContext as the second type parameter here...
services.AddIdentityServer().AddApiAuthorization<IdentityUser, MyDbContext>();
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
