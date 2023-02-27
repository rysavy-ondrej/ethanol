using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ethanol.ContextBuilder.Enrichers
{
    /// <summary>
    /// Interface for implementations of host-related data providers.
    /// </summary>
    /// <typeparam name="T">The type of data records.</typeparam>
    public interface IHostDataProvider<T>
    {
        /// <summary>
        /// Gets the tag records for the given <paramref name="host"/>. 
        /// </summary>
        /// <param name="host">The host identifier.</param>
        /// <param name="start">The start of the interval.</param>
        /// <param name="end">The end of the interval.</param>
        /// <returns>A collection of host tag objects.</returns>
        Task<IEnumerable<T>> GetAsync(string host, DateTime start, DateTime end);
        /// <summary>
        /// Gets the tag records for the given <paramref name="host"/>. 
        /// </summary>
        /// <param name="host">The host identifier.</param>
        /// <param name="start">The start of the interval.</param>
        /// <param name="end">The end of the interval.</param>
        /// <returns>A collection of host tag objects.</returns>
        IEnumerable<T> Get(string host, DateTime start, DateTime end);
    }
}