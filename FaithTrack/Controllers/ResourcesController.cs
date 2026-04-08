// =============================================================
// FaithTrack — Controllers/ResourcesController.cs
// Handles all Resource CRUD HTTP requests and delegates
// business logic to IResourceService.
// Defined in Architecture Plan UML Class Diagram (Fig. 15),
// Presentation Layer — ResourcesController.
// CRUD workflow matches Architecture Plan Fig. 2 (CRUD Process
// Flow) and Fig. 6 (Add Resource UML Activity Diagram).
// Sprint 2: Index action updated to support keyword search (US-13).
//
// Author  : Matthew Kollar
// Course  : CST-452 — Grand Canyon University
// =============================================================

using FaithTrack.Services;
using FaithTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FaithTrack.Controllers
{
    /// <summary>
    /// Manages faith-based resource CRUD operations.
    /// All actions are protected by [Authorize] — unauthenticated
    /// requests are redirected to /Account/Login per the
    /// Authentication Flowchart (Architecture Plan Fig. 1).
    /// Delegates all business logic to IResourceService,
    /// enforcing the separation of concerns defined in the
    /// Layered Architecture (Architecture Plan p.7).
    /// User data isolation is enforced by passing the current
    /// user's ID to every service method (NFR — Authorization, p.26).
    /// Sprint 2: Index supports optional keyword search (US-13).
    /// </summary>
    [Authorize]
    public class ResourcesController : Controller
    {
        // ── Private Fields ───────────────────────────────────

        /// <summary>Business logic layer for resource operations.</summary>
        private readonly IResourceService _resourceService;

        /// <summary>Built-in ASP.NET Core logger.</summary>
        private readonly ILogger<ResourcesController> _logger;

        // ── Constructor ──────────────────────────────────────

        /// <summary>
        /// Initialises the controller with the resource service
        /// and logger provided by dependency injection.
        /// </summary>
        public ResourcesController(
            IResourceService resourceService,
            ILogger<ResourcesController> logger)
        {
            _resourceService = resourceService;
            _logger          = logger;
        }

        // ── INDEX — View Resource List ───────────────────────

        /// <summary>
        /// GET /Resources/Index
        /// Retrieves resources for the current authenticated user
        /// with optional keyword search filtering.
        /// Sprint 1 — US-4: View list of resources.
        /// Sprint 2 — US-13: Search resources by keyword.
        /// If a search query is provided, delegates to
        /// SearchResourcesAsync; otherwise returns all resources.
        /// </summary>
        /// <param name="search">Optional keyword search string.</param>
        public async Task<IActionResult> Index(string? search)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation(
                "ResourceController: Index requested by user {UserId}, search='{Search}'.",
                userId, search);

            IEnumerable<ResourceViewModel> resources;

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Sprint 2 — US-13: keyword search
                resources = await _resourceService.SearchResourcesAsync(userId, search);
                ViewData["CurrentSearch"] = search;
            }
            else
            {
                resources = await _resourceService.GetAllResourcesAsync(userId);
            }

            return View(resources);
        }

        // ── DETAILS — View Single Resource ───────────────────

        /// <summary>
        /// GET /Resources/Details/{id}
        /// Displays read-only detail for a single resource.
        /// Implements the View Resource wireframe (Architecture Plan Fig. 13).
        /// Returns 404 if the resource is not found.
        /// </summary>
        /// <param name="id">The ResourceId to display.</param>
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation(
                "ResourceController: Details requested for ResourceId {Id}.", id);

            var vm = await _resourceService.GetResourceByIdAsync(id);
            if (vm == null)
            {
                _logger.LogWarning(
                    "ResourceController: ResourceId {Id} not found.", id);
                return NotFound();
            }
            return View(vm);
        }

        // ── CREATE — Add New Resource ─────────────────────────

        /// <summary>
        /// GET /Resources/Create
        /// Returns the Create Resource form with the Categories
        /// SelectList populated for the dropdown.
        /// Implements the Add Resource wireframe (Architecture Plan Fig. 11).
        /// </summary>
        public async Task<IActionResult> Create()
        {
            _logger.LogInformation("ResourceController: Create GET requested.");

            var vm = new ResourceViewModel
            {
                Categories = await _resourceService.GetCategoriesSelectListAsync()
            };
            return View(vm);
        }

        /// <summary>
        /// POST /Resources/Create
        /// Validates the submitted form, calls the service to
        /// create the resource, and redirects to the Index view.
        /// Implements the Add branch of the CRUD process flow (Fig. 2)
        /// and the Add Resource Sequence Diagram (Architecture Plan Fig. 16).
        /// [ValidateAntiForgeryToken] prevents CSRF (NFR Security p.26).
        /// </summary>
        /// <param name="vm">The bound ResourceViewModel from the form.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResourceViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = await _resourceService.GetCategoriesSelectListAsync(vm.CategoryId);
                return View(vm);
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation(
                "ResourceController: Create POST for '{Title}' by user {UserId}.",
                vm.Title, userId);

            var success = await _resourceService.CreateResourceAsync(vm, userId);
            if (!success)
            {
                ModelState.AddModelError(string.Empty,
                    "An error occurred saving the resource. Please try again.");
                vm.Categories = await _resourceService.GetCategoriesSelectListAsync(vm.CategoryId);
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Resource '{vm.Title}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT — Update Existing Resource ──────────────────

        /// <summary>
        /// GET /Resources/Edit/{id}
        /// Returns the Edit form pre-populated with the existing
        /// resource data. Returns 404 if not found.
        /// Implements the Edit Resource wireframe (Architecture Plan Fig. 12).
        /// </summary>
        /// <param name="id">The ResourceId to edit.</param>
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation(
                "ResourceController: Edit GET for ResourceId {Id}.", id);

            var vm = await _resourceService.GetResourceByIdAsync(id);
            if (vm == null) return NotFound();

            vm.Categories = await _resourceService.GetCategoriesSelectListAsync(vm.CategoryId);
            return View(vm);
        }

        /// <summary>
        /// POST /Resources/Edit/{id}
        /// Validates the submitted form and calls the service to
        /// update the resource. Redirects to Index on success.
        /// Implements the Edit branch of the CRUD process flow (Fig. 2).
        /// </summary>
        /// <param name="id">The ResourceId being edited.</param>
        /// <param name="vm">The bound ResourceViewModel from the form.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResourceViewModel vm)
        {
            if (id != vm.ResourceId) return BadRequest();

            if (!ModelState.IsValid)
            {
                vm.Categories = await _resourceService.GetCategoriesSelectListAsync(vm.CategoryId);
                return View(vm);
            }

            _logger.LogInformation(
                "ResourceController: Edit POST for ResourceId {Id}.", id);

            var success = await _resourceService.UpdateResourceAsync(vm);
            if (!success)
            {
                ModelState.AddModelError(string.Empty,
                    "An error occurred updating the resource.");
                vm.Categories = await _resourceService.GetCategoriesSelectListAsync(vm.CategoryId);
                return View(vm);
            }

            TempData["SuccessMessage"] = $"Resource '{vm.Title}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — Remove Resource ──────────────────────────

        /// <summary>
        /// GET /Resources/Delete/{id}
        /// Returns the Delete Confirmation page with the resource
        /// details displayed. Returns 404 if not found.
        /// Implements the Delete Confirmation wireframe (Architecture Plan Fig. 14).
        /// </summary>
        /// <param name="id">The ResourceId to delete.</param>
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation(
                "ResourceController: Delete GET for ResourceId {Id}.", id);

            var vm = await _resourceService.GetResourceByIdAsync(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        /// <summary>
        /// POST /Resources/Delete/{id}
        /// Permanently deletes the resource after user confirmation
        /// and redirects to the Index view.
        /// Implements the Delete branch of the CRUD process flow (Fig. 2).
        /// </summary>
        /// <param name="id">The ResourceId to permanently delete.</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation(
                "ResourceController: DeleteConfirmed for ResourceId {Id}.", id);

            await _resourceService.DeleteResourceAsync(id);

            TempData["SuccessMessage"] = "Resource deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── Private Helpers ──────────────────────────────────

        /// <summary>
        /// Returns the authenticated user's ID string from the
        /// current ClaimsPrincipal. Used to scope all data access
        /// to the current user (NFR — Authorization, p.26).
        /// </summary>
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
    }
}
