<h1 valign="center"><img src="./Assets/cosmosdb.svg"/> Cosmos DB Provider for ASP.NET Core Identity</h1>

[![.NET 6 Build-Test](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/dotnet.yml/badge.svg)](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/dotnet.yml) [![CodeQL](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/codeql-analysis.yml)
[![Unit Tests](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/unittests.yml/badge.svg)](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/actions/workflows/unittests.yml)

This is a **Cosmos DB** implementation of an Identity provider for .NET 6 that uses the ["EF Core Azure Cosmos DB Provider."](https://docs.microsoft.com/en-us/ef/core/providers/cosmos/?tabs=dotnet-core-cli)

This project was forked from [Piero De Tomi's](https://github.com/pierodetomi) excellent project: [efcore-identity-cosmos](https://github.com/pierodetomi/efcore-identity-cosmos). If you are using .Net 5, it is highly recommended using that project instead of this one.

# Questions and Feedback Welcome

Need help getting started, or have feedback on how we can improve this product, [let us know](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/discussions)!

We appreciate feedback through this project's [discussion boards and issues list](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/discussions)! That greatly helps us know what to improve with this project.

# Installation (NuGet)

To add this provider to your own Asp.Net 6 web project, add the following [NuGet package](https://www.nuget.org/packages/AspNetCore.Identity.CosmosDb):

```shell
PM> Install-Package AspNetCore.Identity.CosmosDb
```

# Integration Steps

## Cosmos Account and optional SendGrid API Key

The following instructions show how to install the Cosmos DB identity provider. To continue please have the following ready:

- [Azure Cosmos DB account](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/create-cosmosdb-resources-portal) - either the serverless or dedicated instance. You do not have to create a database yet.
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
  "SetupCosmosDb": "true", // Importat: Remove this after first run.
  "CosmosIdentityDbName": "YourDabatabaseName",
  "ConnectionStrings": {
    "ApplicationDbContextConnection": "THE CONNECTION STRING TO YOUR COSMOS ACCOUNT"
  }
}
```

## Update Database Context (ApplicationDbContext.cs)

Here you will need to modify the database context to inherit from the `CosmosIdentityDbContext.`  Often
the database context can be found in this location:

`/Data/ApplicationDbContext.cs`

Now modify the file above to look like this:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDb.Example.Data
{
    public class ApplicationDbContext : CosmosIdentityDbContext<IdentityUser, IdentityRole>
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions)
          : base(dbContextOptions) { }
    }
}
```

## Modify Program.cs or Startup.cs File

After the "secrets" have been set, the next task is to modify your project's startup file.  For Asp.net
6 and higher that might be the `Project.cs` file. For other projects it might be your `Startup.cs.`

You will likely need to add these usings:

```csharp
using AspNetCore.Identity.CosmosDb;
using AspNetCore.Identity.CosmosDb.Containers;
using AspNetCore.Identity.CosmosDb.Extensions;
using AspNetCore.Identity.Services.SendGrid;
using AspNetCore.Identity.Services.SendGrid.Extensions;
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

Next, add the code that will trigger the provider to create the database and required containers:

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

Now add the database context that is required for this provider. Note: This context can be modified
to add your own entities (documentation on that is being developed).

Put this in your startup file:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseCosmos(connectionString: connectionString, databaseName: cosmosIdentityDbName));
```

The next step is to add the identity provider to your starup file. Here is an example:

```csharp
builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole>(
      options => options.SignIn.RequireConfirmedAccount = true // Always a good idea :)
    )
    .AddDefaultUI() // Use this if Identity Scaffolding is in use
    .AddDefaultTokenProviders();
```

## Configure Email Provider

When users register accounts or need to reset passwords, you will need (at a minimum), the ability
to send tokens via an [Email provider](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio#configure-an-email-provider). The example below uses a SendGrid provider. Here is how to add it:

Start by adding the following NuGet package to your project:

```shell
PM> Install-Package AspNetCore.Identity.Services.SendGrid
```

Note: You can sign up for a [free SendGrid account](https://sendgrid.com/) if you do not already have one.

## Configure app to support email

Next we need to configure the application to support our Email provider. Start by adding the following code to your startup file:

```csharp
var sendGridApiKey = builder.Configuration.GetValue<string>("SendGridApiKey");
// Modify 'from' email address to your own.
var sendGridOptions = new SendGridEmailProviderOptions(sendGridApiKey, "foo@mycompany.com");

builder.Services.AddSendGridEmailProvider(sendGridOptions);
```

## Modify "Scaffolded" Identity UI

The example web project uses the "scaffolded" Identity UI. By default it does not use an IEmailProvider.
But in our case we have installed one so we need to modify the UI to enable it.

In your project, find this file:

`/Areas/Identity/Pages/Account/RegisterConfirmation.cshtml.cs`

Find the `OnGetAsync()` method, then look for the following line:

```csharp
DisplayConfirmAccountLink = true;
```

Change that line to `false` like the following:

```csharp
DisplayConfirmAccountLink = false;
```

# Complete Startup File Example

The above instructions showed how to modify the startup file to make use of this provider. Sometimes
it is easier to see the end result rather than peicemeal.  Here is an example Asp.Net 6 Project.cs
file configured to work with this provider, scaffolded identity web pages, and the SendGrid email provider:

- [Program.cs](/AspNetCore.Identity.CosmosDb.Example/Program.cs)

An [example website](https://github.com/CosmosSoftware/AspNetCore.Identity.CosmosDb/tree/master/AspNetCore.Identity.CosmosDb.Example) is available for you to download and try.

# Supported LINQ Operators User and Role Stores

Both the user and role stores now support queries via LINQ using Entity Framework.
Here is an example:

```csharp
var userResults = userManager.Users.Where(u => u.Email.StartsWith("bob"));
var roleResults = roleManager.Roles.Where (r => r.Name.Contains("water"));
```

For a list of supported LINQ operations, please see the ["Supported LINQ Operations"](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-linq-to-sql#SupportedLinqOperators)
documentation for more details.

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

## v2.0.5.1

- Implemented IQueryableUserStore and IQueryableRoleStore

# References

To learn more about Asp.Net Identity and items realted to this project, please see the following:

- [.Net 5 version of AspNetCore.Identity.Cosmos (pierodetomi/efcore-identity-cosmos)](https://github.com/pierodetomi/efcore-identity-cosmos)
- [Asp.Net Core Identity on GitHub](https://github.com/dotnet/AspNetCore/tree/main/src/Identity)
- [Introduction to Identity on ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio)
  - [Account confirmation and password recovery in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio)
- [Supported LINQ Operaions for Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-linq-to-sql#SupportedLinqOperators)
