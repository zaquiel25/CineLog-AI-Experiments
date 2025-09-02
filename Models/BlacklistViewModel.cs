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
    // Compatibility alias for views referencing DateAdded
    public DateTime DateAdded 
    { 
        get => BlacklistedDate; 
        set => BlacklistedDate = value; 
    }
    public string PosterUrl { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public int ReleasedYear { get; set; }
    }
}
