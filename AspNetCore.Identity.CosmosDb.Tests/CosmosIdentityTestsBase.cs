using AspNetCore.Identity.CosmosDb.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text;

namespace AspNetCore.Identity.CosmosDb.Tests
{
    public abstract class CosmosIdentityTestsBase
    {
        protected static TestUtilities? _testUtilities;
        protected static CosmosUserStore<IdentityUser>? _userStore;
        protected static CosmosRoleStore<IdentityRole>? _roleStore;
        protected static Random? _random;

        protected static void InitializeClass()
        {
            //
            // Setup context.
            //
            _testUtilities = new TestUtilities();
            _userStore = _testUtilities.GetUserStore();
            _roleStore = _testUtilities.GetRoleStore();
            _random = new Random();

            // Arrange class - remove prior data
            using var dbContext = _testUtilities.GetDbContext();
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
        protected int GetNextRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityRole"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        protected async Task<IdentityRole> GetMockRandomRoleAsync()
        {
            var role = new IdentityRole(GetNextRandomNumber(1000, 9999).ToString());
            role.NormalizedName = role.Name.ToUpper();

            var result = await _roleStore.CreateAsync(role);
            role = await _roleStore.FindByIdAsync(role.Id);
            Assert.IsTrue(result.Succeeded);//Confirm success
            return role;
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityUser"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        protected async Task<IdentityUser> GetMockRandomUserAsync(bool saveToDatabase = true)
        {
            var randomEmail = $"{GetNextRandomNumber(1000, 9999)}@{GetNextRandomNumber(10000, 99999)}.com";
            var user = new IdentityUser(randomEmail) { Email = randomEmail, Id = Guid.NewGuid().ToString() };

            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();

            if (saveToDatabase)
            {
                var result = await _userStore.CreateAsync(user);
                Assert.IsTrue(result.Succeeded);//Confirm success
                user = await _userStore.FindByNameAsync(user.UserName.ToUpper());
            }
            return user;
        }

        /// <summary>
        /// Gets a mock login info for testing purposes
        /// </summary>
        /// <returns></returns>
        protected UserLoginInfo GetMockLoginInfoAsync()
        {
            return new UserLoginInfo("Twitter", Guid.NewGuid().ToString(), "Twitter");
        }

        public UserManager<TUser> GetTestUserManager<TUser>(IUserStore<TUser> store) where TUser : class
        {
            store = store ?? new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());
            var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, MockLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);
            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
            return userManager;
        }

        public ILookupNormalizer MockLookupNormalizer()
        {
            var normalizerFunc = new Func<string, string>(i =>
            {
                if (i == null)
                {
                    return null;
                }
                else
                {
                    return i.ToUpperInvariant();
                }
            });
            var lookupNormalizer = new Mock<ILookupNormalizer>();
            lookupNormalizer.Setup(i => i.NormalizeName(It.IsAny<string>())).Returns(normalizerFunc);
            lookupNormalizer.Setup(i => i.NormalizeEmail(It.IsAny<string>())).Returns(normalizerFunc);
            return lookupNormalizer.Object;
        }
    }
}
