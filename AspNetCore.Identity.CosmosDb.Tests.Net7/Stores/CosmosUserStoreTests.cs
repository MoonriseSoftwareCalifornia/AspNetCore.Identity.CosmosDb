using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.CosmosDb.Tests.Net7.Stores
{
    [TestClass()]
    public class CosmosUserStoreTests : CosmosIdentityTestsBase
    {

        //private static TestUtilities? utils;
        //private static CosmosUserStore<IdentityUser>? userStore;
        //private static CosmosRoleStore<IdentityRole>? _roleStore;
        private static string phoneNumber = "0000000000";
        //private static Random? _random;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            //
            // Setup context.
            //
            InitializeClass();
        }

        /// <summary>
        /// Create an IdentityUser test
        /// </summary>
        /// <returns></returns>
        [TestMethod()]
        public async Task CreateAsyncTest()
        {
            using var userStore = _testUtilities.GetUserStore();
            // Create a bunch of users in rapid succession
            for (int i = 0; i < 35; i++)
            {
                var r = await GetMockRandomUserAsync(userStore);
            }

            // Arrange - setup the new user

            var user = new IdentityUser(TestUtilities.IDENUSER1EMAIL) { Email = TestUtilities.IDENUSER1EMAIL };
            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();

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
            Assert.AreEqual(user2.NormalizedUserName, TestUtilities.IDENUSER1EMAIL.ToUpper());
            Assert.AreEqual(user2.NormalizedEmail, TestUtilities.IDENUSER1EMAIL.ToUpper());
        }

        [TestMethod()]
        public async Task DeleteAsyncTest()
        {
            // Arrange - setup the new user
            using var userStore = _testUtilities.GetUserStore();
            using var roleStore = _testUtilities.GetRoleStore();
            using var dbContext = _testUtilities.GetDbContext();
            var user = await GetMockRandomUserAsync(userStore);
            var userId = user.Id;
            var role = await GetMockRandomRoleAsync(roleStore);
            var claim = GetMockClaim();
            var login = GetMockLoginInfoAsync();
            await userStore.AddClaimsAsync(user, new[] { claim });
            await userStore.AddLoginAsync(user, login);
            await userStore.AddToRoleAsync(user, role.NormalizedName);

            // Act
            var result = await userStore.DeleteAsync(user);

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(dbContext.Users.Where(a => a.Id == userId).Count() == 0);
            Assert.IsTrue(dbContext.UserClaims.Where(a => a.UserId == userId).Count() == 0);
            Assert.IsTrue(dbContext.UserLogins.Where(a => a.UserId == userId).Count() == 0);
            Assert.IsTrue(dbContext.UserRoles.Where(a => a.UserId == userId).Count() == 0);           // Assert
        }

        [TestMethod()]
        public async Task FindByEmailAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var user1 = await userStore.FindByEmailAsync(user.Email.ToUpper());

            // Assert
            Assert.IsNotNull(user1);
            Assert.AreEqual(user.Email, user1.Email);
        }

        [TestMethod()]
        public async Task FindByIdAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var user1 = await userStore.FindByIdAsync(user.Id);

            // Assert
            Assert.IsNotNull(user1);
            Assert.AreEqual(user.Id, user1.Id);
        }

        [TestMethod()]
        public async Task FindByNameAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var user1 = await userStore.FindByNameAsync(user.UserName.ToUpper());

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(user.UserName, user1.UserName);
        }

        [TestMethod()]
        public async Task GetEmailAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetEmailAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result);
        }

        [TestMethod()]
        public async Task GetEmailConfirmedAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var result = await userStore.GetEmailConfirmedAsync(user);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);

            // Arrange - user name and email are the same with this test
            await userStore.SetEmailConfirmedAsync(user, true);

            // Act
            result = await userStore.GetEmailConfirmedAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetEmailConfirmedAsyncTestFail()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var result = await userStore.GetEmailConfirmedAsync(user);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
            await userStore.SetEmailConfirmedAsync(user, true);

            // Act
            result = await userStore.GetEmailConfirmedAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetNormalizedEmailAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetNormalizedEmailAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.NormalizedEmail, result);
        }

        [TestMethod()]
        public async Task GetNormalizedUserNameAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetNormalizedUserNameAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.NormalizedUserName, result);
        }

        [TestMethod()]
        public async Task GetPasswordHashAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var hash = await userStore.GetPasswordHashAsync(user); // Should be no hash now
            Assert.IsTrue(string.IsNullOrEmpty(hash));
            var password = Guid.NewGuid().ToString(); // Now add hash
            await userStore.SetPasswordHashAsync(user, password);

            // Act
            hash = await userStore.GetPasswordHashAsync(user);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(hash));
            Assert.AreSame(password, hash); // The hash should be different than original
        }

        [TestMethod()]
        public async Task GetPhoneNumberAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var phoneNumber = "1234567899";
            await userStore.SetPhoneNumberAsync(user, phoneNumber);
            //user = await userStore.FindByIdAsync(user.Id);

            // Act
            user = await userStore.FindByIdAsync(user.Id);
            var result2 = await userStore.GetPhoneNumberAsync(user);

            // Assert
            Assert.AreSame(phoneNumber, result2);
        }

        [TestMethod()]
        public async Task GetPhoneNumberConfirmedAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            await userStore.SetPhoneNumberAsync(user, phoneNumber);
            //user = await userStore.FindByIdAsync(user.Id);
            await userStore.SetPhoneNumberConfirmedAsync(user, true);
            //user = await userStore.FindByIdAsync(user.Id);

            // Act
            var result = await userStore.GetPhoneNumberConfirmedAsync(user);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetUserIdAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetUserIdAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result);
        }

        [TestMethod()]
        public async Task GetUserNameAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetUserNameAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.UserName, result);
        }

        [TestMethod()]
        public async Task HasPasswordAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var hash = await userStore.GetPasswordHashAsync(user); // Should be no hash now
            Assert.IsTrue(string.IsNullOrEmpty(hash));
            var password = Guid.NewGuid().ToString(); // Now add hash

            await userStore.SetPasswordHashAsync(user, password);

            // Act
            var result1 = await userStore.HasPasswordAsync(user);

            // Assert
            Assert.IsTrue(result1);
        }

        [TestMethod()]
        public async Task SetEmailAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            await userStore.SetEmailAsync(user, TestUtilities.IDENUSER2EMAIL);

            // Assert
            var user2 = await userStore.FindByIdAsync(user.Id);

            Assert.IsNotNull(user2);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);

            Assert.AreEqual(user.UserName, user2.UserName);
        }

        [TestMethod()]
        public async Task SetEmailConfirmedAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            Assert.IsFalse(user.EmailConfirmed);

            // Act
            await userStore.SetEmailConfirmedAsync(user, true);

            // Assert
            var result = await userStore.GetEmailConfirmedAsync(user);
            user = await userStore.FindByIdAsync(user.Id);
            Assert.IsTrue(user.EmailConfirmed);
            Assert.IsTrue(result);
        }

        // This function is tested with SetEmailAsync().
        [TestMethod()]
        public async Task SetNormalizedEmailAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var newEmail = $"A{GetNextRandomNumber(111, 9999).ToString()}@foo.com";

            // Act
            await userStore.SetNormalizedEmailAsync(user, newEmail.ToUpper());

            // Assert
            user = await userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newEmail.ToUpper(), user.NormalizedEmail);
        }

        // This method is tested with SetUserNameAsync().
        [TestMethod()]
        public async Task SetNormalizedUserNameAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var newEmail = $"A{GetNextRandomNumber(111, 9999).ToString()}@foo.com";

            // Act
            await userStore.SetNormalizedUserNameAsync(user, newEmail.ToUpper());

            // Assert
            var user2 = await userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newEmail.ToUpper(), user2.NormalizedUserName);
        }

        [TestMethod()]
        public async Task SetPasswordHashAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            Assert.IsTrue(string.IsNullOrEmpty(user.PasswordHash));

            // Act
            await userStore.SetPasswordHashAsync(user, Guid.NewGuid().ToString());

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));


        }

        [TestMethod()]
        public async Task SetPhoneNumberAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            Assert.IsTrue(string.IsNullOrEmpty(user.PhoneNumber));

            // Act
            await userStore.SetPhoneNumberAsync(user, phoneNumber);

            // Assert
            var user2 = await userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(phoneNumber, user2.PhoneNumber);
        }

        [TestMethod()]
        public async Task SetPhoneNumberConfirmedAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            Assert.IsFalse(user.PhoneNumberConfirmed);

            // Act
            await userStore.SetPhoneNumberConfirmedAsync(user, true);

            // Assert
            var result = await userStore.GetPhoneNumberConfirmedAsync(user);
            user = await userStore.FindByIdAsync(user.Id);
            Assert.IsTrue(user.PhoneNumberConfirmed);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task SetUserNameAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var newUserName = "A" + user.UserName;

            // Act
            await userStore.SetUserNameAsync(user, newUserName);

            // Assert
            user = await userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newUserName, user.UserName);

        }

        // This method tested with SetPasswordHashAsyncTest() | UserManager.AddPasswordAsync()
        [TestMethod()]
        public async Task UpdateAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var phoneNumber = "1234567890";

            // Act
            user.Email = TestUtilities.IDENUSER1EMAIL;
            user.NormalizedEmail = TestUtilities.IDENUSER1EMAIL.ToUpper();
            user.PhoneNumber = phoneNumber;

            var result = await userStore.UpdateAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Succeeded);

            var user1 = await userStore.FindByIdAsync(user.Id);

            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user1.Email);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToUpper(), user1.NormalizedEmail);
            Assert.AreEqual(phoneNumber, user1.PhoneNumber);

        }

        [TestMethod()]
        public async Task AddLoginAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);

            // Assert
            var logins = await userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

        }

        [TestMethod()]
        public async Task RemoveLoginAsyncTest()
        {

            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);
            var logins = await userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

            // Act
            await userStore.RemoveLoginAsync(user, "Twitter", loginInfo.ProviderKey);

            // Assert
            logins = await userStore.GetLoginsAsync(user);
            Assert.AreEqual(0, logins.Count);

        }

        [TestMethod()]
        public async Task GetLoginsAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);

            // Act
            var logins = await userStore.GetLoginsAsync(user);

            // Assert
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));
        }

        [TestMethod()]
        public async Task FindByLoginAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);
            var logins = await userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

            // Arrange
            var user2 = await userStore.FindByLoginAsync("Twitter", loginInfo.ProviderKey);

            // Assert
            Assert.AreEqual(user.Id, user2.Id);
        }

        [TestMethod()]
        public async Task AddToRoleAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            using var roleStore = _testUtilities.GetRoleStore();
            var user = await GetMockRandomUserAsync(userStore);
            var role = await GetMockRandomRoleAsync(roleStore);
            var users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users

            // Act
            await userStore.AddToRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(await userStore.IsInRoleAsync(user, role.Name));
            users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(1, users.Count); // Should be one user
            Assert.IsTrue(users.Any(u => u.Id == user.Id));
        }

        [TestMethod()]
        public async Task RemoveFromRoleAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            using var roleStore = _testUtilities.GetRoleStore();
            var user = await GetMockRandomUserAsync(userStore);
            var role = await GetMockRandomRoleAsync(roleStore);
            var users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users
            await userStore.AddToRoleAsync(user, role.Name);
            Assert.IsTrue(await userStore.IsInRoleAsync(user, role.Name));
            users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(1, users.Count); // Should be one user
            Assert.IsTrue(users.Any(u => u.Id == user.Id));

            // Act
            await userStore.RemoveFromRoleAsync(user, role.Name);

            // Assert
            users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users

        }

        [TestMethod()]
        public async Task GetRolesAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            using var roleStore = _testUtilities.GetRoleStore();
            var user = await GetMockRandomUserAsync(userStore);
            var role1 = await GetMockRandomRoleAsync(roleStore);
            var role2 = await GetMockRandomRoleAsync(roleStore);
            var users1 = await userStore.GetUsersInRoleAsync(role1.Name);
            Assert.AreEqual(0, users1.Count); // Should be no users
            var users2 = await userStore.GetUsersInRoleAsync(role1.Name);
            Assert.AreEqual(0, users2.Count); // Should be no users

            await userStore.AddToRoleAsync(user, role1.Name);
            await userStore.AddToRoleAsync(user, role2.Name);

            Assert.IsTrue(await userStore.IsInRoleAsync(user, role1.Name));
            Assert.IsTrue(await userStore.IsInRoleAsync(user, role2.Name));

            // Act
            var roles = await userStore.GetRolesAsync(user);

            // Assert
            Assert.AreEqual(2, roles.Count); // Should be two
            Assert.IsTrue(roles.Contains(role1.Name));
            Assert.IsTrue(roles.Contains(role2.Name));

        }

        [TestMethod()]
        public async Task IsInRoleAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            using var roleStore = _testUtilities.GetRoleStore();
            var user = await GetMockRandomUserAsync(userStore);
            var role = await GetMockRandomRoleAsync(roleStore);
            var users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users
            await userStore.AddToRoleAsync(user, role.Name);

            // Act
            var result = await userStore.IsInRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        public async Task GetUsersInRoleAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            using var roleStore = _testUtilities.GetRoleStore();
            var user1 = await GetMockRandomUserAsync(userStore);
            var user2 = await GetMockRandomUserAsync(userStore);
            var role = await GetMockRandomRoleAsync(roleStore);
            await userStore.AddToRoleAsync(user1, role.Name);
            await userStore.AddToRoleAsync(user2, role.Name);

            // Act
            var result = await userStore.GetUsersInRoleAsync(role.Name);

            // Assert
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result.Any(r => r.Id == user1.Id));
            Assert.IsTrue(result.Any(r => r.Id == user2.Id));

        }

        [TestMethod]
        public async Task QueryUsersTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user1 = await GetMockRandomUserAsync(userStore);

            // Act
            var result = userStore.Users.ToList();

            // Assert
            Assert.IsInstanceOfType(userStore.Users, typeof(IQueryable<IdentityUser>));
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod()]
        public async Task SetAndGetAuthenticatorKeyAsyncTest()
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore();
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);
            await userStore.SetAuthenticatorKeyAsync(user, "AuthenticatorKey", default);
            var code = await userStore.GetAuthenticatorKeyAsync(user, default);

            // Assert
            Assert.IsNotNull(code);
        }
    }
}