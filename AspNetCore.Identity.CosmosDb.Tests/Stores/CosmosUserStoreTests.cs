using AspNetCore.Identity.CosmosDb.Tests;
using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.CosmosDb.Stores.Tests
{
    [TestClass()]
    public class CosmosUserStoreTests
    {

        private static TestUtilities? utils;
        private static CosmosUserStore<IdentityUser>? _userStore;
        private static CosmosRoleStore<IdentityRole>? _roleStore;
        private static string phoneNumber = "0000000000";
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
            var role = new IdentityRole(GetNextRandomNumber(1000, 9999).ToString());
            var result = await _roleStore.CreateAsync(role);
            Assert.IsTrue(result.Succeeded);//Confirm success
            role = await _roleStore.FindByIdAsync(role.Id);
            return role;
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityUser"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        private async Task<IdentityUser> GetMockRandomUserAsync()
        {
            var randomEmail = $"{GetNextRandomNumber(1000, 9999)}@{GetNextRandomNumber(10000, 99999)}.com";
            var user = new IdentityUser(randomEmail) { Email = randomEmail, Id = Guid.NewGuid().ToString() };
            var result = await _userStore.CreateAsync(user);
            Assert.IsTrue(result.Succeeded);//Confirm success
            user = await _userStore.FindByNameAsync(user.UserName);
            return user;
        }

        /// <summary>
        /// Gets a mock login info for testing purposes
        /// </summary>
        /// <returns></returns>
        private UserLoginInfo GetMockLoginInfoAsync()
        {
            return new UserLoginInfo("Twitter", Guid.NewGuid().ToString(), "Twitter");
        }

        /// <summary>
        /// Create an IdentityUser test
        /// </summary>
        /// <returns></returns>
        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            // Create a bunch of users in rapid succession
            for (int i = 0; i < 35; i++)
            {
                var r = await GetMockRandomUserAsync();
            }

            // Arrange - setup the new user
            var user = new IdentityUser(TestUtilities.IDENUSER1EMAIL) { Email = TestUtilities.IDENUSER1EMAIL };

            user.Id = TestUtilities.IDENUSER1ID;


            // Act - create the user
            var result = await _userStore.CreateAsync(user);

            // Assert - User should have been created
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Succeeded);

            var user2 = await _userStore.FindByIdAsync(TestUtilities.IDENUSER1ID);

            Assert.IsNotNull(user2);
            Assert.AreEqual(user2.UserName, TestUtilities.IDENUSER1EMAIL);
            Assert.AreEqual(user2.Email, TestUtilities.IDENUSER1EMAIL);
            Assert.AreEqual(user2.NormalizedUserName, TestUtilities.IDENUSER1EMAIL.ToLower());
            Assert.AreEqual(user2.NormalizedEmail, TestUtilities.IDENUSER1EMAIL.ToLower());
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {

            // Arrange - setup the new user
            using var dbContext = utils.GetDbContext();
            var user = await GetMockRandomUserAsync();

            // Act
            var result = await _userStore.DeleteAsync(user);

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(dbContext.Users.Where(a => a.UserName == user.UserName).Count() == 0);            // Assert
        }

        [TestMethod()]
        public async Task FindByEmailAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var user1 = await _userStore.FindByEmailAsync(user.Email);

            // Assert
            Assert.IsNotNull(user1);
            Assert.AreEqual(user.Email, user1.Email);
        }

        [TestMethod()]
        public async Task FindByIdAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var user1 = await _userStore.FindByIdAsync(user.Id);

            // Assert
            Assert.IsNotNull(user1);
            Assert.AreEqual(user.Id, user1.Id);
        }

        [TestMethod()]
        public async Task FindByNameAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var user1 = await _userStore.FindByNameAsync(user.UserName);

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(user.UserName, user1.UserName);
        }

        [TestMethod()]
        public async Task GetEmailAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var result = await _userStore.GetEmailAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result);
        }

        [TestMethod()]
        public async Task GetEmailConfirmedAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var result = await _userStore.GetEmailConfirmedAsync(user);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);

            // Arrange - user name and email are the same with this test
            await _userStore.SetEmailConfirmedAsync(user, true);

            // Act
            result = await _userStore.GetEmailConfirmedAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetEmailConfirmedAsyncTestFail()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var result = await _userStore.GetEmailConfirmedAsync(user);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
            await _userStore.SetEmailConfirmedAsync(user, true);

            // Act
            result = await _userStore.GetEmailConfirmedAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetNormalizedEmailAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var result = await _userStore.GetNormalizedEmailAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.NormalizedEmail, result);
        }

        [TestMethod()]
        public async Task GetNormalizedUserNameAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var result = await _userStore.GetNormalizedUserNameAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.NormalizedUserName, result);
        }

        [TestMethod()]
        public async Task GetPasswordHashAsyncTest()
        {
            // Arrange
            //var userManager = utils.GetUserManager();
            var user = await GetMockRandomUserAsync();
            var hash = await _userStore.GetPasswordHashAsync(user); // Should be no hash now
            Assert.IsTrue(string.IsNullOrEmpty(hash));
            var password = Guid.NewGuid().ToString(); // Now add hash
            await _userStore.SetPasswordHashAsync(user, password);

            // Act
            hash = await _userStore.GetPasswordHashAsync(user);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(hash));
            Assert.AreSame(password, hash); // The hash should be different than original
        }

        [TestMethod()]
        public async Task GetPhoneNumberAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var phoneNumber = "1234567899";
            await _userStore.SetPhoneNumberAsync(user, phoneNumber);
            //user = await userStore.FindByIdAsync(user.Id);

            // Act
            user = await _userStore.FindByIdAsync(user.Id);
            var result2 = await _userStore.GetPhoneNumberAsync(user);

            // Assert
            Assert.AreSame(phoneNumber, result2);
        }

        [TestMethod()]
        public async Task GetPhoneNumberConfirmedAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            await _userStore.SetPhoneNumberAsync(user, phoneNumber);
            //user = await userStore.FindByIdAsync(user.Id);
            await _userStore.SetPhoneNumberConfirmedAsync(user, true);
            //user = await userStore.FindByIdAsync(user.Id);

            // Act
            var result = await _userStore.GetPhoneNumberConfirmedAsync(user);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetUserIdAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var result = await _userStore.GetUserIdAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result);
        }

        [TestMethod()]
        public async Task GetUserNameAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var result = await _userStore.GetUserNameAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.UserName, result);
        }

        [TestMethod()]
        public async Task HasPasswordAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var hash = await _userStore.GetPasswordHashAsync(user); // Should be no hash now
            Assert.IsTrue(string.IsNullOrEmpty(hash));
            var password = Guid.NewGuid().ToString(); // Now add hash

            await _userStore.SetPasswordHashAsync(user, password);

            // Act
            var result1 = await _userStore.HasPasswordAsync(user);

            // Assert
            Assert.IsTrue(result1);
        }

        [TestMethod()]
        public async Task SetEmailAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            await _userStore.SetEmailAsync(user, TestUtilities.IDENUSER2EMAIL);

            // Assert
            var user2 = await _userStore.FindByIdAsync(user.Id);

            Assert.IsNotNull(user2);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL.ToLower(), user2.NormalizedEmail);

            Assert.AreEqual(user.UserName, user2.UserName);
        }

        [TestMethod()]
        public async Task SetEmailConfirmedAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            Assert.IsFalse(user.EmailConfirmed);

            // Act
            await _userStore.SetEmailConfirmedAsync(user, true);

            // Assert
            var result = await _userStore.GetEmailConfirmedAsync(user);
            user = await _userStore.FindByIdAsync(user.Id);
            Assert.IsTrue(user.EmailConfirmed);
            Assert.IsTrue(result);
        }

        // This function is tested with SetEmailAsync().
        [TestMethod()]
        public async Task SetNormalizedEmailAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var newEmail = $"A{GetNextRandomNumber(111, 9999).ToString()}@foo.com";

            // Act
            await _userStore.SetNormalizedEmailAsync(user, newEmail);

            // Assert
            user = await _userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newEmail.ToLower(), user.NormalizedEmail);
        }

        // This method is tested with SetUserNameAsync().
        [TestMethod()]
        public async Task SetNormalizedUserNameAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var newEmail = $"A{GetNextRandomNumber(111, 9999).ToString()}@foo.com";

            // Act
            await _userStore.SetNormalizedUserNameAsync(user, newEmail);

            // Assert
            var user2 = await _userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newEmail.ToLower(), user2.NormalizedUserName);
        }

        [TestMethod()]
        public async Task SetPasswordHashAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            Assert.IsTrue(string.IsNullOrEmpty(user.PasswordHash));

            // Act
            await _userStore.SetPasswordHashAsync(user, Guid.NewGuid().ToString());

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));


        }

        [TestMethod()]
        public async Task SetPhoneNumberAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            Assert.IsTrue(string.IsNullOrEmpty(user.PhoneNumber));

            // Act
            await _userStore.SetPhoneNumberAsync(user, phoneNumber);

            // Assert
            var user2 = await _userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(phoneNumber, user2.PhoneNumber);
        }

        [TestMethod()]
        public async Task SetPhoneNumberConfirmedAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            Assert.IsFalse(user.PhoneNumberConfirmed);

            // Act
            await _userStore.SetPhoneNumberConfirmedAsync(user, true);

            // Assert
            var result = await _userStore.GetPhoneNumberConfirmedAsync(user);
            user = await _userStore.FindByIdAsync(user.Id);
            Assert.IsTrue(user.PhoneNumberConfirmed);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task SetUserNameAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var newUserName = "A" + user.UserName;

            // Act
            await _userStore.SetUserNameAsync(user, newUserName);

            // Assert
            user = await _userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newUserName, user.UserName);
            Assert.AreEqual(newUserName.ToLower(), user.NormalizedUserName);

        }

        // This method tested with SetPasswordHashAsyncTest() | UserManager.AddPasswordAsync()
        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var phoneNumber = "1234567890";

            // Act
            user.Email = TestUtilities.IDENUSER1EMAIL;
            user.NormalizedEmail = TestUtilities.IDENUSER1EMAIL.ToLower();
            user.PhoneNumber = phoneNumber;

            var result = await _userStore.UpdateAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Succeeded);

            var user1 = await _userStore.FindByIdAsync(user.Id);

            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user1.Email);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToLower(), user1.NormalizedEmail);
            Assert.AreEqual(phoneNumber, user1.PhoneNumber);

        }

        [TestMethod()]
        public async Task AddLoginAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();

            // Act
            var loginInfo = GetMockLoginInfoAsync();
            await _userStore.AddLoginAsync(user, loginInfo);

            // Assert
            var logins = await _userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

        }

        [TestMethod()]
        public async Task RemoveLoginAsyncTest()
        {

            // Arrange
            var user = await GetMockRandomUserAsync();
            var loginInfo = GetMockLoginInfoAsync();
            await _userStore.AddLoginAsync(user, loginInfo);
            var logins = await _userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

            // Act
            await _userStore.RemoveLoginAsync(user, "Twitter", loginInfo.ProviderKey);

            // Assert
            logins = await _userStore.GetLoginsAsync(user);
            Assert.AreEqual(0, logins.Count);

        }

        [TestMethod()]
        public async Task GetLoginsAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var loginInfo = GetMockLoginInfoAsync();
            await _userStore.AddLoginAsync(user, loginInfo);

            // Act
            var logins = await _userStore.GetLoginsAsync(user);

            // Assert
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));
        }

        [TestMethod()]
        public async Task FindByLoginAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var loginInfo = GetMockLoginInfoAsync();
            await _userStore.AddLoginAsync(user, loginInfo);
            var logins = await _userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

            // Arrange
            var user2 = await _userStore.FindByLoginAsync("Twitter", loginInfo.ProviderKey);

            // Assert
            Assert.AreEqual(user.Id, user2.Id);
        }

        [TestMethod()]
        public async Task AddToRoleAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var role = await GetMockRandomRoleAsync();
            var users = await _userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users

            // Act
            await _userStore.AddToRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(await _userStore.IsInRoleAsync(user, role.Name));
            users = await _userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(1, users.Count); // Should be one user
            Assert.IsTrue(users.Any(u => u.Id == user.Id));
        }

        [TestMethod()]
        public async Task RemoveFromRoleAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var role = await GetMockRandomRoleAsync();
            var users = await _userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users
            await _userStore.AddToRoleAsync(user, role.Name);
            Assert.IsTrue(await _userStore.IsInRoleAsync(user, role.Name));
            users = await _userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(1, users.Count); // Should be one user
            Assert.IsTrue(users.Any(u => u.Id == user.Id));

            // Act
            await _userStore.RemoveFromRoleAsync(user, role.Name);

            // Assert
            users = await _userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users

        }

        [TestMethod()]
        public async Task GetRolesAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var role1 = await GetMockRandomRoleAsync();
            var role2 = await GetMockRandomRoleAsync();
            var users1 = await _userStore.GetUsersInRoleAsync(role1.Name);
            Assert.AreEqual(0, users1.Count); // Should be no users
            var users2 = await _userStore.GetUsersInRoleAsync(role1.Name);
            Assert.AreEqual(0, users2.Count); // Should be no users

            await _userStore.AddToRoleAsync(user, role1.Name);
            await _userStore.AddToRoleAsync(user, role2.Name);

            Assert.IsTrue(await _userStore.IsInRoleAsync(user, role1.Name));
            Assert.IsTrue(await _userStore.IsInRoleAsync(user, role2.Name));

            // Act
            var roles = await _userStore.GetRolesAsync(user);

            // Assert
            Assert.AreEqual(2, roles.Count); // Should be two
            Assert.IsTrue(roles.Contains(role1.Name));
            Assert.IsTrue(roles.Contains(role2.Name));

        }

        [TestMethod()]
        public async Task IsInRoleAsyncTest()
        {
            // Arrange
            var user = await GetMockRandomUserAsync();
            var role = await GetMockRandomRoleAsync();
            var users = await _userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users
            await _userStore.AddToRoleAsync(user, role.Name);

            // Act
            var result = await _userStore.IsInRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetUsersInRoleAsyncTest()
        {
            // Arrange
            var user1 = await GetMockRandomUserAsync();
            var user2 = await GetMockRandomUserAsync();
            var role = await GetMockRandomRoleAsync();
            await _userStore.AddToRoleAsync(user1, role.Name);
            await _userStore.AddToRoleAsync(user2, role.Name);

            // Act
            var result = await _userStore.GetUsersInRoleAsync(role.Name);

            // Assert
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result.Any(r => r.Id == user1.Id));
            Assert.IsTrue(result.Any(r => r.Id == user2.Id));

        }

    }
}