using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace myFeed.Repositories.Implementations {
    /// <summary>
    /// Abstract repository layer to work with EntityFramework ORM.
    /// </summary>
    /// <typeparam name="T">Database set data type.</typeparam>
    public abstract class AbstractRepository<T> : IAbstractRepository<T> where T : class {
        private readonly DbContext _context;

        /// <summary>
        /// Initializes new abstract repository.
        /// </summary>
        protected AbstractRepository(DbContext context) {
            var hasMigrations = context.Database.GetAppliedMigrations().Any();
            if (!hasMigrations) context.Database.Migrate();
            _context = context;
        }

        /// <summary>
        /// Returns queryable of all items in given Set.
        /// </summary>
        protected IQueryable<T> GetAllQueryable() {
            return _context.Set<T>().AsQueryable();
        }

        /// <summary>
        /// Inserts entity and saves changes.
        /// </summary>
        /// <param name="entity">Entity to insert.</param>
        public virtual async Task InsertAsync(T entity) {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Inserts range and saves changes.
        /// </summary>
        /// <param name="entities">Entities to insert.</param>
        public virtual async Task InsertRangeAsync(IEnumerable<T> entities) {
            _context.Set<T>().AddRange(entities);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes entity and saves changes.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        public virtual async Task RemoveAsync(T entity) {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Removes range and saves changes.
        /// </summary>
        /// <param name="entities">Entities to remove.</param>
        public virtual async Task RemoveRangeAsync(IEnumerable<T> entities) {
            _context.Set<T>().RemoveRange(entities);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates entry by changing its state to modified and saves changes.
        /// </summary>
        /// <param name="entity">Entity to operate.</param>
        public virtual async Task UpdateAsync(T entity) {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Queries all items in Set and returns it's enumerable.
        /// </summary>
        /// <returns></returns>
        public virtual Task<IEnumerable<T>> GetAllAsync() {
            var enumerable = GetAllQueryable().AsEnumerable();
            return Task.FromResult(enumerable);
        }

        /// <summary>
        /// Disposes abstract repository.
        /// </summary>
        public void Dispose() {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}