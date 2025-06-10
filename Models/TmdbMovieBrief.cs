using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi
{
    public class TmdbMovieBrief
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("job")]
        public string? Job { get; set; }

        // VVVV ADD THIS NEW PROPERTY VVVV
        [JsonPropertyName("popularity")]
        public double Popularity { get; set; }
        // ^^^^ END OF NEW PROPERTY ^^^^
    }
}