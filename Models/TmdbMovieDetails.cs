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

        [JsonPropertyName("videos")]
        public TmdbVideoResponse? Videos { get; set; }

        public string? GetDirector()
        {
            if (Credits?.Crew == null) return null;
            var director = Credits.Crew.FirstOrDefault(c => c.Job == "Director");
            return director?.Name;
        }

        /// <summary>
        /// FEATURE: Returns the YouTube key for the best available trailer.
        /// Priority: official trailer > any trailer > teaser.
        /// </summary>
        public string? GetTrailerYouTubeKey()
        {
            var trailer = Videos?.Results?
                .Where(v => v.Site == "YouTube")
                .OrderByDescending(v => v.Type == "Trailer" && v.Official)
                .ThenByDescending(v => v.Type == "Trailer")
                .ThenByDescending(v => v.Type == "Teaser")
                .FirstOrDefault();
            return trailer?.Key;
        }
    }

    public class TmdbCredits
    {
        [JsonPropertyName("crew")]
        public List<TmdbCrewPerson> Crew { get; set; } = new();

        [JsonPropertyName("cast")]
        // Cast information from TMDB credits response
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