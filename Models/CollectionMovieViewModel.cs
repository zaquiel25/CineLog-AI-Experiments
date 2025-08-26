using Ezequiel_Movies1.Models.Entities;

namespace Ezequiel_Movies1.Models
{
    /// <summary>
    /// Represents a movie in the collection view with watch count statistics.
    /// 
    /// FEATURE: Enables collection browsing by deduplicating movies based on TmdbId
    /// and providing watch count information while preserving latest movie details.
    /// </summary>
    public class CollectionMovieViewModel
    {
        /// <summary>
        /// The latest movie entry for this title (used for display details).
        /// Contains the most recent poster, rating, and other metadata.
        /// </summary>
        public Movies Movie { get; set; } = null!;

        /// <summary>
        /// Total number of times this movie has been watched by the user.
        /// Calculated by counting all movie entries with the same TmdbId.
        /// </summary>
        public int WatchCount { get; set; }

        /// <summary>
        /// The date when this movie was first watched by the user.
        /// Represents the earliest DateWatched for this TmdbId.
        /// </summary>
        public DateTime? FirstWatched { get; set; }

        /// <summary>
        /// The date when this movie was most recently watched by the user.
        /// Represents the latest DateWatched for this TmdbId.
        /// </summary>
        public DateTime? LastWatched { get; set; }

        /// <summary>
        /// Indicates if this movie has been watched multiple times.
        /// Convenience property for UI display of rewatch status.
        /// </summary>
        public bool IsRewatched => WatchCount > 1;
    }
}