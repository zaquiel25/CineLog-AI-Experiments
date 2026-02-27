using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi;

/// <summary>
/// FEATURE: TMDB video response — used to extract YouTube trailer links for movie preview/details pages.
/// </summary>
public class TmdbVideoResponse
{
    [JsonPropertyName("results")]
    public List<TmdbVideo> Results { get; set; } = new();
}

/// <summary>
/// Represents a single video entry from TMDB (trailer, teaser, featurette, etc.).
/// </summary>
public class TmdbVideo
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("site")]
    public string? Site { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("official")]
    public bool Official { get; set; }
}
