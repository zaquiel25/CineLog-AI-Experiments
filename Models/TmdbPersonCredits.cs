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
}