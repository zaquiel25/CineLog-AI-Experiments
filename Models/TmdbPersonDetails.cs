using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi
{
    public class TmdbPersonDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("profile_path")]
        public string? ProfilePath { get; set; }
    }
}