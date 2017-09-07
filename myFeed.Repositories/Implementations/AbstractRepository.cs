using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations {
    public abstract class AbstractRepository<T> : IAbstractRepository<T> where T : class {
        private readonly DbContext _context;
        protected AbstractRepository(DbContext context) {
            var hasMigrations = context.Database.GetAppliedMigrations().Any();
            if (!hasMigrations) context.Database.Migrate();
            _context = context;
        }

        protected IQueryable<T> GetAllQueryable() {
            return _context.Set<T>().AsQueryable();
        }

        public virtual async Task InsertAsync(T entity) {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task InsertRangeAsync(IEnumerable<T> entities) {
            _context.Set<T>().AddRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task RemoveAsync(T entity) {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<T> entities) {
            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity) {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public virtual Task<IEnumerable<T>> GetAllAsync() {
            var enumerable = GetAllQueryable().AsEnumerable();
            return Task.FromResult(enumerable);
        }

        public void Dispose() {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}