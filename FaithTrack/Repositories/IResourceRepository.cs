// =============================================================
// FaithTrack — Repositories/IResourceRepository.cs
// Repository interface for Resource data access operations.
// Defined in the Architecture Plan UML Class Diagram (Fig. 15),
// Data Access Layer — IResourceRepository.
// Abstracts EF Core from the service layer, enabling loose
// coupling and testability as described in the Architecture
// Plan Detailed Technical Design (p.11).
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Models;

namespace FaithTrack.Repositories
{
    /// <summary>
    /// Defines the contract for all Resource persistence operations.
    /// The concrete implementation (ResourceRepository) uses EF Core
    /// and FaithTrackDbContext. Registered in Program.cs via
    /// AddScoped&lt;IResourceRepository, ResourceRepository&gt;().
    /// </summary>
    public interface IResourceRepository
    {
        /// <summary>
        /// Retrieves all resources from the database, including
        /// their associated Category navigation property.
        /// </summary>
        /// <returns>An enumerable collection of all resources.</returns>
        Task<IEnumerable<Resource>> GetAllAsync();

        /// <summary>
        /// Retrieves all resources created by a specific user,
        /// including their associated Category. Used in the
        /// ResourceController.Index() to enforce user data isolation
        /// per Architecture Plan NFR — Authorization (p.26).
        /// </summary>
        /// <param name="userId">The ASP.NET Identity user ID.</param>
        /// <returns>Resources owned by the specified user.</returns>
        Task<IEnumerable<Resource>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Retrieves a single resource by its primary key,
        /// including the Category navigation property.
        /// Returns null if no matching record exists.
        /// </summary>
        /// <param name="id">The ResourceId primary key.</param>
        /// <returns>The matching resource, or null.</returns>
        Task<Resource?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new resource to the database and saves changes.
        /// Implements the INSERT step in the Add Resource Workflow
        /// (Architecture Plan Fig. 6 — UML Activity Diagram).
        /// </summary>
        /// <param name="resource">The resource entity to persist.</param>
        Task AddAsync(Resource resource);

        /// <summary>
        /// Updates an existing resource in the database and saves changes.
        /// Sets UpdatedDate on the entity before committing.
        /// </summary>
        /// <param name="resource">The modified resource entity.</param>
        Task UpdateAsync(Resource resource);

        /// <summary>
        /// Permanently deletes a resource from the database by ID.
        /// Called after the user confirms deletion on the Delete
        /// Confirmation page (Architecture Plan Fig. 14 wireframe).
        /// </summary>
        /// <param name="id">The ResourceId of the record to delete.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Searches resources by title or description keyword for
        /// the specified user. Supports User Story 13 (Sprint 2).
        /// </summary>
        /// <param name="userId">The authenticated user's ID.</param>
        /// <param name="query">The keyword search string.</param>
        /// <returns>Matching resources for the user.</returns>
        Task<IEnumerable<Resource>> SearchAsync(string userId, string query);
    }
}
