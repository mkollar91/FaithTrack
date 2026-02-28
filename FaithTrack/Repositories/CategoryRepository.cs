// =============================================================
// FaithTrack — Repositories/CategoryRepository.cs
// EF Core implementation of ICategoryRepository.
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Data;
using FaithTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FaithTrack.Repositories
{
    /// <summary>
    /// Concrete EF Core implementation of ICategoryRepository.
    /// Injected into CategoryService via DI (Program.cs).
    /// Sprint 1 active methods: GetAllAsync, GetByIdAsync.
    /// Sprint 2 methods stubbed and ready for implementation.
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        /// <summary>EF Core database context.</summary>
        private readonly FaithTrackDbContext _context;

        /// <summary>
        /// Initialises the repository with the DbContext
        /// provided by dependency injection.
        /// </summary>
        /// <param name="context">The EF Core DbContext.</param>
        public CategoryRepository(FaithTrackDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            // Order alphabetically so the dropdown list is predictable
            return await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Category category)
        {
            // Sprint 2 — US-9: Create Category
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Category category)
        {
            // Sprint 2 — US-10: Edit Category
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            // Sprint 2 — US-11: Delete Category
            // Caller (CategoryService) must call HasResourcesAsync first
            // to prevent FK constraint violation.
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HasResourcesAsync(int categoryId)
        {
            // Sprint 2 — US-11: used before deleting a category
            return await _context.Resources
                .AnyAsync(r => r.CategoryId == categoryId);
        }
    }
}
