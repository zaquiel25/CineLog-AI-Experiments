using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Ezequiel_Movies1.Models.Entities
{
    public class BlacklistedMovie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TmdbId { get; set; }

        // Foreign key for the user who owns this blacklist entry
        [Required]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}