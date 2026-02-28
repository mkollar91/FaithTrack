// =============================================================
// FaithTrack — Models/Resource.cs
// Domain entity for a faith-based resource.
// Maps to the Resources table defined in:
//   - Architecture Plan ER Diagram (Fig. 5)
//   - DDL Script: CREATE TABLE Resources (p.14)
//   - Data Dictionary: Resources table
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FaithTrack.Models
{
    /// <summary>
    /// Represents a faith-based resource created and managed by an
    /// authenticated FaithTrack user. Resources have a title,
    /// optional description and URL, belong to one category, and
    /// are owned by one user.
    /// Examples: "The Gospel of John Study", "Prayer Guide",
    /// "Weekly Scripture Reading Plan".
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Primary key, auto-incremented.
        /// DDL: ResourceId INT IDENTITY(1,1) PRIMARY KEY
        /// </summary>
        public int ResourceId { get; set; }

        /// <summary>
        /// Title of the faith-based resource. Required.
        /// Displayed in the Resources List and Detail views (Fig. 10, 13).
        /// DDL: Title NVARCHAR(200) NOT NULL
        /// Data Dictionary: Required, max 200 chars.
        /// </summary>
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the resource. Optional.
        /// DDL: Description NVARCHAR(MAX) NULL
        /// Data Dictionary: Optional, supports long text.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional URL or external reference link for the resource.
        /// DDL: Url NVARCHAR(500) NULL
        /// Data Dictionary: Optional, max 500 chars, valid URL format.
        /// </summary>
        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters.")]
        [Display(Name = "Resource URL / Reference")]
        public string? Url { get; set; }

        /// <summary>
        /// FK to the Categories table. Required — every resource
        /// must belong to a category (per Add Resource wireframe, Fig. 11).
        /// DDL: CategoryId INT NOT NULL FK → Categories(CategoryId)
        ///      ON DELETE NO ACTION
        /// </summary>
        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        /// <summary>
        /// FK to the AspNetUsers table (ASP.NET Core Identity).
        /// Identifies the authenticated user who created this resource.
        /// DDL: CreatedByUserId → Users(UserId) ON DELETE NO ACTION
        /// Data Dictionary: Must reference existing user.
        /// </summary>
        public string CreatedByUserId { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp set automatically when the resource is created.
        /// DDL: CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
        /// Data Dictionary: System-generated timestamp.
        /// </summary>
        [Display(Name = "Date Added")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp updated each time the resource is edited.
        /// Null until the first edit occurs.
        /// DDL: UpdatedDate DATETIME2 NULL
        /// Data Dictionary: Updated on each modification.
        /// </summary>
        [Display(Name = "Last Modified")]
        public DateTime? UpdatedDate { get; set; }

        // ── Navigation Properties ────────────────────────────

        /// <summary>
        /// Navigation property: the Category this resource belongs to.
        /// Loaded via EF Core .Include(r => r.Category) in queries.
        /// </summary>
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        /// <summary>
        /// Navigation property: the user who created this resource.
        /// </summary>
        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }
}
