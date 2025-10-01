using System;
using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies1.Models.Entities; // Assuming this is the correct namespace for your Movies entity
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Ezequiel_Movies.Models;

namespace Ezequiel_Movies.Data
{
    /// <summary>
    /// Application database context with Identity support.
    /// TECHNICAL: Uses base IdentityDbContext to avoid key configuration conflicts.
    /// DisplayName is accessed directly via AspNetUsers table extension.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Movies> Movies { get; set; }

        // VVVV ADD THESE TWO NEW LINES VVVV
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<BlacklistedMovie> BlacklistedMovies { get; set; }


        // VVVV ADD THIS METHOD VVVV
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Good practice to call the base method

            // Configure the UserRating property for the Movies entity
            modelBuilder.Entity<Movies>(entity =>
            {
                entity.Property(m => m.UserRating)
                      .HasPrecision(3, 1); // Sets precision to 3 and scale to 1 (e.g., can store 12.3, 5.0, 0.5)
                
                // PERFORMANCE: Add composite indexes for efficient user-specific queries
                entity.HasIndex(m => new { m.UserId, m.DateWatched })
                      .HasDatabaseName("IX_Movies_UserId_DateWatched")
                      .HasFilter("[DateWatched] IS NOT NULL");
                      
                entity.HasIndex(m => new { m.UserId, m.Director })
                      .HasDatabaseName("IX_Movies_UserId_Director")
                      .HasFilter("[Director] IS NOT NULL");
                      
                entity.HasIndex(m => new { m.UserId, m.Genres })
                      .HasDatabaseName("IX_Movies_UserId_Genres")
                      .HasFilter("[Genres] IS NOT NULL");
                      
                entity.HasIndex(m => new { m.UserId, m.UserRating })
                      .HasDatabaseName("IX_Movies_UserId_UserRating")
                      .HasFilter("[UserRating] IS NOT NULL");
                      
                entity.HasIndex(m => new { m.UserId, m.TmdbId })
                      .HasDatabaseName("IX_Movies_UserId_TmdbId")
                      .HasFilter("[TmdbId] IS NOT NULL");
                      
                entity.HasIndex(m => new { m.UserId, m.ReleasedYear })
                      .HasDatabaseName("IX_Movies_UserId_ReleasedYear")
                      .HasFilter("[ReleasedYear] IS NOT NULL");
                      
                entity.HasIndex(m => new { m.UserId, m.DateCreated })
                      .HasDatabaseName("IX_Movies_UserId_DateCreated");
                      
                entity.HasIndex(m => new { m.UserId, m.Title })
                      .HasDatabaseName("IX_Movies_UserId_Title");
            });
            
            // Configure WishlistItems indexes
            modelBuilder.Entity<WishlistItem>(entity =>
            {
                entity.HasIndex(w => new { w.UserId, w.TmdbId })
                      .HasDatabaseName("IX_WishlistItems_UserId_TmdbId");
                      
                entity.HasIndex(w => new { w.UserId, w.DateAdded })
                      .HasDatabaseName("IX_WishlistItems_UserId_DateAdded");
            });
            
            // Configure BlacklistedMovies indexes
            modelBuilder.Entity<BlacklistedMovie>(entity =>
            {
                entity.HasIndex(b => new { b.UserId, b.BlacklistedDate })
                      .HasDatabaseName("IX_BlacklistedMovies_UserId_BlacklistedDate");
                      
                entity.HasIndex(b => new { b.UserId, b.Title })
                      .HasDatabaseName("IX_BlacklistedMovies_UserId_Title");
            });


            // If you have other entity configurations in the future, they would also go in this method.
        }
        // ^^^^ END OF ADDED METHOD ^^^^
    }
}