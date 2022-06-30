using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetCore.Identity.CosmosDb.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace AspNetCore.Identity.CosmosDb.Stores
{
    /// <summary>
    /// Cosmos DB Role Store
    /// </summary>
    /// <typeparam name="TRoleEntity"></typeparam>
    public class CosmosRoleStore<TRoleEntity> : IRoleStore<TRoleEntity>,
        IRoleClaimStore<TRoleEntity> where TRoleEntity : IdentityRole, new()
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
                .SingleOrDefaultAsync(_ => _.Id == roleId, cancellationToken: cancellationToken);

            return role;
        }

        // <inheritdoc />
        public async Task<TRoleEntity> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
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

            return Task.FromResult(role.Id);
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
        public Task SetNormalizedRoleNameAsync(TRoleEntity role, string normalizedName, CancellationToken cancellationToken = default)
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

        private void SetRoleProperty<T>(TRoleEntity user, T value, Action<TRoleEntity, T> setter, CancellationToken cancellationToken = default)
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

            var claims = await _repo.Table<IdentityRoleClaim<string>>().Where(c => c.RoleId == role.Id).ToListAsync(cancellationToken);

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
                nextId = (await _repo.Table<IdentityRoleClaim<string>>().MaxAsync(m => m.Id)) + 1;
            }
            catch (Exception e)
            {
                var t = e; // for debugging
            }

            var identityRoleClaim = new IdentityRoleClaim<string>()
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                RoleId = role.Id,
                Id = nextId
            };

            _repo.Add<IdentityRoleClaim<string>>(identityRoleClaim);
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

            var doomed = await _repo.Table<IdentityRoleClaim<string>>()
                .FirstOrDefaultAsync(c => c.RoleId == role.Id &&
                    c.ClaimValue == claim.Value && c.ClaimType == c.ClaimType, cancellationToken);

            _repo.Delete(doomed);
            await _repo.SaveChangesAsync().WaitAsync(cancellationToken);
        }

        #endregion
    }
}