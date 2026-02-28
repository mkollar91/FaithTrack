// =============================================================
// FaithTrack — Models/ApplicationUser.cs
// Extends ASP.NET Core Identity IdentityUser to represent a
// registered FaithTrack user. Maps to the AspNetUsers table
// managed by ASP.NET Core Identity, which satisfies the
// Users table in the Architecture Plan ER Diagram (Fig. 5)
// and Data Dictionary.
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using Microsoft.AspNetCore.Identity;

namespace FaithTrack.Models
{
    /// <summary>
    /// Represents a registered and authenticated FaithTrack user.
    /// Inherits from IdentityUser which provides:
    ///   - Id (string GUID — maps to UserId PK)
    ///   - Email / NormalizedEmail
    ///   - PasswordHash (PBKDF2 — never stored plain text)
    ///   - SecurityStamp, ConcurrencyStamp
    ///   - LockoutEnabled, AccessFailedCount
    /// Additional navigation property exposes the user's resources.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Navigation property: the collection of faith-based
        /// resources created by this user (one-to-many).
        /// FK: Resource.CreatedByUserId → ApplicationUser.Id
        /// </summary>
        public virtual ICollection<Resource> Resources { get; set; }
            = new List<Resource>();
    }
}
