using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Stores;
using PieroDeTomi.EntityFrameworkCore.Identity.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Stores.Tests
{
    [TestClass()]
    public class CosmosUserStoreTests
    {

        private static TestUtilities utils;
        private static CosmosUserStore<IdentityUser> userStore;
        private static string phoneNumber = "0000000000";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            //
            // Setup context.
            //
            utils = new TestUtilities();
            userStore = utils.GetUserStore();
        }

        /// <summary>
        /// Create a new User Store test
        /// </summary>
        [TestMethod()]
        public async Task CosmosUserStoreTest()
        {
            Assert.IsNotNull(userStore);
        }

        /// <summary>
        /// Create an IdentityUser test
        /// </summary>
        /// <returns></returns>
        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            // Arrange - remove all prior users and create new user
            using var dbContext = utils.GetDbContext();
            dbContext.UserRoles.RemoveRange(dbContext.UserRoles.ToList());
            dbContext.Roles.RemoveRange(dbContext.Roles.ToList());
            dbContext.UserLogins.RemoveRange(dbContext.UserLogins.ToList());
            dbContext.Users.RemoveRange(dbContext.Users.ToList());
            var result1 = await dbContext.SaveChangesAsync();

            // Arrange - setup the new user
            var user = new IdentityUser(TestUtilities.IDENUSER1EMAIL) {  Email = TestUtilities.IDENUSER1EMAIL };

            user.Id = TestUtilities.IDENUSER1ID;


            // Act - create the user
            var result = await userStore.CreateAsync(user);

            // Assert - User should have been created
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Succeeded);

            var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

            Assert.IsNotNull(user2);
            Assert.AreEqual(user2.UserName, TestUtilities.IDENUSER1EMAIL);
            Assert.AreEqual(user2.Email, TestUtilities.IDENUSER1EMAIL);
            Assert.AreEqual(user2.NormalizedUserName, TestUtilities.IDENUSER1EMAIL.ToLower());
            Assert.AreEqual(user2.NormalizedEmail, TestUtilities.IDENUSER1EMAIL.ToLower());
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public async Task FindByEmailAsyncTest()
        {
            // Act
            var user = await userStore.FindByEmailAsync(TestUtilities.IDENUSER1EMAIL);
            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user.Email);
        }

        [TestMethod()]
        public async Task FindByIdAsyncTest()
        {
            // Act
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user.Email);
        }

        [TestMethod()]
        public async Task FindByNameAsyncTest()
        {
            // Act - username and email are the same with this test
            var user = await userStore.FindByNameAsync(TestUtilities.IDENUSER1EMAIL);
            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user.Email);
        }

        [TestMethod()]
        public async Task GetEmailAsyncTest()
        {
            // Arrange - username and email are the same with this test
            var user = await userStore.FindByNameAsync(TestUtilities.IDENUSER1EMAIL);

            // Act
            var result = await userStore.GetEmailAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, result);
        }

        [TestMethod()]
        public async Task GetEmailConfirmedAsyncTest()
        {
            // Arrange - username and email are the same with this test
            var user = await userStore.FindByNameAsync(TestUtilities.IDENUSER1EMAIL);

            // Act
            var result = await userStore.GetEmailConfirmedAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetEmailConfirmedAsyncTestFail()
        {
            // Arrange - username and email are the same with this test
            var user = await userStore.FindByNameAsync(TestUtilities.IDENUSER1EMAIL);

            // Act
            var result = await userStore.GetEmailConfirmedAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public async Task GetNormalizedEmailAsyncTest()
        {

            // Arrange - username and email are the same with this test
            var user = await userStore.FindByNameAsync(TestUtilities.IDENUSER1EMAIL);

            // Act
            var result = await userStore.GetNormalizedEmailAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), result);
        }

        [TestMethod()]
        public async Task GetNormalizedUserNameAsyncTest()
        {

            // Arrange - username and email are the same with this test
            var user = await userStore.FindByNameAsync(TestUtilities.IDENUSER1EMAIL);

            // Act
            var result = await userStore.GetNormalizedUserNameAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), result);
        }

        [TestMethod()]
        public async Task GetPasswordHashAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public async Task GetPhoneNumberAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public async Task GetPhoneNumberConfirmedAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public async Task GetUserIdAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public async Task GetUserNameAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public async Task HasPasswordAsyncTestFail()
        {
            // Act - username and email are the same with this test
            var user = await userStore.FindByNameAsync(TestUtilities.IDENUSER1EMAIL);
            var result = await userStore.HasPasswordAsync(user);
            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod()]
        public async Task HasPasswordAsyncTest()
        {
            // Act - username and email are the same with this test
            var user = await userStore.FindByNameAsync(TestUtilities.IDENUSER1EMAIL);
            var result = await userStore.HasPasswordAsync(user);
            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task SetEmailAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

            // Act
            await userStore.SetEmailAsync(user, TestUtilities.IDENUSER2EMAIL);

            // Assert
            var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

            Assert.IsNotNull(user2);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);

            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user2.UserName);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user2.NormalizedUserName);
        }

        [TestMethod()]
        public async Task SetEmailConfirmedAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.IsFalse(user.EmailConfirmed);

            // Act
            await userStore.SetEmailConfirmedAsync(user, true);

            // Assert
            var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.IsTrue(user2.EmailConfirmed);

            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);

            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user2.UserName);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user2.NormalizedUserName);
        }

        // This function is tested with SetEmailAsync().
        //[TestMethod()]
        //public async Task SetNormalizedEmailAsyncTest()
        //{
        //    // Arrange
        //    var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
        //    Assert.IsTrue(string.IsNullOrEmpty(user.NormalizedEmail));

        //    // Act
        //    await userStore.SetNormalizedEmailAsync(user, TestUtilities.IDENUSER2EMAIL);

        //    // Assert
        //    var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
        //    Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);
        //}

        // This method is tested with SetUserNameAsync().
        //[TestMethod()]
        //public async Task SetNormalizedUserNameAsyncTest()
        //{
        //    // Arrange
        //    var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
        //    Assert.IsTrue(string.IsNullOrEmpty(user.NormalizedUserName));

        //    // Act
        //    await userStore.SetNormalizedUserNameAsync(user, TestUtilities.IDENUSER1EMAIL);

        //    // Assert
        //    var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
        //    Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user2.NormalizedUserName);
        //}

        [TestMethod()]
        public async Task SetPasswordHashAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.IsTrue(string.IsNullOrEmpty(user.PasswordHash));
            var userManger = utils.GetUserManager();

            // Act -- use the User Manager's built-in hasher.
            await userManger.AddPasswordAsync(user, Guid.NewGuid().ToString());

            // Assert
            var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));

            Assert.IsTrue(user2.EmailConfirmed);

            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);

            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user2.UserName);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user2.NormalizedUserName);

        }

        [TestMethod()]
        public async Task SetPhoneNumberAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.IsTrue(string.IsNullOrEmpty(user.PhoneNumber));
            

            // Act
            await userStore.SetPhoneNumberAsync(user, phoneNumber);

            // Assert
            var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.AreEqual(phoneNumber, user2.PhoneNumber);

            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));

            Assert.IsTrue(user2.EmailConfirmed);

            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);

            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user2.UserName);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user2.NormalizedUserName);
        }

        [TestMethod()]
        public async Task SetPhoneNumberConfirmedAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.IsFalse(user.PhoneNumberConfirmed);

            // Act
            await userStore.SetPhoneNumberConfirmedAsync(user, true);

            // Assert
            var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.IsTrue(user2.PhoneNumberConfirmed);

            Assert.AreEqual(phoneNumber, user2.PhoneNumber);

            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));

            Assert.IsTrue(user2.EmailConfirmed);

            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);

            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user2.UserName);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user2.NormalizedUserName);
        }

        [TestMethod()]
        public async Task SetUserNameAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user.UserName);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user.NormalizedUserName);

            // Act
            await userStore.SetUserNameAsync(user, TestUtilities.IDENUSER2EMAIL);

            // Assert
            var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.UserName);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedUserName);

            Assert.IsTrue(user2.PhoneNumberConfirmed);

            Assert.AreEqual(phoneNumber, user2.PhoneNumber);

            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));

            Assert.IsTrue(user2.EmailConfirmed);

            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);

            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);

        }

        // This method tested with SetPasswordHashAsyncTest() | UserManager.AddPasswordAsync()
        //[TestMethod()]
        //public async Task UpdateAsyncTest()
        //{
        //    // Arrange
        //    var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        //    // Act
        //    var result = await userStore.UpdateAsync(user);

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.Succeeded);

        //    var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
        //    Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user2.UserName);
        //    Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user2.NormalizedUserName);
        //    Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user2.Email);
        //    Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user2.NormalizedEmail);

        //}

        // X1_ added to place test at end of UserStoreWriteTests.playlist
        [TestMethod()]
        public async Task X1_AddLoginAsyncTest()
        {
            // Arrange - username and email are the same with this test
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);
            // Act
            var loginInfo = new UserLoginInfo("Twitter", "123456", "Twitter");
            var userManager = utils.GetUserManager();
            var result = await userManager.AddLoginAsync(user, loginInfo);

            // Assert
            Assert.IsTrue(result.Succeeded);
            var user2 = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID); // Refresh user info
            var logins = await userStore.GetLoginsAsync(user2);
            Assert.AreEqual(1, logins.Count);
        }

        [TestMethod()]
        public async Task RemoveLoginAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        }

        [TestMethod()]
        public async Task GetLoginsAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        }

        [TestMethod()]
        public async Task FindByLoginAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        }

        [TestMethod()]
        public async Task AddToRoleAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        }

        [TestMethod()]
        public async Task RemoveFromRoleAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        }

        [TestMethod()]
        public async Task GetRolesAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        }

        [TestMethod()]
        public async Task IsInRoleAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        }

        [TestMethod()]
        public async Task GetUsersInRoleAsyncTest()
        {
            // Arrange
            var user = await userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

        }

        [TestMethod()]
        public void DisposeTest()
        {
            Assert.Fail();
        }
    }
}