using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetCore.Identity.CosmosDb.Contracts;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AspNetCore.Identity.CosmosDb.Repositories
{
    public class CosmosIdentityRepository<TDbContext, TUserEntity, TRoleEntity> : IRepository
        where TDbContext : CosmosIdentityDbContext<TUserEntity, TRoleEntity>
        where TUserEntity : IdentityUser
        where TRoleEntity : IdentityRole
    {
        protected TDbContext _db;

        public IQueryable Users
        {
            get { return _db.Users.AsQueryable(); }
        }

        public IQueryable Roles
        {
            get { return _db.Roles.AsQueryable(); }
        }

        public CosmosIdentityRepository(TDbContext db)
        {
            _db = db;
        }

        public DbSet<TEntity> Table<TEntity>() where TEntity : class, new()
        {
            return _db.Set<TEntity>();
        }

        public TEntity GetById<TEntity>(string id) where TEntity : class, new()
        {
            return _db.Set<TEntity>().WithPartitionKey(id).Single();
        }

        public TEntity TryFindOne<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, new()
        {
            return _db.Set<TEntity>().SingleOrDefault(predicate);
        }

        public IQueryable<TEntity> Find<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, new()
        {
            return _db.Set<TEntity>().Where(predicate);
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class, new()
        {
            _db.Add(entity);
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class, new()
        {
            var dbEntry = _db.Entry(entity);
            dbEntry.State = EntityState.Modified;
        }

        public void DeleteById<TEntity>(string id) where TEntity : class, new()
        {
            var entity = GetById<TEntity>(id);
            Delete(entity);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class, new()
        {
            _db.Remove(entity);
        }

        public void Delete<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class, new()
        {
            var entities = _db.Set<TEntity>().Where(predicate).ToList();
            entities.ForEach(entity => _db.Remove(entity));
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}