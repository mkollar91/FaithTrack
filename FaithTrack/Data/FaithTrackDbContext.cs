// =============================================================
// FaithTrack — Data/FaithTrackDbContext.cs
// EF Core DbContext. Extends IdentityDbContext<ApplicationUser>
// to integrate ASP.NET Core Identity tables alongside the
// FaithTrack domain tables (Resources, Categories).
// Schema matches the Architecture Plan ER Diagram (Fig. 5)
// and DDL Scripts (pp. 13–14).
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FaithTrack.Data
{
    /// <summary>
    /// EF Core database context for the FaithTrack application.
    /// Inheriting IdentityDbContext provides the Identity tables:
    ///   AspNetUsers, AspNetRoles, AspNetUserRoles,
    ///   AspNetUserClaims, AspNetRoleClaims, AspNetUserTokens,
    ///   AspNetUserLogins
    /// which satisfy the Users, Roles, and UserRoles tables
    /// defined in the Architecture Plan ER Diagram (Fig. 5).
    /// The FaithTrack domain tables are Resources and Categories.
    /// </summary>
    public class FaithTrackDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initialises the context with the options provided by
        /// the DI container (configured in Program.cs).
        /// </summary>
        /// <param name="options">EF Core context options including
        /// the SQL Server connection string.</param>
        public FaithTrackDbContext(DbContextOptions<FaithTrackDbContext> options)
            : base(options)
        {
        }

        // ── Domain Tables ────────────────────────────────────

        /// <summary>
        /// DbSet for the Resources table.
        /// DDL: CREATE TABLE Resources (ResourceId, Title, Description,
        ///      Url, CategoryId, CreatedByUserId, CreatedDate, UpdatedDate)
        /// </summary>
        public DbSet<Resource> Resources { get; set; }

        /// <summary>
        /// DbSet for the Categories table.
        /// DDL: CREATE TABLE Categories (CategoryId, Name, Description)
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Configures entity relationships and constraints to match
        /// the DDL scripts defined in the Architecture Plan (pp. 13-14).
        /// </summary>
        /// <param name="builder">The model builder instance.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Must call base to configure Identity tables first
            base.OnModelCreating(builder);

            // ── Resource → Category relationship ─────────────
            // FK: Resources.CategoryId → Categories.CategoryId
            // ON DELETE NO ACTION (per DDL script p.14):
            // prevents orphaned resources if category is deleted.
            builder.Entity<Resource>()
                .HasOne(r => r.Category)
                .WithMany(c => c.Resources)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Resource → ApplicationUser relationship ───────
            // FK: Resources.CreatedByUserId → AspNetUsers.Id
            // ON DELETE NO ACTION (per DDL script p.14):
            // preserves resources if a user account is removed.
            builder.Entity<Resource>()
                .HasOne(r => r.CreatedByUser)
                .WithMany(u => u.Resources)
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Category.Name index ───────────────────────────
            // Supports fast category lookup by name.
            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique(false);
        }
    }
}
