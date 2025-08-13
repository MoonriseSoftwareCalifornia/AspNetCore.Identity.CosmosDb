# Cosmos DB, MySQL and MS SQL Provider for ASP.NET Core Identity

[![CodeQL](https://github.com/MoonriseSoftwareCalifornia/AspNetCore.Identity.CosmosDb/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/MoonriseSoftwareCalifornia/AspNetCore.Identity.CosmosDb/actions/workflows/codeql-analysis.yml)
![Net 9 Tests (192)](https://github.com/MoonriseSoftwareCalifornia/AspNetCore.Identity.CosmosDb/actions/workflows/donet9tests.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/AspNetCore.Identity.CosmosDb.svg)](https://www.nuget.org/packages/AspNetCore.Identity.CosmosDb)

This is an ASP.NET Core Identity provider built for **Cosmos DB, MySQL and MS SQL**. It automatically loads the correct provider based on the connection string provided. It is used for the open source project [Cosmos CMS](https://cosmos.moonrise.net/) ([GitHub](https://github.com/MoonriseSoftwareCalifornia/CosmosCMS)).

## Version 9 compatibility with version 8 Cosmos DB databases

Cosmos DB Entity Framework verion 9 had [important changes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#cosmos-breaking-changes) that required important changes to this project.  Listed below are the changes addressed in this project.

### [The id property no longer contains the discriminator by default](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#cosmos-id-property-changes)

This project **requires** the "Descriminator" to be part of the ID value like this:

`IdentityUser|07c09eac-8815-43d3-9141-30876ef0e465`

 However EF 9 does not include the descrimintator by default. To include it the following code `builder.HasDiscriminatorInJsonIds();` is added to the `OnModelCreating` method of the `CosmosIdentityDbContext` class:

 ```csharp
        protected override void OnModelCreating(ModelBuilder builder)
        {

            // dotnet/efcore#35224
            // New behavior for Cosmos DB EF is new.  For backward compatibility,
            // we need to add the following line to the OnModelCreating method.
            builder.HasDiscriminatorInJsonIds();
            .
            .
            .
        }
 ```

### [The discriminator property is now named $type instead of Discriminator](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#cosmos-discriminator-name-change)

When using EF 9 to read a database created with EF 8 or earlier, there may not be any error thrown.  Instead, the read will return no records, which can be mystifying.  The reason is the Discriminator field name has changed from "Descriminator" to "$type".  Consequently, EF 9 does not find the descriminator value.

This was fixed in the project by adding the following code `builder.HasEmbeddedDiscriminatorName("Discriminator")` to the `OnModelCreating` method of the `CosmosIdentityDbContext` class. Also note in the following code the field `_backwardCompatibility`.

```csharp
        protected override void OnModelCreating(ModelBuilder builder)
        {
            .
            .
            .
            if (_backwardCompatibility)
            {
                builder.HasEmbeddedDiscriminatorName("Discriminator");
            }
        }
```

To enable backward compatibility with the "Discriminator,"  set the `backwardCompatibility` field of the context constructor to `true` (note: default is `false`).  Here is an example:

```csharp
var options = new DbContextOptionsBuilder<CosmosIdentityDbContext>();
options.UseCosmos(connectionString, databaseName);
using var dbContext = new ApplicationDbContext(tempBuilder.Options, true); // True is set for backward compatibility

```

### [HasIndex now throws instead of being ignored](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#cosmos-hasindex-throws)

There are two ways to handle this: (1) Remove the index definitions on the entities, or (2), add the following code to 
suppress the exception in the `OnConfiguring' method like this:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.ConfigureWarnings(w => w.Ignore(CosmosEventId.SyncNotSupported));
}
```

## Updates to unit tests

A new unit test was added to test backward compatibility. The tests use two different Cosmos DB accounts. It is suggested to use "serverless" accounts to keep costs down.

The "secrets" file now needs two database connection strings like this:

```json
{
  "CosmosIdentityDbName": "localtests", // Both test projects use this database name.
  "ConnectionStrings": {
    // Database account used for backward compatibility tests.
    "ApplicationDbContextConnection2": "AccountEndpoint=[YOUR CONNECTION STRING HERE]",
    // Used for EF version 9 and above tests.
    "ApplicationDbContextConnection": "AccountEndpoint=[YOUR CONNECTION STRING HERE]"
  }
}
```

## Upgrading from version 2.x to 8.x

When upgrading to version 8 from 2, you will need to make two changes to your project:

The old way of creating the Database Context looked like this:

```csharp
public class ApplicationDbContext : CosmosIdentityDbContext<IdentityUser, IdentityRole>
```

The new way is like this (with 'string' added):

```csharp
public class ApplicationDbContext : CosmosIdentityDbContext<IdentityUser, IdentityRole, string>
```

Next, in your Program.cs or Startup.cs files, change from this:

```csharp
 builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole>
```

To this (with 'string' added):

```csharp
 builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole, string>
```

## Installation

> **Tip**: This package uses seven (7) "[containers](https://learn.microsoft.com/en-us/azure/cosmos-db/resource-model#azure-cosmos-db-containers)."  If the RU throughput for your Cosmos Account is
> configured at the "***container***" level, this can require your Cosmos DB Account to require a higher minimum RU.
>
> ### Sharing Throughput
> To keep costs down, consider ***sharing throughput at the database level*** as described here in the [documentation](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-provision-database-throughput?tabs=dotnetv2#provision-throughput-using-azure-portal).  This allows you to, for example, set the RU at the database to be 1,000 RU, then have all containers within that database ***share*** those RU's.
>
> ### Autoscale
> Next, set the RU to "***[autoscale](https://learn.microsoft.com/en-us/azure/cosmos-db/provision-throughput-autoscale)***."  According to [documentation](https://learn.microsoft.com/en-us/azure/cosmos-db/provision-throughput-autoscale), "*Autoscale provisioned throughput in Azure Cosmos DB allows you to scale the throughput (RU/s) of your database or container automatically and instantly.*" This will allow your database to scale down and up as needed, thus reducing your monthly costs further.

### Nuget Package

Add the following [NuGet package](https://www.nuget.org/packages/AspNetCore.Identity.CosmosDb) to your project:

```shell
PM> Install-Package AspNetCore.Identity.CosmosDb
```

Create an [Azure Cosmos DB account](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/create-cosmosdb-resources-portal) - either the free, serverless or dedicated instance. For testing and development purposes it is recommended to use a free
account. [See documentation](https://github.com/MoonriseSoftwareCalifornia/AspNetCore.Identity.CosmosDb#choice-of-cosmos-db-account-type) to help choose which type of Cosmos account is best for you.

Set your configuration settings with the connection string and database name. Below is an example of a `secrets.json` file:

```json
{
  "SetupCosmosDb": "true", // Importat: Remove this after first run.
  "CosmosIdentityDbName": "YourDabatabaseName",
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "THE CONNECTION STRING TO YOUR COSMOS ACCOUNT"
  }
}
```

### Update Database Context

Modify the database context to inherit from the `CosmosIdentityDbContext` like this:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDb.Example.Data
{
    public class ApplicationDbContext : CosmosIdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions)
          : base(dbContextOptions) { }
    }
}
```

### Modify Program.cs or Startup.cs File

After the "secrets" have been set, the next task is to modify your project's startup file.  For Asp.net
6 and higher that might be the `Project.cs` file. For other projects it might be your `Startup.cs.`

You will likely need to add these usings:

```csharp
using AspNetCore.Identity.CosmosDb;
using AspNetCore.Identity.CosmosDb.Containers;
using AspNetCore.Identity.CosmosDb.Extensions;
```

Next, the configuration variables need to be retrieved. Add the following to your startup file:

```csharp
// The Cosmos connection string
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");

// Name of the Cosmos database to use
var cosmosIdentityDbName = builder.Configuration.GetValue<string>("CosmosIdentityDbName");

// If this is set, the Cosmos identity provider will:
// 1. Create the database if it does not already exist.
// 2. Create the required containers if they do not already exist.
// IMPORTANT: Remove this setting if after first run. It will improve startup performance.
var setupCosmosDb = builder.Configuration.GetValue<string>("SetupCosmosDb");

```

Add this code if you want the provider to create the database and required containers:

```csharp
// If the following is set, it will create the Cosmos database and
//  required containers.
if (bool.TryParse(setupCosmosDb, out var setup) && setup)
{
    var builder1 = new DbContextOptionsBuilder<ApplicationDbContext>();
    builder1.UseCosmos(connectionString, cosmosIdentityDbName);

    using (var dbContext = new ApplicationDbContext(builder1.Options))
    {
        dbContext.Database.EnsureCreated();
    }
}

```

Now add the database context in your startup file like this:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseCosmos(connectionString: connectionString, databaseName: cosmosIdentityDbName));
```

Follow that up with the identity provider. Here is an example:

```csharp
builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole, string>(
      options => options.SignIn.RequireConfirmedAccount = true // Always a good idea :)
    )
    .AddDefaultUI() // Use this if Identity Scaffolding is in use
    .AddDefaultTokenProviders();
```

## Adding Google or Microsoft OAuth providers

This library works with external OAuth providers, and below is an example of how we implement this.

Begin by adding these two NuGet packages to your project:
- [Microsoft.AspNetCore.Authentication.Google](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.Google)
- [Microsoft.AspNetCore.Authentication.MicrosoftAccount](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.MicrosoftAccount)

Then add the code below to your Project.cs file.


```csharp
// Example of adding OAuth Providers
// Add Google if keys are present
var googleClientId = Configuration["Authentication_Google_ClientId"];
var googleClientSecret = Configuration["Authentication_Google_ClientSecret"];

// If Google ID and secret are both found, then add the provider.
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
}

// Add Microsoft if keys are present
var microsoftClientId = Configuration["Authentication_Microsoft_ClientId"];
var microsoftClientSecret = Configuration["Authentication_Microsoft_ClientSecret"];

// If Microsoft ID and secret are both found, then add the provider.
if (!string.IsNullOrEmpty(microsoftClientId) && !string.IsNullOrEmpty(microsoftClientSecret))
{
    builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
    {
        options.ClientId = microsoftClientId;
        options.ClientSecret = microsoftClientSecret;
    });
}

```

To learn more about external OAuth providers, please see the [Microsoft documentation](https://learn.microsoft.com/en-us/aspnet/web-api/overview/security/external-authentication-services) on this subject.

## Complete Startup File Example

The above instructions showed how to modify the startup file to make use of this provider. Sometimes
it is easier to see the end result rather than peicemeal.  Here is an example Asp.Net 6 Project.cs
file configured to work with this provider, scaffolded identity web pages, and the SendGrid email provider:

- [Program.cs](https://github.com/MoonriseSoftwareCalifornia/CosmosCMS/blob/main/Editor/Program.cs)

An [example web application](https://github.com/MoonriseSoftwareCalifornia/CosmosCMS/tree/main) is also available.

## Supported LINQ Operators User and Role Stores

Both the user and role stores now support queries via LINQ using Entity Framework.
Here is an example:

```csharp
var userResults = userManager.Users.Where(u => u.Email.StartsWith("bob"));
var roleResults = roleManager.Roles.Where (r => r.Name.Contains("water"));
```

For a list of supported LINQ operations, please see the ["Supported LINQ Operations"](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-linq-to-sql#SupportedLinqOperators)
documentation for more details.

### Help Find Bugs

Find a bug? Let us know by contacting us [via NuGet](https://www.nuget.org/packages/AspNetCore.Identity.CosmosDb/2.0.10/ContactOwners) or submit a bug report on our [GitHub issues section](https://github.com/MoonriseSoftwareCalifornia/AspNetCore.Identity.CosmosDb/issues). Thank you in advance!

## Changelog

This change log notes major changes beyond routine documentation and NuGet dependency updates.

### v10.0.0.0-preview

Cosmos CMS has a need to be multi-cloud compatible. To that end this library is being modified to be able to use Cosmos DB, Azure or MS SQL, or MySQL databases.

### v9.0.1.0

Removed the sample application included here as it was out of date.  In its place, an ongoing
project ([Cosmos CMS](https://github.com/MoonriseSoftwareCalifornia/CosmosCMS)) that uses this package as an example.

### v9.0.0.3

- Backward campatibility added for databases created EF version 8 or earlier.
- Added unit tests for backward compatibility testing.

### v9.0.0.1

- Updated for .Net 9, and version 9 of the Entity Framework.

### v8.0.0.3

- Now supports generic keys.
- Applied patch for issue #14.

### v8.0.0.1

- Now built for .Net 8, and removed support for 6 and 7.
- Updated NuGet packages to latest releases.

### v2.1.1

- Added support for .Net 6 and .Net 7.

### v2.0.20

- Addressing [bug #9](https://github.com/MoonriseSoftwareCalifornia/AspNetCore.Identity.CosmosDb/issues/9), implemented interfaces IUserAuthenticatorKeyStore and IUserTwoFactorRecoveryCodeStore to support two factor authentication.  Example website updated to demonstrate capability with QR code generation.

### v1.0.6

- Introduced support for `IUserLoginStore<TUser>` in User Store

### v1.0.5

- Introduced support for `IUserPhoneNumberStore<TUser>` in User Store

### v1.0.4

- Introduced support for `IUserEmailStore<TUser>` in User Store

### v2.0.0-alpha

- Forked from source repository [pierodetomi/efcore-identity-cosmos](https://github.com/pierodetomi/efcore-identity-cosmos).
- Refactored for .Net 6 LTS.
- Added `UserStore`, `RoleStore`, `UserManager` and `RoleManager` unit tests.
- Namespace changed to one more generic: `AspNetCore.Identity.CosmosDb`
- Implemented `IUserLockoutStore` interface for `UserStore`

### v2.0.1.0

- Added example web project

### v2.0.5.1

- Implemented IQueryableUserStore and IQueryableRoleStore

## Unit Test Instructions

To run the unit tests you will need two things: (1) A Cosmos DB Account, and (2) a connection string to that account.
Here is an example of a `secrets.json` file created for the unit test project:

```json
{
  "CosmosIdentityDbName" : "YOURDATABASENAME",
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "AccountEndpoint=YOURCONNECTIONSTRING;"
  }
}
```

### Choice of Cosmos DB Account Type

This implementation will work with the "Free" Cosmos DB tier.  You can have one per account.

It also works the "serverless" and "provisioned" account types.

## References

To learn more about Asp.Net Identity and items realted to this project, please see the following:

- [.Net 5 version of AspNetCore.Identity.Cosmos (pierodetomi/efcore-identity-cosmos)](https://github.com/pierodetomi/efcore-identity-cosmos)
- [Asp.Net Core Identity on GitHub](https://github.com/dotnet/AspNetCore/tree/main/src/Identity)
- [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio)
  - [Account confirmation and password recovery in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio)
- [Supported LINQ Operaions for Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-linq-to-sql#SupportedLinqOperators)
