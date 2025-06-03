// In Ezequiel_Movies/Models/TmdbApi/TmdbMovieDetails.cs (or your chosen path)
using System.Collections.Generic;
using System.Linq; // Required for .Where(), .Select(), .FirstOrDefault()
using System.Text.Json.Serialization; // Required for JsonPropertyName

namespace Ezequiel_Movies.Models.TmdbApi // Ensure this namespace matches your other DTOs
{
    public class TmdbCrewMember
    {
        [JsonPropertyName("job")]
        public string? Job { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        // You could add other crew properties if needed, e.g., "department"
        // [JsonPropertyName("department")]
        // public string? Department { get; set; }
    }

    public class TmdbCredits
    {
        [JsonPropertyName("crew")]
        public List<TmdbCrewMember>? Crew { get; set; }
        // You could also add "cast" here if you need actor information
        // [JsonPropertyName("cast")]
        // public List<TmdbCastMember>? Cast { get; set; }
    }

    public class TmdbMovieDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; } // Format from TMDB: "YYYY-MM-DD"

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; } // This is a partial path, e.g., "/xyz.jpg"

        [JsonPropertyName("credits")]
        public TmdbCredits? Credits { get; set; }

        // Helper method to extract the first director's name
        public string? GetDirector()
        {
            if (Credits?.Crew == null)
            {
                return null;
            }
            // Find the first crew member whose job is "Director"
            var director = Credits.Crew.FirstOrDefault(c => c.Job == "Director");
            return director?.Name;
            // If you expect multiple directors, you could use:
            // return string.Join(", ", Credits.Crew.Where(c => c.Job == "Director").Select(c => c.Name));
        }
    }
}