using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDbSqlServerStore.Tests.Stores
{
    [TestClass()]
    public class CosmosRoleStoreTests : CosmosIdentityTestsBase
    {
        private static string connectionString;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            connectionString = TestUtilities.GetKeyValue("ApplicationDbContextConnection4");
            InitializeClass(connectionString);
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityRole"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        private async Task<IdentityRole> GetMockRandomRoleAsync()
        {
            var role = new IdentityRole($"HUB{GetNextRandomNumber(1000, 9999)}");
            role.NormalizedName = role.Name.ToUpper();
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var result = await roleStore.CreateAsync(role);
            Assert.IsTrue(result.Succeeded);
            return role;
        }

        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            // Act
            // Create a bunch of roles in rapid succession
            using var dbContext = _testUtilities.GetDbContext(connectionString);
            var currentCount = await dbContext.Roles.CountAsync();
            var roleList = new List<IdentityRole>();
            for (int i = 0; i < 35; i++)
            {
                var r = await GetMockRandomRoleAsync();
                roleList.Add(r);
            }

            // Assert that they were created.
            var ids = roleList.Select(a => a.Id).ToList();
            var roleResult = await dbContext.Roles.Where(a => ids.Contains(a.Id)).ToListAsync();
            Assert.AreEqual(35, roleResult.Count);
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            using var userStore = _testUtilities.GetUserStore(connectionString);
            using var dbContext = _testUtilities.GetDbContext(connectionString);
            var role = await GetMockRandomRoleAsync(roleStore);
            var user = await GetMockRandomUserAsync(userStore);
            var roleClaim = GetMockClaim();
            await roleStore.AddClaimAsync(role, roleClaim);
            await userStore.AddToRoleAsync(user, role.NormalizedName);

            var roleId = role.Id;

            // Act
            var result = await roleStore.DeleteAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(await dbContext.Roles.Where(a => a.Name == role.Name).CountAsync() == 0);
            Assert.IsTrue(await dbContext.RoleClaims.Where(a => a.RoleId == roleId).CountAsync() == 0);
            Assert.IsTrue(await dbContext.UserRoles.Where(a => a.RoleId == roleId).CountAsync() == 0);
        }

        [TestMethod()]
        public async Task FindByIdAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await roleStore.FindByIdAsync(role.Id);

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task FindByNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task GetNormalizedRoleNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();

            // Act
            var r = await roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id);
        }

        [TestMethod()]
        public async Task GetRoleIdAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();

            // Act
            var result = await roleStore.GetRoleIdAsync(role);

            // Assert
            Assert.AreEqual(role.Id, result);
        }

        [TestMethod()]
        public async Task GetRoleNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();

            // Act
            var result = await roleStore.GetRoleNameAsync(role);

            // Assert
            Assert.AreEqual(role.Name, result);
        }

        [TestMethod()]
        public async Task SetNormalizedRoleNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            // Act
            await roleStore.SetNormalizedRoleNameAsync(role, newName.ToUpper(), default);

            // Assert
            var result = await roleStore.GetNormalizedRoleNameAsync(role);
            Assert.AreEqual(newName.ToUpper(), result);
        }

        [TestMethod()]
        public async Task SetRoleNameAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            // Act
            await roleStore.SetRoleNameAsync(role, newName);

            // Assert
            var result1 = await roleStore.GetRoleNameAsync(role);

            Assert.AreEqual(newName, result1);
        }

        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync(roleStore);
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            role.Name = newName;
            role.NormalizedName = newName.ToLower();

            // Act
            var result = await roleStore.UpdateAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded);
            role = await roleStore.FindByIdAsync(role.Id);
            Assert.AreEqual(newName, role.Name);
            Assert.AreEqual(newName.ToLower(), role.NormalizedName);
        }

        [TestMethod()]
        public async Task GetClaimsAsyncTest()
        {
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var claims = new Claim[] { GetMockClaim(), GetMockClaim(), GetMockClaim() };
            var role = await GetMockRandomRoleAsync();
            await roleStore.AddClaimAsync(role, claims[0], default);
            await roleStore.AddClaimAsync(role, claims[1], default);
            await roleStore.AddClaimAsync(role, claims[2], default);

            // Act
            var result2 = await roleStore.GetClaimsAsync(role, default);

            // Assert
            Assert.AreEqual(3, result2.Count);
        }

        [TestMethod()]
        public async Task AddClaimAsyncTest()
        {
            // Assert
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();
            var claim = GetMockClaim();

            // Act
            await roleStore.AddClaimAsync(role, claim, default);

            // Assert
            var result2 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(1, result2.Count);

        }
        [TestMethod()]
        public async Task RemoveClaimAsyncTest()
        {
            // Assert
            using var roleStore = _testUtilities.GetRoleStore(connectionString);
            var role = await GetMockRandomRoleAsync();
            var claim = GetMockClaim();
            await roleStore.AddClaimAsync(role, claim, default);
            var result2 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(1, result2.Count);

            // Act
            await roleStore.RemoveClaimAsync(role, claim, default);

            // Assert
            var result3 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(0, result3.Count);
        }

        [TestMethod]
        public async Task QueryRolesTest()
        {
            // Arrange
            using var roletore = _testUtilities.GetRoleStore(connectionString);
            var user1 = await GetMockRandomRoleAsync(roletore);

            // Act
            var result = await roletore.Roles.ToListAsync();

            // Assert
            Assert.IsTrue(result.Count > 0);
        }
    }
}