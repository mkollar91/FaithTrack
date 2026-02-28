// =============================================================
// FaithTrack — Services/CategoryService.cs
// Business logic implementation for Category operations.
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Models;
using FaithTrack.Repositories;

namespace FaithTrack.Services
{
    /// <summary>
    /// Concrete implementation of ICategoryService.
    /// Sprint 1: provides GetAllCategoriesAsync for resource dropdowns.
    /// Sprint 2: full CRUD for CategoryController (US 8–11).
    /// </summary>
    public class CategoryService : ICategoryService
    {
        /// <summary>Data access abstraction for Category records.</summary>
        private readonly ICategoryRepository _categoryRepo;

        /// <summary>Built-in logger.</summary>
        private readonly ILogger<CategoryService> _logger;

        /// <summary>
        /// Initialises the service via dependency injection.
        /// </summary>
        public CategoryService(
            ICategoryRepository categoryRepo,
            ILogger<CategoryService> logger)
        {
            _categoryRepo = categoryRepo;
            _logger       = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepo.GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepo.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<bool> CreateCategoryAsync(Category category)
        {
            // Sprint 2 — US-9
            if (string.IsNullOrWhiteSpace(category.Name)) return false;
            await _categoryRepo.AddAsync(category);
            _logger.LogInformation(
                "CategoryService: Category '{Name}' created.", category.Name);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            // Sprint 2 — US-10
            if (string.IsNullOrWhiteSpace(category.Name)) return false;
            await _categoryRepo.UpdateAsync(category);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            // Sprint 2 — US-11
            // Block deletion if any resources reference this category
            // (FK ON DELETE NO ACTION per DDL, Architecture Plan p.14)
            if (await _categoryRepo.HasResourcesAsync(id))
            {
                _logger.LogWarning(
                    "CategoryService: Cannot delete CategoryId {Id} — resources exist.", id);
                return false;
            }
            await _categoryRepo.DeleteAsync(id);
            return true;
        }
    }
}
