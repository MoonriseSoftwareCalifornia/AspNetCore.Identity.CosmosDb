using AspNetCore.Identity.CosmosDb.Containers;
using AspNetCore.Identity.CosmosDb.Repositories;
using AspNetCore.Identity.CosmosDb.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace AspNetCore.Identity.CosmosDb.Tests
{
    public class TestUtilities
    {
        private IConfigurationRoot _configuration;

        /// <summary>
        /// Non-normalized email address for user 1
        /// </summary>
        public const string IDENUSER1EMAIL = "Foo1@acme.com";
        /// <summary>
        /// Non-normalized email address for user 2
        /// </summary>
        public const string IDENUSER2EMAIL = "Foo2@acme.com";


        public const string IDENUSER1ID = "507b7565-493e-49d7-94c7-d60e21036b4a";
        public const string IDENUSER2ID = "55250c6f-7c91-465a-a9ce-ea9bbe6caf81";
        //public const string DATABASENAME = "cosmosdb";
        private readonly string _databaseName;

        /// <summary>
        /// Gets the configuration
        /// </summary>
        /// <returns></returns>
        public IConfigurationRoot GetConfig()
        {
            if (_configuration != null) return _configuration;

            // the type specified here is just so the secrets library can 
            // find the UserSecretId we added in the csproj file
            var jsonConfig = Path.Combine(Environment.CurrentDirectory, "appsettings.json");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(jsonConfig, true)
                .AddEnvironmentVariables() // Added to read environment variables from GitHub Actions
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true); // User secrets override all - put here

            _configuration = Retry.Do(() => builder.Build(), TimeSpan.FromSeconds(1));

            return _configuration;
        }

        /// <summary>
        /// Gets the value of a configuration key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKeyValue(string key)
        {
            return GetKeyValue(GetConfig(), key);
        }

        private string GetKeyValue(IConfigurationRoot config, string key)
        {
            var data = config[key];

            if (string.IsNullOrEmpty(data))
            {
                // First attempt to get the value of the key as named.
                data = Environment.GetEnvironmentVariable(key);

                if (string.IsNullOrEmpty(data))
                {
                    // For Github Actions, secrets are forced upper case
                    data = Environment.GetEnvironmentVariable(key.ToUpper());
                }
            }
            return string.IsNullOrEmpty(data) ? string.Empty : data;
        }

        /// <summary>
        /// Get Cosmos DB Options
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public DbContextOptions GetDbOptions(string connectionName = "ApplicationDbContextConnection")
        {
            var config = GetConfig();
            var connectionString = config.GetConnectionString("ApplicationDbContextConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = GetKeyValue("ApplicationDbContextConnection");
            }

            var builder = new DbContextOptionsBuilder();
            builder.UseCosmos(connectionString, GetKeyValue("CosmosIdentityDbName"));

            return builder.Options;
        }

        /// <summary>
        /// Gets an instance of the container utilities
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public ContainerUtilities GetContainerUtilities(string connectionName = "ApplicationDbContextConnection")
        {
            var config = GetConfig();
            var connectionString = config.GetConnectionString(connectionName);

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = GetKeyValue("ApplicationDbContextConnection");
            }

            var utilities = new ContainerUtilities(connectionString, GetKeyValue("CosmosIdentityDbName"));
            return utilities;
        }

        /// <summary>
        /// Get an instance of the Cosmos DB context.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public CosmosIdentityDbContext<IdentityUser, IdentityRole> GetDbContext(string connectionName = "ApplicationDbContextConnection")
        {
            var dbContext = new CosmosIdentityDbContext<IdentityUser, IdentityRole>(GetDbOptions(connectionName));
            return dbContext;
        }

        /// <summary>
        /// Get an instance of the Cosmos DB user store.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public CosmosUserStore<IdentityUser> GetUserStore(string connectionName = "ApplicationDbContextConnection")
        {

            var repository = new CosmosIdentityRepository<CosmosIdentityDbContext<IdentityUser, IdentityRole>, IdentityUser, IdentityRole>(GetDbContext());

            var userStore = new CosmosUserStore<IdentityUser>(repository);

            return userStore;
        }

        /// <summary>
        /// Get an instance of the Cosmos DB role store
        /// </summary>
        /// <returns></returns>
        public CosmosRoleStore<IdentityUser, IdentityRole> GetRoleStore()
        {
            var repository = new CosmosIdentityRepository<CosmosIdentityDbContext<IdentityUser, IdentityRole>, IdentityUser, IdentityRole>(GetDbContext());

            var rolestore = new CosmosRoleStore<IdentityUser, IdentityRole>(repository);

            return rolestore;
        }

        /// <summary>
        /// Get an instance of the role manager
        /// </summary>
        /// <returns></returns>
        public RoleManager<IdentityRole> GetRoleManager()
        {
            var userStore = GetRoleStore();
            var userManager = new RoleManager<IdentityRole>(userStore, null, null, null, GetLogger<RoleManager<IdentityRole>>());
            return userManager;
        }

        /// <summary>
        /// Get a mock logger
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ILogger<T> GetLogger<T>()
        {
            return new Logger<T>(new NullLoggerFactory());
        }
    }
}
