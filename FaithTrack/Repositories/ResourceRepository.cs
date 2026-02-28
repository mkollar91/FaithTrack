// =============================================================
// FaithTrack — Repositories/ResourceRepository.cs
// EF Core implementation of IResourceRepository.
// Defined in the Architecture Plan UML Class Diagram (Fig. 15),
// Data Access Layer — ResourceRepository.
// All database operations use async/await and EF Core LINQ
// (parameterized queries) per NFR — Security (p.26).
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Data;
using FaithTrack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FaithTrack.Repositories
{
    /// <summary>
    /// Concrete EF Core implementation of IResourceRepository.
    /// Uses FaithTrackDbContext to perform all CRUD operations
    /// on the Resources table. Injected into ResourceService
    /// via the constructor by the DI container (Program.cs).
    /// </summary>
    public class ResourceRepository : IResourceRepository
    {
        // ── Private Fields ───────────────────────────────────

        /// <summary>EF Core database context.</summary>
        private readonly FaithTrackDbContext _context;

        /// <summary>Built-in ASP.NET Core logger.</summary>
        private readonly ILogger<ResourceRepository> _logger;

        // ── Constructor ──────────────────────────────────────

        /// <summary>
        /// Initialises the repository with the DbContext and logger
        /// provided by dependency injection.
        /// </summary>
        /// <param name="context">The EF Core DbContext.</param>
        /// <param name="logger">The ASP.NET Core logger.</param>
        public ResourceRepository(
            FaithTrackDbContext context,
            ILogger<ResourceRepository> logger)
        {
            _context = context;
            _logger  = logger;
        }

        // ── Interface Implementation ─────────────────────────

        /// <inheritdoc/>
        public async Task<IEnumerable<Resource>> GetAllAsync()
        {
            _logger.LogInformation("ResourceRepository: GetAllAsync called.");

            // Include Category so the category name is available
            // in the Resources List view without a second query.
            return await _context.Resources
                .Include(r => r.Category)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Resource>> GetByUserIdAsync(string userId)
        {
            _logger.LogInformation(
                "ResourceRepository: GetByUserIdAsync called for user {UserId}.", userId);

            // Filter by CreatedByUserId to enforce user data isolation
            // per Architecture Plan NFR — Authorization (p.26).
            return await _context.Resources
                .Include(r => r.Category)
                .Where(r => r.CreatedByUserId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Resource?> GetByIdAsync(int id)
        {
            _logger.LogInformation(
                "ResourceRepository: GetByIdAsync called for ResourceId {Id}.", id);

            return await _context.Resources
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.ResourceId == id);
        }

        /// <inheritdoc/>
        public async Task AddAsync(Resource resource)
        {
            _logger.LogInformation(
                "ResourceRepository: AddAsync called for resource '{Title}'.", resource.Title);

            await _context.Resources.AddAsync(resource);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(Resource resource)
        {
            _logger.LogInformation(
                "ResourceRepository: UpdateAsync called for ResourceId {Id}.", resource.ResourceId);

            // Set UpdatedDate timestamp before saving
            resource.UpdatedDate = DateTime.UtcNow;

            _context.Resources.Update(resource);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation(
                "ResourceRepository: DeleteAsync called for ResourceId {Id}.", id);

            var resource = await _context.Resources.FindAsync(id);
            if (resource != null)
            {
                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Resource>> SearchAsync(string userId, string query)
        {
            _logger.LogInformation(
                "ResourceRepository: SearchAsync called for user {UserId}, query '{Query}'.",
                userId, query);

            // EF Core LINQ .Contains() generates a parameterized
            // LIKE query — safe against SQL injection (NFR p.26).
            var lower = query.ToLower();
            return await _context.Resources
                .Include(r => r.Category)
                .Where(r => r.CreatedByUserId == userId &&
                           (r.Title.ToLower().Contains(lower) ||
                           (r.Description != null && r.Description.ToLower().Contains(lower))))
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }
    }
}
