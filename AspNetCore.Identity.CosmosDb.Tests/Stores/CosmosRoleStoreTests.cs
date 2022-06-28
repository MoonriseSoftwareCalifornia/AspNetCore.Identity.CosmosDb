using AspNetCore.Identity.CosmosDb.Tests;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.CosmosDb.Stores.Tests
{
    [TestClass()]
    public class CosmosRoleStoreTests
    {


        private static TestUtilities? utils;
        private static CosmosUserStore<IdentityUser>? _userStore;
        private static CosmosRoleStore<IdentityRole>? _roleStore;
        private static Random _random;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            //
            // Setup context.
            //
            utils = new TestUtilities();
            _userStore = utils.GetUserStore();
            _roleStore = utils.GetRoleStore();
            _random = new Random();

            // Arrange class - remove prior data
            using var dbContext = utils.GetDbContext();
            dbContext.UserRoles.RemoveRange(dbContext.UserRoles.ToList());
            dbContext.Roles.RemoveRange(dbContext.Roles.ToList());
            dbContext.UserLogins.RemoveRange(dbContext.UserLogins.ToList());
            dbContext.Users.RemoveRange(dbContext.Users.ToList());
            var result = dbContext.SaveChanges();
        }

        /// <summary>
        /// Gets a random number
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private int GetNextRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityRole"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        private async Task<IdentityRole> GetMockRandomRoleAsync()
        {
            var role = new IdentityRole($"HUB{GetNextRandomNumber(1000, 9999)}");
            var result = await _roleStore.CreateAsync(role);
            Assert.IsTrue(result.Succeeded);
            return role;
        }

        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            // Act
            // Create a bunch of roles in rapid succession
            for (int i = 0; i < 35; i++)
            {
                var r = await GetMockRandomRoleAsync();
            }

            // Assert
            using var dbContext = utils.GetDbContext();
            Assert.AreEqual(35, dbContext.Roles.Count());

        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            // Arrange
            using var dbContext = utils.GetDbContext();
            var role = await GetMockRandomRoleAsync();

            // Act
            var result = await _roleStore.DeleteAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(dbContext.Roles.Where(a => a.Name == role.Name).Count() == 0);
        }

        [TestMethod()]
        public async Task FindByIdAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await _roleStore.FindByIdAsync(role.Id);

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task FindByNameAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await _roleStore.FindByNameAsync(role.Name);

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task GetNormalizedRoleNameAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await _roleStore.FindByNameAsync(role.Name);

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task GetRoleIdAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();

            // Act
            var result = await _roleStore.GetRoleIdAsync(role);

            // Assert
            Assert.AreEqual(role.Id, result);
        }

        [TestMethod()]
        public async Task GetRoleNameAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();

            // Act
            var result = await _roleStore.GetRoleNameAsync(role);

            // Assert
            Assert.AreEqual(role.Name, result);
        }

        [TestMethod()]
        public async Task SetNormalizedRoleNameAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            // Act
            await _roleStore.SetNormalizedRoleNameAsync(role, newName);

            // Assert
            var result = await _roleStore.GetNormalizedRoleNameAsync(role);
            Assert.AreEqual(newName.ToLower(), result);
        }

        [TestMethod()]
        public async Task SetRoleNameAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            // Act
            await _roleStore.SetRoleNameAsync(role, newName);

            // Assert
            var result1 = await _roleStore.GetRoleNameAsync(role);
            var result2 = await _roleStore.GetNormalizedRoleNameAsync(role);

            Assert.AreEqual(newName, result1);
            Assert.AreEqual(newName.ToLower(), result2);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            role.Name = newName;
            role.NormalizedName = newName.ToLower();

            // Act
            var result = await _roleStore.UpdateAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded);
            role = await _roleStore.FindByIdAsync(role.Id);
            Assert.AreEqual(newName, role.Name);
            Assert.AreEqual(newName.ToLower(), role.NormalizedName);
        }
    }
}