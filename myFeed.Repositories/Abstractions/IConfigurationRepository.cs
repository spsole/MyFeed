﻿using System.Threading.Tasks;

namespace myFeed.Repositories.Abstractions
{
    /// <summary>
    /// Configuration repository.
    /// </summary>
    public interface IConfigurationRepository
    {
        /// <summary>
        /// Finds resource with given name in database and returns it.
        /// </summary>
        Task<string> GetByNameAsync(string name);

        /// <summary>
        /// Inserts new resource into database with given name if not exists,
        /// otherwise finds existing resource in database and updates it
        /// with newer value.
        /// </summary>
        Task SetByNameAsync(string name, string value);
    }
}