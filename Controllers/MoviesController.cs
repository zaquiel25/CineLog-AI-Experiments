        /// <summary>
        /// ShowSuggestions - Director recommendation system with bulletproof fallback
        /// 
        /// CRITICAL FIXES IMPLEMENTED:
        /// - Bulletproof fallback: NEVER shows "No available director suggestions"
        /// - Smart skip deduplication: Avoids showing same director consecutively  
        /// - Fresh start reset: Resets sequence on page refresh
        /// - Anti-repetition in random: Avoids repeating last shown director
        /// 
        /// SEQUENCE LOGIC:
        /// 1. Recent director (most recently watched)
        /// 2. Frequent director (most watched overall)  
        /// 3. Rated director (highest average rating)
        /// 4. Random director (infinite, with anti-repetition)
        /// 
        /// EDGE CASES HANDLED:
        /// - Single director scenario: System works with minimal data
        /// - Missing dates/ratings: Directors still appear in random pool
        /// - Empty suggestions: Bulletproof fallback always shows something
        /// 
        /// DEBUGGING: Enable logging to see sequence flow and director selection
        /// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using Ezequiel_Movies.Models;
using Ezequiel_Movies.Helpers;
using Ezequiel_Movies.Models.TmdbApi;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Ezequiel_Movies.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Ezequiel_Movies1.Models.Entities;

namespace Ezequiel_Movies.Controllers
{

    [Authorize]
    public class MoviesController : Controller
    {
        // DRY Helper: Get all blacklisted TMDB IDs for a user
        private async Task<HashSet<int>> GetUserBlacklistedTmdbIdsAsync(string userId)
        {
            return (await _dbContext.BlacklistedMovies
                .Where(b => b.UserId == userId)
                .Select(b => b.TmdbId)
                .ToListAsync()).ToHashSet();
        }
        // Helper: Get current user ID with logging
        private string? GetCurrentUserId()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User authentication failed - no user ID found");
            }
            return userId;
        }

        // Helper: Check if movie exists in wishlist
        private async Task<bool> MovieExistsInWishlistAsync(string userId, int tmdbId)
        {
            try
            {
                return await _dbContext.WishlistItems.AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if movie {TmdbId} exists in wishlist for user {UserId}", tmdbId, userId);
                throw;
            }
        }

        // Helper: Check if movie exists in blacklist
        private async Task<bool> MovieExistsInBlacklistAsync(string userId, int tmdbId)
        {
            try
            {
                return await _dbContext.BlacklistedMovies.AnyAsync(b => b.UserId == userId && b.TmdbId == tmdbId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if movie {TmdbId} exists in blacklist for user {UserId}", tmdbId, userId);
                throw;
            }
        }

        // Helper: Get TMDB movie details with error logging
        private async Task<TmdbMovieDetails?> GetMovieDetailsWithLoggingAsync(int tmdbId)
        {
            try
            {
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
                if (movieDetails == null)
                {
                    _logger.LogWarning("Movie details not found for TMDB ID: {TmdbId}", tmdbId);
                }
                return movieDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching movie details for TMDB ID: {TmdbId}", tmdbId);
                throw;
            }
        }
        private readonly ApplicationDbContext _dbContext;
        private readonly TmdbService _tmdbService;
        private readonly ILogger<MoviesController> _logger;
        private readonly UserManager<IdentityUser> _userManager;


    public MoviesController(ApplicationDbContext dbContext, TmdbService tmdbService, ILogger<MoviesController> logger, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _tmdbService = tmdbService;
        _logger = logger;
        _userManager = userManager;
    }

        // DRY Helper: Get poster URL for a TMDB movie, with fallback to TMDB API if missing
        private async Task<string?> GetPosterUrlAsync(int tmdbId, string? existingPosterUrl)
        {
            if (!string.IsNullOrEmpty(existingPosterUrl))
            {
                return existingPosterUrl;
            }
            try
            {
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
                return movieDetails?.PosterPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching TMDB details for movie {TmdbId}", tmdbId);
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToBlacklist(int tmdbId, string returnUrl = "/")
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (await MovieExistsInBlacklistAsync(userId, tmdbId))
            {
                return RedirectToAction(nameof(Blacklist));
            }
            // Mutual exclusion: Prevent adding to blacklist if in wishlist
            if (await MovieExistsInWishlistAsync(userId, tmdbId))
            {
                TempData["ErrorMessage"] = "Cannot add to blacklist: Movie is in your wishlist. Remove from wishlist first.";
                return LocalRedirect(returnUrl);
            }
            try
            {
                var movieDetails = await GetMovieDetailsWithLoggingAsync(tmdbId);
                if (movieDetails == null)
                {
                    return NotFound();
                }
                var blacklistedMovie = new Ezequiel_Movies1.Models.Entities.BlacklistedMovie
                {
                    UserId = userId,
                    TmdbId = tmdbId,
                    Title = movieDetails.Title ?? string.Empty,
                    BlacklistedDate = DateTime.Now,
                    PosterUrl = movieDetails.PosterPath
                };
                _dbContext.BlacklistedMovies.Add(blacklistedMovie);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Movie {TmdbId} added to blacklist for user {UserId}", tmdbId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding movie {TmdbId} to blacklist for user {UserId}", tmdbId, userId);
                throw;
            }
            return RedirectToAction(nameof(Blacklist));
        }

    // Removed duplicate Blacklist() method. Only the version with searchString and sortOrder remains.
    [HttpGet]
    public async Task<IActionResult> Blacklist(string? searchString = null, string? sortOrder = null)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var blacklistQuery = _dbContext.BlacklistedMovies
                .Where(b => b.UserId == userId)
                .AsQueryable();

            // Case-insensitive search by title
            if (!string.IsNullOrEmpty(searchString))
            {
                blacklistQuery = blacklistQuery.Where(b => b.Title.ToLower().Contains(searchString.ToLower()));
            }

            // Sorting
            ViewData["TitleSortParm"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;

            switch (sortOrder)
            {
                case "title_desc":
                    blacklistQuery = blacklistQuery.OrderByDescending(b => b.Title);
                    break;
                case "Date":
                    blacklistQuery = blacklistQuery.OrderBy(b => b.BlacklistedDate);
                    break;
                case "date_desc":
                    blacklistQuery = blacklistQuery.OrderByDescending(b => b.BlacklistedDate);
                    break;
                default:
                    blacklistQuery = blacklistQuery.OrderBy(b => b.Title);
                    break;
            }

            var blacklistedMovies = await blacklistQuery.ToListAsync();

            // Use DRY helper for poster fetching
            var moviesWithPosters = new List<dynamic>();
            foreach (var movie in blacklistedMovies)
            {
                var posterUrl = await GetPosterUrlAsync(movie.TmdbId, movie.PosterUrl);
                moviesWithPosters.Add(new {
                    Id = movie.Id,
                    Title = movie.Title,
                    TmdbId = movie.TmdbId,
                    BlacklistedDate = movie.BlacklistedDate,
                    PosterUrl = posterUrl
                });
            }
            return View(moviesWithPosters);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromBlacklist(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var blacklistedMovie = await _dbContext.BlacklistedMovies
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (blacklistedMovie == null)
            {
                return NotFound();
            }
            _dbContext.BlacklistedMovies.Remove(blacklistedMovie);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Blacklist));
        }

        // In MoviesController.cs

    // Removed duplicate Wishlist() method. Only the version with searchString and sortOrder remains.
    [HttpGet]
    public async Task<IActionResult> Wishlist(string? searchString = null, string? sortOrder = null)
        {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var wishlistQuery = _dbContext.WishlistItems
            .Where(w => w.UserId == userId)
            .AsQueryable();

        // Case-insensitive search by title
        if (!string.IsNullOrEmpty(searchString))
        {
            wishlistQuery = wishlistQuery.Where(w => w.Title.ToLower().Contains(searchString.ToLower()));
        }

        // Sorting
        ViewData["TitleSortParm"] = string.IsNullOrEmpty(sortOrder) ? "title_desc" : "";
        ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentSort"] = sortOrder;

        switch (sortOrder)
        {
            case "title_desc":
                wishlistQuery = wishlistQuery.OrderByDescending(w => w.Title);
                break;
            case "Date":
                wishlistQuery = wishlistQuery.OrderBy(w => w.DateAdded);
                break;
            case "date_desc":
                wishlistQuery = wishlistQuery.OrderByDescending(w => w.DateAdded);
                break;
            default:
                wishlistQuery = wishlistQuery.OrderBy(w => w.Title);
                break;
        }

        var wishlistItems = await wishlistQuery.ToListAsync();

        // Get all user's logged movies (for join)
        var userMovies = await _dbContext.Movies
            .Where(m => m.UserId == userId && m.TmdbId.HasValue)
            .ToListAsync();

        var wishlistWithDetails = new List<dynamic>();

        foreach (var w in wishlistItems)
        {
            var movie = userMovies.FirstOrDefault(m => m.TmdbId == w.TmdbId);
            string director = string.Empty;
            if (movie != null)
            {
                director = string.IsNullOrEmpty(movie.Director) ? "No Director in DB" : movie.Director;
            }
            else
            {
                // Fallback: fetch from TMDB if not found in Movies table
                var tmdbDetails = await _tmdbService.GetMovieDetailsAsync(w.TmdbId);
                if (tmdbDetails?.Credits?.Crew != null)
                {
                    var directorPerson = tmdbDetails.Credits.Crew.FirstOrDefault(c => c.Job == "Director");
                    director = directorPerson?.Name ?? "Unknown (TMDB)";
                }
                else
                {
                    director = "Unknown (TMDB)";
                }
            }
            wishlistWithDetails.Add(new
            {
                w.Id,
                w.TmdbId,
                w.Title,
                w.PosterPath,
                w.ReleasedYear,
                Director = director,
                MovieTitle = movie != null ? movie.Title : "N/A",
                MovieTmdbId = movie != null ? movie.TmdbId : (int?)null
            });
        }

        return View(wishlistWithDetails);
        }

        // In MoviesController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int tmdbId, string returnUrl = "/")
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || tmdbId == 0)
            {
                return BadRequest(); // Invalid request
            }

            if (await MovieExistsInWishlistAsync(userId, tmdbId))
            {
                return LocalRedirect(returnUrl);
            }

            // Mutual exclusion: Prevent adding to wishlist if in blacklist
            if (await MovieExistsInBlacklistAsync(userId, tmdbId))
            {
                TempData["ErrorMessage"] = "Cannot add to wishlist: Movie is in your blacklist. Remove from blacklist first.";
                return LocalRedirect(returnUrl);
            }



            try
            {
                var movieDetails = await GetMovieDetailsWithLoggingAsync(tmdbId);
                if (movieDetails != null)
                {
                    var wishlistItem = new WishlistItem
                    {
                        TmdbId = tmdbId,
                        UserId = userId,
                        Title = movieDetails.Title ?? string.Empty,
                        PosterPath = movieDetails.PosterPath,
                        ReleasedYear = !string.IsNullOrEmpty(movieDetails.ReleaseDate) && movieDetails.ReleaseDate.Length >= 4
                                         ? int.Parse(movieDetails.ReleaseDate.Substring(0, 4))
                                         : null,
                        DateAdded = DateTime.UtcNow
                    };

                    _dbContext.WishlistItems.Add(wishlistItem);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Movie {TmdbId} added to wishlist for user {UserId}", tmdbId, userId);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding movie {TmdbId} to wishlist for user {UserId}", tmdbId, userId);
                throw;
            }

            return RedirectToAction("Wishlist");
        }
        // Helper to remove from wishlist by TMDB ID (for Option 2, not used in Option 1)
        private async Task RemoveFromWishlistByTmdbIdAsync(string userId, int tmdbId)
        {
            try
            {
                var wishlistItem = await _dbContext.WishlistItems
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
                if (wishlistItem != null)
                {
                    _dbContext.WishlistItems.Remove(wishlistItem);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Movie {TmdbId} removed from wishlist for user {UserId}", tmdbId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing movie {TmdbId} from wishlist for user {UserId}", tmdbId, userId);
                throw;
            }
        }

        // Helper to remove from blacklist by TMDB ID (for Option 2, not used in Option 1)
        private async Task RemoveFromBlacklistByTmdbIdAsync(string userId, int tmdbId)
        {
            try
            {
                var blacklistedMovie = await _dbContext.BlacklistedMovies
                    .FirstOrDefaultAsync(b => b.UserId == userId && b.TmdbId == tmdbId);
                if (blacklistedMovie != null)
                {
                    _dbContext.BlacklistedMovies.Remove(blacklistedMovie);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Movie {TmdbId} removed from blacklist for user {UserId}", tmdbId, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing movie {TmdbId} from blacklist for user {UserId}", tmdbId, userId);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> RemoveFromWishlist(int tmdbId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || tmdbId == 0)
            {
                return BadRequest();
            }

            var wishlistItem = await _dbContext.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.TmdbId == tmdbId);

            if (wishlistItem != null)
            {
                _dbContext.WishlistItems.Remove(wishlistItem);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Wishlist");
        }

        // In MoviesController.cs

        [HttpGet]
        public async Task<IActionResult> Preview(int tmdbId)
        {
            if (tmdbId == 0)
            {
                return BadRequest();
            }

            // 1. Get all the movie details from TMDB
            var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
            if (movieDetails == null)
            {
                return NotFound();
            }

            // 2. Get the "Where to Watch" providers (Streaming, Buy, Rent)
            var watchProviders = await _tmdbService.GetWatchProvidersAsync(tmdbId, movieDetails.Title ?? string.Empty);
            if (watchProviders?.Results != null)
            {
                // Region priority: IE → US → GB
                WatchProviderCountry? providers = null;
                if (watchProviders.Results.TryGetValue("IE", out var ie)) providers = ie;
                else if (watchProviders.Results.TryGetValue("US", out var us)) providers = us;
                else if (watchProviders.Results.TryGetValue("GB", out var gb)) providers = gb;
                else if (watchProviders.Results.Count > 0) providers = watchProviders.Results.Values.FirstOrDefault();

                var allProviders = new Dictionary<string, List<ProviderInfo>>();
                if (providers != null)
                {
                    if (providers.Streaming?.Any() == true)
                        allProviders["Streaming"] = providers.Streaming.ToList();
                    if (providers.Buy?.Any() == true)
                        allProviders["Buy"] = providers.Buy.ToList();
                    if (providers.Rent?.Any() == true)
                        allProviders["Rent"] = providers.Rent.ToList();
                }
                ViewData["WatchProviders"] = allProviders;
            }

            // 3. Check if the currently logged-in user has already logged this movie
            var userId = _userManager.GetUserId(User);
            bool isAlreadyLogged = await _dbContext.Movies
                .AnyAsync(m => m.UserId == userId && m.TmdbId == tmdbId);
            bool isInWishlist = await _dbContext.WishlistItems.AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
            bool isInBlacklist = await _dbContext.BlacklistedMovies.AnyAsync(b => b.UserId == userId && b.TmdbId == tmdbId);

            ViewData["IsAlreadyLogged"] = isAlreadyLogged;
            ViewData["IsInWishlist"] = isInWishlist;
            ViewData["IsInBlacklist"] = isInBlacklist;

            // Pass the full movie details object from TMDB to the new view
            return View(movieDetails);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (movie == null)
            {
                return NotFound();
            }

            if (movie.TmdbId.HasValue)
            {
                // Get "Where to Watch" info (Streaming, Buy, Rent)
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId.Value);
                var watchProviders = await _tmdbService.GetWatchProvidersAsync(movie.TmdbId.Value, movieDetails?.Title ?? string.Empty);
                if (watchProviders?.Results != null)
                {
                    WatchProviderCountry? providers = null;
                    if (watchProviders.Results.TryGetValue("IE", out var ie)) providers = ie;
                    else if (watchProviders.Results.TryGetValue("US", out var us)) providers = us;
                    else if (watchProviders.Results.TryGetValue("GB", out var gb)) providers = gb;
                    else if (watchProviders.Results.Count > 0) providers = watchProviders.Results.Values.FirstOrDefault();

                    var allProviders = new Dictionary<string, List<ProviderInfo>>();
                    if (providers != null)
                    {
                        if (providers.Streaming?.Any() == true)
                            allProviders["Streaming"] = providers.Streaming.ToList();
                        if (providers.Buy?.Any() == true)
                            allProviders["Buy"] = providers.Buy.ToList();
                        if (providers.Rent?.Any() == true)
                            allProviders["Rent"] = providers.Rent.ToList();
                    }
                    ViewData["WatchProviders"] = allProviders;
                }

                // VVVV NEW LOGIC TO GET THE CAST VVVV
                if (movieDetails?.Credits?.Cast != null)
                {
                    // Pass the top 3 cast members to the view
                    ViewData["Cast"] = movieDetails.Credits.Cast.Take(3).ToList();
                }
                // ^^^^ END OF NEW LOGIC ^^^^
            }

            return View(movie);
        }

        [HttpGet]
        public async Task<IActionResult> GetSurpriseSuggestion()
        {
            _logger.LogInformation("GetSurpriseSuggestion action invoked.");

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all logged movies for this user (for pools and for recent exclusion)
            var loggedMovies = await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue && !string.IsNullOrEmpty(m.Director) && !string.IsNullOrEmpty(m.Genres) && m.ReleasedYear.HasValue)
                .ToListAsync();

            if (loggedMovies.Count < 3)
            {
                ViewData["SuggestionTitle"] = "Log at least 3 movies to get a 'Surprise Me!' suggestion.";
                ViewData["ShowAddMovieButton"] = true;
                return View("Suggest", new List<TmdbMovieBrief>());
            }

            // --- 1. Create Ingredient Pools ---
            var random = new Random();
            var directorPool = loggedMovies.Select(m => m.Director!).Distinct().ToList();
            var genrePool = loggedMovies.SelectMany(m => m.Genres!.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();
            var actorPool = new List<TmdbCastPerson>();
            foreach (var movie in loggedMovies.OrderByDescending(m => m.DateWatched).Take(15))
            {
                if (!movie.TmdbId.HasValue) continue;
                var tmdbIdValue = movie.TmdbId ?? 0;
                if (tmdbIdValue == 0) continue;
                var details = await _tmdbService.GetMovieDetailsAsync(tmdbIdValue);
                if (details?.Credits?.Cast != null)
                {
                    actorPool.AddRange(details.Credits.Cast.Take(3));
                }
            }
            actorPool = actorPool.DistinctBy(p => p.Id).ToList();

            if (!actorPool.Any())
            {
                ViewData["SuggestionTitle"] = "Could not find cast info in your recent logs.";
                return View("Suggest", new List<TmdbMovieBrief>());
            }

            // --- 2. Get recent 15 movie IDs with specific WatchedDate to exclude ---
            var recentMovieIds = await _dbContext.Movies
                .Where(m => m.UserId == userId && m.DateWatched.HasValue)
                .OrderByDescending(m => m.DateWatched)
                .Take(15)
                .Select(m => m.TmdbId)
                .ToListAsync();
            var recentMovieIdSet = recentMovieIds.Where(id => id.HasValue).Select(id => id!.Value).ToHashSet();

            // --- 3. Get user blacklist for filtering ---
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);

            // --- 4. Get session anti-repetition list ---
            var shownSurpriseIds = HttpContext.Session.Get<List<int>>("ShownSurpriseIds") ?? new List<int>();

            // --- 5. Get or set cycle step ---
            string cycleKey = "SurpriseCycleStep";
            int? sessionCycleStep = HttpContext.Session.GetInt32(cycleKey);
            int cycleStep;
            if (!sessionCycleStep.HasValue)
            {
                cycleStep = 1;
                HttpContext.Session.SetInt32(cycleKey, cycleStep);
                _logger.LogInformation($"DEBUG: Surprise Cycle Step not found in session. Initializing to 1.");
            }
            else
            {
                cycleStep = sessionCycleStep.Value;
            }
            _logger.LogInformation($"DEBUG: Current Surprise Cycle Step: {cycleStep}");

            // --- 6. Build ingredient IDs ---
            var randDirector = directorPool[random.Next(directorPool.Count)];
            var randDirectorId = await _tmdbService.GetPersonIdAsync(randDirector);
            var randActor = actorPool[random.Next(actorPool.Count)];
            var randActorId = randActor.Id;
            var allGenres = await _tmdbService.GetAllGenresAsync();
            var randGenre = genrePool[random.Next(genrePool.Count)];
            var randGenreId = allGenres.FirstOrDefault(g => g.Name == randGenre)?.Id;

            // --- 7. Cyclic 4-step logic ---
            List<TmdbMovieBrief> foundMovies = new();
            TmdbMovieBrief? suggestedMovie = null;
            string suggestionTitle = "Your Surprise Suggestion...";

            if (cycleStep == 1)
            {
                // Only 3/3 match
                var movies = await _tmdbService.DiscoverMoviesAsync(randDirectorId, randActorId, randGenreId, null);
                foundMovies = movies
                    .Where(m => m.VoteAverage >= 4.5)
                    .Where(m => !userBlacklistedIds.Contains(m.Id))
                    .Where(m => !recentMovieIdSet.Contains(m.Id))
                    .Where(m => !shownSurpriseIds.Contains(m.Id))
                    .DistinctBy(m => m.Id)
                    .ToList();
                if (foundMovies.Any())
                {
                    suggestedMovie = foundMovies[random.Next(foundMovies.Count)];
                }
            }
            else if (cycleStep == 2)
            {
                // Only 2/3 match (try all 2/3 combos, pick first with results)
                var combos = new List<(int? directorId, int? actorId, int? genreId)>
                {
                    (randDirectorId, randActorId, null),
                    (randDirectorId, null, randGenreId),
                    (null, randActorId, randGenreId)
                };
                foreach (var combo in combos)
                {
                    var movies = await _tmdbService.DiscoverMoviesAsync(combo.directorId, combo.actorId, combo.genreId, null);
                    var filtered = movies
                        .Where(m => m.VoteAverage >= 4.5)
                        .Where(m => !userBlacklistedIds.Contains(m.Id))
                        .Where(m => !recentMovieIdSet.Contains(m.Id))
                        .Where(m => !shownSurpriseIds.Contains(m.Id))
                        .DistinctBy(m => m.Id)
                        .ToList();
                    if (filtered.Any())
                    {
                        foundMovies = filtered;
                        suggestedMovie = foundMovies[random.Next(foundMovies.Count)];
                        break;
                    }
                }
            }
            else if (cycleStep == 3)
            {
                // Only 1/3 match (try all 1/3 combos, pick first with results)
                var combos = new List<(int? directorId, int? actorId, int? genreId)>
                {
                    (randDirectorId, null, null),
                    (null, randActorId, null),
                    (null, null, randGenreId)
                };
                foreach (var combo in combos)
                {
                    var movies = await _tmdbService.DiscoverMoviesAsync(combo.directorId, combo.actorId, combo.genreId, null);
                    var filtered = movies
                        .Where(m => m.VoteAverage >= 4.5)
                        .Where(m => !userBlacklistedIds.Contains(m.Id))
                        .Where(m => !recentMovieIdSet.Contains(m.Id))
                        .Where(m => !shownSurpriseIds.Contains(m.Id))
                        .DistinctBy(m => m.Id)
                        .ToList();
                    if (filtered.Any())
                    {
                        foundMovies = filtered;
                        suggestedMovie = foundMovies[random.Next(foundMovies.Count)];
                        break;
                    }
                }
            }
            else if (cycleStep == 4)
            {
                // Only random popular, with infinite retry logic
                int maxRetries = 10;
                int attempts = 0;
                TmdbMovieBrief? fallbackMovie = null;
                while (fallbackMovie == null && attempts < maxRetries)
                {
                    var candidateMovie = await _tmdbService.GetRandomPopularMovieAsync();
                    if (candidateMovie != null && candidateMovie.VoteAverage >= 4.5
                        && !userBlacklistedIds.Contains(candidateMovie.Id)
                        && !recentMovieIdSet.Contains(candidateMovie.Id)
                        && !shownSurpriseIds.Contains(candidateMovie.Id))
                    {
                        fallbackMovie = candidateMovie;
                    }
                    attempts++;
                }
                // If still no movie after retries, get ANY popular movie above 4.5
                if (fallbackMovie == null)
                {
                    int fallbackAttempts = 0;
                    while (fallbackMovie == null && fallbackAttempts < maxRetries)
                    {
                        var candidateMovie = await _tmdbService.GetRandomPopularMovieAsync();
                        if (candidateMovie != null && candidateMovie.VoteAverage >= 4.5)
                        {
                            fallbackMovie = candidateMovie;
                        }
                        fallbackAttempts++;
                    }
                }
                if (fallbackMovie != null)
                {
                    suggestedMovie = fallbackMovie;
                }
            }

            // If no suggestion found, fallback to random popular (with all filters except anti-repetition), infinite retry
            if (suggestedMovie == null)
            {
                int maxRetries = 10;
                int attempts = 0;
                TmdbMovieBrief? fallbackMovie = null;
                while (fallbackMovie == null && attempts < maxRetries)
                {
                    var candidateMovie = await _tmdbService.GetRandomPopularMovieAsync();
                    if (candidateMovie != null && candidateMovie.VoteAverage >= 4.5
                        && !userBlacklistedIds.Contains(candidateMovie.Id)
                        && !recentMovieIdSet.Contains(candidateMovie.Id))
                    {
                        fallbackMovie = candidateMovie;
                    }
                    attempts++;
                }
                // If still no movie after retries, get ANY popular movie above 4.5
                if (fallbackMovie == null)
                {
                    int fallbackAttempts = 0;
                    while (fallbackMovie == null && fallbackAttempts < maxRetries)
                    {
                        var candidateMovie = await _tmdbService.GetRandomPopularMovieAsync();
                        if (candidateMovie != null && candidateMovie.VoteAverage >= 4.5)
                        {
                            fallbackMovie = candidateMovie;
                        }
                        fallbackAttempts++;
                    }
                }
                if (fallbackMovie != null)
                {
                    suggestedMovie = fallbackMovie;
                }
            }
            // Never show a failure message; always find something

            // --- 8. Session anti-repetition memory update ---
            var suggestedMoviesList = new List<TmdbMovieBrief>();
            if (suggestedMovie != null)
            {
                suggestedMoviesList.Add(suggestedMovie);
                shownSurpriseIds.Add(suggestedMovie.Id);
                if (shownSurpriseIds.Count > 20) shownSurpriseIds.RemoveAt(0);
                HttpContext.Session.Set("ShownSurpriseIds", shownSurpriseIds);
            }
            else
            {
                HttpContext.Session.Remove("ShownSurpriseIds");
            }

            // --- 9. Advance cycle step ---
            int nextStep = (cycleStep % 4) + 1;
            _logger.LogInformation($"DEBUG: Setting next Surprise Cycle Step to: {nextStep}");
            HttpContext.Session.SetInt32(cycleKey, nextStep);

            ViewData["SuggestionTitle"] = suggestionTitle;
            ViewData["NextSuggestionType"] = "surprise_me";
            return View("Suggest", suggestedMoviesList);
        }

        [HttpGet]
        public async Task<IActionResult> SelectGenre()
        {
            _logger.LogInformation("SelectGenre action invoked.");

            var loggedMovies = await _dbContext.Movies
                .Where(m => m.TmdbId.HasValue)
                .ToListAsync();

            if (!loggedMovies.Any())
            {
                ViewData["SuggestionTitle"] = "Log some movies to get genre suggestions!";
                ViewData["ShowAddMovieButton"] = true;
                return View("Suggest");
            }

            // Fetch all genres from our service one time
            var allGenres = await _tmdbService.GetAllGenresAsync();
            var allGenresDict = (allGenres ?? new List<TmdbGenre>()).ToDictionary(g => g.Id, g => g.Name ?? string.Empty);

            var genreCounts = new Dictionary<int, int>();

            // This can be slow if there are many movies. We can optimize it later.
            foreach (var movie in loggedMovies)
            {
                if (!movie.TmdbId.HasValue)
                {
                    continue;
                }
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId.Value);
                if (movieDetails?.Genres != null)
                {
                    foreach (var genre in movieDetails.Genres)
                    {
                        if (genreCounts.ContainsKey(genre.Id))
                        {
                            genreCounts[genre.Id]++;
                        }
                        else
                        {
                            genreCounts[genre.Id] = 1;
                        }
                    }
                }
            }

            // Get the top 6 genres based on frequency
            var topGenreIds = genreCounts.OrderByDescending(kvp => kvp.Value).Take(6).Select(kvp => kvp.Key).ToList();

            var topGenres = (allGenres ?? new List<TmdbGenre>()).Where(g => topGenreIds.Contains(g.Id)).ToList();

            ViewData["GenreSuggestions"] = topGenres;
            ViewData["SuggestionTitle"] = "Choose Your Favorite Genre";

            return View("Suggest");
        }

        [HttpGet]
        public async Task<IActionResult> SearchTmdbApi(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { results = new List<object>() });
            }

            var searchResult = await _tmdbService.SearchMoviesAsync(query);

            if (searchResult != null && searchResult.Results != null)
            {
                // This part is the fix: It creates a clean list of results
                // in the exact format the JavaScript expects.
                var simplifiedResults = searchResult.Results.Select(m => new
                {
                    id = m.Id,
                    title = m.Title,
                    releaseDate = m.ReleaseDate,
                    overview = m.Overview
                }).ToList();

                return Json(new { results = simplifiedResults });
            }

            return Json(new { results = new List<object>(), error = "Search failed or no results found." });
        }


        [HttpGet]
        public IActionResult Suggest()
        {
            // This action is for displaying the initial suggestion page.
            // It doesn't need to fetch any data yet, it just shows the view with the criteria buttons.
            // When it returns View(), it will look for Views/Movies/Suggest.cshtml.
            return View();
        }


        // In MoviesController.cs - PASTE THIS ENTIRE BLOCK OF CODE

        #region Suggestion Actions & Helpers

        [HttpGet]
        public async Task<IActionResult> ShowSuggestions(string suggestionType, string? query = null, int page = 1)
        // === FRESH START RESET ===
        // Resets director sequence when user refreshes page (query is null/empty)
        // Ensures page refresh always starts with recent director, not random
                    // === SMART SKIP DEDUPLICATION ===  
                    // Prevents showing same director consecutively in sequence
                    // Works by deduplicating topDirectorQueue before director selection
                    // Handles edge case where recent = frequent = rated director
    // === ANTI-REPETITION LOGIC ===
    // Avoids repeating last shown director in random mode
    // Fallback allows repetition if only one director available
                // === BULLETPROOF FALLBACK (CRITICAL) ===
                // This section ensures we NEVER show "No available director suggestions"
                // If any director selection fails, force random selection as last resort
                // DO NOT REMOVE - prevents app crashes and user frustration
        /*
         * HISTORICAL CONTEXT - WHY THESE FIXES WERE NEEDED:
         * 
         * PROBLEM 1: "No available director suggestions" error
         * - Occurred when director selection logic failed silently
         * - Solution: Bulletproof fallback that always shows something
         * 
         * PROBLEM 2: Same director showing repeatedly in sequence
         * - Occurred when recent = frequent = rated director  
         * - Solution: Smart deduplication of topDirectorQueue
         * 
         * PROBLEM 3: Page refresh continuing random sequence instead of starting fresh
         * - Occurred because session state persisted across page loads
         * - Solution: Fresh start detection and sequence reset
         * 
         * LESSONS LEARNED:
         * - Always have bulletproof fallbacks for user-facing features
         * - Edge cases matter: single director, missing data, etc.
         * - Session state management is tricky across page refreshes
         * - Incremental changes safer than large refactors
         * 
         * WARNING: This code has been problematic in the past. 
         * Test thoroughly before making changes.
         */
        {
            _logger.LogInformation("ShowSuggestions invoked with type: {Type}, query: {Query}, page: {Page}", suggestionType, query, page);

            List<TmdbMovieBrief> suggestedMovies = new List<TmdbMovieBrief>();
            string suggestionTitle = "Suggestions";
            string nextSuggestionType = suggestionType;
            string? nextQuery = query;


            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            switch (suggestionType?.ToLower())
            {
                case "trending":
                    suggestionTitle = "Trending Movies Today";
                    suggestedMovies = (await _tmdbService.GetTrendingMoviesAsync(page)).Take(3).ToList();
                    ViewData["NextPage"] = page + 1;
                    break;

                case "director_recent":
                case "director_frequent":
                case "director_rated":
                case "director_random":
                {
                    // --- Secuencia de sugerencias por director: recent → frequent → rated → random ---
    string directorTypeKey = $"DirectorTypeSequence_{userId}";
    bool isFreshStart = string.IsNullOrWhiteSpace(query);
    int directorTypeCount;
    if (isFreshStart)
    {
        HttpContext.Session.SetInt32(directorTypeKey, 0);
        directorTypeCount = 0;
        _logger.LogInformation("[SEQUENCE RESET] Fresh start detected, resetting director sequence to 0 (recent)");
    }
    else
    {
        directorTypeCount = HttpContext.Session.GetInt32(directorTypeKey) ?? 0;
    }
    // Secuencia: reciente, frecuente, mejor valorado, luego random infinito
    string[] directorTypes = new[] { "director_recent", "director_frequent", "director_rated" };
    int maxDirectorTypeIndex = directorTypes.Length - 1;
    string currentDirectorType = directorTypeCount <= maxDirectorTypeIndex
        ? directorTypes[directorTypeCount]
        : "director_random";
    string nextDirectorType = directorTypeCount < maxDirectorTypeIndex
        ? directorTypes[directorTypeCount + 1]
        : "director_random";
    // Avanzar la secuencia: después de los tres primeros, quedarse en random para siempre
    if (directorTypeCount <= maxDirectorTypeIndex)
    {
        HttpContext.Session.SetInt32(directorTypeKey, directorTypeCount + 1);
    }
    else
    {
        // Ya en random infinito, mantener el contador en ese valor
        HttpContext.Session.SetInt32(directorTypeKey, maxDirectorTypeIndex + 1);
    }
    _logger.LogInformation("=== DIRECTOR SEQUENCE DEBUG ===");
    _logger.LogInformation("Session key: {Key}", directorTypeKey);
    _logger.LogInformation("Session value: {Value}", directorTypeCount);
    _logger.LogInformation("Current director type: {Type}", currentDirectorType);
    _logger.LogInformation("Expected sequence: recent(0) → frequent(1) → rated(2) → random(3)");
    _logger.LogInformation("Director suggestion type: {DirectorType} for user {UserId}", currentDirectorType, userId);

                    // Forzar el tipo de sugerencia según la secuencia
                    var loggedDirectorMovies = await _dbContext.Movies.Where(m => m.UserId == userId && !string.IsNullOrEmpty(m.Director) && m.Director != "N/A" && m.TmdbId.HasValue).ToListAsync();
                    if (!loggedDirectorMovies.Any())
                    {
                        suggestionTitle = "Log some movies to get director suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }


    var topDirectorQueue = new List<string>();
    var recentDirector = loggedDirectorMovies.OrderByDescending(m => m.DateWatched).FirstOrDefault()?.Director;
    var frequentDirector = loggedDirectorMovies.GroupBy(m => m.Director!).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
    // Obtener lista ordenada de directores por promedio de rating (mínimo 2 películas calificadas)
    var ratedDirectors = loggedDirectorMovies
        .Where(m => m.UserRating.HasValue)
        .GroupBy(m => m.Director!)
        .Where(g => g.Count() >= 2) // Opcional: mínimo 2 películas calificadas
        .Select(g => new { Name = g.Key, Avg = g.Average(m => m.UserRating!.Value) })
        .OrderByDescending(d => d.Avg)
        .Select(d => d.Name)
        .ToList();
    if (recentDirector is string rd) topDirectorQueue.Add(rd);
    if (frequentDirector is string fd && !topDirectorQueue.Contains(fd)) topDirectorQueue.Add(fd);
    if (ratedDirectors.Any() && !topDirectorQueue.Contains(ratedDirectors[0])) topDirectorQueue.Add(ratedDirectors[0]);
    // Debug: Log original and deduplicated director queues
    _logger.LogInformation("Original topDirectorQueue: {Queue}", string.Join(", ", topDirectorQueue));
    var dedupedDirectorQueue = topDirectorQueue.Distinct().ToList();
    _logger.LogInformation("Deduplicated queue: {Queue}", string.Join(", ", dedupedDirectorQueue));
    _logger.LogInformation("Deduped queue count: {Count}", dedupedDirectorQueue.Count);
    _logger.LogInformation("=== TOP DIRECTOR QUEUE DEBUG ===");
    _logger.LogInformation("Recent director: {Recent}", recentDirector);
    _logger.LogInformation("Frequent director: {Frequent}", frequentDirector);
    _logger.LogInformation("Rated director: {Rated}", ratedDirectors.FirstOrDefault());

                    string? directorToSuggest = null;
                    List<TmdbMovieBrief> directorSuggestions = new();

    if (currentDirectorType == "director_recent" && dedupedDirectorQueue.Count > 0)
    {
        _logger.LogInformation("EXECUTING: director_recent block");
        _logger.LogInformation("Using director from index {Index}: {Director}", 0, dedupedDirectorQueue[0]);
        var d = dedupedDirectorQueue[0];
        var movies = await GetSuggestionsForDirector(d, userId);
        if (movies.Any())
        {
            directorToSuggest = d;
            directorSuggestions = movies;
        }
    }
    else if (currentDirectorType == "director_frequent" && dedupedDirectorQueue.Count > 1)
    {
        _logger.LogInformation("EXECUTING: director_frequent block");
        _logger.LogInformation("Using director from index {Index}: {Director}", 1, dedupedDirectorQueue[1]);
        var d = dedupedDirectorQueue[1];
        var movies = await GetSuggestionsForDirector(d, userId);
        if (movies.Any())
        {
            directorToSuggest = d;
            directorSuggestions = movies;
        }
    }
    else if (currentDirectorType == "director_rated" && dedupedDirectorQueue.Count > 2)
    {
        _logger.LogInformation("EXECUTING: director_rated block");
        foreach (var d in ratedDirectors)
        {
            if (dedupedDirectorQueue.Contains(d)) // Solo sugerir si está en la queue deduplicada
            {
                var movies = await GetSuggestionsForDirector(d, userId);
                if (movies.Any())
                {
                    _logger.LogInformation("Using rated director: {Director}", d);
                    directorToSuggest = d;
                    directorSuggestions = movies;
                    break;
                }
            }
        }
    }
    // True random director selection y anti-repetición de películas SOLO para director_random
    var allDirectors = loggedDirectorMovies.Select(m => m.Director!).Distinct().ToList();
                if (currentDirectorType == "director_random")
    {
        _logger.LogInformation("EXECUTING: director_random block");
        if (!allDirectors.Any())
        {
            suggestionTitle = "No available director suggestions. Try reshuffling or logging more movies.";
            break;
        }
        // Evitar repetir el último director random sugerido (si hay más de uno)
        string lastRandomDirectorKey = $"LastRandomDirector_{userId}";
        string? lastRandomDirector = HttpContext.Session.GetString(lastRandomDirectorKey);
        var availableDirectors = allDirectors;
        if (!string.IsNullOrEmpty(lastRandomDirector) && allDirectors.Count > 1)
        {
            var filtered = allDirectors.Where(d => d != lastRandomDirector).ToList();
            // Si al filtrar queda vacío, volvemos a permitir el último
            if (filtered.Count > 0)
                availableDirectors = filtered;
        }
        var random = Random.Shared;
        var randomIndex = random.Next(0, availableDirectors.Count);
        _logger.LogInformation("All directors pool: {Directors}", string.Join(", ", allDirectors));
        _logger.LogInformation("Available directors (no repeat): {Directors}", string.Join(", ", availableDirectors));
        _logger.LogInformation("Random index selected: {Index}", randomIndex);
        var selectedDirector = availableDirectors[randomIndex];
        _logger.LogInformation("Selected director (before checking movies): {Director}", selectedDirector);
        var movies = await GetSuggestionsForDirector(selectedDirector, userId);
        directorToSuggest = selectedDirector;
        directorSuggestions = movies.Take(Math.Min(3, movies.Count)).ToList();
        nextSuggestionType = "director_random";
        // Guardar el último director random sugerido
        HttpContext.Session.SetString(lastRandomDirectorKey, selectedDirector);
    }
                // Bulletproof fallback: if no director suggestions found, force random selection
                if (directorSuggestions.Count == 0)
                {
                    _logger.LogWarning("No director suggestions found for {Type}, forcing random fallback", currentDirectorType);
                    // Force random selection as ultimate fallback
                    var allDirectorsFallback = loggedDirectorMovies.Select(m => m.Director!).Distinct().ToList();
                    if (allDirectorsFallback.Any())
                    {
                        var random = Random.Shared;
                        var fallbackDirector = allDirectorsFallback[random.Next(allDirectorsFallback.Count)];
                        var fallbackMovies = await GetSuggestionsForDirector(fallbackDirector, userId);
                        directorToSuggest = fallbackDirector;
                        directorSuggestions = fallbackMovies.Take(Math.Min(3, fallbackMovies.Count)).ToList();
                        suggestionTitle = $"Because you like {fallbackDirector}... (Random)";
                        _logger.LogInformation("Fallback director selected: {Director}", fallbackDirector);
                    }
                    else
                    {
                        // Absolute last resort (should never happen)
                        suggestionTitle = "Log some movies to get director suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }
                }
    suggestedMovies = directorSuggestions;
    suggestionTitle = $"Because you like {directorToSuggest}...";
    nextSuggestionType = nextDirectorType;
    nextQuery = directorToSuggest;
    break;
                }

                case "genre_recent":
                case "genre_frequent":
                case "genre_rated":
                case "genre_random":
                    // Genre Suggestion Logic with skip for all-blacklisted
                    #region Genre Suggestion Logic
                    var loggedGenreMovies = await _dbContext.Movies.Where(m => m.UserId == userId && !string.IsNullOrEmpty(m.Genres) && m.TmdbId.HasValue).ToListAsync();
                    if (!loggedGenreMovies.Any())
                    {
                        suggestionTitle = "Log movies with genres to get suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }

                    // Defensive: Only use genres from movies where Genres is not null
                    var allUserGenres = loggedGenreMovies
                        .Where(m => !string.IsNullOrEmpty(m.Genres))
                        .SelectMany(m => (m.Genres ?? string.Empty).Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                        .ToList();

                    var topGenreQueue = new List<string>();
                    var firstGenre = loggedGenreMovies
                        .OrderByDescending(m => m.DateWatched)
                        .FirstOrDefault()?.Genres?.Split(new[] { ", " }, StringSplitOptions.None).FirstOrDefault();
                    if (!string.IsNullOrEmpty(firstGenre)) topGenreQueue.Add(firstGenre.Trim());
                    var mostFrequentGenre = allUserGenres.GroupBy(g => g).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                    if (!string.IsNullOrEmpty(mostFrequentGenre) && !topGenreQueue.Contains(mostFrequentGenre)) topGenreQueue.Add(mostFrequentGenre);
                    var highestRatedGenres = loggedGenreMovies
                        .Where(m => m.UserRating.HasValue && m.UserRating.Value >= 4.0m && !string.IsNullOrEmpty(m.Genres))
                        .SelectMany(m => m.Genres!.Split(new[] { ", " }, StringSplitOptions.None))
                        .ToList();
                    var highestRatedGenre = highestRatedGenres.GroupBy(g => g).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                    if (!string.IsNullOrEmpty(highestRatedGenre) && !topGenreQueue.Contains(highestRatedGenre)) topGenreQueue.Add(highestRatedGenre);

                    string? genreToSuggest = null;
                    List<TmdbMovieBrief> genreSuggestions = new();
                    int genreStartIdx = 0;
                    if (suggestionType == "genre_recent") genreStartIdx = 0;
                    else if (suggestionType == "genre_frequent") genreStartIdx = 1;
                    else if (suggestionType == "genre_rated") genreStartIdx = 2;
                    else genreStartIdx = 0;

                    for (int i = genreStartIdx; i < topGenreQueue.Count; i++)
                    {
                        var g = topGenreQueue[i];
                        var movies = await GetSuggestionsForGenre(g, userId);
                        if (movies.Any())
                        {
                            genreToSuggest = g;
                            genreSuggestions = movies;
                            nextSuggestionType = i == 0 ? "genre_frequent" : i == 1 ? "genre_rated" : "genre_random";
                            break;
                        }
                    }
                    // If none found, try random
                    if (genreSuggestions.Count == 0)
                    {
                        var potentialGenres = allUserGenres.Distinct().Where(g => g != query).ToList();
                        if (!potentialGenres.Any()) potentialGenres = allUserGenres.Distinct().ToList();
                        foreach (var g in potentialGenres.OrderBy(_ => Guid.NewGuid()))
                        {
                            var movies = await GetSuggestionsForGenre(g, userId);
                            if (movies.Any())
                            {
                                genreToSuggest = g;
                                genreSuggestions = movies;
                                nextSuggestionType = "genre_random";
                                break;
                            }
                        }
                    }
                    if (genreSuggestions.Count == 0)
                    {
                        suggestionTitle = "No available genre suggestions. Try reshuffling or logging more movies.";
                        break;
                    }
                    suggestedMovies = genreSuggestions;
                    suggestionTitle = $"Popular {genreToSuggest} Movies";
                    nextQuery = genreToSuggest;
                    #endregion
                    break;

                case "cast_recent":
                case "cast_frequent":
                case "cast_rated":
                case "cast_random":
                    /*
                     * =================== CAST SUGGESTION SYSTEM DOCUMENTATION ===================
                     *
                     * 1. Estrategia general:
                     *    - Se toma un pool de las últimas 5 películas logueadas por el usuario (ordenadas por DateWatched DESC).
                     *    - De cada película se extraen hasta 3 actores principales (top 3 cast TMDB), generando típicamente ~15 actores únicos.
                     *    - Se utiliza un caché local por request para detalles de TMDB (ver TmdbService), y TMDBService implementa un caché de 6 horas para evitar llamadas redundantes y mejorar performance.
                     *    - Se implementa un sistema anti-duplicado: nunca se muestra el mismo actor dos veces seguidas en ningún reshuffle (se guarda el último actor sugerido en sesión por usuario).
                     *
                     * 2. Lógica de cada tipo de sugerencia:
                     *    - cast_recent: Sugiere 1 actor random del top 5 cast de la película más reciente.
                     *    - cast_frequent: Sugiere 1 actor random de los actores que aparecen en 2+ películas del usuario (si es igual al anterior, salta al siguiente distinto; si no hay, fallback a rated).
                     *    - cast_rated: Sugiere 1 actor random del top 5 cast de la película mejor puntuada por el usuario (si es igual al anterior, salta al siguiente distinto; si no hay, fallback a random).
                     *    - cast_random: Sugiere 1 actor random del pool completo de actores únicos (anti-repetición usando sesión).
                     *
                     * 3. Optimizaciones implementadas:
                     *    - Se usan solo 5 películas recientes para balancear performance y variedad: menos llamadas a TMDB, pero suficiente diversidad para la mayoría de los usuarios.
                     *    - El caché TMDB (6h) y el pool local por request minimizan latencia y evitan sobrecargar la API.
                     *    - Si un reshuffle no encuentra actor válido (por anti-duplicado o falta de datos), se hace fallback automático al siguiente tipo (frequent → rated → random).
                     *
                     * 4. Decisiones de diseño:
                     *    - No se tocan los filtros existentes (por usuario, blacklists, etc.) para mantener seguridad y privacidad.
                     *    - Se mantiene el uso de ViewData y Session para compatibilidad con el resto del sistema y la UI.
                     *    - El anti-duplicado es simple (solo evita repeticiones consecutivas) porque es suficiente para la experiencia y fácil de mantener/extender.
                     *
                     * 5. Futuras mejoras posibles:
                     *    - Permitir expandir el pool a más de 5 películas si usuarios avanzados piden más variedad.
                     *    - Mejorar el anti-duplicado para evitar repeticiones "salteadas" (A, B, A) si hay feedback negativo.
                     *    - Considerar otros criterios de frecuencia (por ejemplo, actores con más películas vistas, o ponderar por rating).
                     *
                     * 6. Objetivo:
                     *    - Que cualquier desarrollador (o nosotros en el futuro) pueda entender exactamente cómo y por qué funciona el sistema de sugerencias de cast, y qué se puede ajustar según necesidades o feedback.
                     * ===============================================================================
                     */
                    // === CAST SUGGESTION LOGIC WITH BULLETPROOF FIXES ===
                    // PHASE 1: Session-based sequence management (like directors)
                    string castTypeKey = $"CastTypeSequence_{userId}";
                    bool isFreshStartCast = string.IsNullOrWhiteSpace(query);
                    int castTypeCount;
                    if (isFreshStartCast)
                    {
                        HttpContext.Session.SetInt32(castTypeKey, 0);
                        castTypeCount = 0;
                        _logger.LogInformation("[SEQUENCE RESET] Fresh start detected, resetting cast sequence to 0 (recent)");
                    }
                    else
                    {
                        castTypeCount = HttpContext.Session.GetInt32(castTypeKey) ?? 0;
                    }
                    string[] castTypes = new[] { "cast_recent", "cast_frequent", "cast_rated" };
                    int maxCastTypeIndex = castTypes.Length - 1;
                    string currentCastType = castTypeCount <= maxCastTypeIndex ? castTypes[castTypeCount] : "cast_random";
                    string nextCastType = castTypeCount < maxCastTypeIndex ? castTypes[castTypeCount + 1] : "cast_random";
                    if (castTypeCount <= maxCastTypeIndex)
                    {
                        HttpContext.Session.SetInt32(castTypeKey, castTypeCount + 1);
                    }
                    else
                    {
                        HttpContext.Session.SetInt32(castTypeKey, maxCastTypeIndex + 1);
                    }
                    _logger.LogInformation("=== CAST SEQUENCE DEBUG ===");
                    _logger.LogInformation("Session key: {Key}", castTypeKey);
                    _logger.LogInformation("Session value: {Value}", castTypeCount);
                    _logger.LogInformation("Current cast type: {Type}", currentCastType);
                    _logger.LogInformation("Expected sequence: recent(0) → frequent(1) → rated(2) → random(3)");
                    _logger.LogInformation("Cast suggestion type: {CastType} for user {UserId}", currentCastType, userId);

                    // PHASE 2: Build and deduplicate actor queue (with local TMDB details pool scoped to this block only)
                    // Local per-request TMDB details pool (scoped strictly to cast logic)
                    var tmdbDetailsPool = new Dictionary<int, TmdbMovieDetails?>();
                    async Task<TmdbMovieDetails?> GetMovieDetailsWithPoolAsync(int tmdbId)
                    {
                        if (tmdbDetailsPool.TryGetValue(tmdbId, out var cached))
                            return cached;
                        var details = await _tmdbService.GetMovieDetailsAsync(tmdbId);
                        tmdbDetailsPool[tmdbId] = details;
                        return details;
                    }

                    var loggedCastMovies = await _dbContext.Movies.Where(m => m.UserId == userId && m.TmdbId.HasValue).OrderByDescending(m => m.DateWatched).ToListAsync();
                    if (loggedCastMovies == null || !loggedCastMovies.Any())
                    {
                        suggestionTitle = "Log some movies to get cast suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }
                    var allTopActors = new List<TmdbCastPerson>();
                    foreach (var movie in loggedCastMovies.Take(5))
                    {
                        if (!movie.TmdbId.HasValue) continue;
                        var details = await GetMovieDetailsWithPoolAsync(movie.TmdbId.Value);
                        if (details?.Credits?.Cast != null) allTopActors.AddRange(details.Credits.Cast.Take(3));
                    }
                    if (!allTopActors.Any()) { suggestionTitle = "Could not find cast info in your recent logs."; break; }

                    // ...existing code for cast suggestion logic...
                    // (No changes to the rest of the cast suggestion logic)

                    /*
                     * === CAST VARIETY IMPLEMENTATION ===
                     * 
                     * PROBLEM SOLVED: Cast suggestions were too predictable
                     * - Recent: Always showed lead actor from last movie
                     * - Frequent: Complex but repetitive selection
                     * - Rated: Always most popular actor
                     * 
                     * SOLUTION IMPLEMENTED:
                     * - Recent: Random selection from top 5 cast of most recent movie
                     * - Frequent: Random selection from top 3 most frequent actors in user's log
                     * - Rated: Random selection from top 5 cast of highest rated movie
                     * 
                     * BENEFITS:
                     * - Much more variety and less predictability
                     * - Still maintains logical meaning of each suggestion type
                     * - Performance conscious (only 2 additional API calls)
                     * - Preserves all session management and bulletproof fallback
                     * 
                     * SAFETY FEATURES:
                     * - Null-safe with pattern matching
                     * - Automatic deduplication prevents same actor in multiple categories
                     * - Handles edge cases: no ratings, no cast data, empty results
                     * - Detailed logging for debugging
                     * 
                     * WARNING: This logic has been carefully tuned for variety vs performance.
                     * Test thoroughly before making changes.
                     */

                    // === INTEGRATION WITH EXISTING SYSTEMS ===
                    // - Works seamlessly with session-based sequence management
                    // - Compatible with smart skip deduplication 
                    // - Preserves bulletproof fallback behavior
                    // - Maintains anti-repetition in random mode
                    // - Supports fresh start reset functionality

                    // --- New topActorQueue Building Logic with Variety and Safety ---
                    var topActorQueue = new List<TmdbCastPerson>();
                    var randomCast = Random.Shared;

                    // === RECENT CAST VARIETY ===
                    // Selects random actor from top 5 cast of most recently watched movie
                    // Provides variety while maintaining "recent" meaning
                    var mostRecentMovie = loggedCastMovies.FirstOrDefault();
                    if (mostRecentMovie?.TmdbId is int recentTmdbId)
                    {
                        var recentDetails = await GetMovieDetailsWithPoolAsync(recentTmdbId);
                        var recentCast = recentDetails?.Credits?.Cast?.Take(5).ToList();
                        if (recentCast != null && recentCast.Any())
                        {
                            var recentActor = recentCast[randomCast.Next(recentCast.Count)];
                            topActorQueue.Add(recentActor);
                            _logger.LogInformation("Recent: Selected {Actor} from most recent movie '{Title}'", recentActor.Name, mostRecentMovie.Title);
                        }
                    }

                    // === FREQUENT CAST VARIETY ===  
                    // Selects random actor from top 3 most frequent actors in user's log
                    // Balances predictability with variety
                    var allActorsById = allTopActors.GroupBy(a => a.Id)
                        .Select(g => new { Actor = g.First(), Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList();

                    if (allActorsById.Any())
                    {
                        // Take top 3 frequent actors for variety
                        var topFrequentActors = allActorsById.Take(3).Select(x => x.Actor).ToList();
                        var frequentActor = topFrequentActors[randomCast.Next(topFrequentActors.Count)];
                        // Avoid duplicate if already added as recent
                        if (!topActorQueue.Any(a => a.Id == frequentActor.Id))
                        {
                            topActorQueue.Add(frequentActor);
                            _logger.LogInformation("Frequent: Selected {Actor} from top frequent actors", frequentActor.Name);
                        }
                    }

                    // === RATED CAST VARIETY ===
                    // Selects random actor from top 5 cast of highest rated movie
                    // Connects suggestions to user's favorite films
                    var highestRatedMovie = loggedCastMovies
                        .Where(m => m.UserRating.HasValue)
                        .OrderByDescending(m => m.UserRating)
                        .FirstOrDefault();

                    if (highestRatedMovie?.TmdbId is int ratedTmdbId)
                    {
                        var ratedDetails = await GetMovieDetailsWithPoolAsync(ratedTmdbId);
                        var ratedCast = ratedDetails?.Credits?.Cast?.Take(5).ToList();
                        if (ratedCast != null && ratedCast.Any())
                        {
                            var ratedActor = ratedCast[randomCast.Next(ratedCast.Count)];
                            if (!topActorQueue.Any(a => a.Id == ratedActor.Id))
                            {
                                topActorQueue.Add(ratedActor);
                                _logger.LogInformation("Rated: Selected {Actor} from highest rated movie '{Title}'", ratedActor.Name, highestRatedMovie.Title);
                            }
                        }
                    }

                    // --- End of new topActorQueue logic ---
                    // Deduplicate as before:
                    var dedupedActorQueue = topActorQueue.DistinctBy(a => a.Id).ToList();
                    _logger.LogInformation("Original topActorQueue count: {Count}", topActorQueue.Count);
                    _logger.LogInformation("Deduplicated queue count: {Count}", dedupedActorQueue.Count);

                    /*
                     * HISTORICAL CONTEXT - WHY THIS VARIETY WAS ADDED:
                     * 
                     * ORIGINAL PROBLEM:
                     * - User complained: "Why does the first cast suggestion always show 
                     *   the same actor from the last movie?"
                     * - Cast suggestions were too predictable and boring
                     * - Always showed lead actors, never supporting cast
                     * 
                     * LESSONS LEARNED FROM DIRECTOR FIXES:
                     * - Ask for Copilot's approach before implementing
                     * - Preserve existing robustness (session, bulletproof fallback)
                     * - Make minimal, targeted changes
                     * - Add comprehensive logging for debugging
                     * 
                     * IMPLEMENTATION NOTES:
                     * - Top 5 cast limit balances variety with relevance
                     * - Top 3 frequent actors prevents too much randomness
                     * - Deduplication ensures no repeated actors in same suggestion
                     * - Performance optimized (only necessary API calls)
                     */

                    TmdbCastPerson? actorToSuggest = null;
                    List<TmdbMovieBrief> actorSuggestions = new();
                    // PHASE 3: Sequence logic using deduped queue, avoiding consecutive repeats across all reshuffles
                    int castStep = -1;
                    if (currentCastType == "cast_recent") castStep = 0;
                    else if (currentCastType == "cast_frequent") castStep = 1;
                    else if (currentCastType == "cast_rated") castStep = 2;

                    // Get last actor ID from session (cross-reshuffle)
                    string lastActorSessionKey = $"LastCastActorId_{userId}";
                    int? lastActorId = null;
                    var lastActorIdStr = HttpContext.Session.GetString(lastActorSessionKey);
                    if (int.TryParse(lastActorIdStr, out var parsedId))
                        lastActorId = parsedId;

                    // Find the first actor in the queue for this step that is not the same as the previous one
                    if (castStep >= 0 && dedupedActorQueue.Count > castStep)
                    {
                        var candidate = dedupedActorQueue.Skip(castStep).FirstOrDefault(a => a.Id != lastActorId) ?? dedupedActorQueue[castStep];
                        var movies = await GetSuggestionsForActor(candidate.Id, userId);
                        if (movies.Any())
                        {
                            actorToSuggest = candidate;
                            actorSuggestions = movies;
                            // Store this actor as last shown for next reshuffle
                            HttpContext.Session.SetString(lastActorSessionKey, candidate.Id.ToString());
                        }
                    }
                    // PHASE 4: Anti-repetition in random
                    var allActors = allTopActors.DistinctBy(p => p.Id).ToList();
                    if (currentCastType == "cast_random")
                    {
                        _logger.LogInformation("EXECUTING: cast_random block");
                        string lastRandomActorKey = $"LastRandomActor_{userId}";
                        string? lastRandomActor = HttpContext.Session.GetString(lastRandomActorKey);
                        var availableActors = allActors;
                        if (!string.IsNullOrEmpty(lastRandomActor) && availableActors.Count > 1)
                        {
                            var filtered = availableActors.Where(a => a.Name != lastRandomActor).ToList();
                            if (filtered.Any()) availableActors = filtered;
                        }
                        var random = Random.Shared;
                        var randomIndex = random.Next(0, availableActors.Count);
                        var selectedActor = availableActors[randomIndex];
                        _logger.LogInformation("Selected actor (before checking movies): {Actor}", selectedActor.Name);
                        var movies = await GetSuggestionsForActor(selectedActor.Id, userId);
                        actorToSuggest = selectedActor;
                        actorSuggestions = movies.Take(Math.Min(3, movies.Count)).ToList();
                        nextCastType = "cast_random";
                        if (!string.IsNullOrEmpty(selectedActor.Name))
                        {
                            HttpContext.Session.SetString(lastRandomActorKey, selectedActor.Name);
                        }
                    }
                    // PHASE 5: Bulletproof fallback
                    if (actorSuggestions.Count == 0)
                    {
                        _logger.LogWarning("No cast suggestions found, forcing random fallback");
                        var fallbackActors = allActors;
                        if (fallbackActors.Any())
                        {
                            var random = Random.Shared;
                            var fallbackActor = fallbackActors[random.Next(fallbackActors.Count)];
                            var fallbackMovies = await GetSuggestionsForActor(fallbackActor.Id, userId);
                            actorToSuggest = fallbackActor;
                            actorSuggestions = fallbackMovies.Take(Math.Min(3, fallbackMovies.Count)).ToList();
                            suggestionTitle = $"Because you like movies with {fallbackActor.Name} (Random)";
                            _logger.LogInformation("Fallback actor selected: {Actor}", fallbackActor.Name);
                        }
                        else
                        {
                            suggestionTitle = "Log some movies to get cast suggestions!";
                            ViewData["ShowAddMovieButton"] = true;
                            break;
                        }
                    }
                    var actorDetails = actorToSuggest != null ? await _tmdbService.GetPersonDetailsAsync(actorToSuggest.Id) : null;
                    suggestedMovies = actorSuggestions;
                    suggestionTitle = $"Because you like movies with {actorToSuggest?.Name}";
                    ViewData["ActorProfilePath"] = actorDetails?.ProfilePath;
                    nextQuery = actorToSuggest?.Name;
                    break;

                case "year_recent":
                case "year_frequent":
                case "year_rated":
                case "year_random":
                    // Decade Suggestion Logic with skip for all-blacklisted
                    var loggedYearMovies = await _dbContext.Movies.Where(m => m.UserId == userId && m.ReleasedYear.HasValue).ToListAsync();
                    if (!loggedYearMovies.Any())
                    {
                        suggestionTitle = "Log some movies to get year-based suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }

                    var decades = loggedYearMovies.Select(m => (m.ReleasedYear!.Value / 10) * 10);
                    var topYearQueue = new List<int>();

                    if (loggedYearMovies.OrderByDescending(m => m.DateWatched).FirstOrDefault()?.ReleasedYear is int ry) topYearQueue.Add((ry / 10) * 10);
                    if (decades.GroupBy(d => d).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() is int fy && fy > 0 && !topYearQueue.Contains(fy)) topYearQueue.Add(fy);
                    var ratedDecade = loggedYearMovies.Where(m => m.UserRating.HasValue).GroupBy(m => (m.ReleasedYear!.Value / 10) * 10).Select(g => new { Decade = g.Key, Avg = g.Average(m => m.UserRating!.Value) }).OrderByDescending(d => d.Avg).Select(d => d.Decade).FirstOrDefault();
                    if (ratedDecade > 0 && !topYearQueue.Contains(ratedDecade)) topYearQueue.Add(ratedDecade);

                    int? decadeToSuggest = null;
                    List<TmdbMovieBrief> decadeSuggestions = new();
                    int decadeStartIdx = 0;
                    if (suggestionType == "year_recent") decadeStartIdx = 0;
                    else if (suggestionType == "year_frequent") decadeStartIdx = 1;
                    else if (suggestionType == "year_rated") decadeStartIdx = 2;
                    else decadeStartIdx = 0;

                    for (int i = decadeStartIdx; i < topYearQueue.Count; i++)
                    {
                        var d = topYearQueue[i];
                        var movies = await GetSuggestionsForDecade(d, userId);
                        if (movies.Any())
                        {
                            decadeToSuggest = d;
                            decadeSuggestions = movies;
                            nextSuggestionType = i == 0 ? "year_frequent" : i == 1 ? "year_rated" : "year_random";
                            break;
                        }
                    }
                    // If none found, try random
                    if (decadeSuggestions.Count == 0)
                    {
                        var allDecades = decades.Distinct().ToList();
                        var potentialDecades = allDecades.Where(d => d.ToString() != query).ToList();
                        if (!potentialDecades.Any()) potentialDecades = allDecades;
                        foreach (var d in potentialDecades.OrderBy(_ => Guid.NewGuid()))
                        {
                            var movies = await GetSuggestionsForDecade(d, userId);
                            if (movies.Any())
                            {
                                decadeToSuggest = d;
                                decadeSuggestions = movies;
                                nextSuggestionType = "year_random";
                                break;
                            }
                        }
                    }
                    if (decadeSuggestions.Count == 0)
                    {
                        suggestionTitle = "No available decade suggestions. Try reshuffling or logging more movies.";
                        break;
                    }
                    suggestedMovies = decadeSuggestions;
                    suggestionTitle = $"Top-Rated movies from the {decadeToSuggest}s";
                    nextQuery = decadeToSuggest?.ToString();
                    break;
                // ^^^^ END OF NEW YEAR LOGIC ^^^^

                default: return RedirectToAction("Suggest");
            }

            ViewData["SuggestionTitle"] = suggestionTitle;
            ViewData["NextSuggestionType"] = nextSuggestionType;
            ViewData["NextQuery"] = nextQuery;
            return View("Suggest", suggestedMovies);
        }



        // THIS IS THE NEW, SIMPLIFIED HELPER METHOD

        private async Task<List<TmdbMovieBrief>> GetSuggestionsForDirector(string directorName, string userId)
        {
            var directorId = await _tmdbService.GetPersonIdAsync(directorName);
            if (!directorId.HasValue) return new List<TmdbMovieBrief>();

            // Get the director's ENTIRE filmography from our service
            var allDirectorMovies = await _tmdbService.GetDirectorFilmographyAsync(directorId.Value);

            // Blacklist filter
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);
            allDirectorMovies = allDirectorMovies.Where(m => !userBlacklistedIds.Contains(m.Id)).ToList();

            if (!allDirectorMovies.Any())
            {
                return new List<TmdbMovieBrief>(); // Return empty if they have no movies
            }

            // If they have 3 or fewer movies, just return them all.
            if (allDirectorMovies.Count <= 3)
            {
                return allDirectorMovies;
            }

            // Otherwise, shuffle the entire list and take 3 random ones.
            var random = new Random();
            var randomSuggestions = allDirectorMovies.OrderBy(x => random.Next()).Take(3).ToList();

            return randomSuggestions;
        }


        private async Task<List<TmdbMovieBrief>> GetSuggestionsForGenre(string genreName, string userId)
        {
            var allGenres = await _tmdbService.GetAllGenresAsync();
            var genre = allGenres.FirstOrDefault(g => g.Name != null && g.Name.Equals(genreName, StringComparison.OrdinalIgnoreCase));
            if (genre == null) return new List<TmdbMovieBrief>();

            string sessionKey = $"GenrePage_{genreName.Replace(" ", "")}";
            int pageToFetch = HttpContext.Session.GetInt32(sessionKey) ?? 1;
            _logger.LogInformation("HELPER: Finding movies for genre {Genre}, starting at page {Page}", genreName, pageToFetch);

            var movies = await _tmdbService.DiscoverMoviesByGenreAsync(genre.Id, pageToFetch);

            // Filter out movies already logged by this user
            var userLoggedTmdbIds = (await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue && m.TmdbId != null)
                .Select(m => m.TmdbId)
                .ToListAsync())
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
            // Blacklist filter
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);
            movies = movies.Where(m => !userLoggedTmdbIds.Contains(m.Id) && !userBlacklistedIds.Contains(m.Id)).ToList();

            if (movies.Any())
            {
                HttpContext.Session.SetInt32(sessionKey, pageToFetch + 1);
            }
            else
            {
                // If we ran out of pages, reset and fetch page 1 again as a fallback.
                HttpContext.Session.SetInt32(sessionKey, 1);
                movies = await _tmdbService.DiscoverMoviesByGenreAsync(genre.Id, 1);
                movies = movies.Where(m => !userLoggedTmdbIds.Contains(m.Id) && !userBlacklistedIds.Contains(m.Id)).ToList();
            }

            return movies.Take(3).ToList();
        }

      

        private async Task<List<TmdbMovieBrief>> GetSuggestionsForActor(int actorId, string userId)
        {
            _logger.LogInformation("HELPER: Getting random movies for actor ID {ActorId}", actorId);

            // 1. Get the actor's filmography using the service
            var allActorMovies = await _tmdbService.DiscoverMoviesByActorAsync(actorId);

            // Filter out movies already logged by this user
            var userLoggedTmdbIds = (await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue && m.TmdbId != null)
                .Select(m => m.TmdbId)
                .ToListAsync())
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
            // Blacklist filter
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);
            allActorMovies = allActorMovies.Where(m => !userLoggedTmdbIds.Contains(m.Id) && !userBlacklistedIds.Contains(m.Id)).ToList();

            if (!allActorMovies.Any())
            {
                return new List<TmdbMovieBrief>();
            }

            // 2. If they have 3 or fewer movies, just return them all
            if (allActorMovies.Count <= 3)
            {
                return allActorMovies;
            }

            // 3. Otherwise, shuffle the entire list using Guid.NewGuid() and take 3 random ones
            var randomSuggestions = allActorMovies.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

            return randomSuggestions;
        }

        // In MoviesController.cs, add this new helper method


        private async Task<List<TmdbMovieBrief>> GetSuggestionsForDecade(int decade, string userId)
        {
            // Use a unique session key to remember the page for this specific decade
            string sessionKey = $"DecadePage_{decade}";
            int pageToFetch = HttpContext.Session.GetInt32(sessionKey) ?? 1;

            _logger.LogInformation("HELPER: Finding movies for decade {Decade}, starting at page {Page}", decade, pageToFetch);

            var movies = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, pageToFetch);

            // Filter out movies already logged by this user
            var userLoggedTmdbIds = (await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue && m.TmdbId != null)
                .Select(m => m.TmdbId)
                .ToListAsync())
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
            // Blacklist filter
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);
            movies = movies.Where(m => !userLoggedTmdbIds.Contains(m.Id) && !userBlacklistedIds.Contains(m.Id)).ToList();

            if (movies.Any())
            {
                // If we found movies, save the NEXT page number for the next reshuffle.
                HttpContext.Session.SetInt32(sessionKey, pageToFetch + 1);
            }
            else
            {
                // If we ran out of pages, reset the page counter and fetch page 1 again as a fallback.
                _logger.LogWarning("No movies found on page {Page} for {Decade}. Resetting to page 1.", pageToFetch, decade);
                HttpContext.Session.SetInt32(sessionKey, 1);
                movies = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, 1);
                movies = movies.Where(m => !userLoggedTmdbIds.Contains(m.Id) && !userBlacklistedIds.Contains(m.Id)).ToList();
            }

            return movies.Take(3).ToList();
        }



        ///

        [HttpGet]
        public async Task<IActionResult> GetTmdbMovieDetailsJson(int id) // 'id' here is the TMDB movie ID
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid TMDB movie ID." });
            }

            System.Diagnostics.Debug.WriteLine($"--- Controller: Attempting to get TMDB details for ID: {id} ---");
            var movieDetails = await _tmdbService.GetMovieDetailsAsync(id); // Assumes _tmdbService is injected

            // In MoviesController.cs -> GetTmdbMovieDetailsJson action
            if (movieDetails != null)
            {
                // ...
                return Json(new
                {
                    tmdbId = movieDetails.Id,
                    title = movieDetails.Title,
                    director = movieDetails.GetDirector(),
                    releaseYear = !string.IsNullOrEmpty(movieDetails.ReleaseDate) && movieDetails.ReleaseDate.Length >= 4
                                    ? movieDetails.ReleaseDate.Substring(0, 4)
                                    : null,
                    posterPath = movieDetails.PosterPath,
                    overview = movieDetails.Overview
                });
            }

            System.Diagnostics.Debug.WriteLine($"--- Controller: Movie details not found in TMDB for ID: {id} ---");
            return NotFound(new { error = "Movie details not found in TMDB." }); // Returns a 404 if TMDB doesn't find the movie
        }




        [HttpGet]
        public async Task<IActionResult> Add(int? tmdbId)
        {
            var viewModel = new AddMoviesViewModel
            {
                DateWatched = DateTime.Today // Set the default date here
            };

            if (tmdbId.HasValue && tmdbId.Value > 0)
            {
                // A TMDB ID was provided (e.g., from the Suggestion page), so pre-fill the ViewModel
                Console.WriteLine($"Add GET - Pre-filling form for TMDB ID: {tmdbId.Value}");
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId.Value);
                if (movieDetails != null)
                {
                    // Overwrite TMDB fields, but DateWatched remains as DateTime.Today
                    viewModel.Title = movieDetails.Title ?? string.Empty;
                    viewModel.Director = movieDetails.GetDirector() ?? "N/A";
                    viewModel.ReleasedYear = !string.IsNullOrEmpty(movieDetails.ReleaseDate) && movieDetails.ReleaseDate.Length >= 4
                                                ? int.Parse(movieDetails.ReleaseDate.Substring(0, 4))
                                                : null;
                    viewModel.PosterPath = movieDetails.PosterPath;
                    viewModel.Overview = movieDetails.Overview;
                    viewModel.TmdbId = movieDetails.Id;
                }
            }

            // This will return the view with either a blank viewModel (with DateWatched set)
            // or a pre-filled viewModel (which also has DateWatched set).
            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> Add(AddMoviesViewModel viewModel)
        {
            // Optional: Log received ViewModel values at the start for debugging
            Console.WriteLine(""); // Blank line for readability
            Console.WriteLine("--- Add POST Action Invoked ---");
            Console.WriteLine($"ViewModel Title Received: [{viewModel.Title}]");
            Console.WriteLine($"ViewModel Director Received: [{viewModel.Director}]");
            Console.WriteLine($"ViewModel ReleasedYear Received: [{viewModel.ReleasedYear}]");
            Console.WriteLine($"ViewModel PosterPath Received: [{viewModel.PosterPath}]");
            Console.WriteLine($"ViewModel Overview Snippet: [{viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 50))}...]");
            Console.WriteLine($"ViewModel DateWatched Received: {viewModel.DateWatched}");
            Console.WriteLine($"ViewModel WatchedLocation Received: {viewModel.WatchedLocation}");
            Console.WriteLine($"ViewModel IsRewatch Received: {viewModel.IsRewatch}"); // Log IsRewatch
            Console.WriteLine($"ViewModel TmdbId Received: [{viewModel.TmdbId}]"); // <<< ENSURE THIS LINE IS PRESENT
            Console.WriteLine($"ViewModel Subscribed Received: {viewModel.Subscribed}"); // Log Subscribed
            Console.WriteLine("-----------------------------");

            if (ModelState.IsValid)
            {


            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                // This will prevent non-logged-in users from adding movies.
                // We'll improve this with an [Authorize] tag later.
                return Unauthorized();
            }
            // ^^^^ END OF NEW LOGIC ^^^^


            var movie = new Ezequiel_Movies1.Models.Entities.Movies
            {
                Id = Guid.NewGuid(),
                Title = viewModel.Title,
                Director = viewModel.Director,
                ReleasedYear = viewModel.ReleasedYear,
                DateWatched = viewModel.DateWatched,
                WatchedLocation = viewModel.WatchedLocation,
                PosterPath = viewModel.PosterPath,
                Overview = viewModel.Overview,
                IsRewatch = viewModel.IsRewatch,
                Subscribed = viewModel.Subscribed,
                UserRating = viewModel.UserRating,
                TmdbId = viewModel.TmdbId,
                UserId = userId
            };

            // VVVV NEW LOGIC TO FETCH AND SAVE GENRES VVVV
            if (movie.TmdbId.HasValue)
            {
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId.Value);
                if (movieDetails?.Genres != null && movieDetails.Genres.Any())
                {
                    movie.Genres = string.Join(", ", movieDetails.Genres.Select(g => g.Name));
                    _logger.LogInformation("Saving genres for movie '{Title}': {Genres}", movie.Title, movie.Genres);
                }
            }
            // ^^^^ END OF NEW LOGIC ^^^^

            // Logging line for IsRewatch, after 'movie' object is created
            Console.WriteLine($"Adding Movie '{movie.Title}', UserRating from ViewModel: {viewModel.UserRating}, Value being saved to entity: {movie.UserRating}");

            await _dbContext.Movies.AddAsync(movie);

            // --- WISHLIST REMOVAL LOGIC (Atomic, User-Scoped) ---
            if (movie.TmdbId.HasValue)
            {
                var wishlistItem = await _dbContext.WishlistItems
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.TmdbId == movie.TmdbId);
                if (wishlistItem != null)
                {
                    _dbContext.WishlistItems.Remove(wishlistItem);
                    Console.WriteLine($"WishlistItem with TmdbId {(movie.TmdbId.HasValue ? movie.TmdbId.Value.ToString() : "N/A")} removed for user {userId} during Add.");
                }
            }
            // --- END WISHLIST REMOVAL LOGIC ---

            await _dbContext.SaveChangesAsync();
            Console.WriteLine($"Add POST - Movie Added Successfully: {movie.Title}");
            return RedirectToAction("List");
            }
            else // ModelState is NOT valid
            {
                Console.WriteLine("Add POST - ModelState IS INVALID. Movie not added.");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    if (value != null && value.Errors != null && value.Errors.Any())
                    {
                        Console.WriteLine($"-- Errors for '{modelStateKey}' --");
                        foreach (var error in value.Errors)
                        {
                            Console.WriteLine($"  - Message: {error.ErrorMessage}");
                            if (error.Exception != null)
                            {
                                Console.WriteLine($"    Exception: {error.Exception.Message}");
                            }
                        }
                    }
                }
                // Re-display the Add page with the submitted data and validation errors.
                // The JavaScript's showPreviewCardFromModelOnLoad() should handle re-showing the TMDB preview.
                Console.WriteLine("Add POST - Returning View(viewModel) due to invalid ModelState.");
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> List(string sortOrder, string searchString, int? pageNumber)
        {

            // 1. Get the current logged-in user's ID
            var userId = _userManager.GetUserId(User);

            // 2. Start the query AND immediately filter it to only include movies where the UserId matches.
            var moviesQuery = _dbContext.Movies.Where(m => m.UserId == userId);

            // Get the total movie count for this user and store it in ViewData.
            ViewData["MovieCount"] = await moviesQuery.CountAsync();
            // The rest of your existing logic for sorting and searching will now
            // automatically apply only to this filtered list of the user's own movies.

            _logger.LogInformation("Fetching movie list for User ID: {UserId}", userId);

            Console.WriteLine($"--- List Action Invoked --- SortOrder: [{sortOrder}], SearchString: [{searchString}], PageNumber: [{pageNumber}]");

            ViewData["CurrentFilter"] = searchString;

            // Determine the actual sort order to apply
            // If no sort order is specified from the URL, default to our preferred sort
            string actualSortToApply = string.IsNullOrEmpty(sortOrder) ? "datewatched_desc" : sortOrder;
            ViewData["CurrentSort"] = actualSortToApply; // Store the sort order that is actually being applied

            // --- VVVV REVISED & SIMPLIFIED LOGIC FOR SORT LINK PARAMETERS VVVV ---
            // The link for each column should point to the opposite of its own current state,
            // or to its primary sort direction if another column is active.
            ViewData["TitleSortParm"] = actualSortToApply == "title_asc" ? "title_desc" : "title_asc";
            ViewData["DirectorSortParm"] = actualSortToApply == "director_asc" ? "director_desc" : "director_asc";
            ViewData["YearSortParm"] = actualSortToApply == "year_asc" ? "year_desc" : "year_asc";
            ViewData["WatchedAtSortParm"] = actualSortToApply == "watchedat_asc" ? "watchedat_desc" : "watchedat_asc";

            // For these two, the logic correctly handles toggling from their preferred descending state.
            ViewData["DateWatchedSortParm"] = actualSortToApply == "datewatched_desc" ? "datewatched_asc" : "datewatched_desc";
            ViewData["RatingSortParm"] = actualSortToApply == "rating_desc" ? "rating_asc" : "rating_desc";
            // --- ^^^^ END OF REVISED LOGIC ^^^^ ---

            

            if (!String.IsNullOrEmpty(searchString))
            {
                moviesQuery = moviesQuery.Where(m =>
                    (!string.IsNullOrEmpty(m.Title) && m.Title.Contains(searchString)) ||
                    (!string.IsNullOrEmpty(m.Director) && m.Director.Contains(searchString)) ||
                    (m.ReleasedYear != null && m.ReleasedYear.Value.ToString().Contains(searchString))
                );
            }

            switch (actualSortToApply)
            {
                case "title_asc": moviesQuery = moviesQuery.OrderBy(m => m.Title); break;
                case "title_desc": moviesQuery = moviesQuery.OrderByDescending(m => m.Title); break;
                case "director_asc": moviesQuery = moviesQuery.OrderBy(m => m.Director); break;
                case "director_desc": moviesQuery = moviesQuery.OrderByDescending(m => m.Director); break;
                case "year_asc": moviesQuery = moviesQuery.OrderBy(m => m.ReleasedYear); break;
                case "year_desc": moviesQuery = moviesQuery.OrderByDescending(m => m.ReleasedYear); break;
                case "datewatched_asc": moviesQuery = moviesQuery.OrderBy(m => m.DateWatched); break;
                case "watchedat_asc": moviesQuery = moviesQuery.OrderBy(m => m.WatchedLocation); break;
                case "watchedat_desc": moviesQuery = moviesQuery.OrderByDescending(m => m.WatchedLocation); break;
                case "rating_desc": moviesQuery = moviesQuery.OrderByDescending(m => m.UserRating); break;
                case "rating_asc": moviesQuery = moviesQuery.OrderBy(m => m.UserRating); break;
                case "datewatched_desc":
                default: // Default case
                    moviesQuery = moviesQuery.OrderByDescending(m => m.DateWatched);
                    break;
            }

            int pageSize = 8;
            var paginatedMovies = await PaginatedList<Ezequiel_Movies1.Models.Entities.Movies>.CreateAsync(
                moviesQuery.AsNoTracking(),
                pageNumber ?? 1,
                pageSize);

            return View(paginatedMovies);
        }





        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userId = _userManager.GetUserId(User);

            // Find the movie only if the ID matches AND it belongs to the current user.
            var movieEntity = await _dbContext.Movies
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (movieEntity == null)
            {
                // If the movie doesn't exist or doesn't belong to the user, return Not Found.
                return NotFound();
            }

            // Enhanced log after loading the entity from the database
            Console.WriteLine($"Edit GET - Loaded Movie from DB (ID: {movieEntity.Id}): " +
                              $"Title='{movieEntity.Title}', " +
                              $"Director='{movieEntity.Director}', " +
                              $"ReleasedYear='{movieEntity.ReleasedYear}', " +
                              $"PosterPath='{movieEntity.PosterPath}', " +
                              $"Overview (snippet)='{movieEntity.Overview?.Substring(0, Math.Min(movieEntity.Overview?.Length ?? 0, 30))}...', " +
                              $"IsRewatch='{movieEntity.IsRewatch}', " +
                              $"UserRating='{movieEntity.UserRating}', " +
                              $"TmdbId='{movieEntity.TmdbId}'");


            var viewModel = new AddMoviesViewModel
            {
                Id = movieEntity.Id,
                Title = movieEntity.Title ?? string.Empty,
                Director = movieEntity.Director ?? string.Empty,
                ReleasedYear = movieEntity.ReleasedYear,
                DateWatched = movieEntity.DateWatched,
                WatchedLocation = movieEntity.WatchedLocation,
                Subscribed = movieEntity.Subscribed,
                PosterPath = movieEntity.PosterPath,
                Overview = movieEntity.Overview,
                IsRewatch = movieEntity.IsRewatch,
                UserRating = movieEntity.UserRating,
                TmdbId = movieEntity.TmdbId,
                Genres = movieEntity.Genres
            };

            // Enhanced log for the ViewModel being sent to the View
            Console.WriteLine($"Edit GET - ViewModel being sent to View (ID: {viewModel.Id}): " +
                              $"Title='{viewModel.Title}', " +
                              $"Director='{viewModel.Director}', " +
                              $"ReleasedYear='{viewModel.ReleasedYear}', " +
                              $"PosterPath='{viewModel.PosterPath}', " +
                              $"Overview (snippet)='{viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 30))}...', " +
                              $"IsRewatch='{viewModel.IsRewatch}', " +
                              $"UserRating='{viewModel.UserRating}', " +
                              $"TmdbId='{viewModel.TmdbId}'");

            return View(viewModel);
        }




        [HttpPost]
        public async Task<IActionResult> Edit(AddMoviesViewModel viewModel)
        {
            Console.WriteLine(""); // Blank line for readability in logs
            Console.WriteLine("--- Edit POST Action Invoked ---");
            Console.WriteLine($"ViewModel ID Received: {viewModel.Id}");
            Console.WriteLine($"ViewModel Title Received: [{viewModel.Title}]");
            Console.WriteLine($"ViewModel Director Received: [{viewModel.Director}]");
            Console.WriteLine($"ViewModel ReleasedYear Received FROM FORM: [{viewModel.ReleasedYear}]");
            Console.WriteLine($"ViewModel PosterPath Received: [{viewModel.PosterPath}]");
            Console.WriteLine($"ViewModel Overview Snippet: [{viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 50))}...]");
            Console.WriteLine($"ViewModel DateWatched Received: {viewModel.DateWatched}");
            Console.WriteLine($"ViewModel WatchedLocation Received: {viewModel.WatchedLocation}");
            Console.WriteLine($"ViewModel IsRewatch Received: {viewModel.IsRewatch}");
            Console.WriteLine($"ViewModel UserRating Received: [{viewModel.UserRating}]"); // Added brackets for consistency
            Console.WriteLine($"ViewModel TmdbId Received: [{viewModel.TmdbId}]");     // <<< ENSURE THIS IS PRESENT AND LOGS VIEWMODEL TmdbId
            Console.WriteLine($"ViewModel Subscribed Received: {viewModel.Subscribed}");
            Console.WriteLine("-----------------------------");

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                // VVVV THIS IS THE KEY CHANGE VVVV
                // We now find the movie only if the ID and UserId both match.
                var movieEntity = await _dbContext.Movies
                    .FirstOrDefaultAsync(m => m.Id == viewModel.Id && m.UserId == userId);

                if (movieEntity != null)
                {
                    // Log original values from DB, including TmdbId
                    Console.WriteLine($"Edit POST - Movie Found (ID: {movieEntity.Id}). Original DB Values - Title: '{movieEntity.Title}', Director: '{movieEntity.Director}', Year: {movieEntity.ReleasedYear}, IsRewatch: {movieEntity.IsRewatch}, UserRating: {movieEntity.UserRating}, TmdbId: {movieEntity.TmdbId}");

                    // Assign values from ViewModel to the Entity
                    movieEntity.Title = viewModel.Title;
                    movieEntity.Director = viewModel.Director;
                    movieEntity.ReleasedYear = viewModel.ReleasedYear;
                    movieEntity.DateWatched = viewModel.DateWatched;
                    movieEntity.WatchedLocation = viewModel.WatchedLocation;
                    movieEntity.PosterPath = viewModel.PosterPath;
                    movieEntity.Overview = viewModel.Overview;
                    movieEntity.IsRewatch = viewModel.IsRewatch;
                    movieEntity.Subscribed = viewModel.Subscribed;
                    movieEntity.UserRating = viewModel.UserRating;
                    movieEntity.TmdbId = viewModel.TmdbId;
                    movieEntity.Genres = viewModel.Genres;// <<< This assignment is crucial and already in your code

                    // VVVV NEW LOGIC TO FETCH AND SAVE GENRES IF THEY ARE MISSING VVVV
                    if (movieEntity.TmdbId.HasValue && string.IsNullOrEmpty(movieEntity.Genres))
                    {
                        var movieDetails = await _tmdbService.GetMovieDetailsAsync(movieEntity.TmdbId.Value);
                        if (movieDetails?.Genres != null && movieDetails.Genres.Any())
                        {
                            movieEntity.Genres = string.Join(", ", movieDetails.Genres.Select(g => g.Name));
                            _logger.LogInformation("Populated missing genres for movie '{Title}': {Genres}", movieEntity.Title, movieEntity.Genres);
                        }
                    }
                    // ^^^^ END OF NEW LOGIC ^^^^

                    // Log entity values just before saving, including TmdbId
                    Console.WriteLine($"Edit POST - Entity values BEFORE SaveChangesAsync (ID: {movieEntity.Id}) - Title: '{movieEntity.Title}', Director: '{movieEntity.Director}', Year: '{movieEntity.ReleasedYear}', IsRewatch: {movieEntity.IsRewatch}, TmdbId: {movieEntity.TmdbId}, UserRating: {movieEntity.UserRating}");

                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"Edit POST - SaveChangesAsync completed successfully for Movie ID {movieEntity.Id}. Redirecting to List.");
                        return RedirectToAction("List", "Movies");
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        Console.WriteLine($"Edit POST - DbUpdateConcurrencyException for Movie ID {movieEntity.Id}: {ex.Message}");
                        ModelState.AddModelError("", "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Edit POST - Exception during SaveChangesAsync for Movie ID {movieEntity.Id}: {ex.Message}");
                        if (ModelState != null)
                        {
                            ModelState.AddModelError("", "An error occurred while saving changes. Please try again.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Edit POST - ERROR: Movie entity with ID {viewModel.Id} not found in DB during save attempt.");
                    return NotFound();
                }
            }
            else // ModelState is NOT valid
            {
                Console.WriteLine("Edit POST - ModelState IS INVALID. Changes will not be saved.");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    if (value != null && value.Errors != null && value.Errors.Any())
                    {
                        Console.WriteLine($"-- Errors for '{modelStateKey}' --");
                        foreach (var error in value.Errors)
                        {
                            Console.WriteLine($"  - Message: {error.ErrorMessage}");
                            if (error.Exception != null)
                            {
                                Console.WriteLine($"    Exception: {error.Exception.Message}");
                            }
                        }
                    }
                }
            }
            // If ModelState invalid or save failed, return to View with viewModel
            Console.WriteLine("Edit POST - Returning View(viewModel) due to invalid ModelState or error for Movie ID: {viewModel.Id}.");
            return View(viewModel);
        }
        // POST: Movies/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            Console.WriteLine($"DEBUG: Delete action in MoviesController hit. ID received: {id}");
            // First, find the movie only if the ID matches AND it belongs to the current user.
            var userId = _userManager.GetUserId(User);
            var movieToDelete = await _dbContext.Movies
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (movieToDelete != null)
                
            {
                
                _dbContext.Movies.Remove(movieToDelete);
                await _dbContext.SaveChangesAsync();
                
            }
            else
            {
                Console.WriteLine($"DEBUG: No movie found with ID: {id}. Nothing to delete.");
            }
            return RedirectToAction("List");
        }



        [HttpGet]
        public async Task<IActionResult> TestTmdbSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                query = "Inception"; // Default search query if none is provided in URL
            }

            System.Diagnostics.Debug.WriteLine($"--- Attempting TMDB Search for query: '{query}' ---");
            var searchResult = await _tmdbService.SearchMoviesAsync(query);

            if (searchResult != null && searchResult.Results != null && searchResult.Results.Any())
            {
                System.Diagnostics.Debug.WriteLine($"SUCCESS: Found {searchResult.TotalResults} movies. Displaying up to the first 5:");
                foreach (var movieItem in searchResult.Results.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"  ID: {movieItem.Id}, Title: {movieItem.Title}, Release Date: {movieItem.ReleaseDate}, Overview: {movieItem.Overview?.Substring(0, Math.Min(movieItem.Overview.Length, 50))}...");
                }
                return Content($"Search for '{query}' successful. Found {searchResult.TotalResults} movies. Check 'Application Output' pad in Visual Studio for details.");
            }
            else if (searchResult != null)
            {
                System.Diagnostics.Debug.WriteLine($"INFO: Search for '{query}' yielded 0 results.");
                return Content($"Search for '{query}' yielded 0 results from TMDB.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: TMDB search for '{query}' failed or the service returned a null response. Check previous logs from TmdbService.");
                return Content($"Search for '{query}' failed. Check 'Application Output' pad and logs from TmdbService.");
            }
        }
    }
// ...existing code...
#endregion
}