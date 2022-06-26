using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AspNetCore.Identity.CosmosDb.Contracts;
using AspNetCore.Identity.CosmosDb.Repositories;
using AspNetCore.Identity.CosmosDb.Stores;
using System;

namespace AspNetCore.Identity.CosmosDb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Cosmos DB Identity.
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <typeparam name="TUserEntity"></typeparam>
        /// <typeparam name="TRoleEntity"></typeparam>
        /// <param name="services"></param>
        /// <param name="identityOptions"></param>
        /// <param name="dbContextOptions">Optional</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>The Cosmos DbContext be automatically added dbContextOptions are set. Otherwise add DbContext prior to this service.</para>
        /// <para>Adds the following to services in order:</para>
        /// <list type="number">
        /// <item>CosmosIdentityDbContext</item>
        /// <item>CosmosUserStore</item>
        /// <item>CosmosRoleStore</item>
        /// <item>CosmosIdentityRepository</item>
        /// </list>
        /// </remarks>
        public static IdentityBuilder AddCosmosIdentity<TDbContext, TUserEntity, TRoleEntity>(
            this IServiceCollection services,
            Action<IdentityOptions> identityOptions,
            Action<DbContextOptionsBuilder> dbContextOptions = null
        )
            where TDbContext : CosmosIdentityDbContext<TUserEntity>
            where TUserEntity : IdentityUser, new()
            where TRoleEntity : IdentityRole, new()
        {
            if (dbContextOptions != null)
                services.AddDbContext<TDbContext>(dbContextOptions);

            var builder = services
                .AddIdentityCore<TUserEntity>(identityOptions)
                .AddEntityFrameworkStores<TDbContext>();

            builder.AddDefaultTokenProviders();

            // Add custom Identity stores
            services.AddTransient<IUserStore<TUserEntity>, CosmosUserStore<TUserEntity>>();
            services.AddTransient<IRoleStore<TRoleEntity>, CosmosRoleStore<TRoleEntity>>();

            // Add repository service
            services.AddTransient<IRepository, CosmosIdentityRepository<TDbContext, TUserEntity>>();

            return builder;
        }
    }
}