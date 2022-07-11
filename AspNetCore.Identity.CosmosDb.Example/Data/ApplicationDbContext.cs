using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDb.Example.Data
{
    public class ApplicationDbContext : CosmosIdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions)
          : base(dbContextOptions) { }

    }
}