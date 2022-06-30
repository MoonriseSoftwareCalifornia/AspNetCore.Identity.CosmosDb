using AspNetCore.Identity.CosmosDb.Tests;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDb.Stores.Tests
{
    [TestClass()]
    public class CosmosRoleStoreTests : CosmosIdentityTestsBase
    {
        //private static TestUtilities? utils;
        //private static CosmosUserStore<IdentityUser>? _userStore;
        //private static CosmosRoleStore<IdentityRole>? _roleStore;
        //private static Random _random;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            InitializeClass();
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityRole"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        private async Task<IdentityRole> GetMockRandomRoleAsync()
        {
            var role = new IdentityRole($"HUB{GetNextRandomNumber(1000, 9999)}");
            role.NormalizedName = role.Name.ToUpper();

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
            using var dbContext = _testUtilities.GetDbContext();
            Assert.AreEqual(35, dbContext.Roles.Count());

        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            // Arrange
            using var dbContext = _testUtilities.GetDbContext();
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
            var r = await _roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task GetNormalizedRoleNameAsyncTest()
        {
            // Arrange
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await _roleStore.FindByNameAsync(role.Name.ToUpper());

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
            await _roleStore.SetNormalizedRoleNameAsync(role, newName.ToUpper());

            // Assert
            var result = await _roleStore.GetNormalizedRoleNameAsync(role);
            Assert.AreEqual(newName.ToUpper(), result);
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

            Assert.AreEqual(newName, result1);
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

        [TestMethod()]
        public async Task GetClaimsAsyncTest()
        {
            // Arrange
            var claims = new Claim[] { new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()) };
            var role = await GetMockRandomRoleAsync();
            await _roleStore.AddClaimAsync(role, claims[0], default);
            await _roleStore.AddClaimAsync(role, claims[1], default);
            await _roleStore.AddClaimAsync(role, claims[2], default);

            // Act
            var result2 = await _roleStore.GetClaimsAsync(role, default);

            // Assert
            Assert.AreEqual(3, result2.Count);
        }

        [TestMethod()]
        public async Task AddClaimAsyncTest()
        {
            // Assert
            var role = await GetMockRandomRoleAsync();
            var claim = new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Act
            await _roleStore.AddClaimAsync(role, claim, default);

            // Assert
            var result2 = await _roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(1, result2.Count);

        }
        [TestMethod()]
        public async Task RemoveClaimAsyncTest()
        {
            // Assert
            var role = await GetMockRandomRoleAsync();
            var claim = new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            await _roleStore.AddClaimAsync(role, claim, default);

            // Act
            await _roleStore.RemoveClaimAsync(role, claim, default);

            // Assert
            var result2 = await _roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(0, result2.Count);
        }
    }
}