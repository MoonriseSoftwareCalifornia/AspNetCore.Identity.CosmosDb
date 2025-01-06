using AspNetCore.Identity.CosmosDb.Tests.Net7;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDb.Tests
{
    [TestClass]
    public class BackkwardCompatibilityTests : CosmosIdentityTestsBase
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            InitializeClass();
        }

        [TestMethod]
        public async Task ReadIdentityUsers()
        {
            // Arrange
            var testUtilities = new TestUtilities();

            var dbContext = testUtilities.GetDbContext(backwardCompatibility: true, dbName: "cosmoscms");

            //// Assert
            var users = await dbContext.Users.ToListAsync();
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count > 0);
        }
    }
}
