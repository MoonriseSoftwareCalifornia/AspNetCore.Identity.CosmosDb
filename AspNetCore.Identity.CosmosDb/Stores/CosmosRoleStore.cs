using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetCore.Identity.CosmosDb.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using AspNetCore.Identity.CosmosDb.Repositories;

namespace AspNetCore.Identity.CosmosDb.Stores
{
    /// <summary>
    /// Cosmos DB Role Store
    /// </summary>
    /// <typeparam name="TRoleEntity"></typeparam>
    public class CosmosRoleStore<TUserRoleEntity, TRoleEntity, TKey> : IRoleStore<TRoleEntity>,
        IQueryableRoleStore<TRoleEntity>,
        IRoleClaimStore<TRoleEntity>
        where TRoleEntity : IdentityRole<TKey>, new()
        where TKey : IEquatable<TKey>

    {
        private readonly IRepository _repo;
        private bool _disposed;

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
        /// Role query
        /// </summary>
        public IQueryable<TRoleEntity> Roles
        {
            get { return (IQueryable<TRoleEntity>)_repo.Roles; }
        }

        /// <summary>
        /// UserRoles query
        /// </summary>
        public IQueryable<IdentityUserRole<TKey>> UserRoles
        {
            get { return (IQueryable<IdentityUserRole<TKey>>)_repo.UserRoles; }
        }

        /// <summary>
        /// UserRoles query
        /// </summary>
        public IQueryable<IdentityRoleClaim<TKey>> RoleClaims
        {
            get { return (IQueryable<IdentityRoleClaim<TKey>>)_repo.RoleClaims; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repo"></param>
        public CosmosRoleStore(IRepository repo)
        {
            _repo = repo;
        }


        // <inheritdoc />
        public async Task<IdentityResult> CreateAsync(TRoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
                throw new ArgumentNullException(nameof(role));

            try
            {
                _repo.Add(role);
                await _repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }

            return IdentityResult.Success;
        }

        // <inheritdoc />
        public async Task<IdentityResult> DeleteAsync(TRoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            try
            {
                var userRoles = await UserRoles.Where(w => w.RoleId.Equals(role.Id)).ToListAsync(cancellationToken);
                foreach (var userRole in userRoles)
                {
                    _repo.Delete(userRole);
                }

                var roleClaims = await RoleClaims.Where(w => w.RoleId.Equals(role.Id)).ToListAsync(cancellationToken);
                foreach (var roleClaim in roleClaims)
                {
                    _repo.Delete(roleClaim);
                }

                _repo.Delete(role);
                await _repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }

            return IdentityResult.Success;
        }

        // <inheritdoc />
        public async Task<TRoleEntity> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(roleId) || string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentNullException(nameof(roleId));

            var role = await _repo.Table<TRoleEntity>()
                .SingleOrDefaultAsync(_ => _.Id.Equals(roleId), cancellationToken: cancellationToken);

            return role;
        }

        // <inheritdoc />
        public async Task<TRoleEntity> FindByNameAsync(string normalizedName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(normalizedName) || string.IsNullOrWhiteSpace(normalizedName))
                throw new ArgumentNullException(nameof(normalizedName));

            var role = await _repo.Table<TRoleEntity>()
                .SingleOrDefaultAsync(_ => _.NormalizedName == normalizedName, cancellationToken: cancellationToken);

            return role;
        }

        // <inheritdoc />
        public Task<string> GetNormalizedRoleNameAsync(TRoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.NormalizedName);
        }

        // <inheritdoc />
        public Task<string> GetRoleIdAsync(TRoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Id.ToString());
        }

        // <inheritdoc />
        public Task<string> GetRoleNameAsync(TRoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.Name);
        }

        // <inheritdoc />
        public Task SetNormalizedRoleNameAsync(TRoleEntity role, string normalizedName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            SetRoleProperty(role, normalizedName, (u, m) => u.NormalizedName = normalizedName, cancellationToken);

            return Task.CompletedTask;
        }

        // <inheritdoc />
        public Task SetRoleNameAsync(TRoleEntity role, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (role == null)
                throw new ArgumentNullException(nameof(role));


            SetRoleProperty(role, roleName, (u, m) => u.Name = roleName, cancellationToken);

            return Task.CompletedTask;
        }

        // <inheritdoc />
        public async Task<IdentityResult> UpdateAsync(TRoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            role.ConcurrencyStamp = Guid.NewGuid().ToString();

            try
            {
                _repo.Update(role);
                await _repo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }

            return IdentityResult.Success;
        }

        private void SetRoleProperty<T>(TRoleEntity user, T value, Action<TRoleEntity, T> setter,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null) throw new ArgumentNullException(nameof(user));
            if (value == null) throw new ArgumentNullException(nameof(value));

            setter(user, value);
        }

        // <inheritdoc />
        public void Dispose()
        {
            _disposed = true;
        }

        #region Methods that implement IRoleClaimStore<TRoleEntity>

        // <inheritdoc />
        public async Task<IList<Claim>> GetClaimsAsync(TRoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var claims = await _repo.Table<IdentityRoleClaim<TKey>>().Where(c => c.RoleId.Equals(role.Id))
                .ToListAsync(cancellationToken);

            return claims.Select(c => c.ToClaim()).ToList();
        }

        // <inheritdoc />
        public async Task AddClaimAsync(TRoleEntity role, Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var nextId = 1;

            try
            {
                var keys = await _repo.Table<IdentityRoleClaim<TKey>>().Select(m => m.Id).ToArrayAsync(); // Convert to int array here.
                nextId = keys.Max() + 1; // Max should now work.
            }
            catch (Exception e)
            {
                var t = e; // for debugging
            }

            var identityRoleClaim = new IdentityRoleClaim<TKey>()
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                RoleId = role.Id,
                Id = nextId
            };

            _repo.Add<IdentityRoleClaim<TKey>>(identityRoleClaim);
            await _repo.SaveChangesAsync().WaitAsync(cancellationToken);
        }

        // <inheritdoc />
        public async Task RemoveClaimAsync(TRoleEntity role, Claim claim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var doomed = await _repo.Table<IdentityRoleClaim<TKey>>()
                .FirstOrDefaultAsync(c => c.RoleId.Equals(role.Id) &&
                                          c.ClaimValue == claim.Value && c.ClaimType == c.ClaimType, cancellationToken);

            _repo.Delete(doomed);
            await _repo.SaveChangesAsync().WaitAsync(cancellationToken);
        }

        #endregion
    }
}