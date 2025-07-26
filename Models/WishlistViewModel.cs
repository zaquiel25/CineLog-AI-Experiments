namespace Ezequiel_Movies.Models
{
    /// <summary>
    /// ViewModel for displaying wishlist items with enhanced performance
    /// </summary>
    public class WishlistViewModel
    {
        public int Id { get; set; }
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public string PosterPath { get; set; }
        public int ReleasedYear { get; set; }
        public string Director { get; set; }
        public string MovieTitle { get; set; }
        public int MovieTmdbId { get; set; }
    }
}
