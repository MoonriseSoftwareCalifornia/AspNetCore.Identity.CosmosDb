using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDb.Tests
{
    [TestClass]
    public class UserManagerTests : CosmosIdentityTestsBase
    {
        // Creates a new test user with a hashed password, using the mock UserManager to do so
        private async Task<IdentityUser> GetTestUser(UserManager<IdentityUser> userManager, string password = "")
        {
            var user = await GetMockRandomUserAsync(false);

            if (string.IsNullOrEmpty(password))
                password = $"A1a{Guid.NewGuid()}";

            var result = await userManager.CreateAsync(user, password);

            Assert.IsTrue(result.Succeeded);
            return await userManager.FindByIdAsync(user.Id);
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            InitializeClass();
        }

        [TestMethod]
        public async Task GetUserNameTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.GetUserNameAsync(user);
        }

        [TestMethod]
        public async Task GetUserIdTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            var result2 = await userManager.GetUserIdAsync(user);
        }

        //[TestMethod]
        //public async Task GetUserAsync_FromClaim_Test()
        //{
        //}

        [TestMethod]
        public async Task CreateAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetMockRandomUserAsync(false);

            // Act
            var result = await userManager.CreateAsync(user);

            // Assert
            var result2 = await userManager.FindByIdAsync(user.Id);
            Assert.IsTrue(user.Id == result2.Id);
        }

        [TestMethod]
        public async Task UpdateAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            user.PhoneNumber = "9998884444";

            // Act
            var result1 = await userManager.UpdateAsync(user);

            // Assert
            user = await userManager.FindByIdAsync(user.Id);
            Assert.AreEqual("9998884444", user.PhoneNumber);

        }

        [TestMethod]
        public async Task DeleteAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var id = user.Id;
            user = await userManager.FindByIdAsync(id);
            Assert.IsNotNull(user);

            // Act
            var result = await userManager.DeleteAsync(user);

            // Assert
            Assert.IsTrue(result.Succeeded);
            user = await userManager.FindByIdAsync(id);
            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task FindByIdAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            user = await userManager.FindByIdAsync(user.Id);

            // Assert
            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task FindByNameAsync()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            user = await userManager.FindByNameAsync(user.UserName);

            // Assert
            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task CreateAsync_WithPassword_Test()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);

            // Act
            var user = await GetTestUser(userManager);

            // Assert
            var result = await userManager.HasPasswordAsync(user);
            Assert.IsNotNull(user);
            Assert.IsTrue(result);
            Assert.IsTrue(!string.IsNullOrEmpty(user.PasswordHash));
        }

        [TestMethod]
        public async Task UpdateNormalizedUserNameAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var userName = "Az" + user.UserName;
            user.UserName = userName;

            // Act
            await userManager.UpdateNormalizedUserNameAsync(user);

            // Assert
            user = await userManager.FindByIdAsync(user.Id);
            Assert.IsTrue(user.NormalizedUserName == userName.ToUpperInvariant());
        }

        [TestMethod]
        public async Task GetUserNameAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            var userName = await userManager.GetUserNameAsync(user);

            // Assert
            Assert.AreEqual(user.UserName, userName);
        }

        [TestMethod]
        public async Task SetUserNameAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var userName = "Az" + user.UserName;

            // Act
            await userManager.SetUserNameAsync(user, userName);

            // Assert
            user = await userManager.FindByIdAsync(user.Id);
            Assert.IsTrue(user.UserName == userName);
        }

        [TestMethod]
        public async Task GetUserIdAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.GetUserIdAsync(user);

            // Assert
            Assert.AreEqual(user.Id, result);
        }

        [TestMethod]
        public async Task CheckPasswordAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var originalPassword = $"A1a{Guid.NewGuid()}";
            var user = await GetTestUser(userManager, originalPassword);

            // Act - fail
            var result = await userManager.ChangePasswordAsync(user, originalPassword, Guid.NewGuid().ToString());

            // Assert - fail
            Assert.IsFalse(result.Succeeded);

            // Act - succeed
            result = await userManager.ChangePasswordAsync(user, originalPassword, $"A1a{Guid.NewGuid()}");

            // Assert - succeed
            Assert.IsTrue(result.Succeeded);
        }

        [TestMethod]
        public async Task HasPasswordAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.HasPasswordAsync(user);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task AddPasswordAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.AddPasswordAsync(user, $"A1a{Guid.NewGuid()}");

            // Assert
            Assert.IsTrue(result.Succeeded);
        }

        [TestMethod]
        public async Task ChangePasswordAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);
            var originalPassword = $"A1a{Guid.NewGuid()}";
            var user = await GetTestUser(userManager, originalPassword);

            // Act
            var result = await userManager.ChangePasswordAsync(user, originalPassword, $"A1a{Guid.NewGuid()}");

            // Assert
            Assert.IsTrue(result.Succeeded);
        }

        [TestMethod]
        public async Task RemovePasswordAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            Assert.IsTrue(await userManager.HasPasswordAsync(user));

            // Act
            var result = await userManager.RemovePasswordAsync(user);

            // Assert
            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(await userManager.HasPasswordAsync(user));
        }

        [TestMethod]
        public async Task GetSecurityStampAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.GetSecurityStampAsync(user);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.AreEqual(result, user.SecurityStamp);
        }

        [TestMethod]
        public async Task UpdateSecurityStampAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var stamp1 = user.SecurityStamp;

            // Act
            var result = await userManager.UpdateSecurityStampAsync(user);

            // Assert
            user = await userManager.FindByIdAsync(user.Id);
            Assert.IsTrue(result.Succeeded);
            Assert.AreNotEqual(stamp1, user.SecurityStamp);
        }

        [TestMethod]
        public async Task GeneratePasswordResetTokenAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.GeneratePasswordResetTokenAsync(user);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public async Task ResetPasswordAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var password = $"A1a{Guid.NewGuid()}";

            // Act
            var result = await userManager.ResetPasswordAsync(user, token, password);

            // Assert
            Assert.IsTrue(result.Succeeded);
        }

        [TestMethod]
        public async Task FindByLoginAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var loginInfo = GetMockLoginInfoAsync();
            await userManager.AddLoginAsync(user, loginInfo);
            var logins = await userManager.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

            // Act
            var user2 = await userManager.FindByLoginAsync("Twitter", loginInfo.ProviderKey);

            // Assert
            Assert.AreEqual(user.Id, user2.Id);

        }

        [TestMethod]
        public async Task RemoveLoginAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var loginInfo = GetMockLoginInfoAsync();
            await userManager.AddLoginAsync(user, loginInfo);
            var logins = await userManager.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));
            var user2 = await userManager.FindByLoginAsync("Twitter", loginInfo.ProviderKey);
            Assert.AreEqual(user.Id, user2.Id);

            // Act
            var result = await userManager.RemoveLoginAsync(user, "Twitter", loginInfo.ProviderKey);

            // Assert
            Assert.IsTrue(result.Succeeded);
            logins = await userManager.GetLoginsAsync(user);
            Assert.AreEqual(0, logins.Count);
        }

        [TestMethod]
        public async Task AddLoginAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var loginInfo = GetMockLoginInfoAsync();

            // Act
            await userManager.AddLoginAsync(user, loginInfo);

            // Assert
            var logins = await userManager.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));
        }

        [TestMethod]
        public async Task GetLoginsAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var loginInfo = GetMockLoginInfoAsync();
            await userManager.AddLoginAsync(user, loginInfo);

            // Act
            var logins = await userManager.GetLoginsAsync(user);

            // Assert
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));
        }

        [TestMethod]
        public async Task AddClaimAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var claim = new Claim("1", "1");

            // Act
            var result = await userManager.AddClaimAsync(user, claim);

            // Assert
            Assert.IsTrue(result.Succeeded);
            var result2 = (await userManager.GetClaimsAsync(user));
            Assert.AreEqual(2, result2.Count);
        }

        [TestMethod]
        public async Task AddClaimsAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var claims = new Claim[] { new Claim("1", "1"), new Claim("2", "2"), new Claim("3", "3") };

            // Act
            var result = await userManager.AddClaimsAsync(user, claims);

            // Assert
            Assert.IsTrue(result.Succeeded);
            claims = (await userManager.GetClaimsAsync(user)).ToArray();
            Assert.AreEqual(3, claims.Count());
        }

        [TestMethod]
        public async Task ReplaceClaimAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var claim = new Claim("1", "1");
            var newClaim = new Claim("1", "2");
            var result1 = await userManager.AddClaimAsync(user, claim);
            Assert.IsTrue(result1.Succeeded);

            // Act
            var result2 = await userManager.ReplaceClaimAsync(user, claim, newClaim);

            // Assert
            Assert.IsTrue(result2.Succeeded);
            var result3 = await userManager.GetClaimsAsync(user);
            Assert.AreEqual(1, result3.Count);
        }

        [TestMethod]
        public async Task RemoveClaimAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var claim = new Claim("1", "1");
            var result1 = await userManager.AddClaimAsync(user, claim);
            Assert.IsTrue(result1.Succeeded);

            // Act
            var result2 = await userManager.RemoveClaimAsync(user, claim);

            // Assert
            Assert.IsTrue(result2.Succeeded);
            var result3 = await userManager.GetClaimsAsync(user);
            Assert.AreEqual(0, result3.Count);
        }

        [TestMethod]
        public async Task RemoveClaimsAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var claims = new Claim[] { new Claim("1", "1"), new Claim("2", "2"), new Claim("3", "3") };
            var result1 = await userManager.AddClaimsAsync(user, claims);
            Assert.IsTrue(result1.Succeeded);

            // Act
            var result2 = await userManager.RemoveClaimsAsync(user, claims);

            // Assert
            Assert.IsTrue(result2.Succeeded);
            claims = (await userManager.GetClaimsAsync(user)).ToArray();
            Assert.AreEqual(0, claims.Count());
        }

        [TestMethod]
        public async Task GetClaimsAsyncTest()
        {
            // Arrange
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var claims = new Claim[] { new Claim("1", "1"), new Claim("2", "2"), new Claim("3", "3") };
            var result1 = await userManager.AddClaimsAsync(user, claims);
            Assert.IsTrue(result1.Succeeded);

            // Act
            var result2 = await userManager.GetClaimsAsync(user);

            // Assert
            Assert.AreEqual(3, result2.Count);


        }

        [TestMethod]
        public async Task AddToRoleAsyncTest()
        {
            // Arrange
            using var roleManager = GetTestRoleManager(_roleStore);
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var role = await GetMockRandomRoleAsync(false);
            var result1 = await roleManager.CreateAsync(role);
            Assert.IsTrue(result1.Succeeded);

            // Act
            var result2 = await userManager.AddToRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(result2.Succeeded);
            var result3 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(1, result3.Count);
        }

        [TestMethod]
        public async Task AddToRolesAsyncTest()
        {
            // Arrange
            using var roleManager = GetTestRoleManager(_roleStore);
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var role1 = await GetMockRandomRoleAsync(false);
            var role2 = await GetMockRandomRoleAsync(false);
            var role3 = await GetMockRandomRoleAsync(false);
            var result1 = await roleManager.CreateAsync(role1);
            Assert.IsTrue(result1.Succeeded);
            var result2 = await roleManager.CreateAsync(role2);
            Assert.IsTrue(result2.Succeeded);
            var result3 = await roleManager.CreateAsync(role3);
            Assert.IsTrue(result3.Succeeded);
            var roles = new string[] { role1.Name, role2.Name, role3.Name };

            // Act
            var result4 = await userManager.AddToRolesAsync(user, roles);

            // Assert
            Assert.IsTrue(result2.Succeeded);
            var result5 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(3, result5.Count);
        }

        [TestMethod]
        public async Task RemoveFromRoleAsyncTest()
        {
            // Arrange
            using var roleManager = GetTestRoleManager(_roleStore);
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var role = await GetMockRandomRoleAsync(false);
            var result1 = await roleManager.CreateAsync(role);
            Assert.IsTrue(result1.Succeeded);

            // Act
            var result2 = await userManager.RemoveFromRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(result2.Succeeded);
            var result3 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(0, result3.Count);
        }

        [TestMethod]
        public async Task RemoveFromRolesAsyncTest()
        {

            // Arrange
            using var roleManager = GetTestRoleManager(_roleStore);
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var role1 = await GetMockRandomRoleAsync(false);
            var role2 = await GetMockRandomRoleAsync(false);
            var role3 = await GetMockRandomRoleAsync(false);
            var result1 = await roleManager.CreateAsync(role1);
            Assert.IsTrue(result1.Succeeded);
            var result2 = await roleManager.CreateAsync(role2);
            Assert.IsTrue(result2.Succeeded);
            var result3 = await roleManager.CreateAsync(role3);
            Assert.IsTrue(result3.Succeeded);
            var roles = new string[] { role1.Name, role2.Name, role3.Name };

            // Act
            var result5 = await userManager.RemoveFromRolesAsync(user, roles);

            // Assert
            Assert.IsTrue(result2.Succeeded);
            var result6 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(0, result6.Count);
        }

        [TestMethod]
        public async Task GetRolesAsyncTest()
        {

            // Arrange
            using var roleManager = GetTestRoleManager(_roleStore);
            using var userManager = GetTestUserManager(_userStore);
            var user = await GetTestUser(userManager);
            var role1 = await GetMockRandomRoleAsync(false);
            var role2 = await GetMockRandomRoleAsync(false);
            var role3 = await GetMockRandomRoleAsync(false);
            var result1 = await roleManager.CreateAsync(role1);
            Assert.IsTrue(result1.Succeeded);
            var result2 = await roleManager.CreateAsync(role2);
            Assert.IsTrue(result2.Succeeded);
            var result3 = await roleManager.CreateAsync(role3);
            Assert.IsTrue(result3.Succeeded);
            var roles = new string[] { role1.Name, role2.Name, role3.Name };

            // Act
            var result5 = await userManager.GetRolesAsync(user);

            // Assert
            Assert.AreEqual(3, result5.Count);
        }

        [TestMethod]
        public async Task IsInRoleAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetEmailAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task SetEmailAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task FindByEmailAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task UpdateNormalizedEmailAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GenerateEmailConfirmationTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ConfirmEmailAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task IsEmailConfirmedAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GenerateChangeEmailTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ChangeEmailAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetPhoneNumberAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task SetPhoneNumberAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ChangePhoneNumberAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task IsPhoneNumberConfirmedAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GenerateChangePhoneNumberTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task VerifyChangePhoneNumberTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task VerifyUserTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GenerateUserTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RegisterTokenProviderTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetValidTwoFactorProvidersAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task VerifyTwoFactorTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GenerateTwoFactorTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetTwoFactorEnabledAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task SetTwoFactorEnabledAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task IsLockedOutAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task SetLockoutEnabledAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetLockoutEnabledAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetLockoutEndDateAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task SetLockoutEndDateAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task AccessFailedAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ResetAccessFailedCountAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetAccessFailedCountAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetUsersForClaimAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetUsersInRoleAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetAuthenticationTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task SetAuthenticationTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RemoveAuthenticationTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetAuthenticatorKeyAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ResetAuthenticatorKeyAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GenerateNewAuthenticatorKeyTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GenerateNewTwoFactorRecoveryCodesAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RedeemTwoFactorRecoveryCodeAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task CountRecoveryCodesAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ValidateUserAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ValidatePasswordAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task UpdateUserAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

    }
}
