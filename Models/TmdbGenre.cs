using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi
{
    public class TmdbGenre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class TmdbGenreListResponse
    {
        [JsonPropertyName("genres")]
        public List<TmdbGenre> Genres { get; set; } = new();
    }
}