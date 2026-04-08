// =============================================================
// FaithTrack — Controllers/CategoryController.cs
// Handles all Category CRUD HTTP requests and delegates
// business logic to ICategoryService.
// Sprint 2 — User Stories 8–11:
//   US-8:  View list of categories
//   US-9:  Add a new category
//   US-10: Edit an existing category
//   US-11: Delete a category (with resource protection)
//
// Author  : Matthew Kollar
// Course  : CST-452 — Grand Canyon University
// =============================================================

using FaithTrack.Models;
using FaithTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithTrack.Controllers
{
    /// <summary>
    /// Manages Category CRUD operations.
    /// All actions are protected by [Authorize] — unauthenticated
    /// requests are redirected to /Account/Login.
    /// Delegates all business logic to ICategoryService.
    /// </summary>
    [Authorize]
    public class CategoryController : Controller
    {
        // ── Private Fields ───────────────────────────────────

        /// <summary>Business logic layer for category operations.</summary>
        private readonly ICategoryService _categoryService;

        /// <summary>Built-in ASP.NET Core logger.</summary>
        private readonly ILogger<CategoryController> _logger;

        // ── Constructor ──────────────────────────────────────

        /// <summary>
        /// Initialises the controller with the category service
        /// and logger provided by dependency injection.
        /// </summary>
        public CategoryController(
            ICategoryService categoryService,
            ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger          = logger;
        }

        // ── INDEX — View Category List ────────────────────────

        /// <summary>
        /// GET /Category/Index
        /// Retrieves all categories and returns the Category
        /// List view. Implements US-8: View list of categories.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("CategoryController: Index requested.");
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        // ── DETAILS — View Single Category ───────────────────

        /// <summary>
        /// GET /Category/Details/{id}
        /// Displays read-only detail for a single category.
        /// Returns 404 if the category is not found.
        /// </summary>
        /// <param name="id">The CategoryId to display.</param>
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation(
                "CategoryController: Details requested for CategoryId {Id}.", id);

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // ── CREATE — Add New Category ─────────────────────────

        /// <summary>
        /// GET /Category/Create
        /// Returns the Create Category form.
        /// Implements US-9: Add a new category.
        /// </summary>
        public IActionResult Create()
        {
            _logger.LogInformation("CategoryController: Create GET requested.");
            return View();
        }

        /// <summary>
        /// POST /Category/Create
        /// Validates the submitted form, calls the service to
        /// create the category, and redirects to Index on success.
        /// [ValidateAntiForgeryToken] prevents CSRF attacks.
        /// </summary>
        /// <param name="category">The bound Category from the form.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
                return View(category);

            _logger.LogInformation(
                "CategoryController: Create POST for '{Name}'.", category.Name);

            var success = await _categoryService.CreateCategoryAsync(category);
            if (!success)
            {
                ModelState.AddModelError(string.Empty,
                    "An error occurred saving the category. Please try again.");
                return View(category);
            }

            TempData["SuccessMessage"] = $"Category '{category.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT — Update Existing Category ──────────────────

        /// <summary>
        /// GET /Category/Edit/{id}
        /// Returns the Edit form pre-populated with existing
        /// category data. Returns 404 if not found.
        /// Implements US-10: Edit an existing category.
        /// </summary>
        /// <param name="id">The CategoryId to edit.</param>
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation(
                "CategoryController: Edit GET for CategoryId {Id}.", id);

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        /// <summary>
        /// POST /Category/Edit/{id}
        /// Validates the submitted form and calls the service to
        /// update the category. Redirects to Index on success.
        /// </summary>
        /// <param name="id">The CategoryId being edited.</param>
        /// <param name="category">The bound Category from the form.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId) return BadRequest();

            if (!ModelState.IsValid)
                return View(category);

            _logger.LogInformation(
                "CategoryController: Edit POST for CategoryId {Id}.", id);

            var success = await _categoryService.UpdateCategoryAsync(category);
            if (!success)
            {
                ModelState.AddModelError(string.Empty,
                    "An error occurred updating the category.");
                return View(category);
            }

            TempData["SuccessMessage"] = $"Category '{category.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE — Remove Category ──────────────────────────

        /// <summary>
        /// GET /Category/Delete/{id}
        /// Returns the Delete Confirmation page.
        /// Returns 404 if not found.
        /// Implements US-11: Delete a category.
        /// </summary>
        /// <param name="id">The CategoryId to delete.</param>
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation(
                "CategoryController: Delete GET for CategoryId {Id}.", id);

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        /// <summary>
        /// POST /Category/Delete/{id}
        /// Permanently deletes the category if no resources are
        /// assigned to it. If resources exist the deletion is
        /// blocked per FK ON DELETE NO ACTION constraint and
        /// a friendly error message is shown.
        /// </summary>
        /// <param name="id">The CategoryId to permanently delete.</param>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation(
                "CategoryController: DeleteConfirmed for CategoryId {Id}.", id);

            var success = await _categoryService.DeleteCategoryAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] =
                    "Cannot delete this category because it has resources assigned to it. " +
                    "Please reassign or delete those resources first.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
