using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Repositories.Abstractions {
    /// <summary>
    /// Represents generic repository supporting basic CRUD operations.
    /// </summary>
    /// <typeparam name="T">DB entity type.</typeparam>
    public interface IAbstractRepository<T> : IDisposable {
        /// <summary>
        /// Adds entity to repository.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        Task InsertAsync(T entity);

        /// <summary>
        /// Adds entities to repository.
        /// </summary>
        /// <param name="entities">Entities to add.</param>
        Task InsertRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Removes entity from repository.
        /// </summary>
        /// <param name="entity">Entity to remove.</param>
        Task RemoveAsync(T entity);

        /// <summary>
        /// Removes range of entities from database set.
        /// </summary>
        /// <param name="entities">Entities to remove.</param>
        Task RemoveRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Updates entity in repository.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Returns sequence of all entities in table.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();
    }
}
