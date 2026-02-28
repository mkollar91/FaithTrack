// =============================================================
// FaithTrack — Models/Category.cs
// Domain entity for a resource category.
// Maps to the Categories table defined in:
//   - Architecture Plan ER Diagram (Fig. 5)
//   - DDL Script: CREATE TABLE Categories (p.13)
//   - Data Dictionary: Categories table
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using System.ComponentModel.DataAnnotations;

namespace FaithTrack.Models
{
    /// <summary>
    /// Represents a category used to organize faith-based resources.
    /// Example values: "Bible Study", "Prayer", "Templates",
    /// "Reading Plans", "Journaling".
    /// Each Resource belongs to exactly one Category (1:N).
    /// FK constraint on Resources uses ON DELETE NO ACTION per DDL.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Primary key, auto-incremented.
        /// DDL: CategoryId INT IDENTITY(1,1) PRIMARY KEY
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Display name of the category. Required field.
        /// DDL: Name NVARCHAR(150) NOT NULL
        /// Data Dictionary: Required, max 150 chars.
        /// </summary>
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description providing context for the category.
        /// DDL: Description NVARCHAR(500) NULL
        /// Data Dictionary: Optional, max 500 chars.
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property: collection of resources in this category.
        /// Cascade behavior: ON DELETE NO ACTION (DDL p.14).
        /// Prevents deletion of a category that has assigned resources.
        /// </summary>
        public virtual ICollection<Resource> Resources { get; set; }
            = new List<Resource>();
    }
}
