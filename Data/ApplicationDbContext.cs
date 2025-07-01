using System;
using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies1.Models.Entities; // Assuming this is the correct namespace for your Movies entity
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Ezequiel_Movies.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Movies> Movies { get; set; }

        // VVVV ADD THIS METHOD VVVV
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Good practice to call the base method

            // Configure the UserRating property for the Movies entity
            modelBuilder.Entity<Movies>(entity =>
            {
                entity.Property(m => m.UserRating)
                      .HasPrecision(3, 1); // Sets precision to 3 and scale to 1 (e.g., can store 12.3, 5.0, 0.5)
            });

            // If you have other entity configurations in the future, they would also go in this method.
        }
        // ^^^^ END OF ADDED METHOD ^^^^
    }
}