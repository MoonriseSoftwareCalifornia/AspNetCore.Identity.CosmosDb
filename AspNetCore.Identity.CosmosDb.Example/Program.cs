using AspNetCore.Identity.CosmosDb;
using AspNetCore.Identity.CosmosDb.Containers;
using AspNetCore.Identity.CosmosDb.Extensions;
using AspNetCore.Identity.Services.SendGrid;
using AspNetCore.Identity.Services.SendGrid.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");
var cosmosIdentityDbName = builder.Configuration.GetValue<string>("CosmosIdentityDbName");
var setupCosmosDb = builder.Configuration.GetValue<string>("SetupCosmosDb");

// If the following is set, then create the identity database and required containers.
// You can omit the following, or simplify it as needed.
if (!string.IsNullOrEmpty(setupCosmosDb))
{
    if (bool.TryParse(setupCosmosDb, out var setup))
    {
        if (setup)
        {
            var utils = new ContainerUtilities(connectionString, cosmosIdentityDbName);
            utils.CreateDatabaseAsync(cosmosIdentityDbName).Wait();
            utils.CreateRequiredContainers().Wait();
        }
    }
}

//
// Add the database context here
//
builder.Services.AddDbContext<CosmosIdentityDbContext<IdentityUser>>(options =>
  options.UseCosmos(connectionString: connectionString, databaseName: cosmosIdentityDbName));

builder.Services.AddCosmosIdentity<CosmosIdentityDbContext<IdentityUser>, IdentityUser, IdentityRole>(
      options => options.SignIn.RequireConfirmedAccount = true
    );

// Must have an Email sender when using Identity Framework.
// You will need an IEmailProvider. Below uses a SendGrid EmailProvider. You can use another.
// NuGet package: AspNetCore.Identity.Services.SendGrid
var sendGridApiKey = builder.Configuration.GetValue<string>("SendGridApiKey");
var sendGridOptions = new SendGridEmailProviderOptions(sendGridApiKey, "eric@moonrise.net");
builder.Services.AddSendGridEmailProvider(sendGridOptions);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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
app.MapRazorPages();

app.Run();
