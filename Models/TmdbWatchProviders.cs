using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi
{
    // This represents a single provider like Netflix or Disney+
    public class ProviderInfo
    {
        [JsonPropertyName("logo_path")]
        public string? LogoPath { get; set; }

        [JsonPropertyName("provider_name")]
        public string? ProviderName { get; set; }
    }

    // This holds the results for a specific country
    public class WatchProviderCountry
    {
        [JsonPropertyName("link")]
        public string? Link { get; set; }

        // "flatrate" is what TMDB calls subscription streaming services
        [JsonPropertyName("flatrate")]
        public List<ProviderInfo>? Streaming { get; set; }

        [JsonPropertyName("rent")]
        public List<ProviderInfo>? Rent { get; set; }

        [JsonPropertyName("buy")]
        public List<ProviderInfo>? Buy { get; set; }
    }

    // This is the main container for the API response
    public class WatchProviderResponse
    {
        [JsonPropertyName("results")]
        public Dictionary<string, WatchProviderCountry>? Results { get; set; }
    }
}