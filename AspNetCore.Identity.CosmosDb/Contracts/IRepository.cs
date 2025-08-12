using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AspNetCore.Identity.CosmosDb.Contracts
{
    /// <summary>
    /// Cosmos Repository interface
    /// </summary>
    public interface IRepository
    {
        string ProviderName { get; }

        IQueryable Users { get; }
        IQueryable Roles { get; }

        IQueryable UserClaims { get; }
        IQueryable UserRoles { get; }

        IQueryable UserLogins { get; }
        IQueryable RoleClaims { get; }
        IQueryable UserTokens { get; }

        DbSet<TEntity> Table<TEntity>() where TEntity : class, new();

        TEntity GetById<TEntity>(string id) where TEntity : class, new();

        TEntity TryFindOne<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, new();

        IQueryable<TEntity> Find<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, new();

        void Add<TEntity>(TEntity entity) where TEntity : class, new();

        void Update<TEntity>(TEntity entity) where TEntity : class, new();

        void DeleteById<TEntity>(string id) where TEntity : class, new();

        void Delete<TEntity>(TEntity entity) where TEntity : class, new();

        void Delete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, new();

        Task SaveChangesAsync();
    }
}