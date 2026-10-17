namespace Ezequiel_Movies.Models
{
    /// <summary>
    /// Page hero shared by My Movies, Wishlist and Suggestions: a film still (TMDB backdrop) with a small
    /// caps kicker, a title, one line of context and an optional main action.
    /// Without a backdrop it renders as plain text.
    /// </summary>
    public class PageHeroViewModel
    {
        public string? Kicker { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? TitleUrl { get; set; }
        public string? Sub { get; set; }
        public string? BackdropPath { get; set; }
        public string? ActionText { get; set; }
        public string? ActionUrl { get; set; }
    }
}
