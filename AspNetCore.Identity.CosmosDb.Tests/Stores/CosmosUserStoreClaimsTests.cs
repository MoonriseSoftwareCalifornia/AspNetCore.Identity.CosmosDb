using AspNetCore.Identity.CosmosDb.Tests;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDb.Stores.Tests
{
	[TestClass()]
	public class CosmosUserStoreClaimsTests : CosmosIdentityTestsBase
	{

		//private static TestUtilities? utils;
		//private static CosmosUserStore<IdentityUser>? userStore;
		//private static CosmosRoleStore<IdentityRole>? _roleStore;
		// private static string phoneNumber = "0000000000";
		//private static Random? _random;

		[ClassInitialize]
		public static void Initialize(TestContext context)
		{
			//
			// Setup context.
			//
			InitializeClass();
		}

		#region methods implementing IUserClaimStore<TUserEntity>

		[TestMethod()]
		public async Task Consolidated_ClaimsAsync_CRUD_Tests()
		{
			// Arrange
			using var userStore = _testUtilities.GetUserStore();
			var user1 = await GetMockRandomUserAsync(userStore);
			var claims = new Claim[] { GetMockClaim(), GetMockClaim(), GetMockClaim() };
			var newClaim = GetMockClaim();
			await userStore.AddClaimsAsync(user1, claims, default);

			// Act - Create
			var result2 = await userStore.GetClaimsAsync(user1, default);

			// Assert - Create
			Assert.AreEqual(3, result2.Count);

			// Act - Replace
			await userStore.ReplaceClaimAsync(user1, claims.FirstOrDefault(), newClaim, default);

			// Assert - Replace
			var result3 = await userStore.GetClaimsAsync(user1, default);
			Assert.IsFalse(result3.Any(a => a.Type == claims.FirstOrDefault().Type));
			Assert.IsTrue(result3.Any(a => a.Type == newClaim.Type));

			// Act - Delete
			await userStore.RemoveClaimsAsync(user1, result3, default); 
			var result4 = await userStore.GetClaimsAsync(user1, default);
			Assert.IsFalse(result4.Any());
		}

		[TestMethod()]
		public async Task GetUsersForClaimAsyncTest()
		{
			// Arrange
			var val = Guid.NewGuid().ToString();
			var claims = new Claim[] { new Claim(val, val) };
			using (var userStore = _testUtilities.GetUserStore())
			{
				var user1 = await GetMockRandomUserAsync(userStore);
				await userStore.AddClaimsAsync(user1, claims, default);
			}

			using (var userStore = _testUtilities.GetUserStore())
			{
				var user2 = await GetMockRandomUserAsync(userStore);
				await userStore.AddClaimsAsync(user2, claims, default);
			}

			using (var userStore = _testUtilities.GetUserStore())
			{
				// Act
				var result1 = await userStore.GetUsersForClaimAsync(claims.FirstOrDefault(), default);
				// Assert
				Assert.AreEqual(2, result1.Count);
			}

		}

		#endregion
	}
}