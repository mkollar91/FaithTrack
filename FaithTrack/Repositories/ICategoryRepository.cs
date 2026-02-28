// =============================================================
// FaithTrack — Repositories/ICategoryRepository.cs
// Repository interface for Category data access operations.
// Defined in the Architecture Plan UML Class Diagram (Fig. 15),
// Data Access Layer — ICategoryRepository.
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Models;

namespace FaithTrack.Repositories
{
    /// <summary>
    /// Defines the contract for all Category persistence operations.
    /// The concrete implementation (CategoryRepository) uses EF Core.
    /// Sprint 1: GetAllAsync and GetByIdAsync are used to populate
    /// the category dropdown on Resource Create/Edit forms.
    /// Full CRUD methods are implemented for Sprint 2 (US 8–11).
    /// </summary>
    public interface ICategoryRepository
    {
        /// <summary>
        /// Retrieves all categories ordered by name.
        /// Used to populate the category SelectList on the
        /// Resource Create/Edit forms (Architecture Plan Fig. 11, 12).
        /// </summary>
        /// <returns>All categories in the system.</returns>
        Task<IEnumerable<Category>> GetAllAsync();

        /// <summary>
        /// Retrieves a single category by its primary key.
        /// Returns null if no matching record exists.
        /// </summary>
        /// <param name="id">The CategoryId primary key.</param>
        /// <returns>The matching category, or null.</returns>
        Task<Category?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new category to the database. Sprint 2 — US-9.
        /// </summary>
        /// <param name="category">The category entity to persist.</param>
        Task AddAsync(Category category);

        /// <summary>
        /// Updates an existing category. Sprint 2 — US-10.
        /// </summary>
        /// <param name="category">The modified category entity.</param>
        Task UpdateAsync(Category category);

        /// <summary>
        /// Permanently deletes a category by ID. Sprint 2 — US-11.
        /// Caller must verify no resources are assigned first
        /// (FK ON DELETE NO ACTION per DDL script, Architecture Plan p.14).
        /// </summary>
        /// <param name="id">The CategoryId to delete.</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Returns true if any resources are currently assigned
        /// to the specified category. Used to block deletion of
        /// categories with active resources (Sprint 2 — US-11).
        /// </summary>
        /// <param name="categoryId">The category to check.</param>
        /// <returns>True if at least one resource uses this category.</returns>
        Task<bool> HasResourcesAsync(int categoryId);
    }
}
