using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi
{


    public class TmdbPersonBrief
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("popularity")]
        public double Popularity { get; set; }
    }
    public class TmdbPersonSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbPersonBrief> Results { get; set; } = new();
    }
}