using System;
using System.Collections.Generic;

namespace Ezequiel_Movies.Models
{
    /// <summary>
    /// ViewModel for Smart Timeline Navigator feature.
    /// 
    /// FEATURE: Provides timeline data aggregation for monthly movie navigation,
    /// enabling users to jump to specific months in their movie timeline with movie counts.
    /// </summary>
    public class TimelineNavigatorViewModel
    {
        /// <summary>
        /// Collection of available months with movie counts for timeline navigation.
        /// Sorted chronologically from newest to oldest for better user experience.
        /// </summary>
        public List<TimelineMonth> AvailableMonths { get; set; } = new List<TimelineMonth>();

        /// <summary>
        /// Currently selected month filter (null means "All Months").
        /// Format: YYYY-MM (e.g., "2024-03" for March 2024).
        /// </summary>
        public string? SelectedMonth { get; set; }

        /// <summary>
        /// Display name for the currently selected month filter.
        /// Used for UI display (e.g., "March 2024" or "All Months").
        /// </summary>
        public string SelectedMonthDisplayName { get; set; } = "All Months";
    }

    /// <summary>
    /// Represents a single month in the timeline with associated movie count.
    /// Used for dropdown options and timeline navigation.
    /// </summary>
    public class TimelineMonth
    {
        /// <summary>
        /// Month identifier in YYYY-MM format for filtering.
        /// Used as parameter value for month-specific filtering.
        /// </summary>
        public string MonthKey { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable display name for the month.
        /// Format: "January 2024 (12 movies)" for better user experience.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Number of movies watched in this specific month.
        /// Includes all watches (first watches and rewatches) for comprehensive counts.
        /// </summary>
        public int MovieCount { get; set; }

        /// <summary>
        /// Year component for grouping and sorting purposes.
        /// Extracted from DateWatched for efficient timeline organization.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Month component (1-12) for sorting and filtering.
        /// Used with Year for chronological ordering in timeline.
        /// </summary>
        public int Month { get; set; }
    }
}