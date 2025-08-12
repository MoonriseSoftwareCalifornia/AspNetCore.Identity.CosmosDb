using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDbSqlServerStore.Tests
{
    [TestClass]
    public class SetupAssemblyInitializer
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            //
            // Setup context.
            //
            var testUtilities = new TestUtilities();
            var random = new Random();

            // Arrange class - remove prior data
            using var dbContext = testUtilities.GetDbContext(TestUtilities.GetConnectionString("ApplicationDbContextConnection4"));
            try
            {
                var task = dbContext.Database.EnsureCreatedAsync();
                task.Wait();

                dbContext.UserRoles.RemoveRange(dbContext.UserRoles.ToListAsync().Result);
                dbContext.Roles.RemoveRange(dbContext.Roles.ToListAsync().Result);
                dbContext.RoleClaims.RemoveRange(dbContext.RoleClaims.ToListAsync().Result);
                dbContext.UserClaims.RemoveRange(dbContext.UserClaims.ToListAsync().Result);
                dbContext.UserLogins.RemoveRange(dbContext.UserLogins.ToListAsync().Result);
                dbContext.Users.RemoveRange(dbContext.Users.ToListAsync().Result);
            }
            catch (Exception ex)
            {
                var trap = ex.Message; //Trap
            }
            var result = dbContext.SaveChanges();
        }
    }
}
