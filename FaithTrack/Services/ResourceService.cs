// =============================================================
// FaithTrack — Services/ResourceService.cs
// Business logic implementation for Resource operations.
// Defined in the Architecture Plan UML Class Diagram (Fig. 15),
// Application Layer — ResourceService.
// Encapsulates validation, ViewModel↔Entity mapping, and
// delegates all persistence to IResourceRepository.
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Models;
using FaithTrack.Repositories;
using FaithTrack.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FaithTrack.Services
{
    /// <summary>
    /// Concrete implementation of IResourceService.
    /// Injected into ResourceController via DI (Program.cs).
    /// Implements the business rules and mapping logic described
    /// in the Architecture Plan Application Layer (p.7) and
    /// the Add Resource Sequence Diagram (Fig. 16).
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
            // Map each entity to a ViewModel for the Index view
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

            // Business rule validation before mapping to entity
            if (!ValidateResource(vm))
            {
                _logger.LogWarning("ResourceService: Validation failed for Create.");
                return false;
            }

            // Map ViewModel → Entity (Add Resource Workflow, Fig. 16 step 7)
            var entity = MapToEntity(vm, userId);

            // Persist via repository (Fig. 16 step 9 — AddAsync)
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

            // Retrieve the existing entity so we preserve CreatedByUserId
            // and CreatedDate — only user-editable fields are updated.
            var existing = await _resourceRepo.GetByIdAsync(vm.ResourceId);
            if (existing == null)
            {
                _logger.LogWarning(
                    "ResourceService: ResourceId {Id} not found for Update.", vm.ResourceId);
                return false;
            }

            // Update only the user-editable fields
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
        /// Called in GetAllResourcesAsync and GetResourceByIdAsync.
        /// Implements the MapToViewModel method defined in the
        /// UML Class Diagram Application Layer (Fig. 15).
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
        /// Implements the MapToEntity method defined in the
        /// UML Class Diagram Application Layer (Fig. 15).
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
