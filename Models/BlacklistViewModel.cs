namespace Ezequiel_Movies.Models
{
    /// <summary>
    /// ViewModel for displaying blacklisted movies with enhanced performance
    /// </summary>
    public class BlacklistViewModel
    {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TmdbId { get; set; }
    public DateTime BlacklistedDate { get; set; }
    public string PosterUrl { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public int ReleasedYear { get; set; }
    }
}
