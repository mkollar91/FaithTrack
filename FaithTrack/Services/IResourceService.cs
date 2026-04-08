// =============================================================
// FaithTrack — Services/IResourceService.cs
// Service interface for the Resource business logic layer.
// Defined in the Architecture Plan UML Class Diagram (Fig. 15),
// Application Layer — IResourceService.
// Controllers depend on this interface (never the concrete
// class) to enforce loose coupling via DI.
//
// Author  : Matthew Kollar
// Course  : CST-452 — Grand Canyon University
// =============================================================

using FaithTrack.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FaithTrack.Services
{
    /// <summary>
    /// Defines the contract for all Resource business logic
    /// operations. Encapsulates validation, ViewModel-to-Entity
    /// mapping, and delegates persistence to IResourceRepository.
    /// Registered in Program.cs via
    /// AddScoped&lt;IResourceService, ResourceService&gt;().
    /// </summary>
    public interface IResourceService
    {
        /// <summary>
        /// Retrieves all resources owned by the specified user
        /// as a list of ResourceViewModels for the Index view.
        /// Implements the View branch of the CRUD process flow
        /// (Architecture Plan Fig. 2).
        /// </summary>
        /// <param name="userId">The authenticated user's ID.</param>
        /// <returns>ViewModels for each resource the user owns.</returns>
        Task<IEnumerable<ResourceViewModel>> GetAllResourcesAsync(string userId);

        /// <summary>
        /// Retrieves a single resource by ID as a ResourceViewModel.
        /// Returns null if not found.
        /// </summary>
        /// <param name="id">The ResourceId to retrieve.</param>
        /// <returns>The matching ResourceViewModel, or null.</returns>
        Task<ResourceViewModel?> GetResourceByIdAsync(int id);

        /// <summary>
        /// Validates the ViewModel, maps it to a Resource entity,
        /// and persists it via the repository.
        /// Implements the Add branch of the CRUD process flow
        /// and the Add Resource sequence diagram (Fig. 16).
        /// </summary>
        /// <param name="vm">The ResourceViewModel from the Create form.</param>
        /// <param name="userId">The authenticated user's ID.</param>
        /// <returns>True if the resource was saved successfully.</returns>
        Task<bool> CreateResourceAsync(ResourceViewModel vm, string userId);

        /// <summary>
        /// Validates the ViewModel, maps it to the existing entity,
        /// and persists the changes via the repository.
        /// </summary>
        /// <param name="vm">The ResourceViewModel from the Edit form.</param>
        /// <returns>True if the update was saved successfully.</returns>
        Task<bool> UpdateResourceAsync(ResourceViewModel vm);

        /// <summary>
        /// Permanently deletes the resource with the given ID.
        /// Called after the user confirms deletion (Fig. 14 wireframe).
        /// </summary>
        /// <param name="id">The ResourceId to delete.</param>
        Task DeleteResourceAsync(int id);

        /// <summary>
        /// Returns a SelectList of all categories for use in the
        /// Create/Edit form category dropdown (Fig. 11, 12 wireframes).
        /// </summary>
        /// <param name="selectedId">Pre-selected category ID for Edit.</param>
        /// <returns>A SelectList of Category name/value pairs.</returns>
        Task<SelectList> GetCategoriesSelectListAsync(int? selectedId = null);

        /// <summary>
        /// Searches resources by keyword in title or description
        /// for the specified user. Sprint 2 — US-13: Search resources.
        /// Delegates to IResourceRepository.SearchAsync() which uses
        /// EF Core parameterized LIKE queries (SQL injection safe).
        /// </summary>
        /// <param name="userId">The authenticated user's ID.</param>
        /// <param name="query">The keyword search string.</param>
        /// <returns>Matching ResourceViewModels for the user.</returns>
        Task<IEnumerable<ResourceViewModel>> SearchResourcesAsync(string userId, string query);
    }
}
