// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AspNetCore.Identity.CosmosDb;
using AspNetCore.Identity.CosmosDb.Extensions;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using AspNetCore.Identity.CosmosDb.Tests.Shared;

namespace AspNetCore.Identity.Test
{
    public class IdentitySpecificationTests : IdentitySpecificationTestBase<IdentityUser, IdentityRole, string>
    {

        #region COSMOS DB CONFIG DB OPTIONS DB CONTEXT

        private IConfigurationRoot _configuration;
        public const string DATABASENAME = "cosmosdb";

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
        protected override void AddDbContext(IServiceCollection services, object context)
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
        protected override void AddIdentity(IServiceCollection services, object context)
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
            // Nothing to do here.
        }

        protected override void AddUserStore(IServiceCollection services, object context = null)
        {
            // Nothing to do here.
        }

        protected override object CreateTestContext()
        {
            //throw new NotImplementedException();
            return null;
        }

        protected override IdentityRole CreateTestRole(string roleNamePrefix = "", bool useRoleNamePrefixAsRoleName = false)
        {
            if (useRoleNamePrefixAsRoleName)
                return new IdentityRole(roleNamePrefix);

            return new IdentityRole($"{roleNamePrefix}NewRole");
        }

        protected override IdentityUser CreateTestUser(string namePrefix = "", string email = "", string phoneNumber = "", bool lockoutEnabled = false, DateTimeOffset? lockoutEnd = null, bool useNamePrefixAsUserName = false)
        {
            if (useNamePrefixAsUserName)
                return new IdentityUser(namePrefix)
                {
                    Email = email,
                    PhoneNumber = phoneNumber,
                    LockoutEnabled = lockoutEnabled,
                    LockoutEnd = lockoutEnd
                };

            return new IdentityUser($"{namePrefix}{email}");
        }

        protected override Expression<Func<IdentityRole, bool>> RoleNameEqualsPredicate(string roleName)
        {
            return identityRole => identityRole.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase);
        }

        protected override Expression<Func<IdentityRole, bool>> RoleNameStartsWithPredicate(string roleName)
        {
            return identityRole => identityRole.Name.StartsWith(roleName);
        }

        protected override void SetUserPasswordHash(IdentityUser user, string hashedPassword)
        {
            var utils = new TestUtilities();
            var userStore = utils.GetUserStore();
            userStore.SetPasswordHashAsync(user, hashedPassword).Wait();
        }

        protected override Expression<Func<IdentityUser, bool>> UserNameEqualsPredicate(string userName)
        {
            return identityUser => identityUser.UserName.Equals(userName);
        }

        protected override Expression<Func<IdentityUser, bool>> UserNameStartsWithPredicate(string userName)
        {
            return identityUser => identityUser.UserName.StartsWith(userName);
        }
    }
}
