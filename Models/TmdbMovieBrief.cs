using System.Text.Json.Serialization; // Required for JsonPropertyName

namespace Ezequiel_Movies.Models.TmdbApi
{
    public class TmdbMovieBrief
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; } // TMDB often provides dates as strings "YYYY-MM-DD"

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; } // This will be a partial path
    }
}