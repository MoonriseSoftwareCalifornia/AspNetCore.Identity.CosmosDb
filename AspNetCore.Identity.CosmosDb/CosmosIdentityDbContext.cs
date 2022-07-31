using AspNetCore.Identity.CosmosDb.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDb
{
    /// <summary>
    /// Cosmos Identity Database Context
    /// </summary>
    /// <typeparam name="TUserEntity"></typeparam>
    public class CosmosIdentityDbContext<TUser, TRole> : 
        IdentityDbContext<TUser, TRole, string> 
        where TUser : IdentityUser
        where TRole : IdentityRole
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="createDbAndContainers">Context with create the database and containers upon model creating.</param>
        public CosmosIdentityDbContext(
            DbContextOptions options) : base(options) {
        }

        /// <summary>
        /// OnModelCreating event override.
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyIdentityMappings<TUser>();
        }
    }
}