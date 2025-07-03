// In Models/TmdbApi/TmdbMovieDetails.cs

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Ezequiel_Movies.Models.TmdbApi
{
    public class TmdbMovieDetails
    {
        [JsonPropertyName("tagline")]
        public string? Tagline { get; set; }

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

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

        [JsonPropertyName("credits")]
        public TmdbCredits? Credits { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenre> Genres { get; set; } = new();

        public string? GetDirector()
        {
            if (Credits?.Crew == null) return null;
            var director = Credits.Crew.FirstOrDefault(c => c.Job == "Director");
            return director?.Name;
        }
    }

    public class TmdbCredits
    {
        [JsonPropertyName("crew")]
        public List<TmdbCrewPerson> Crew { get; set; } = new();

        // VVVV THIS IS THE CORRECT PLACE FOR THE NEW CAST PROPERTY VVVV
        [JsonPropertyName("cast")]
        public List<TmdbCastPerson> Cast { get; set; } = new();
    }

    public class TmdbCrewPerson
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("job")]
        public string? Job { get; set; }
    }
}