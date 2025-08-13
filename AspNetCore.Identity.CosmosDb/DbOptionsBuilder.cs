using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Tls;
using System;
using System.Linq;

namespace AspNetCore.Identity.CosmosDb
{
    /// <summary>
    /// Database options builder for Entity Framework Core supporting Cosmos DB, SQL Server, and MySQL.
    /// </summary>
    public static class DbOptionsBuilder
    {
        /// <summary>
        /// Automatically builds <see cref="DbContextOptions"/> for either Cosmos DB, SQL Server, or MySQL based on the connection string provided.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para> This method inspects the provided connection string to determine the appropriate database provider to use. Here are some example connection strings:</para>
        /// <para><b>Cosmos DB:</b> AccountEndpoint=https://{Your Cosmos account DNS name}:443/;AccountKey={Your Key};Database={Your database name};</para>
        /// <para><b>SQL Server:</b> Server=tcp:{your_server}.database.windows.net,1433;Initial Catalog={your_database};Persist Security Info=False;User ID={your_user};Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;</para>
        /// <para><b>MySQL:</b> Server={your_server};Port=3306;uid={your_user};pwd={your_password};database={your_database};</para>
        /// </remarks>
        public static DbContextOptions<TContext> GetDbOptions<TContext>(string connectionString) where TContext : DbContext
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            if (connectionString.Contains("User ID", StringComparison.InvariantCultureIgnoreCase))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
            else if (connectionString.Contains("uid=", StringComparison.InvariantCultureIgnoreCase))
            {
                optionsBuilder.UseMySQL(connectionString);
            }
            else if (connectionString.Contains("AccountEndpoint=", StringComparison.InvariantCultureIgnoreCase))
            {
                var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var dbNamePart = parts.FirstOrDefault(p => p.StartsWith("Database=", StringComparison.InvariantCultureIgnoreCase));
                var endpointPart = parts.FirstOrDefault(p => p.StartsWith("AccountEndpoint=", StringComparison.InvariantCultureIgnoreCase));
                var keyPart = parts.FirstOrDefault(p => p.StartsWith("AccountKey=", StringComparison.InvariantCultureIgnoreCase));
                if (dbNamePart == null || endpointPart == null || keyPart == null)
                {
                    throw new ArgumentException("The provided Cosmos DB connection string is missing required components.", nameof(connectionString));
                }
                if (keyPart[1].ToString().Equals("AccessToken", StringComparison.CurrentCultureIgnoreCase))
                {
                    optionsBuilder.UseCosmos(accountEndpoint: endpointPart[1].ToString(), tokenCredential: new DefaultAzureCredential(), databaseName: dbNamePart[1].ToString());
                }
                else
                {
                    optionsBuilder.UseCosmos(accountEndpoint: endpointPart[1].ToString(), accountKey: keyPart[1].ToString(), databaseName: dbNamePart[1].ToString());
                }
            }
            else
            {
                throw new ArgumentException("The provided connection string does not appear to be valid for Cosmos DB, SQL Server, or MySQL.", nameof(connectionString));
            }

            return optionsBuilder.Options;
        }
    }
}
