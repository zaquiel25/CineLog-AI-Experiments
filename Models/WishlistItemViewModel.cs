using System;

namespace Ezequiel_Movies1.Models
{
    public class WishlistItemViewModel
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public int? ReleasedYear { get; set; }
        public string? Director { get; set; }
    }
}
