using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Ezequiel_Movies1.Models.Entities
{
    public class WishlistItem
    {
        public WishlistItem()
        {
            UserId = string.Empty;
            Title = string.Empty;
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public int TmdbId { get; set; }

    public string Title { get; set; } = string.Empty;

        public string? PosterPath { get; set; }


    public int? ReleasedYear { get; set; }

    // Name of the director (optional, for display)
    public string? Director { get; set; }

        public DateTime DateAdded { get; set; }

        // Foreign key for the user who owns this wishlist item
        [Required]
        public string UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}