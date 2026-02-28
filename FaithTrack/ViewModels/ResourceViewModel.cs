// =============================================================
// FaithTrack — ViewModels/ResourceViewModel.cs
// ViewModel used for Resource Create and Edit forms.
// Matches the ResourceViewModel class defined in the
// Architecture Plan UML Class Diagram (Fig. 15,
// Presentation Layer).
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FaithTrack.ViewModels
{
    /// <summary>
    /// Carries form data between the View and ResourceController
    /// for the Create and Edit resource workflows.
    /// Includes a SelectList of available categories to populate
    /// the category dropdown (per Add/Edit Resource wireframes,
    /// Architecture Plan Fig. 11 and Fig. 12).
    /// Data annotations provide client-side and server-side
    /// validation per Architecture Plan NFR — Input Validation (p.26).
    /// </summary>
    public class ResourceViewModel
    {
        /// <summary>
        /// Resource identifier. Zero on Create, populated on Edit.
        /// </summary>
        public int ResourceId { get; set; }

        /// <summary>
        /// Title of the resource. Required field marked with * in views.
        /// Max 200 characters per Data Dictionary specification.
        /// </summary>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the resource. Optional field.
        /// Supports multi-line text input in the form textarea.
        /// </summary>
        [Display(Name = "Description")]
        public string? Description { get; set; }

        /// <summary>
        /// Optional URL or external reference link.
        /// Max 500 characters per Data Dictionary specification.
        /// </summary>
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters.")]
        [Display(Name = "Resource URL / Reference (optional)")]
        public string? Url { get; set; }

        /// <summary>
        /// Selected category ID. Required — every resource must
        /// belong to a category (per Architecture Plan ER Diagram Fig. 5).
        /// </summary>
        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Populated by the controller before returning the Create/Edit
        /// view. Provides the list of available categories for the
        /// category dropdown select element (per Fig. 11 wireframe).
        /// </summary>
        public SelectList? Categories { get; set; }

        /// <summary>
        /// Date the resource was originally created. Set by the
        /// service layer; not editable by the user.
        /// </summary>
        [Display(Name = "Date Added")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date the resource was last updated. Set by the service
        /// layer on each Edit; not editable by the user.
        /// </summary>
        [Display(Name = "Last Modified")]
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// ID of the authenticated user who owns the resource.
        /// Set by the controller; not exposed in the view form.
        /// </summary>
        public string CreatedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the category. Populated from the
        /// Category navigation property for read-only views.
        /// </summary>
        [Display(Name = "Category")]
        public string? CategoryName { get; set; }
    }
}
