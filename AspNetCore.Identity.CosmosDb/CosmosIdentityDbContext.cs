using AspNetCore.Identity.CosmosDb.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AspNetCore.Identity.CosmosDb
{
    /// <summary>
    /// Cosmos Identity Database Context
    /// </summary>
    /// <typeparam name="TUserEntity"></typeparam>
    public class CosmosIdentityDbContext<TUser, TRole, TKey> :
        IdentityDbContext<TUser, TRole, TKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {

        private readonly bool _backwardCompatibility;

        private StoreOptions? GetStoreOptions() => this.GetService<IDbContextOptions>()
                        .Extensions.OfType<CoreOptionsExtension>()
                        .FirstOrDefault()?.ApplicationServiceProvider
                        ?.GetService<IOptions<IdentityOptions>>()
                        ?.Value?.Stores;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="createDbAndContainers">Context with create the database and containers upon model creating.</param>
        public CosmosIdentityDbContext(
            DbContextOptions options,
            bool backwardCompatibility = false)
            : base(options)
        {
            _backwardCompatibility = backwardCompatibility;
        }

        /// <summary>
        /// OnModelCreating event override.
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {

            // dotnet/efcore#35224
            // New behavior for Cosmos DB EF is new.  For backward compatibility,
            // we need to add the following line to the OnModelCreating method.
            builder.HasDiscriminatorInJsonIds();

            // dotnet/efcore#35264
            // New behavior for Cosmos DB EF is to throw an error whenever it detects
            // an entity has an index.  This means we have to completely override the base
            // OnModelCreating method and not call it.
#pragma warning disable S125 // Sections of code should not be commented out
            // base.OnModelCreating(builder);
#pragma warning restore S125 // Sections of code should not be commented out


            // The following code is from the base.OnModelCreating method.
            var storeOptions = GetStoreOptions();
            var maxKeyLength = storeOptions?.MaxLengthForKeys ?? 0;

            if (maxKeyLength == 0)
            {
                maxKeyLength = 128;
            }

            var encryptPersonalData = storeOptions?.ProtectPersonalData ?? false;
            PersonalDataConverter? dataConverter = null;

            if (encryptPersonalData)
            {
                dataConverter = new PersonalDataConverter(this.GetService<IPersonalDataProtector>());
            }

            // Cosmos DB Modifications
            builder.ApplyIdentityMappings<TUser, TRole, TKey>(dataConverter, maxKeyLength);

            if (_backwardCompatibility)
            {
                builder.HasEmbeddedDiscriminatorName("Discriminator");
            }
        }

    }
}