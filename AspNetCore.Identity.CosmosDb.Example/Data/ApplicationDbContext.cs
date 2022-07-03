using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDb.Example.Data
{
    public class ApplicationDbContext : CosmosIdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions)
          : base(dbContextOptions) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // DO NOT REMOVE THIS LINE. If you do, your context won't work as expected.
            base.OnModelCreating(builder);

            // TODO: Add your own fluent mappings
        }
    }
}