using AspNetCore.Identity.CosmosDb.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Identity.CosmosDb.Stores
{
    /// <summary>
    /// Cosmos DB User store
    /// </summary>
    /// <typeparam name="TUserEntity"></typeparam>
    public class CosmosUserStore<TUserEntity, TRoleEntity, TKey> : IdentityStoreBase,
        IUserStore<TUserEntity>,
        IUserRoleStore<TUserEntity>,
        IUserEmailStore<TUserEntity>,
        IUserPasswordStore<TUserEntity>,
        IUserPhoneNumberStore<TUserEntity>,
        IUserLockoutStore<TUserEntity>,
        IUserClaimStore<TUserEntity>,
        IUserSecurityStampStore<TUserEntity>,
        IUserTwoFactorStore<TUserEntity>,
        IUserTwoFactorRecoveryCodeStore<TUserEntity>,
        IQueryableUserStore<TUserEntity>,
        IUserAuthenticatorKeyStore<TUserEntity>,
        IUserLoginStore<TUserEntity>
        where TUserEntity : IdentityUser<TKey>, new()
        where TRoleEntity : IdentityRole<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        private const string InternalLoginProvider = "[AspNetUserStore]";
        private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
        private const string RecoveryCodeTokenName = "RecoveryCodes";

        private readonly IRepository _repo;
        private bool _disposed;

        public IQueryable<TUserEntity> Users
        {
            get { return (IQueryable<TUserEntity>)_repo.Users; }
        }

        /// <summary>
        /// Throws if this class has been disposed.
        /// </summary>
        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo"></param>
        public CosmosUserStore(IRepository repo)
            : base(repo)
        {
            _repo = repo;
        }

        /// <inheritdoc/>
        public async Task<IdentityResult> CreateAsync(TUserEntity user,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(user.Email))
                throw new ArgumentNullException(nameof(user.Email));

            if (string.IsNullOrEmpty(user.UserName))
                throw new ArgumentNullException(nameof(user.UserName));

            try
            {
                _repo.Add(user);
                await _repo.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return ProcessExceptions(e);
            }

            return IdentityResult.Success;
        }

        // <inheritdoc />
        public async Task<IdentityResult> DeleteAsync(TUserEntity user,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var roles = await GetRolesAsync(user);
                var claims = await GetClaimsAsync(user);
                var logins = await GetLoginsAsync(user);

                foreach (var role in roles)
                {
                    await RemoveFromRoleAsync(user, role, cancellationToken);
                }

                if (claims.Any())
                    await RemoveClaimsAsync(user, claims, cancellationToken);

                foreach (var login in logins)
                {
                    await RemoveLoginAsync(user, login.ProviderDisplayName, login.ProviderKey, cancellationToken);
                }

                _repo.Delete(user);
                await _repo.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return ProcessExceptions(e);
            }

            return IdentityResult.Success;
        }

        // <inheritdoc />
        public async Task<TUserEntity> FindByEmailAsync(string normalizedEmailName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(normalizedEmailName))
                throw new ArgumentNullException(nameof(normalizedEmailName));

            var user = await _repo.Table<TUserEntity>()
                .SingleOrDefaultAsync(_ => _.NormalizedEmail == normalizedEmailName,
                    cancellationToken: cancellationToken);

            return user;
        }

        // <inheritdoc />
        public async Task<TUserEntity> FindByIdAsync(string userId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            var user = await _repo.Table<TUserEntity>()
                .WithPartitionKey(userId)
                .SingleOrDefaultAsync(cancellationToken);

            return user;
        }

        // <inheritdoc />
        public async Task<TUserEntity> FindByNameAsync(string normalizedUserName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(normalizedUserName))
                throw new ArgumentNullException(nameof(normalizedUserName));

            return await _repo.Table<TUserEntity>()
                .SingleOrDefaultAsync(_ => _.NormalizedUserName == normalizedUserName || _.NormalizedEmail == normalizedUserName);
        }

        // <inheritdoc />
        public Task<string> GetEmailAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.Email, cancellationToken));
        }

        // <inheritdoc />
        public Task<bool> GetEmailConfirmedAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.EmailConfirmed, cancellationToken));
        }

        // <inheritdoc />
        public Task<string> GetNormalizedEmailAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.NormalizedEmail, cancellationToken));
        }

        // <inheritdoc />
        public Task<string> GetNormalizedUserNameAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.NormalizedUserName, cancellationToken));
        }

        // <inheritdoc />
        public Task<string> GetPasswordHashAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.PasswordHash, cancellationToken));
        }

        // <inheritdoc />
        public Task<string> GetPhoneNumberAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.PhoneNumber, cancellationToken));
        }

        // <inheritdoc />
        public Task<bool> GetPhoneNumberConfirmedAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.PhoneNumberConfirmed, cancellationToken));
        }


        // <inheritdoc />
        public Task<string> GetUserIdAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.Id.ToString(), cancellationToken));
        }

        /// <summary>
        /// Get the user name for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetUserNameAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => user.UserName, cancellationToken));
        }

        /// <summary>
        /// Determine if a user has a password.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> HasPasswordAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            return Task.FromResult(
                GetUserProperty(user, user => !string.IsNullOrEmpty(user.PasswordHash), cancellationToken));
        }

        /// <summary>
        /// Sets a user's email address
        /// </summary>
        /// <param name="user"></param>
        /// <param name="emailAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetEmailAsync(TUserEntity user, string emailAddress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentNullException(nameof(emailAddress));

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, emailAddress, (u, m) => u.Email = emailAddress, cancellationToken);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the confirmation status of a user's email address
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetEmailConfirmedAsync(TUserEntity user, bool confirmed,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, confirmed, (u, m) => u.EmailConfirmed = confirmed, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetNormalizedEmailAsync(TUserEntity user, string normalizedEmail,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(normalizedEmail))
                throw new ArgumentNullException(nameof(normalizedEmail));

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, normalizedEmail, (u, m) => u.NormalizedEmail = normalizedEmail, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetNormalizedUserNameAsync(TUserEntity user, string normalizedName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(normalizedName))
                throw new ArgumentNullException(nameof(normalizedName));

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, normalizedName, (u, m) => u.NormalizedUserName = normalizedName, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetPasswordHashAsync(TUserEntity user, string passwordHash,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, passwordHash, (u, m) => u.PasswordHash = passwordHash, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetPhoneNumberAsync(TUserEntity user, string phoneNumber,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, phoneNumber, (u, v) => user.PhoneNumber = v, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetPhoneNumberConfirmedAsync(TUserEntity user, bool confirmed,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, confirmed, (u, v) => user.PhoneNumberConfirmed = v, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetUserNameAsync(TUserEntity user, string userName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            SetUserProperty(user, userName, (u, m) => u.UserName = userName, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public async Task<IdentityResult> UpdateAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            try
            {
                _repo.Update(user);

                await _repo.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return ProcessExceptions(e);
            }

            return IdentityResult.Success;
        }

        private T GetUserProperty<T>(TUserEntity user, Func<TUserEntity, T> accessor,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return accessor(user);
        }

        private void SetUserProperty<T>(TUserEntity user, T value, Action<TUserEntity, T> setter,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            //if (value == null) throw new ArgumentNullException(nameof(value));

            setter(user, value);
        }

        // <inheritdoc />
        public async Task AddLoginAsync(TUserEntity user, UserLoginInfo login,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (login == null) throw new ArgumentNullException(nameof(login));

            try
            {
                IdentityUserLogin<TKey> loginEntity = new IdentityUserLogin<TKey>
                {
                    UserId = user.Id,
                    LoginProvider = login.LoginProvider,
                    ProviderKey = login.ProviderKey,
                    ProviderDisplayName = login.ProviderDisplayName
                };

                _repo.Add(loginEntity);
                await _repo.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // Debugging purposes.
                //var x = e;
                throw;
            }
        }

        // <inheritdoc />
        public async Task RemoveLoginAsync(TUserEntity user, string loginProvider, string providerKey,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null) throw new ArgumentNullException(nameof(providerKey));

            try
            {
                var login = await _repo.Table<IdentityUserLogin<TKey>>()
                    .SingleOrDefaultAsync(l =>
                        l.UserId.Equals(user.Id) &&
                        l.LoginProvider == loginProvider &&
                        l.ProviderKey == providerKey
                    );

                if (login != null)
                {
                    _repo.Delete(login);
                    await _repo.SaveChangesAsync();
                }
            }
            catch
            {
            }
        }

        // <inheritdoc />
        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUserEntity user,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            IList<UserLoginInfo> res = _repo
                .Table<IdentityUserLogin<TKey>>()
                .Where(l => l.UserId.Equals(user.Id))
                .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, user.UserName)
                {
                    ProviderDisplayName = l.ProviderDisplayName
                })
                .ToListAsync().Result;

            return Task.FromResult(res);
        }

        // <inheritdoc />
        public async Task<TUserEntity> FindByLoginAsync(string loginProvider, string providerKey,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null) throw new ArgumentNullException(nameof(providerKey));

            var user =
                await _repo.Table<IdentityUserLogin<TKey>>().SingleOrDefaultAsync(l =>
                    l.LoginProvider == loginProvider && l.ProviderKey == providerKey);


            return user is null
                ? default(TUserEntity)
                : await FindByIdAsync(user.UserId?.ToString(), cancellationToken);
        }

        // <inheritdoc />
        public async Task AddToRoleAsync(TUserEntity user, string normalizedRoleName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
                throw new ArgumentNullException(nameof(normalizedRoleName));

            var role = await _repo.Table<TRoleEntity>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == normalizedRoleName,
                    cancellationToken: cancellationToken);

            if (role == null) throw new InvalidOperationException("Role not found.");

            try
            {
                IdentityUserRole<TKey> userRole = new IdentityUserRole<TKey>
                {
                    RoleId = role.Id,
                    UserId = user.Id
                };

                _repo.Add(userRole);
                await _repo.SaveChangesAsync();
            }
            catch
            {
            }
        }

        /// <inheritdoc/>>
        public async Task RemoveFromRoleAsync(TUserEntity user, string roleName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<TRoleEntity>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userRole = await _repo.Table<IdentityUserRole<TKey>>()
                    .SingleOrDefaultAsync(_ => _.RoleId.Equals(role.Id) && _.UserId.Equals(user.Id), cancellationToken);
                if (userRole != null)
                {
                    _repo.Delete(userRole);
                    await _repo.SaveChangesAsync();
                }
            }
        }

        // <inheritdoc />
        public async Task<IList<string>> GetRolesAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var roleIds = await _repo
                .Table<IdentityUserRole<TKey>>()
                .Where(m => m.UserId.Equals(user.Id))
                .Select(m => m.RoleId)
                .ToListAsync(cancellationToken);

            IList<string> res = await _repo
                .Table<TRoleEntity>()
                .Where(m => roleIds.Contains(m.Id))
                .Select(m => m.Name)
                .ToListAsync(cancellationToken);

            return res;
        }

        // <inheritdoc />
        public async Task<bool> IsInRoleAsync(TUserEntity user, string roleName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userRole = await _repo.Table<IdentityUserRole<TKey>>()
                    .SingleOrDefaultAsync(_ => _.RoleId.Equals(role.Id) && _.UserId.Equals(user.Id), cancellationToken);

                return userRole != null;
            }

            return false;
        }

        // <inheritdoc />
        public async Task<IList<TUserEntity>> GetUsersInRoleAsync(string roleName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userIds = await _repo.Table<IdentityUserRole<TKey>>()
                    .Where(m => m.RoleId.Equals(role.Id))
                    .Select(m => m.UserId)
                    .ToListAsync(cancellationToken);

                var users = await _repo.Table<TUserEntity>()
                    .Where(m => userIds.Contains(m.Id))
                    .ToListAsync(cancellationToken);

                return users;
            }

            return new List<TUserEntity>();
        }

        // <inheritdoc />
        public void Dispose()
        {
            _disposed = true;
        }

        #region IUserLockoutStore methods

        // <inheritdoc />
        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(TUserEntity user,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var entity = await _repo.Table<TUserEntity>()
                .FirstOrDefaultAsync(m => m.Id.Equals(user.Id), cancellationToken);

            return entity.LockoutEnd;
        }

        // <inheritdoc />
        public Task SetLockoutEndDateAsync(TUserEntity user, DateTimeOffset? lockoutEnd,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            SetUserProperty(user, lockoutEnd, (u, v) => user.LockoutEnd = v, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public async Task<int> IncrementAccessFailedCountAsync(TUserEntity user,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var entity = await _repo.Table<TUserEntity>()
                .FirstOrDefaultAsync(m => m.Id.Equals(user.Id), cancellationToken);

            var count = entity.AccessFailedCount + 1;
            SetUserProperty(user, count, (u, v) => user.AccessFailedCount = v, cancellationToken);

            return count;
        }

        // <inheritdoc />
        public Task ResetAccessFailedCountAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            SetUserProperty(user, 0, (u, v) => user.AccessFailedCount = v, cancellationToken);

            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task<int> GetAccessFailedCountAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(
                GetUserProperty(user, user => user.AccessFailedCount, cancellationToken));
        }

        // <inheritdoc />
        public Task<bool> GetLockoutEnabledAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(
                GetUserProperty(user, user => user.LockoutEnabled, cancellationToken));
        }

        // <inheritdoc />
        public Task SetLockoutEnabledAsync(TUserEntity user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            SetUserProperty(user, enabled, (u, v) => user.LockoutEnabled = v, cancellationToken);

            return Task.CompletedTask;
        }

        #endregion

        #region methods implementing IUserClaimStore<TUserEntity>

        // <inheritdoc />
        public async Task<IList<Claim>> GetClaimsAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = await _repo.Table<IdentityUserClaim<TKey>>().Where(c => c.UserId.Equals(user.Id))
                .ToListAsync(cancellationToken);

            return claims.Select(c => c.ToClaim()).ToList();
        }

        // <inheritdoc />
        public async Task AddClaimsAsync(TUserEntity user, IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claims == null || claims.Any() == false)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
            {
                var nextId = 1;

                try
                {
                    var keys = await _repo.Table<IdentityUserClaim<TKey>>().Select(m => m.Id).ToListAsync(); // Convert to int array here.

                    if (keys.Any())
                    {
                        nextId = keys.Max() + 1; // Max should now work.
                    }
                    else
                    {
                        nextId = 1;
                    }

                }
                catch (Exception e)
                {
                    var t = e; // for debugging
                }

                var identityUserClaim = new IdentityUserClaim<TKey>()
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    UserId = user.Id,
                    Id = nextId
                };

                _repo.Add(identityUserClaim);
                await _repo.SaveChangesAsync().WaitAsync(cancellationToken);
            }
        }

        // <inheritdoc />
        public async Task ReplaceClaimAsync(TUserEntity user, Claim claim, Claim newClaim,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));
            if (newClaim == null)
                throw new ArgumentNullException(nameof(claim));

            // Remove the claim if it exists
            await RemoveClaimsAsync(user, new[] { claim }, cancellationToken);

            // Add the new claim
            await AddClaimsAsync(user, new[] { newClaim }, cancellationToken).WaitAsync(cancellationToken);
        }

        // <inheritdoc />
        public async Task RemoveClaimsAsync(TUserEntity user, IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claims == null || claims.Any() == false)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
            {
                var doomed = await _repo.Table<IdentityUserClaim<TKey>>()
                    .SingleOrDefaultAsync(c => c.UserId.Equals(user.Id) &&
                                               c.ClaimValue == claim.Value && c.ClaimType == claim.Type,
                        cancellationToken);

                if (doomed != null)
                    _repo.Delete<IdentityUserClaim<TKey>>(doomed);
            }

            await _repo.SaveChangesAsync().WaitAsync(cancellationToken);
        }

        // <inheritdoc />
        public async Task<IList<TUserEntity>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            var userIds = await _repo.Table<IdentityUserClaim<TKey>>()
                .Where(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value)
                .Select(s => s.UserId).ToArrayAsync(cancellationToken);

            var users = await _repo.Table<TUserEntity>()
                .Where(w => userIds.Contains(w.Id)).ToListAsync(cancellationToken);

            return (IList<TUserEntity>)users;
        }

        #endregion

        #region Methods implementing IUserSecurityStampStore<TUserEntity> interface

        // <inheritdoc />
        public Task SetSecurityStampAsync(TUserEntity user, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(stamp))
                throw new ArgumentNullException(nameof(stamp));

            SetUserProperty(user, stamp, (u, v) => user.SecurityStamp = v, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task<string> GetSecurityStampAsync(TUserEntity user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(
                GetUserProperty(user, user => user.SecurityStamp, cancellationToken));
        }

        #endregion

        #region Methods that implement IUserTwoFactorStore<TUserEntity>

        // <inheritdoc />
        public Task SetTwoFactorEnabledAsync(TUserEntity user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            SetUserProperty(user, enabled, (u, v) => user.TwoFactorEnabled = v, cancellationToken);
            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task<bool> GetTwoFactorEnabledAsync(TUserEntity user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(
                GetUserProperty(user, user => user.TwoFactorEnabled, cancellationToken));
        }

        #endregion

        #region Methods implementing IUserAuthenticatorKeyStore<TUserEntity> interface

        /// <summary>
        /// Sets the authenticator key for the specified <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user whose authenticator key should be set.</param>
        /// <param name="key">The authenticator key to set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual Task SetAuthenticatorKeyAsync(TUserEntity user, string key, CancellationToken cancellationToken)
            => SetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, key, cancellationToken);

        /// <summary>
        /// Sets the token value for a particular user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="loginProvider">The authentication provider for the token.</param>
        /// <param name="name">The name of the token.</param>
        /// <param name="value">The value of the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public virtual async Task SetTokenAsync(TUserEntity user, string loginProvider, string name, string? value,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrEmpty(value))
            {
                value = GenerateNewAuthenticatorKey();
            }

            var queryable = (IQueryable<IdentityUserToken<TKey>>)_repo.UserTokens;

            var token = await queryable.FirstOrDefaultAsync(t =>
                t.UserId.Equals(user.Id) && t.LoginProvider == loginProvider && t.Name == name);

            //var token = await FindTokenAsync(user, loginProvider, name, cancellationToken).ConfigureAwait(false);
            if (token == null)
            {
                token = new IdentityUserToken<TKey>()
                {
                    LoginProvider = loginProvider,
                    Name = name,
                    UserId = user.Id,
                    Value = GenerateNewAuthenticatorKey()
                };
                try
                {
                    _repo.Add(token);
                    await _repo.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    ProcessExceptions(e);
                }
                //await AddUserTokenAsync(CreateUserToken(user, loginProvider, name, value)).ConfigureAwait(false);
            }
            else
            {
                token.Value = value;
            }
        }

        /// <summary>
        /// Get the authenticator key for the specified <paramref name="user" />.
        /// </summary>
        /// <param name="user">The user whose security stamp should be set.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the security stamp for the specified <paramref name="user"/>.</returns>
        public async Task<string> GetAuthenticatorKeyAsync(TUserEntity user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var queryable = (IQueryable<IdentityUserToken<TKey>>)_repo.UserTokens;

            return await GetTokenAsync(user, InternalLoginProvider, AuthenticatorKeyTokenName, cancellationToken);
        }


        private async Task<string> GetTokenAsync(TUserEntity user, string provider, string tokenName,
            CancellationToken cancellationToken)
        {
            var queryable = (IQueryable<IdentityUserToken<TKey>>)_repo.UserTokens;
            var token = await queryable.FirstOrDefaultAsync(
                t => t.UserId.Equals(user.Id) && t.LoginProvider == provider && t.Name == tokenName,
                cancellationToken: cancellationToken);
            if (token == null)
            {
                return null;
            }

            return token.Value;
        }

        public string GenerateNewAuthenticatorKey()
            => NewSecurityStamp();

        private static string NewSecurityStamp()
        {
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                byte[] bytes = new byte[64];
                cryptoProvider.GetBytes(bytes);

                string secureRandomString = Convert.ToBase64String(bytes);

                var lettersOrNumbers = new string(secureRandomString.Where(char.IsLetter).ToArray());

                // Output example: Secure random string: OfGER+tSZIOSz314OlHk1aM+N8oNXDRHqTn3c5EVknYO5b5s0kqq40lJzoGj99ZXCvoFhkNG8KwQQvBPaR0FtQ==
                // Works: auth enti cato rkey
                return lettersOrNumbers.Substring(0, 16);
            }
        }

        #endregion

        #region Methods that implement IUserTwoFactorRecoveryCodeStore<TUserEntity>

        // <inheritdoc />
        public Task ReplaceCodesAsync(TUserEntity user, IEnumerable<string> recoveryCodes,
            CancellationToken cancellationToken)
        {
            var mergedCodes = string.Join(";", recoveryCodes);
            return SetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, mergedCodes, cancellationToken);
        }

        // <inheritdoc />
        public async Task<bool> RedeemCodeAsync(TUserEntity user, string code, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken)
                .ConfigureAwait(false) ?? "";
            var splitCodes = mergedCodes.Split(';');
            if (splitCodes.Contains(code))
            {
                var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
                await ReplaceCodesAsync(user, updatedCodes, cancellationToken).ConfigureAwait(false);
                return true;
            }

            return false;
        }


        // <inheritdoc />
        public async Task<int> CountCodesAsync(TUserEntity user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var mergedCodes = await GetTokenAsync(user, InternalLoginProvider, RecoveryCodeTokenName, cancellationToken)
                .ConfigureAwait(false) ?? "";
            if (mergedCodes.Length > 0)
            {
                return mergedCodes.Split(';').Length;
            }

            return 0;
        }

        #endregion
    }
}