using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Ezequiel_Movies1.Models.Entities
{
    public class BlacklistedMovie
    {
        public BlacklistedMovie()
        {
            UserId = string.Empty;
            Title = string.Empty;
        }

        public string Title { get; set; } = string.Empty;

        [Key]
        public int Id { get; set; }

        [Required]
        public int TmdbId { get; set; }

        // Foreign key for the user who owns this blacklist entry
        [Required]
        public string UserId { get; set; }
        public IdentityUser? User { get; set; }

        // Date when the movie was blacklisted
        [Required]
        public DateTime BlacklistedDate { get; set; }

        // Poster path from TMDB (optional, for display)
        public string? PosterUrl { get; set; }

        // NUEVAS PROPIEDADES: Director y Año (nullable, seguro para datos existentes)
        public string? Director { get; set; }
        public int? ReleasedYear { get; set; }
    }
}