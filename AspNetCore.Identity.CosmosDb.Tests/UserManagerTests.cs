using Microsoft.AspNetCore.Identity;

namespace AspNetCore.Identity.CosmosDb.Tests
{
    [TestClass]
    public class UserManagerTests : CosmosIdentityTestsBase
    {
        // Creates a new test user with a hashed password, using the mock UserManager to do so
        private async Task<IdentityUser> GetTestUser(UserManager<IdentityUser> userManager)
        {
            var user = await GetMockRandomUserAsync(false);
            var result = await userManager.CreateAsync(user, $"A1a{Guid.NewGuid()}");

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
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task HasPasswordAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task AddPasswordAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ChangePasswordAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RemovePasswordAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task VerifyPasswordAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetSecurityStampAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task UpdateSecurityStampAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GeneratePasswordResetTokenAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ResetPasswordAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task FindByLoginAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RemoveLoginAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task AddLoginAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetLoginsAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task AddClaimAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task AddClaimsAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task ReplaceClaimAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RemoveClaimAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RemoveClaimsAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetClaimsAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task AddToRoleAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task AddToRolesAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RemoveFromRoleAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task RemoveFromRolesAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

        }

        [TestMethod]
        public async Task GetRolesAsyncTest()
        {
            using var userManager = GetTestUserManager(_userStore);

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
