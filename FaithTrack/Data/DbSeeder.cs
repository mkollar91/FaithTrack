// =============================================================
// FaithTrack — Data/DbSeeder.cs
// Seeds the database with default categories on first run.
// Ensures the Category dropdown on the Create Resource form
// is never empty when the application is first launched.
//
// Author  : Matthew Kollar
// Course  : CST-451 — Grand Canyon University
// =============================================================

using FaithTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FaithTrack.Data
{
    /// <summary>
    /// Provides static seed data for the FaithTrackDb database.
    /// Called from Program.cs after the application builds.
    /// Only inserts records if the Categories table is empty,
    /// making the operation idempotent (safe to run on every start).
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Applies any pending EF Core migrations and seeds
        /// default categories if none exist.
        /// </summary>
        /// <param name="serviceProvider">The application's
        /// DI service provider from Program.cs.</param>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            // Resolve DbContext from the DI container
            var context = serviceProvider
                .GetRequiredService<FaithTrackDbContext>();

            // Apply any pending EF Core migrations automatically
            await context.Database.MigrateAsync();

            // Only seed if the Categories table is empty
            if (!await context.Categories.AnyAsync())
            {
                var defaultCategories = new List<Category>
                {
                    new Category
                    {
                        Name        = "Bible Study",
                        Description = "In-depth studies and guides for exploring scripture."
                    },
                    new Category
                    {
                        Name        = "Prayer",
                        Description = "Prayer guides, journals, and meditation resources."
                    },
                    new Category
                    {
                        Name        = "Templates",
                        Description = "Reusable templates for sermons, notes, and planning."
                    },
                    new Category
                    {
                        Name        = "Reading Plans",
                        Description = "Structured scripture reading and devotional plans."
                    },
                    new Category
                    {
                        Name        = "Journaling",
                        Description = "Faith journey reflection and journaling resources."
                    }
                };

                await context.Categories.AddRangeAsync(defaultCategories);
                await context.SaveChangesAsync();
            }
        }
    }
}
