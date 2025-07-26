namespace Ezequiel_Movies.Models
{
    /// <summary>
    /// ViewModel for displaying wishlist items with enhanced performance
    /// </summary>
    public class WishlistViewModel
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string PosterPath { get; set; } = string.Empty;
        public int ReleasedYear { get; set; }
        public string Director { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public int MovieTmdbId { get; set; }
    }
}
