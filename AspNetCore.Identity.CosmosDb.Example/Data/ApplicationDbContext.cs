using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDb.Example.Data
{
    public class ApplicationDbContext : CosmosIdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions)
          : base(dbContextOptions) { }

    }
}