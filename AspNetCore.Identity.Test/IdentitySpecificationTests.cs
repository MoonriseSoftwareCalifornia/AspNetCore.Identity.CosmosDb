// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PieroDeTomi.EntityFrameworkCore.Identity.Cosmos;
using PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Extensions;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace AspNetCore.Identity.Test
{
    public class IdentitySpecificationTests : IdentitySpecificationTestBase<IdentityUser, IdentityRole, string>
    {
        private IConfigurationRoot _configuration;
        public const string DATABASENAME = "cosmosdb";

        public IdentitySpecificationTests()
        {

        }

        #region COSMOS DB CONFIG DB OPTIONS DB CONTEXT

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
        /// Get Cosmos DB Options
        /// </summary>
        /// <returns></returns>
        public DbContextOptions GetDbOptions()
        {
            var config = GetConfig();
            var connectionString = config.GetConnectionString("ApplicationDbContextConnection");
            var builder = new DbContextOptionsBuilder();
            builder.UseCosmos(connectionString, DATABASENAME);

            return builder.Options;
        }

        /// <summary>
        /// Adds the Cosmos DB Context
        /// </summary>
        /// <param name="services"></param>
        /// <param name="context"></param>
        protected override void AddCosmosDbContext(IServiceCollection services, object context)
        {
            var config = GetConfig();
            var connectionString = config.GetConnectionString("ApplicationDbContextConnection");

            services.AddDbContext<CosmosIdentityDbContext<IdentityUser>>(options =>
                options.UseCosmos(connectionString, DATABASENAME));
        }

        /// <summary>
        /// Adds the Cosmos Identity, user and role stores.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="context"></param>
        protected override void AddCosmosIdentity(IServiceCollection services, object context)
        {
            services.AddCosmosIdentity<CosmosIdentityDbContext<IdentityUser>, IdentityUser, IdentityRole>(
            options =>
            {
               //options.UseCosmos("", "", databaseName: "cosmosdb");
            });
        }

        #endregion

        /// <summary>
        /// Adds a role store
        /// </summary>
        /// <param name="services"></param>
        /// <param name="context"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void AddRoleStore(IServiceCollection services, object context = null)
        {
            throw new NotImplementedException();
        }

        protected override void AddUserStore(IServiceCollection services, object context = null)
        {
            throw new NotImplementedException();
        }

        protected override object CreateTestContext()
        {
            throw new NotImplementedException();
        }

        protected override IdentityRole CreateTestRole(string roleNamePrefix = "", bool useRoleNamePrefixAsRoleName = false)
        {
            throw new NotImplementedException();
        }

        protected override IdentityUser CreateTestUser(string namePrefix = "", string email = "", string phoneNumber = "", bool lockoutEnabled = false, DateTimeOffset? lockoutEnd = null, bool useNamePrefixAsUserName = false)
        {
            throw new NotImplementedException();
        }

        protected override Expression<Func<IdentityRole, bool>> RoleNameEqualsPredicate(string roleName)
        {
            throw new NotImplementedException();
        }

        protected override Expression<Func<IdentityRole, bool>> RoleNameStartsWithPredicate(string roleName)
        {
            throw new NotImplementedException();
        }

        protected override void SetUserPasswordHash(IdentityUser user, string hashedPassword)
        {
            throw new NotImplementedException();
        }

        protected override Expression<Func<IdentityUser, bool>> UserNameEqualsPredicate(string userName)
        {
            throw new NotImplementedException();
        }

        protected override Expression<Func<IdentityUser, bool>> UserNameStartsWithPredicate(string userName)
        {
            throw new NotImplementedException();
        }
    }
}
