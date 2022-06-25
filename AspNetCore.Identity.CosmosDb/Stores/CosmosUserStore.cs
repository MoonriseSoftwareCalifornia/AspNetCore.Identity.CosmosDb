using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PieroDeTomi.EntityFrameworkCore.Identity.Cosmos.Stores
{
    /// <summary>
    /// Cosmos DB User store
    /// </summary>
    /// <typeparam name="TUserEntity"></typeparam>
    public class CosmosUserStore<TUserEntity> :
        IUserStore<TUserEntity>,
        IUserRoleStore<TUserEntity>,
        IUserEmailStore<TUserEntity>,
        IUserPasswordStore<TUserEntity>,
        IUserPhoneNumberStore<TUserEntity>,
        IUserLoginStore<TUserEntity> where TUserEntity : IdentityUser, new()
    {
        private readonly IRepository _repo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo"></param>
        public CosmosUserStore(IRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<IdentityResult> CreateAsync(TUserEntity user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(user.Email))
                throw new ArgumentNullException(nameof(user.Email));

            if (string.IsNullOrEmpty(user.UserName))
                throw new ArgumentNullException(nameof(user.UserName));

            try
            {
                user.NormalizedEmail = user.Email.ToLower();
                user.NormalizedUserName = user.UserName.ToLower();

                _repo.Add(user);
                await _repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<IdentityResult> DeleteAsync(TUserEntity user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                _repo.Delete(user);
                await _repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Find a user by a email address
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TUserEntity> FindByEmailAsync(string emailAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(emailAddress))
                throw new ArgumentNullException(nameof(emailAddress));

            var user = await _repo.Table<TUserEntity>()
                .SingleOrDefaultAsync(_ => _.NormalizedEmail == emailAddress.ToLower(), cancellationToken: cancellationToken);

            return user;
        }

        /// <summary>
        /// Find a user by user ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TUserEntity> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            var user = await _repo.Table<TUserEntity>()
                .WithPartitionKey(userId)
                .SingleOrDefaultAsync(cancellationToken);

            return user;
        }

        /// <summary>
        /// Find a user by user name
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TUserEntity> FindByNameAsync(string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            return await _repo.Table<TUserEntity>().SingleOrDefaultAsync(_ => _.NormalizedUserName == userName.ToLower());
        }

        /// <summary>
        /// Get email address for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetEmailAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.Email, cancellationToken));
        }

        /// <summary>
        /// Get a user's email confirmation status
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> GetEmailConfirmedAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.EmailConfirmed, cancellationToken));
        }

        /// <summary>
        /// Get a normailized email address for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetNormalizedEmailAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.NormalizedEmail, cancellationToken));
        }

        /// <summary>
        /// Get a normailized user name for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetNormalizedUserNameAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.NormalizedUserName, cancellationToken));
        }

        /// <summary>
        /// Get the password hash for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetPasswordHashAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.PasswordHash, cancellationToken));
        }

        /// <summary>
        /// Get the telephone number for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetPhoneNumberAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.PhoneNumber, cancellationToken));
        }

        /// <summary>
        /// Get a user's telephone number confirmation status
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> GetPhoneNumberConfirmedAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                GetUserProperty(user, user => user.PhoneNumberConfirmed, cancellationToken));
        }

        /// <summary>
        /// Get the User ID for a user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<string> GetUserIdAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
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
        public async Task SetEmailAsync(TUserEntity user, string emailAddress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentNullException(nameof(emailAddress));
            }

            SetUserProperty(user, emailAddress, (u, m) => u.Email = emailAddress, cancellationToken);

            // Always keep the normalized email address in synch
            await SetNormalizedEmailAsync(user, emailAddress, cancellationToken);

        }

        /// <summary>
        /// Sets the confirmation status of a user's email address
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetEmailConfirmedAsync(TUserEntity user, bool confirmed, CancellationToken cancellationToken = default)
        {
            SetUserProperty(user, confirmed, (u, m) => u.EmailConfirmed = confirmed, cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the normailized email address for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="emailAddress"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetNormalizedEmailAsync(TUserEntity user, string emailAddress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentNullException(nameof(emailAddress));
            }
            SetUserProperty(user, emailAddress, (u, m) => u.NormalizedEmail = emailAddress.ToLower(), cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the normalized user name for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetNormalizedUserNameAsync(TUserEntity user, string userName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            SetUserProperty(user, userName, (u, m) => u.NormalizedUserName = userName.ToLower(), cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the password hash for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwordHash"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetPasswordHashAsync(TUserEntity user, string passwordHash, CancellationToken cancellationToken = default)
        {
            SetUserProperty(user, passwordHash, (u, m) => u.PasswordHash = passwordHash, cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the phone number for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetPhoneNumberAsync(TUserEntity user, string phoneNumber, CancellationToken cancellationToken = default)
        {
            SetUserProperty(user, phoneNumber, (u, v) => user.PhoneNumber = v, cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the confirmation status of a user's phone number
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SetPhoneNumberConfirmedAsync(TUserEntity user, bool confirmed, CancellationToken cancellationToken = default)
        {
            SetUserProperty(user, confirmed, (u, v) => user.PhoneNumberConfirmed = v, cancellationToken);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the user name for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SetUserNameAsync(TUserEntity user, string userName, CancellationToken cancellationToken = default)
        {
            SetUserProperty(user, userName, (u, m) => u.UserName = userName, cancellationToken);
            // Always keep the normalized user name in synch.
            await SetNormalizedUserNameAsync(user, userName, cancellationToken);
        }

        /// <summary>
        /// Updates an identity user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IdentityResult> UpdateAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            try
            {
                // Keep these things in sync just as with CreateAsync() method.
                user.NormalizedEmail = user.Email.ToLower();
                //user.NormalizedUserName = user.UserName.ToLower();

                _repo.Update(user);

                await _repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }

            return IdentityResult.Success;
        }

        private T GetUserProperty<T>(TUserEntity user, Func<TUserEntity, T> accessor, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return accessor(user);
        }

        private void SetUserProperty<T>(TUserEntity user, T value, Action<TUserEntity, T> setter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (value == null) throw new ArgumentNullException(nameof(value));

            setter(user, value);
        }

        /// <summary>
        /// Add a login for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="login"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task AddLoginAsync(TUserEntity user, UserLoginInfo login, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (login == null) throw new ArgumentNullException(nameof(login));

            try
            {
                IdentityUserLogin<string> loginEntity = new IdentityUserLogin<string>
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
                var x = e;
            }
        }

        /// <summary>
        /// Removes a login for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task RemoveLoginAsync(TUserEntity user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null) throw new ArgumentNullException(nameof(providerKey));

            try
            {
                var login = await _repo.Table<IdentityUserLogin<string>>()
                    .SingleOrDefaultAsync(l =>
                        l.UserId == user.Id &&
                        l.LoginProvider == loginProvider &&
                        l.ProviderKey == providerKey
                    );

                if (login != null)
                {
                    _repo.Delete(login);
                    await _repo.SaveChangesAsync();
                }
            }
            catch { }
        }

        /// <summary>
        /// Gets the logins for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            IList<UserLoginInfo> res = _repo
                .Table<IdentityUserLogin<string>>()
                .Where(l => l.UserId == user.Id)
                .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, user.UserName)
                {
                    ProviderDisplayName = l.ProviderDisplayName
                })
                .ToList();

            return Task.FromResult(res);
        }

        /// <summary>
        /// Find a user by a login
        /// </summary>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<TUserEntity> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (loginProvider == null) throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null) throw new ArgumentNullException(nameof(providerKey));

            var userId = (
                await _repo.Table<IdentityUserLogin<string>>().SingleOrDefaultAsync(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey)
            )?.UserId;

            return string.IsNullOrEmpty(userId)
                ? default(TUserEntity)
                : await FindByIdAsync(userId, cancellationToken);
        }

        /// <summary>
        /// Add a user to a role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task AddToRoleAsync(TUserEntity user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == roleName.ToLower(), cancellationToken: cancellationToken);

            if (role == null) throw new InvalidOperationException("Role not found.");

            try
            {
                IdentityUserRole<string> userRole = new IdentityUserRole<string>
                {
                    RoleId = role.Id,
                    UserId = user.Id
                };

                _repo.Add(userRole);
                await _repo.SaveChangesAsync();
            }
            catch { }
        }

        /// <summary>
        /// Removes a user from a role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task RemoveFromRoleAsync(TUserEntity user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userRole = await _repo.Table<IdentityUserRole<string>>().SingleOrDefaultAsync(_ => _.RoleId == role.Id, cancellationToken);
                if (userRole != null)
                {
                    _repo.Delete(userRole);
                    await _repo.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Gets the roles for a user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<IList<string>> GetRolesAsync(TUserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var roleIds = await _repo
                .Table<IdentityUserRole<string>>()
                .Where(m => m.UserId == user.Id)
                .Select(m => m.RoleId)
                .ToListAsync(cancellationToken);

            IList<string> res = await _repo
                .Table<IdentityRole>()
                .Where(m => roleIds.Contains(m.Id))
                .Select(m => m.Name)
                .ToListAsync(cancellationToken);

            return res;
        }

        /// <summary>
        /// Determines if a user is in a role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<bool> IsInRoleAsync(TUserEntity user, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userRole = await _repo.Table<IdentityUserRole<string>>()
                    .SingleOrDefaultAsync(_ => _.RoleId == role.Id && _.UserId == user.Id, cancellationToken);

                return userRole != null;
            }

            return false;
        }

        /// <summary>
        /// Gets a list of users in a role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<IList<TUserEntity>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentNullException(nameof(roleName));

            var role = await _repo.Table<IdentityRole>()
                            .SingleOrDefaultAsync(_ => _.NormalizedName == roleName, cancellationToken: cancellationToken);

            if (role != null)
            {
                var userIds = await _repo.Table<IdentityUserRole<string>>()
                    .Where(m => m.RoleId == role.Id)
                    .Select(m => m.UserId)
                    .ToListAsync(cancellationToken);

                var users = await _repo.Table<TUserEntity>()
                    .Where(m => userIds.Contains(m.Id))
                    .ToListAsync(cancellationToken);

                return users;
            }

            return new List<TUserEntity>();
        }

        /// <summary>
        /// Disposes this class
        /// </summary>
        public void Dispose()
        { }
    }
}