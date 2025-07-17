// In Ezequiel_Movies/Models/TmdbApi/TmdbSearchResponse.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi
{
    public class TmdbSearchResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<TmdbMovieBrief>? Results { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }
    }
}