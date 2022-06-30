using Microsoft.VisualStudio.TestTools.UnitTesting;
using AspNetCore.Identity.CosmosDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDb.Tests
{
    [TestClass()]
    public class RoleManagerInterOperabilityTests : CosmosIdentityTestsBase
    {

        // Creates a new test role using the mock RoleManager to do so
        private async Task<IdentityRole> GetTestRole(RoleManager<IdentityRole> roleManager)
        {
            var role = await GetMockRandomRoleAsync(false);

            var result = await roleManager.CreateAsync(role);

            Assert.IsTrue(result.Succeeded);
            return await roleManager.FindByIdAsync(role.Id);
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            InitializeClass();
        }
        [TestMethod]
        public async Task AddClaimAsyncTest()
        {
            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var claim = new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Act
            var result1 = await roleManager.AddClaimAsync(role, claim);

            // Assert
            Assert.IsTrue(result1.Succeeded);
            var result2 = await roleManager.GetClaimsAsync(role);
            // Two claims: LOCAL AUTHORITY and the new claim above
            Assert.AreEqual(2, result2?.Count);

        }

        [TestMethod]
        public async Task CreateAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());

            // Act
            var role = await GetTestRole(roleManager);

            // Assert
            Assert.IsNotNull(role);
        }

        [TestMethod]
        public async Task DeleteAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var id = role.Id;

            // Act
            var result1 = await roleManager.DeleteAsync(role);

            // Assert
            Assert.IsTrue(result1.Succeeded);
            var result2 = await roleManager.FindByIdAsync(id);
            Assert.IsNull(result2);

        }
        [TestMethod]
        public async Task FindByIdAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var id = role.Id;

            // Act
            var result = await roleManager.FindByIdAsync(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);

        }

        [TestMethod]
        public async Task FindByNameAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var name = role.Name;

            // Act
            var result = await roleManager.FindByNameAsync(name);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(name, result.Name);

        }
        [TestMethod]
        public async Task GetClaimsAsyncTest()
        {
            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var claim1 = new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var claim2 = new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var result1 = await roleManager.AddClaimAsync(role, claim1);
            var result2 = await roleManager.AddClaimAsync(role, claim2);
            Assert.IsTrue(result1.Succeeded);
            Assert.IsTrue(result2.Succeeded);

            // Act
            var result = await roleManager.GetClaimsAsync(role);

            // Assert
            Assert.AreEqual(2, result.Count);

        }

        [TestMethod]
        public async Task GetRoleIdAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);

            // Act
            var result = await roleManager.GetRoleIdAsync(role);

            // Assert
            Assert.AreEqual(role.Id, result);
        }

        [TestMethod]
        public async Task GetRoleNameAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);

            // Act
            var result = await roleManager.GetRoleNameAsync(role);

            // Assert
            Assert.AreEqual(role.Name, result);
        }

        [TestMethod]
        public async Task NormalizeKeyTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var key = Guid.NewGuid().ToString();

            // Act
            var result = roleManager.NormalizeKey(key);

            // Assert
            Assert.AreEqual(key.ToUpperInvariant(), result);
        }

        [TestMethod]
        public async Task RemoveClaimAsyncTest()
        {
            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var claim = new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            var result1 = await roleManager.AddClaimAsync(role, claim);
            Assert.IsTrue(result1.Succeeded);
            var result2 = await roleManager.GetClaimsAsync(role);
            Assert.AreEqual(2, result2.Count);

            // Act
            var result3 = await roleManager.RemoveClaimAsync(role, claim);

            // Assert
            Assert.IsTrue(result3.Succeeded);
            var result4 = await roleManager.GetClaimsAsync(role);
            Assert.AreEqual(1, result4.Count);
        }

        [TestMethod]
        public async Task RoleExistsAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);

            // Act
            var result = await roleManager.RoleExistsAsync(role.Name);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task SetRoleNameAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var name = Guid.NewGuid().ToString();

            // Act
            var result = await roleManager.SetRoleNameAsync(role, name);

            // Assert
            Assert.IsTrue(result.Succeeded);
            var result2 = await roleManager.FindByIdAsync(role.Id);
            Assert.AreEqual(name, result2.Name);

        }

        [TestMethod]
        public async Task UpdateAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);

            // Act

            // Assert

        }

        [TestMethod]
        public async Task UpdateNormalizedRoleNameAsyncTest()
        {
            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var name = Guid.NewGuid().ToString();
            var result = await roleManager.SetRoleNameAsync(role, name);
            Assert.IsTrue(result.Succeeded);
            var result2 = await roleManager.FindByIdAsync(role.Id);
            Assert.AreEqual(name, result2.Name);

            // Act
            await roleManager.UpdateNormalizedRoleNameAsync(role);

            // Assert
            var result3 = await roleManager.FindByIdAsync(role.Id);
            Assert.AreEqual(name.ToUpperInvariant(), result3.NormalizedName);
        }

        [TestMethod]
        protected async Task UpdateRoleAsyncTest()
        {

            // Assert
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());
            var role = await GetTestRole(roleManager);
            var stamp = Guid.NewGuid().ToString();
            role.ConcurrencyStamp = stamp;

            // Act
            var result1 = await roleManager.UpdateAsync(role);

            // Assert
            Assert.IsTrue(result1.Succeeded);
            var result2 = await roleManager.FindByIdAsync(role.Id);
            Assert.AreEqual(stamp, result2.ConcurrencyStamp);
        }

    }
}