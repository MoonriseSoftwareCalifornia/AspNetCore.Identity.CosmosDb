using AspNetCore.Identity.CosmosDb;
using AspNetCore.Identity.CosmosDb.Containers;
using AspNetCore.Identity.CosmosDb.Example.Data;
using AspNetCore.Identity.CosmosDb.Extensions;
using AspNetCore.Identity.Services.SendGrid;
using AspNetCore.Identity.Services.SendGrid.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// The Cosmos connection string
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");

// Name of the Cosmos database to use
var cosmosIdentityDbName = builder.Configuration.GetValue<string>("CosmosIdentityDbName");

// If this is set, the Cosmos identity provider will:
// 1. Create the database if it does not already exist.
// 2. Create the required containers if they do not already exist.
// IMPORTANT: Remove this variable if after first run. It will improve startup performance.
var setupCosmosDb = builder.Configuration.GetValue<string>("SetupCosmosDb");

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

//
// Add the Cosmos database context here
//
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseCosmos(connectionString: connectionString, databaseName: cosmosIdentityDbName));

//
// Add Cosmos Identity here
//
builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole>(
      options => options.SignIn.RequireConfirmedAccount = true
    )
    .AddDefaultUI() // Use this if Identity Scaffolding added
    .AddDefaultTokenProviders();

//
// Must have an Email sender when using Identity Framework.
// You will need an IEmailProvider. Below uses a SendGrid EmailProvider. You can use another.
// Below users NuGet package: AspNetCore.Identity.Services.SendGrid
var sendGridApiKey = builder.Configuration.GetValue<string>("SendGridApiKey");
var sendGridOptions = new SendGridEmailProviderOptions(sendGridApiKey, "eric@moonrise.net");
builder.Services.AddSendGridEmailProvider(sendGridOptions);
// End add SendGrid

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Use this if Identity Scaffolding added

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Use this if Identity Scaffolding added

app.Run();
