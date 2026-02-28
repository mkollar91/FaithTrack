// =============================================================
// FaithTrack — Services/ICategoryService.cs
// Service interface for Category business logic.
// Defined in Architecture Plan UML Class Diagram (Fig. 15),
// Application Layer — ICategoryService.
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Models;

namespace FaithTrack.Services
{
    /// <summary>
    /// Defines the contract for Category business logic operations.
    /// Sprint 1: GetAllCategoriesAsync used to populate dropdowns.
    /// Sprint 2: Full CRUD methods for US-8 through US-11.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Retrieves all categories ordered alphabetically.
        /// Used in Sprint 1 to populate the resource form dropdown.
        /// </summary>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Retrieves a single category by ID. Returns null if not found.
        /// </summary>
        Task<Category?> GetCategoryByIdAsync(int id);

        /// <summary>
        /// Creates a new category. Sprint 2 — US-9.
        /// </summary>
        Task<bool> CreateCategoryAsync(Category category);

        /// <summary>
        /// Updates an existing category. Sprint 2 — US-10.
        /// </summary>
        Task<bool> UpdateCategoryAsync(Category category);

        /// <summary>
        /// Deletes a category if no resources are assigned.
        /// Returns false and does not delete if resources exist.
        /// Sprint 2 — US-11. FK ON DELETE NO ACTION per DDL (p.14).
        /// </summary>
        Task<bool> DeleteCategoryAsync(int id);
    }
}
