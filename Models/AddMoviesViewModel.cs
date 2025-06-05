using System;
using System.ComponentModel.DataAnnotations;


namespace Ezequiel_Movies.Models
{
    public class AddMoviesViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please enter the movie title.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the director's name.")]
        public string Director { get; set; } = string.Empty;

        [Display(Name = "released year")]
        [ValidReleasedYear(AllowFutureYears = 1, ErrorMessage = "The {0} must be between {1} and {2}.")]
        public int? ReleasedYear { get; set; }

        [NoFutureDate(ErrorMessage = "Date watched cannot be in the future.")]
        public DateTime? DateWatched { get; set; }

        [Required(ErrorMessage = "Please select where you watched the movie.")]
        public WatchedLocationType? WatchedLocation { get; set; }

        public bool Subscribed { get; set; }

        public string? PosterPath { get; set; }

        public string? Overview { get; set; }

        public bool IsRewatch { get; set; }

        [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0.0 and 5.0.")]
        [RegularExpression(@"^[0-5](\.0|\.5)?$", ErrorMessage = "Rating must be in 0.5 increments (e.g., 3.0, 3.5).")] // Optional: more specific validation
        public decimal? UserRating { get; set; } // <<< ADD THIS NEW PROPERTY

        public int? TmdbId { get; set; }
    }
}