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
            var role = await GetMockRandomRoleAsync();
            var claim = new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore());

            // Act
            var result1 = await roleManager.AddClaimAsync(role, claim);

            // Assert
            Assert.IsTrue(result1.Succeeded);

        }

        [TestMethod]
        public Task CreateAsyncTest()
        {

        }

        [TestMethod]
        public Task DeleteAsyncTest()
        {

        }
        [TestMethod]
        public Task FindByIdAsyncTest()
        {

        }

        [TestMethod]
        public Task FindByNameAsyncTest()
        {

        }
        [TestMethod]
        public Task GetClaimsAsyncTest()
        {

        }

        [TestMethod]
        public Task GetRoleIdAsyncTest()
        {

        }

        [TestMethod]
        public Task GetRoleNameAsyncTest()
        {

        }

        [TestMethod]
        public Task NormalizeKeyTest()
        {

        }

        [TestMethod]
        public Task RemoveClaimAsyncTest()
        {

        }

        [TestMethod]
        public virtual Task RoleExistsAsyncTest()
        {

        }

        [TestMethod]
        public Task SetRoleNameAsyncTest()
        {

        }

        [TestMethod]
        public virtual Task UpdateAsyncTest()
        {

        }

        [TestMethod]
        public virtual Task UpdateNormalizedRoleNameAsyncTest()
        {

        }

        [TestMethod]
        protected virtual Task UpdateRoleAsyncTest()
        {

        }

        [TestMethod]
        protected Task ValidateRoleAsyncTest()
        {

        }

    }
}