// =============================================================
// FaithTrack — Services/ResourceService.cs
// Business logic implementation for Resource operations.
// Defined in the Architecture Plan UML Class Diagram (Fig. 15),
// Application Layer — ResourceService.
// Encapsulates validation, ViewModel↔Entity mapping, and
// delegates all persistence to IResourceRepository.
// Sprint 2: SearchResourcesAsync added for US-13.
//
// Author  : Matthew Kollar
// Course  : CST-452 — Grand Canyon University
// =============================================================

using FaithTrack.Models;
using FaithTrack.Repositories;
using FaithTrack.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FaithTrack.Services
{
    /// <summary>
    /// Concrete implementation of IResourceService.
    /// Injected into ResourcesController via DI (Program.cs).
    /// Implements the business rules and mapping logic described
    /// in the Architecture Plan Application Layer (p.7) and
    /// the Add Resource Sequence Diagram (Fig. 16).
    /// Sprint 2 adds SearchResourcesAsync for keyword search (US-13).
    /// </summary>
    public class ResourceService : IResourceService
    {
        // ── Private Fields ───────────────────────────────────

        /// <summary>Data access abstraction for Resource records.</summary>
        private readonly IResourceRepository _resourceRepo;

        /// <summary>Data access abstraction for Category records.</summary>
        private readonly ICategoryRepository _categoryRepo;

        /// <summary>Built-in ASP.NET Core logger.</summary>
        private readonly ILogger<ResourceService> _logger;

        // ── Constructor ──────────────────────────────────────

        /// <summary>
        /// Initialises the service with repository and logger
        /// instances provided by dependency injection.
        /// </summary>
        public ResourceService(
            IResourceRepository resourceRepo,
            ICategoryRepository categoryRepo,
            ILogger<ResourceService> logger)
        {
            _resourceRepo = resourceRepo;
            _categoryRepo = categoryRepo;
            _logger       = logger;
        }

        // ── Public Methods ───────────────────────────────────

        /// <inheritdoc/>
        public async Task<IEnumerable<ResourceViewModel>> GetAllResourcesAsync(string userId)
        {
            _logger.LogInformation(
                "ResourceService: GetAllResourcesAsync for user {UserId}.", userId);

            var resources = await _resourceRepo.GetByUserIdAsync(userId);
            return resources.Select(r => MapToViewModel(r));
        }

        /// <inheritdoc/>
        public async Task<ResourceViewModel?> GetResourceByIdAsync(int id)
        {
            _logger.LogInformation(
                "ResourceService: GetResourceByIdAsync for ResourceId {Id}.", id);

            var resource = await _resourceRepo.GetByIdAsync(id);
            if (resource == null)
            {
                _logger.LogWarning(
                    "ResourceService: ResourceId {Id} not found.", id);
                return null;
            }
            return MapToViewModel(resource);
        }

        /// <inheritdoc/>
        public async Task<bool> CreateResourceAsync(ResourceViewModel vm, string userId)
        {
            _logger.LogInformation(
                "ResourceService: CreateResourceAsync called for '{Title}'.", vm.Title);

            if (!ValidateResource(vm))
            {
                _logger.LogWarning("ResourceService: Validation failed for Create.");
                return false;
            }

            var entity = MapToEntity(vm, userId);
            await _resourceRepo.AddAsync(entity);

            _logger.LogInformation(
                "ResourceService: Resource '{Title}' created successfully.", vm.Title);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateResourceAsync(ResourceViewModel vm)
        {
            _logger.LogInformation(
                "ResourceService: UpdateResourceAsync for ResourceId {Id}.", vm.ResourceId);

            if (!ValidateResource(vm))
            {
                _logger.LogWarning("ResourceService: Validation failed for Update.");
                return false;
            }

            var existing = await _resourceRepo.GetByIdAsync(vm.ResourceId);
            if (existing == null)
            {
                _logger.LogWarning(
                    "ResourceService: ResourceId {Id} not found for Update.", vm.ResourceId);
                return false;
            }

            existing.Title       = vm.Title;
            existing.Description = vm.Description;
            existing.Url         = vm.Url;
            existing.CategoryId  = vm.CategoryId;
            existing.UpdatedDate = DateTime.UtcNow;

            await _resourceRepo.UpdateAsync(existing);

            _logger.LogInformation(
                "ResourceService: ResourceId {Id} updated successfully.", vm.ResourceId);
            return true;
        }

        /// <inheritdoc/>
        public async Task DeleteResourceAsync(int id)
        {
            _logger.LogInformation(
                "ResourceService: DeleteResourceAsync for ResourceId {Id}.", id);

            await _resourceRepo.DeleteAsync(id);
        }

        /// <inheritdoc/>
        public async Task<SelectList> GetCategoriesSelectListAsync(int? selectedId = null)
        {
            var categories = await _categoryRepo.GetAllAsync();
            return new SelectList(categories, "CategoryId", "Name", selectedId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ResourceViewModel>> SearchResourcesAsync(
            string userId, string query)
        {
            _logger.LogInformation(
                "ResourceService: SearchResourcesAsync for user {UserId}, query '{Query}'.",
                userId, query);

            // Delegates to repository SearchAsync which uses EF Core
            // parameterized LIKE query — safe against SQL injection (NFR p.26).
            var resources = await _resourceRepo.SearchAsync(userId, query);
            return resources.Select(r => MapToViewModel(r));
        }

        // ── Private Helpers ──────────────────────────────────

        /// <summary>
        /// Validates required business rules on the ResourceViewModel
        /// before allowing persistence. Returns false if any rule fails.
        /// Title and CategoryId are required per the Data Dictionary
        /// and Add Resource wireframe (Architecture Plan Fig. 11).
        /// </summary>
        /// <param name="vm">The ViewModel to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        private static bool ValidateResource(ResourceViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Title)) return false;
            if (vm.CategoryId <= 0) return false;
            return true;
        }

        /// <summary>
        /// Maps a Resource domain entity to a ResourceViewModel
        /// for use in the Presentation Layer views.
        /// </summary>
        /// <param name="entity">The Resource entity to map.</param>
        /// <returns>A populated ResourceViewModel.</returns>
        private static ResourceViewModel MapToViewModel(Resource entity)
        {
            return new ResourceViewModel
            {
                ResourceId      = entity.ResourceId,
                Title           = entity.Title,
                Description     = entity.Description,
                Url             = entity.Url,
                CategoryId      = entity.CategoryId,
                CategoryName    = entity.Category?.Name,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedDate     = entity.CreatedDate,
                UpdatedDate     = entity.UpdatedDate
            };
        }

        /// <summary>
        /// Maps a ResourceViewModel to a new Resource entity
        /// for persistence. Sets CreatedByUserId and CreatedDate.
        /// </summary>
        /// <param name="vm">The ViewModel to map.</param>
        /// <param name="userId">The authenticated user's ID.</param>
        /// <returns>A new Resource entity ready for AddAsync.</returns>
        private static Resource MapToEntity(ResourceViewModel vm, string userId)
        {
            return new Resource
            {
                Title           = vm.Title,
                Description     = vm.Description,
                Url             = vm.Url,
                CategoryId      = vm.CategoryId,
                CreatedByUserId = userId,
                CreatedDate     = DateTime.UtcNow
            };
        }
    }
}
