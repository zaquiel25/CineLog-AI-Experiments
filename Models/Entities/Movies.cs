using System;
using Ezequiel_Movies.Models;
namespace Ezequiel_Movies1.Models.Entities
{
	public class Movies
	{
		public Guid Id { get; set; }

		public string Title { get; set; } = string.Empty;

        public string Director { get; set; } = string.Empty;

        public int? ReleasedYear {get ; set; }

        public DateTime? DateWatched { get; set; }

        public WatchedLocationType? WatchedLocation { get; set; }

        public bool Subscribed { get; set; }

        public string? PosterPath { get; set; }

        public string? Overview { get; set; }

        public bool IsRewatch { get; set; }

        public decimal? UserRating { get; set; }

        public int? TmdbId { get; set; }

        public string? Genres { get; set; }


    }
}

