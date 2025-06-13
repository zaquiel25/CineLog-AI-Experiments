// In Models/TmdbApi/TmdbPersonCredits.cs

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi
{
    // This class represents the response from TMDB when asking for a person's movie credits.
    public class TmdbPersonMovieCreditsResponse
    {
        [JsonPropertyName("crew")]
        public List<TmdbMovieBrief> Crew { get; set; } = new();

        [JsonPropertyName("cast")]
        public List<TmdbMovieBrief> Cast { get; set; } = new();
    }

    // This class represents a single cast member in a movie's credits
    public class TmdbCastPerson
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("popularity")]
        public double Popularity { get; set; }
    }
}