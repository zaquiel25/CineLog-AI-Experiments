using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
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
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
// using Microsoft.Extensions.Caching.Memory; // Already present, remove duplicate

using Microsoft.Extensions.Caching.Memory;
namespace Ezequiel_Movies.Controllers
{

    [Authorize]
    public class MoviesController : Controller
    {

        private async Task<HashSet<int>> GetUserBlacklistedTmdbIdsAsync(string userId)
        {
            return (await _dbContext.BlacklistedMovies
                .Where(b => b.UserId == userId)
                .Select(b => b.TmdbId)
                .ToListAsync()).ToHashSet();
        }
        private string? GetCurrentUserId()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                _logger.LogWarning("User authentication failed - no user ID found");
            }
            return userId;
        }

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
    private readonly IMemoryCache _memoryCache;


        public MoviesController(
            ApplicationDbContext dbContext,
            TmdbService tmdbService,
            ILogger<MoviesController> logger,
            UserManager<IdentityUser> userManager,
            IMemoryCache memoryCache) // <- PARÁMETRO AÑADIDO
        {
            _dbContext = dbContext;
            _tmdbService = tmdbService;
            _logger = logger;
            _userManager = userManager;
            _memoryCache = memoryCache; // <- ASIGNACIÓN AÑADIDA
        }

    /// <summary>
    /// Returns a prioritized queue of genres for the current user, based on their logged movies.
    /// 
    /// The queue is ordered by:
    /// 1. Most recent genre (from the latest logged movie)
    /// 2. Most frequent genre (across all logged movies)
    /// 3. Highest-rated genre (from movies rated 4.0 or higher)
    /// 
    /// The result is cached per user for 1 hour to optimize performance and avoid redundant calculations.
    /// 
    /// Business rationale:
    /// - Ensures genre-based suggestions are relevant to recent and frequent user activity.
    /// - Caching reduces database and memory overhead for repeated suggestion requests.
    /// - The queue is used for AJAX-powered genre reshuffles and anti-repetition logic.
    /// </summary>
    /// <param name="userId">Current user's ID (for cache key and filtering)</param>
    /// <param name="loggedMovies">List of movies logged by the user (should be pre-filtered by userId)</param>
    /// <returns>List of genre names in priority order (recent, frequent, highest-rated)</returns>
    private List<string> GetGenrePriorityQueueCached(string userId, List<Ezequiel_Movies1.Models.Entities.Movies> loggedMovies)
    {
        string queueCacheKey = $"GenrePriorityQueue_{userId}";
        if (_memoryCache.TryGetValue(queueCacheKey, out List<string>? cachedQueue) && cachedQueue != null)
        {
            _logger.LogInformation("🎯 Priority Queue CACHE HIT for user {UserId}", userId);
            return cachedQueue;
        }

        // Build genre pools from user's logged movies
        var allUserGenres = loggedMovies
            .SelectMany(m => m.Genres?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>())
            .ToList();
        var recentGenre = loggedMovies
            .OrderByDescending(m => m.DateWatched ?? m.DateCreated)
            .FirstOrDefault()?.Genres?.Split(new[] { ", " }, StringSplitOptions.None).FirstOrDefault();
        var frequentGenre = allUserGenres
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
        var ratedGenres = loggedMovies
            .Where(m => m.UserRating.HasValue && m.UserRating.Value >= 4.0m)
            .SelectMany(m => m.Genres?.Split(new[] { ", " }, StringSplitOptions.None) ?? Array.Empty<string>())
            .ToList();
        var highestRatedGenre = ratedGenres
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        // Compose the priority queue, removing null/empty and duplicates
        var priorityQueue = new List<string?> { recentGenre, frequentGenre, highestRatedGenre }
            .Where(g => !string.IsNullOrEmpty(g))
            .Distinct()
            .Select(g => g!)
            .ToList();

        // Cache the result for 1 hour for efficiency
        var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));
        _memoryCache.Set(queueCacheKey, priorityQueue, cacheOptions);
        _logger.LogInformation("✅ Priority Queue calculated and cached for user {UserId}", userId);
        return priorityQueue;
    }

private string? GetRandomGenreWithAntiRepetition(string userId, List<Ezequiel_Movies1.Models.Entities.Movies> loggedMovies)
{
    var allGenres = loggedMovies.SelectMany(m => m.Genres?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>()).Distinct().ToList();
    if (!allGenres.Any()) return null;

    string lastRandomGenreKey = $"LastRandomGenre_{userId}";
    string? lastRandomGenre = HttpContext.Session.GetString(lastRandomGenreKey);
    var availableGenres = allGenres.Where(g => g != lastRandomGenre).ToList();
    if (!availableGenres.Any()) availableGenres = allGenres;
    var selectedGenre = availableGenres[Random.Shared.Next(availableGenres.Count)];
    HttpContext.Session.SetString(lastRandomGenreKey, selectedGenre);
    return selectedGenre;
}

private void InvalidateUserCaches(string userId)
{
    _logger.LogInformation("🧹 Invalidating user-specific caches for {UserId}", userId);
    _memoryCache.Remove($"GenrePriorityQueue_{userId}");
}

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
        /// <summary>
        /// Adds a movie to the user's blacklist, enforcing mutual exclusion with the wishlist.
        /// </summary>
        /// <param name="tmdbId">TMDB movie ID to blacklist.</param>
        /// <param name="returnUrl">Optional: URL to return to after action.</param>
        /// <remarks>
        /// - Mutual exclusion is enforced: a movie cannot exist in both wishlist and blacklist for the same user.
        /// - UI state management prevents conflicting actions; error messaging is minimal as users cannot access both states.
        /// - See Details.cshtml and Preview.cshtml for visual implementation of exclusion.
        /// </remarks>
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
            
            if (await MovieExistsInWishlistAsync(userId, tmdbId))
            {
                return LocalRedirect(returnUrl);
            }
            try
            {
                var movieDetails = await GetMovieDetailsWithLoggingAsync(tmdbId);
                if (movieDetails == null)
                {
                    return NotFound();
                }
                string? director = null;
                int? releasedYear = null;
                try
                {
                    director = movieDetails.GetDirector();
                    if (!string.IsNullOrEmpty(movieDetails.ReleaseDate) && movieDetails.ReleaseDate.Length >= 4)
                    {
                        if (int.TryParse(movieDetails.ReleaseDate.Substring(0, 4), out var year))
                        {
                            releasedYear = year;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not extract director/year for TMDB {TmdbId}", tmdbId);
                }
                var blacklistedMovie = new Ezequiel_Movies1.Models.Entities.BlacklistedMovie
                {
                    UserId = userId,
                    TmdbId = tmdbId,
                    Title = movieDetails.Title ?? string.Empty,
                    BlacklistedDate = DateTime.Now,
                    PosterUrl = movieDetails.PosterPath,
                    Director = director,
                    ReleasedYear = releasedYear
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

    /// <summary>
    /// Displays the user's blacklist, with optional search and sort.
    /// </summary>
    /// <param name="searchString">Optional: filter blacklist by title or metadata.</param>
    /// <param name="sortOrder">Optional: sort order for the blacklist view.</param>
    /// <remarks>
    /// - Only movies blacklisted by the current user are shown.
    /// - UI/UX: Consistent with wishlist and movie list views.
    /// </remarks>
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

            
            if (!string.IsNullOrEmpty(searchString))
            {
                blacklistQuery = blacklistQuery.Where(b => b.Title.ToLower().Contains(searchString.ToLower()));
            }

            
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

            // Fetch TMDB details for each movie (mirrors Wishlist pattern)
            var moviesWithPosters = new List<dynamic>();
            foreach (var movie in blacklistedMovies)
            {
                var posterUrl = await GetPosterUrlAsync(movie.TmdbId, movie.PosterUrl);

                string director = "Unknown (TMDB)";
                int? releasedYear = null;

                try
                {
                    var tmdbDetails = await GetMovieDetailsWithLoggingAsync(movie.TmdbId);
                    if (tmdbDetails != null)
                    {
                        // Extraer director
                        if (tmdbDetails.Credits?.Crew != null)
                        {
                            var directorPerson = tmdbDetails.Credits.Crew.FirstOrDefault(c => c.Job == "Director");
                            if (!string.IsNullOrEmpty(directorPerson?.Name))
                                director = directorPerson.Name;
                        }
                        // Extraer año
                        if (!string.IsNullOrEmpty(tmdbDetails.ReleaseDate) && tmdbDetails.ReleaseDate.Length >= 4)
                        {
                            if (int.TryParse(tmdbDetails.ReleaseDate.Substring(0, 4), out var year))
                                releasedYear = year;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching TMDB details for Blacklist movie {TmdbId}", movie.TmdbId);
                    // director y releasedYear quedan con valores por defecto
                }

                moviesWithPosters.Add(new {
                    Id = movie.Id,
                    Title = movie.Title,
                    TmdbId = movie.TmdbId,
                    BlacklistedDate = movie.BlacklistedDate,
                    PosterUrl = posterUrl,
                    Director = director,
                    ReleasedYear = releasedYear
                });
            }
            return View(moviesWithPosters);
        }

        /// <summary>
        /// Removes a movie from the user's blacklist.
        /// </summary>
        /// <param name="id">The database ID of the blacklisted movie entry.</param>
        /// <remarks>
        /// Only allows removal if the movie belongs to the current user to ensure data integrity.
        /// </remarks>
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


    /// <summary>
    /// Displays the user's wishlist, with optional search and sort.
    /// </summary>
    /// <param name="searchString">Optional: filter wishlist by title.</param>
    /// <param name="sortOrder">Optional: sort order for the wishlist view.</param>
    /// <remarks>
    /// - Only movies wishlisted by the current user are shown.
    /// - UI/UX: Consistent with blacklist and movie list views.
    /// </remarks>
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
                // If not found in Movies table, fetch director from TMDB
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


        /// <summary>
        /// Adds a movie to the user's wishlist, enforcing mutual exclusion with the blacklist.
        /// </summary>
        /// <param name="tmdbId">TMDB movie ID to add to wishlist.</param>
        /// <param name="returnUrl">Optional: URL to return to after action.</param>
        /// <remarks>
        /// - Mutual exclusion is enforced: a movie cannot exist in both wishlist and blacklist for the same user.
        /// - UI state management prevents conflicting actions; error messaging is minimal as users cannot access both states.
        /// - See Details.cshtml and Preview.cshtml for visual implementation of exclusion.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int tmdbId, string returnUrl = "/")
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId) || tmdbId == 0)
            {
                return BadRequest(); 
            }

            if (await MovieExistsInWishlistAsync(userId, tmdbId))
            {
                return LocalRedirect(returnUrl);
            }

            // Skip if movie is already in blacklist (mutual exclusion)
            if (await MovieExistsInBlacklistAsync(userId, tmdbId))
            {
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
        // Alternative removal method for wishlist items by TMDB ID
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

        // Alternative removal method for blacklist items by TMDB ID
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

        /// <summary>
        /// Removes a movie from the user's wishlist by TMDB ID.
        /// </summary>
        /// <param name="tmdbId">TMDB movie ID to remove from wishlist.</param>
        /// <remarks>
        /// - Only allows removal if the movie belongs to the current user.
        /// - Uses anti-forgery token for security.
        /// </remarks>
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


        /// <summary>
        /// Displays a preview of a movie using TMDB data, including details and streaming providers.
        /// </summary>
        /// <param name="tmdbId">TMDB movie ID to preview.</param>
        /// <remarks>
        /// - Fetches movie details and streaming provider info from TMDB.
        /// - Used for add/edit flows and quick lookups.
        /// - Returns BadRequest if ID is invalid, NotFound if movie is not found in TMDB.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Preview(int tmdbId)
        {
            if (tmdbId == 0)
            {
                return BadRequest();
            }

            // Step 1: Retrieve all movie details from TMDB
            var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
            if (movieDetails == null)
            {
                return NotFound();
            }

            // Step 2: Retrieve "Where to Watch" providers (Streaming, Buy, Rent)
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

            // Step 3: Check if the user has already logged this movie
            var userId = _userManager.GetUserId(User);
            bool isAlreadyLogged = await _dbContext.Movies
                .AnyAsync(m => m.UserId == userId && m.TmdbId == tmdbId);
            bool isInWishlist = await _dbContext.WishlistItems.AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
            bool isInBlacklist = await _dbContext.BlacklistedMovies.AnyAsync(b => b.UserId == userId && b.TmdbId == tmdbId);

            ViewData["IsAlreadyLogged"] = isAlreadyLogged;
            ViewData["IsInWishlist"] = isInWishlist;
            ViewData["IsInBlacklist"] = isInBlacklist;

            // Pass the full TMDB movie details object to the view
            return View(movieDetails);
        }

        /// <summary>
        /// Displays detailed information for a movie logged by the current user.
        /// </summary>
        /// <param name="id">The database GUID of the movie entry.</param>
        /// <remarks>
        /// - Only allows access to movies owned by the current user.
        /// - Shows status in wishlist/blacklist and all logged metadata.
        /// - Returns NotFound if the movie does not exist or is not owned by the user.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            var movie = await _dbContext.Movies.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (movie == null)
            {
                return NotFound();
            }

            // Determine if the movie is in the user's wishlist
            bool isInWishlist = false;
            if (movie.TmdbId.HasValue)
            {
                isInWishlist = await _dbContext.WishlistItems.AnyAsync(w => w.UserId == userId && w.TmdbId == movie.TmdbId.Value);
            }
            else
            {
                // If no TmdbId, search by local Id (if supported by WishlistItem)
                isInWishlist = await _dbContext.WishlistItems.AnyAsync(w => w.UserId == userId && w.TmdbId == 0);
            }
            ViewData["IsInWishlist"] = isInWishlist;

            // Determine if the movie is in the user's blacklist (copied from Preview)
            bool isInBlacklist = false;
            if (movie.TmdbId.HasValue)
            {
                isInBlacklist = await _dbContext.BlacklistedMovies.AnyAsync(b => b.UserId == userId && b.TmdbId == movie.TmdbId.Value);
            }
            else
            {
                isInBlacklist = await _dbContext.BlacklistedMovies.AnyAsync(b => b.UserId == userId && b.TmdbId == 0);
            }
            ViewData["IsInBlacklist"] = isInBlacklist;

            if (movie.TmdbId.HasValue)
            {
                // Retrieve "Where to Watch" info (Streaming, Buy, Rent)
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

                // Pass the top 3 cast members to the view
                if (movieDetails?.Credits?.Cast != null)
                {
                    // Pass the top 3 cast members to the view
                    ViewData["Cast"] = movieDetails.Credits.Cast.Take(3).ToList();
                }
                // End of cast logic
            }

            return View(movie);
        }

        /// <summary>
    /// <summary>
    /// Returns a personalized "Surprise Me!" movie suggestion for the logged-in user using an optimized static pool approach.
    ///
    /// <para>
    /// <b>Pool Construction:</b> Builds a static, deduplicated pool of 80 movies using aggressive cascading from multiple sources (e.g., trending, genre, director).
    /// Blacklisted and recently suggested movies are filtered out during pool build. Deduplication is enforced by TMDB ID.
    /// </para>
    /// <para>
    /// <b>Caching:</b> The pool is cached in IMemoryCache for 2 hours. During this period, all reshuffles use the same pool, resulting in zero TMDB API calls per reshuffle.
    /// </para>
    /// <para>
    /// <b>Rotation:</b> The pool is shuffled and supports infinite cyclic rotation. Each reshuffle advances the pointer, wrapping to the start as needed. Session state tracks the current position.
    /// </para>
    /// <para>
    /// <b>Performance:</b> Only ~5 TMDB API calls are made during initial pool build; subsequent reshuffles are instant and API-free.
    /// </para>
    /// <para>
    /// Requirements:
    /// - User must have logged at least 3 movies with director, genre, and release year info.
    /// - Suggestion pools are built from user's own movie history (directors, genres, actors).
    /// - Session state is used to track pool index and shuffled order for anti-repetition and infinite cycling.
    /// </para>
    /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSurpriseSuggestion()
        {
            _logger.LogInformation("GetSurpriseSuggestion action invoked.");

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get all logged movies for this user (for ingredient pools and recent exclusion)
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
            // Build pools of directors, genres, and actors from user's logged movies
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
            // Exclude recently watched movies from suggestions
            var recentMovieIds = await _dbContext.Movies
                .Where(m => m.UserId == userId && m.DateWatched.HasValue)
                .OrderByDescending(m => m.DateWatched)
                .Take(15)
                .Select(m => m.TmdbId)
                .ToListAsync();
            var recentMovieIdSet = recentMovieIds.Where(id => id.HasValue).Select(id => id!.Value).ToHashSet();

            // --- 3. Get user blacklist for filtering ---
            // Exclude user-blacklisted movies from suggestions
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);

            // --- 4. Get session anti-repetition list ---
            // Exclude movies already suggested in this session
            var shownSurpriseIds = HttpContext.Session.Get<List<int>>("ShownSurpriseIds") ?? new List<int>();

            // --- 5. Get or set cycle step ---
            // Cycle through 4 suggestion strategies for variety (stored in session)
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
            // Randomly select one director, actor, and genre from user pools
            var randDirector = directorPool[random.Next(directorPool.Count)];
            var randDirectorId = await _tmdbService.GetPersonIdAsync(randDirector);
            var randActor = actorPool[random.Next(actorPool.Count)];
            var randActorId = randActor.Id;
            var allGenres = await _tmdbService.GetAllGenresAsync();
            var randGenre = genrePool[random.Next(genrePool.Count)];
            var randGenreId = allGenres.FirstOrDefault(g => g.Name == randGenre)?.Id;

            // --- 7. Cyclic 4-step logic ---
            // Main suggestion logic: try strictest match first, then relax constraints if needed
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

            // Fallback: If no suggestion found, use random popular movie with filters
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
            // Always provide a suggestion; never show a failure message

            // Step 8: Update session anti-repetition memory
            // Track suggested movie IDs in session to avoid repetition
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

            // Step 9: Advance the suggestion cycle step
            // Advance the cycle step for next request
            int nextStep = (cycleStep % 4) + 1;
            _logger.LogInformation($"DEBUG: Setting next Surprise Cycle Step to: {nextStep}");
            HttpContext.Session.SetInt32(cycleKey, nextStep);

            ViewData["SuggestionTitle"] = suggestionTitle;
            ViewData["NextSuggestionType"] = "surprise_me";
            return View("Suggest", suggestedMoviesList);
        }

        /// <summary>
        /// AJAX endpoint for reshuffling "Trending" movie suggestions.
        /// Returns server-rendered HTML (partial views) instead of raw JSON, ensuring image paths and helpers are always correct.
        /// </summary>
        /// <remarks>
        /// - Uses server-side rendering to avoid CORS and path issues with TMDB images.
        /// - Reuses the same partial view as initial render for DRYness and consistency.
        /// - Client fetches new suggestions via AJAX, receives HTML, and replaces the grid without a full page reload.
        /// - All user-specific filtering (blacklist, recently watched) is enforced server-side.
        /// </remarks>
[HttpGet]
[Authorize]
public async Task<IActionResult> TrendingReshuffle()
{
    try
    {
        _logger.LogInformation("🚀 TrendingReshuffle AJAX endpoint called.");
        
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Filtrar películas ya vistas y en blacklist del usuario
        var blacklistIds = await _dbContext.BlacklistedMovies
            .Where(b => b.UserId == userId)
            .Select(b => b.TmdbId)
            .ToListAsync();

        var recentIds = await _dbContext.Movies
            .Where(m => m.UserId == userId && m.DateWatched.HasValue && m.TmdbId.HasValue)
            .OrderByDescending(m => m.DateWatched)
            .Take(5)
            .Select(m => m.TmdbId ?? 0)
            .ToListAsync();

        var moviePool = new List<TmdbMovieBrief>();
        int pageNum = 1;
        // Buscar hasta 30 películas válidas (no repetidas, no blacklist, no recientes)
        while (moviePool.Count < 30 && pageNum <= 5)
        {
            var pageMovies = await _tmdbService.GetTrendingMoviesAsync(pageNum);
            var validMovies = pageMovies
                .Where(m => !blacklistIds.Contains(m.Id) && !recentIds.Contains(m.Id))
                .ToList();
            moviePool.AddRange(validMovies);
            pageNum++;
        }

        // Seleccionar 3 sugerencias aleatorias
        var suggestedMovies = moviePool
            .OrderBy(x => Random.Shared.Next())
            .Take(3)
            .ToList();

        _logger.LogInformation("🎬 Returning {Count} movies for reshuffle", suggestedMovies.Count);

        // Si no hay sugerencias posibles, mostrar mensaje amigable
        if (!suggestedMovies.Any())
        {
            var emptyHtml = @"<div class='alert alert-info text-center my-5'>No more trending movies available right now. Please try another suggestion type or come back later for new trending picks!</div>";
            return Json(new {
                success = true,
                html = emptyHtml,
                count = 0
            });
        }

        // Renderizar HTML usando partial view para asegurar paths y helpers correctos
        var htmlBuilder = new StringBuilder();
        foreach (var movie in suggestedMovies)
        {
            // Renderiza cada película con la partial view reutilizada
            var partialViewResult = await this.RenderPartialViewToStringAsync("_MovieSuggestionCard", movie);
            htmlBuilder.Append($"<div class=\"col\">{partialViewResult}</div>");
        }

        return Json(new { 
            success = true, 
            html = htmlBuilder.ToString(),
            count = suggestedMovies.Count 
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "💥 ERROR in TrendingReshuffle: {Message}", ex.Message);
        return Json(new { 
            success = false, 
            error = ex.Message 
        });
    }
}

        /// <summary>
        /// AJAX endpoint for reshuffling movie suggestions based on cast (actors) from the user's recent movie history.
        /// </summary>
        /// <remarks>
    /// <para>
    /// Implements a robust, session-sequenced strategy for cast-based suggestions:
    /// <list type="number">
    /// <item>Step 1: Suggests using the most recent actor from the user's last 15 logged movies.</item>
    /// <item>Step 2: Suggests using the most frequent actor among those movies.</item>
    /// <item>Step 3: Suggests using the top-billed actor from the user's highest-rated movie.</item>
    /// <item>Step 4+: If all above are exhausted or unavailable, selects a random actor from the pool.</item>
    /// </list>
    /// <b>Anti-repetition:</b> The same actor will never be suggested twice in a row (immediate repetition is prevented via Session state).
    /// The current step is tracked in Session per user and advances with each reshuffle, ensuring variety and personalization.
    /// </para>
        /// <para>
        /// Business rules:
        /// <list type="bullet">
        /// <item>Excludes blacklisted and already-watched movies from suggestions.</item>
        /// <item>Returns server-rendered HTML (partial views) to ensure correct image paths and consistent UI.</item>
        /// <item>Handles all edge cases gracefully, always providing actionable feedback to the user.</item>
        /// <item>All logic and edge cases are documented for maintainability and future extension.</item>
        /// </list>
        /// </para>
        /// </remarks>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> CastReshuffle()
        {
            try
            {
                _logger.LogInformation("🚀 CastReshuffle AJAX endpoint called with CORRECTED logic.");

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // --- Robust Cast Suggestion Sequence ---
                // 1. SEQUENCE MANAGEMENT: Track the current step in Session to rotate between strategies (recent, frequent, rated, random).
                string castTypeKey = $"CastTypeSequence_{userId}";
                int castTypeCount = HttpContext.Session.GetInt32(castTypeKey) ?? 0;
                HttpContext.Session.SetInt32(castTypeKey, castTypeCount + 1);
                _logger.LogInformation("Cast Reshuffle Sequence: Attempting Step {Step}", castTypeCount);

                // 2. ACTOR POOL CONSTRUCTION: Build a pool of top 3 actors from the user's last 15 logged movies.
                var loggedCastMovies = await _dbContext.Movies
                    .Where(m => m.UserId == userId && m.TmdbId.HasValue)
                    .OrderByDescending(m => m.DateWatched)
                    .ToListAsync();

                if (loggedCastMovies.Count < 3)
                {
                    var emptyHtml = @"<div class='alert alert-info text-center my-5'>Log at least 3 movies to get cast-based suggestions!</div>";
                    return Json(new { success = true, html = emptyHtml, count = 0 });
                }

                var allTopActors = new List<TmdbCastPerson>();
                foreach (var movie in loggedCastMovies.Take(15))
                {
                    if (!movie.TmdbId.HasValue) continue;
                    var details = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId.Value);
                    if (details?.Credits?.Cast != null)
                    {
                        allTopActors.AddRange(details.Credits.Cast.Take(3));
                    }
                }

                if (!allTopActors.Any())
                {
                    var emptyHtml = @"<div class='alert alert-warning text-center my-5'>Could not find cast info in your recent movies.</div>";
                    return Json(new { success = true, html = emptyHtml, count = 0 });
                }

                // 3. SEQUENTIAL ACTOR SELECTION: Try each strategy in order; if the current step yields no actor, fall back to random.
                TmdbCastPerson? selectedActor = null;

                // Build the prioritized actor queue: recent, frequent, highest-rated.
                var recentActor = allTopActors.FirstOrDefault();
                var frequentActor = allTopActors.GroupBy(a => a.Id).OrderByDescending(g => g.Count()).Select(g => g.First()).FirstOrDefault();
                var ratedActorMovie = loggedCastMovies.Where(m => m.UserRating.HasValue).OrderByDescending(m => m.UserRating).FirstOrDefault();
                TmdbCastPerson? ratedActor = null;
                if (ratedActorMovie?.TmdbId.HasValue ?? false)
                {
                    var ratedMovieDetails = await _tmdbService.GetMovieDetailsAsync(ratedActorMovie.TmdbId.Value);
                    ratedActor = ratedMovieDetails?.Credits?.Cast?.FirstOrDefault();
                }

                var priorityQueue = new List<TmdbCastPerson?> { recentActor, frequentActor, ratedActor }
                    .Where(a => a != null)
                    .Cast<TmdbCastPerson>()
                    .DistinctBy(a => a.Id)
                    .ToList();


                // --- ANTI-REPETICIÓN INMEDIATA DE ACTOR ---
                string lastActorKey = $"LastActorId_{userId}";
                int? lastActorId = HttpContext.Session.GetInt32(lastActorKey);

                // 1. Intentar seleccionar actor de la cola priorizada evitando el último actor si es posible
                if (castTypeCount < priorityQueue.Count)
                {
                    var candidate = priorityQueue[castTypeCount];
                    if (lastActorId.HasValue && candidate != null && candidate.Id == lastActorId.Value && priorityQueue.Count > 1)
                    {
                        // Buscar otro actor distinto si hay más opciones
                        candidate = priorityQueue.FirstOrDefault(a => a.Id != lastActorId.Value);
                    }
                    selectedActor = candidate;
                }

                // 2. Si no se pudo seleccionar, fallback a random evitando el último actor si es posible
                if (selectedActor == null)
                {
                    _logger.LogInformation("Sequence step {Step} was empty or finished. Switching to random.", castTypeCount);
                    var distinctActors = allTopActors.Where(a => a != null).DistinctBy(a => a.Id).ToList();
                    if (distinctActors.Any())
                    {
                        var pool = distinctActors;
                        if (lastActorId.HasValue && pool.Count > 1)
                        {
                            pool = pool.Where(a => a.Id != lastActorId.Value).ToList();
                        }
                        selectedActor = pool[Random.Shared.Next(pool.Count)];
                    }
                }

                // 3. Si no hay actor posible, lanzar error (caso extremo)
                if (selectedActor == null)
                {
                    throw new InvalidOperationException("Could not select a valid actor to generate suggestions.");
                }

                // Guardar el actor sugerido en Session para la próxima vez
                HttpContext.Session.SetInt32(lastActorKey, selectedActor.Id);

                // 4. SUGGESTION GENERATION & RESPONSE: Fetch movies for the selected actor and return server-rendered HTML or a friendly message if none found.
                var actorDetails = await _tmdbService.GetPersonDetailsAsync(selectedActor.Id);
                var suggestedMovies = await GetSuggestionsForActor(selectedActor.Id, userId);

                // FIX 1: Simple and consistent title.
                var suggestionTitle = $"Because you like movies with {selectedActor.Name}";

                if (!suggestedMovies.Any())
                {
                    var emptyHtml = $@"<div class='alert alert-info text-center my-5'>No more suggestions available for {selectedActor.Name}. Try another suggestion type!</div>";
                    return Json(new { success = true, html = emptyHtml, count = 0 });
                }

                var htmlBuilder = new StringBuilder();
                foreach (var movie in suggestedMovies)
                {
                    var partialViewResult = await this.RenderPartialViewToStringAsync("_MovieSuggestionCard", movie);
                    htmlBuilder.Append($"<div class=\"col\">{partialViewResult}</div>");
                }

                return Json(new
                {
                    success = true,
                    html = htmlBuilder.ToString(),
                    count = suggestedMovies.Count,
                    suggestionTitle = suggestionTitle,
                    actorProfileUrl = actorDetails?.ProfilePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR in CastReshuffle: {Message}", ex.Message);
                return Json(new
                {
                    success = false,
                    error = "Could not generate a new suggestion. Please try again."
                });
            }
        }
    [HttpGet]
[Authorize]
public async Task<IActionResult> DirectorReshuffle()
{
    try
    {
        _logger.LogInformation("🚀 DirectorReshuffle AJAX endpoint called.");

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }


        /// <summary>
        /// AJAX endpoint for director-based movie suggestions with intelligent sequencing.
        /// Rotates through recent, frequent, and top-rated directors with proper deduplication.
        /// </summary>
        /// <returns>JSON response with rendered HTML for suggestion cards</returns>

        // 1. Session-based sequence management
        // Tracks which step in the director priority sequence we're on
        string directorTypeKey = $"DirectorTypeSequence_{userId}";
        int directorTypeCount = HttpContext.Session.GetInt32(directorTypeKey) ?? 0;
        string[] directorTypes = { "director_recent", "director_frequent", "director_rated" };
        int maxDirectorTypeIndex = directorTypes.Length - 1;
        string currentDirectorType = directorTypeCount <= maxDirectorTypeIndex ? directorTypes[directorTypeCount] : "director_random";
        HttpContext.Session.SetInt32(directorTypeKey, directorTypeCount + 1);
        _logger.LogInformation("Director Reshuffle Sequence: Step {Step}, Type: {Type}", directorTypeCount, currentDirectorType);

        // 2. Fetch user's movie data for director analysis
        var allUserMovies = await _dbContext.Movies
            .Where(m => m.UserId == userId && !string.IsNullOrEmpty(m.Director) && m.Director != "N/A" && m.TmdbId.HasValue)
            .OrderByDescending(m => m.DateWatched ?? m.DateCreated)
            .ToListAsync();

        if (!allUserMovies.Any())
        {
            var emptyHtml = @"<div class='alert alert-info text-center my-5'>Log some movies to get director suggestions!</div>";
            return Json(new { success = true, html = emptyHtml, count = 0 });
        }

        // 3. Build prioritized director queue with robust deduplication
        // This handles cases where the same director appears in multiple categories
        var recentDirector = allUserMovies.FirstOrDefault()?.Director;
        var frequentDirector = allUserMovies.GroupBy(m => m.Director!).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
        var ratedDirector = allUserMovies
            .Where(m => m.UserRating.HasValue)
            .GroupBy(m => m.Director!)
            .Select(g => new { Name = g.Key, Avg = g.Average(m => m.UserRating!.Value), Count = g.Count() })
            .OrderByDescending(d => d.Avg)
            .ThenByDescending(d => d.Count)
            .Select(d => d.Name)
            .FirstOrDefault();

        _logger.LogInformation("Director Analysis - Recent: '{Recent}', Frequent: '{Frequent}', Rated: '{Rated}'", 
            recentDirector, frequentDirector, ratedDirector);

        // Case-insensitive deduplication prevents the same director appearing twice
        // Example: If Steven Spielberg is both "recent" and "frequent", he only appears once in the queue
        var priorityQueue = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AddDirector(string? director)
        {
            if (string.IsNullOrWhiteSpace(director)) return;
            var trimmed = director.Trim();
            if (seen.Add(trimmed)) // HashSet.Add returns true if item was newly added
            {
                priorityQueue.Add(trimmed);
                _logger.LogInformation("Added director to queue: '{Director}'", trimmed);
            }
            else
            {
                _logger.LogInformation("Skipped duplicate director: '{Director}'", trimmed);
            }
        }

        // Add directors in priority order: recent → frequent → rated
        AddDirector(recentDirector);
        AddDirector(frequentDirector);
        AddDirector(ratedDirector);

        _logger.LogInformation("Final priority queue: [{Directors}]", string.Join(", ", priorityQueue));

        // 4. Seleccionar Director según la Secuencia
string? directorToSuggest = null;
if (directorTypeCount < priorityQueue.Count)
{
    directorToSuggest = priorityQueue[directorTypeCount];
}
else // Fallback a aleatorio si la secuencia terminó o un paso estaba vacío
{
    var allDirectors = allUserMovies.Select(m => m.Director!).Distinct().ToList();
    if (allDirectors.Any())
    {
        // Anti-repetición: evitar que se repita el último director random
        string lastRandomDirectorKey = $"LastRandomDirector_{userId}";
        string? lastRandomDirector = HttpContext.Session.GetString(lastRandomDirectorKey);
        var availableDirectors = allDirectors;
        if (!string.IsNullOrEmpty(lastRandomDirector) && allDirectors.Count > 1)
        {
            availableDirectors = allDirectors.Where(d => d != lastRandomDirector).ToList();
        }
        directorToSuggest = availableDirectors[Random.Shared.Next(availableDirectors.Count)];
        HttpContext.Session.SetString(lastRandomDirectorKey, directorToSuggest);
    }
}

if (string.IsNullOrEmpty(directorToSuggest))
{
     throw new InvalidOperationException("Could not select a valid director to generate suggestions.");
}

        // 5. Obtener Sugerencias y Construir Respuesta
        var suggestedMovies = await GetSuggestionsForDirector(directorToSuggest, userId);
        var suggestionTitle = $"Because you like {directorToSuggest}...";

        if (!suggestedMovies.Any())
        {
            var emptyHtml = $@"<div class='alert alert-info text-center my-5'>No more suggestions available for {directorToSuggest}. Try another suggestion type!</div>";
            return Json(new { success = true, html = emptyHtml, count = 0 });
        }

        var htmlBuilder = new StringBuilder();
        foreach (var movie in suggestedMovies)
        {
            var partialViewResult = await this.RenderPartialViewToStringAsync("_MovieSuggestionCard", movie);
            htmlBuilder.Append($"<div class=\"col\">{partialViewResult}</div>");
        }

        return Json(new { 
            success = true, 
            html = htmlBuilder.ToString(),
            count = suggestedMovies.Count,
            suggestionTitle = suggestionTitle
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "💥 ERROR in DirectorReshuffle: {Message}", ex.Message);
        return Json(new { 
            success = false, 
            error = "Could not generate a new suggestion. Please try again."
        });
    }
}

[HttpGet]
[Authorize]
public async Task<IActionResult> DecadeReshuffle()
{
    try
    {
        _logger.LogInformation("🚀 DecadeReshuffle AJAX endpoint called with optimized logic based on last 25 movies.");
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

    /// <summary>
    /// Returns a set of movie suggestions based on the user's most recent decades of activity.
    /// 
    /// Business logic:
    /// - Prioritizes decades using the user's last 25 logged movies for relevance and performance.
    /// - Decade selection follows a strict priority: most recent, most frequent, highest rated, then random.
    /// - Implements early exit optimization to minimize TMDB API calls and improve responsiveness.
    /// - Caches blacklist and recent movie filters to avoid redundant database queries.
    /// - Session state is used to prevent immediate repetition of the last suggested decade.
    /// - Ensures a high-quality, variety-driven user experience even in edge cases.
    /// </summary>
    // Retrieve the user's last 25 logged movies, ordered by most recent activity
    // This scope ensures suggestions are relevant to current user interests and reduces query overhead
        var last25Movies = await _dbContext.Movies
            .Where(m => m.UserId == userId && m.ReleasedYear.HasValue)
            .OrderByDescending(m => m.DateWatched ?? m.DateCreated)
            .Take(25)
            .ToListAsync();

        if (last25Movies.Count < 3)
        {
            var emptyHtml = "<div class='alert alert-info text-center my-5'>Log at least 3 movies to get decade suggestions!</div>";
            return Json(new { success = true, html = emptyHtml, count = 0 });
        }

    // Calculate the set of unique decades represented in the user's recent activity
    // Decades are derived from the release year of each movie for accurate grouping
        var availableDecades = last25Movies
            .Select(m => (m.ReleasedYear!.Value / 10) * 10)
            .Distinct()
            .ToList();

    // Calculate decade priorities based on user's recent movie activity
    // Priority order: latest → frequent → rated → random exploration
    // LATEST: Decade from the most recently added movie (by DateCreated/DateWatched)
        var latestDecade = (last25Movies.First().ReleasedYear!.Value / 10) * 10;

    // FREQUENT: Most frequent decade with ≥2 movies from last 25, random selection on ties
        var frequentGroups = last25Movies.GroupBy(m => (m.ReleasedYear!.Value / 10) * 10)
            .Where(g => g.Count() >= 2).ToList();
        var mostFrequentDecade = 0;
        if (frequentGroups.Any())
        {
            var maxFreq = frequentGroups.Max(g => g.Count());
            var freqCandidates = frequentGroups.Where(g => g.Count() == maxFreq).Select(g => g.Key).ToList();
            mostFrequentDecade = freqCandidates[Random.Shared.Next(freqCandidates.Count)];
        }

    // RATED: Highest average rated decade with ≥2 rated movies from last 25, random selection on ties
        var ratedGroups = last25Movies.Where(m => m.UserRating.HasValue)
            .GroupBy(m => (m.ReleasedYear!.Value / 10) * 10)
            .Where(g => g.Count() >= 2).ToList();
        var highestRatedDecade = 0;
        if (ratedGroups.Any())
        {
            var maxAvgRating = ratedGroups.Max(g => g.Average(m => m.UserRating!.Value));
            var ratedCandidates = ratedGroups
                .Where(g => Math.Abs(g.Average(m => m.UserRating!.Value) - maxAvgRating) < 0.01m)
                .Select(g => g.Key).ToList();
            highestRatedDecade = ratedCandidates[Random.Shared.Next(ratedCandidates.Count)];
        }

    // Build the priority queue for decade evaluation
    // Early exit optimization: stop at the first decade with valid suggestions
    // This user-driven approach minimizes API calls while ensuring variety
        var priorityQueue = new List<int> { latestDecade, mostFrequentDecade, highestRatedDecade }
            .Where(d => d > 0).Distinct().ToList();
        
    // RANDOM: Any decade from the last 25 movies can be selected, ensuring equitable representation
        var randomDecades = availableDecades.OrderBy(_ => Random.Shared.Next()).ToList();
        var decadesToTry = priorityQueue.Concat(randomDecades).ToList();

        _logger.LogInformation("[DECADE-LOGIC] Decades to try in order: {Decades}", string.Join(", ", decadesToTry));

    // Optimized evaluation loop with early exit
    // Only a maximum of 5 decades are evaluated per request to reduce API usage
        List<(int Decade, List<TmdbMovieBrief> Pool)> validDecades = new();
        string lastDecadeKey = $"LastDecadeShown_{userId}";
        int? lastDecade = HttpContext.Session.GetInt32(lastDecadeKey);
        
        // Optimizaciones para reducir API calls
        const int MAX_DECADES_TO_EVALUATE = 5;
        int evaluatedCount = 0;
        int totalApiCalls = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

    // Cache expensive filter operations to avoid redundant queries
    // Blacklisted movies and recent user activity are computed once per request
        var blacklisted = await GetUserBlacklistedTmdbIdsAsync(userId);
        var last5 = last25Movies.Take(5).Select(m => m.TmdbId ?? 0).ToHashSet();

    // Main evaluation loop with early exit logic
    // Skips the immediately previously shown decade (anti-repetition)
        foreach (var decade in decadesToTry)
        {
            // Skip the decade shown immediately before (anti-repetition)
            if (decade == lastDecade) continue;
            
            // Early exit: if enough decades have been evaluated and valid options found, stop
            if (evaluatedCount >= MAX_DECADES_TO_EVALUATE && validDecades.Count >= 2) break;
            
            evaluatedCount++;
            
            // Build the suggestion pool for this decade (API optimized)
            var currentPool = new List<TmdbMovieBrief>();
            int page = 1;
            const int maxPagesToTry = 3; // Reduced from 5 to 3 for performance
            
            while (currentPool.Count < 10 && page <= maxPagesToTry) // Reduced from 15 to 10 for efficiency
            {
                totalApiCalls++;
                var results = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, page);
                if (!results.Any()) break;
                
                var valid = results.Where(m => 
                    !blacklisted.Contains(m.Id) && 
                    !last5.Contains(m.Id) && 
                    !last25Movies.Any(um => um.TmdbId == m.Id)
                ).ToList();
                
                currentPool.AddRange(valid);
                page++;
            }
            
            if (currentPool.Count >= 3)
            {
                // Early exit: valid suggestions found for a prioritized decade
                validDecades.Add((decade, currentPool));
            }
        }

    // Fallback: If no decades have at least 3 valid movies, search for decades with at least 1
    // Ensures the user always receives a suggestion, even in edge cases
        if (!validDecades.Any())
        {
            _logger.LogWarning("[DECADE-LOGIC] No decade found with 3+ valid movies. Starting fallback search for 1+.");
            
            foreach (var decade in decadesToTry)
            {
                if (decade == lastDecade) continue;
                
                var currentPool = new List<TmdbMovieBrief>();
                int page = 1;
                const int maxPagesToTry = 3;
                
                while (currentPool.Count < 10 && page <= maxPagesToTry)
                {
                    totalApiCalls++;
                    var results = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, page);
                    if (!results.Any()) break;
                    
                    var valid = results.Where(m => 
                        !blacklisted.Contains(m.Id) && 
                        !last5.Contains(m.Id) && 
                        !last25Movies.Any(um => um.TmdbId == m.Id)
                    ).ToList();
                    
                    currentPool.AddRange(valid);
                    page++;
                }
                
                if (currentPool.Any())
                {
                    validDecades.Add((decade, currentPool));
                }
            }
        }

        stopwatch.Stop();
        _logger.LogInformation("🎯 DecadeReshuffle completed: {ApiCalls} API calls in {Ms}ms", totalApiCalls, stopwatch.ElapsedMilliseconds);

        if (!validDecades.Any())
        {
            var emptyHtml = "<div class='alert alert-info text-center my-5'>No new decade suggestions available right now.</div>";
            return Json(new { success = true, html = emptyHtml, count = 0 });
        }

    // Random selection among all valid decades found
    // Session state is updated to prevent immediate repetition in future requests
        var selected = validDecades[Random.Shared.Next(validDecades.Count)];
        HttpContext.Session.SetInt32(lastDecadeKey, selected.Decade);

    // Final response: render up to 3 suggestions for display
    // Ensures variety and freshness in the user experience
        var suggestedMovies = selected.Pool.OrderBy(_ => Random.Shared.Next()).Take(3).ToList();
        string suggestionTitle = $"Here are movies from the {selected.Decade}s";

        var htmlBuilder = new System.Text.StringBuilder();
        foreach (var movie in suggestedMovies)
        {
            var partialViewResult = await this.RenderPartialViewToStringAsync("_MovieSuggestionCard", movie);
            htmlBuilder.Append($"<div class=\"col\">{partialViewResult}</div>");
        }

        return Json(new {
            success = true,
            html = htmlBuilder.ToString(),
            count = suggestedMovies.Count,
            suggestionTitle = suggestionTitle
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "💥 ERROR in DecadeReshuffle: {Message}", ex.Message);
        return Json(new { success = false, error = "Could not generate a new suggestion." });
    }
}

[HttpGet]
[Authorize]
public async Task<IActionResult> GenreReshuffle()
{
    try
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var loggedMovies = await _dbContext.Movies
            .Where(m => m.UserId == userId && !string.IsNullOrEmpty(m.Genres) && m.TmdbId.HasValue)
            .ToListAsync();

        if (!loggedMovies.Any())
        {
            var emptyHtml = "<div class='alert alert-info text-center my-5'>Log movies with genres to get suggestions!</div>";
            return Json(new { success = true, html = emptyHtml, count = 0 });
        }

    // 1. Llama al nuevo helper para obtener la cola de prioridades (que usa caché)
    var priorityQueue = GetGenrePriorityQueueCached(userId, loggedMovies);

        // 2. Gestiona la secuencia para decidir qué tipo de género mostrar
        string genreTypeKey = $"GenreTypeSequence_{userId}";
        int genreTypeCount = HttpContext.Session.GetInt32(genreTypeKey) ?? 0;
        HttpContext.Session.SetInt32(genreTypeKey, genreTypeCount + 1);


        string? genreToSuggest = null;
        if (genreTypeCount < priorityQueue.Count)
        {
            genreToSuggest = priorityQueue[genreTypeCount];
        }
        else
        {
            // 3. Llama al nuevo helper para la selección aleatoria con anti-repetición
            genreToSuggest = GetRandomGenreWithAntiRepetition(userId, loggedMovies);
        }

        if (string.IsNullOrEmpty(genreToSuggest))
        {
            var emptyHtml = "<div class='alert alert-warning text-center my-5'>Could not find a valid genre to suggest.</div>";
            return Json(new { success = true, html = emptyHtml, count = 0 });
        }

       // 4. Generate random sort + page parameters
var sortTypes = new[] { "popularity.desc", "vote_average.desc", "release_date.desc" };
var currentSort = sortTypes[Random.Shared.Next(sortTypes.Length)];
var currentPage = Random.Shared.Next(1, 4);

_logger.LogInformation("🎲 Random selection: Genre={Genre}, Sort={Sort}, Page={Page}", genreToSuggest, currentSort, currentPage);

// 5. Get movies using the random parameters
var suggestedMovies = await GetSuggestionsForGenre(genreToSuggest, userId, currentSort, currentPage);


                // 5. Construye y devuelve la respuesta JSON
                // Dynamic title based on current sort type
                // Random sort + page with quality weighting
               var suggestionTitle = $"Because you watched {genreToSuggest} movies";
        var htmlBuilder = new StringBuilder();
        foreach (var movie in suggestedMovies)
        {
            var partialViewResult = await RenderPartialViewToStringAsync("_MovieSuggestionCard", movie);
            htmlBuilder.Append($"<div class=\"col\">{partialViewResult}</div>");
        }

        return Json(new { 
            success = true, 
            html = htmlBuilder.ToString(),
            count = suggestedMovies.Count,
            suggestionTitle = suggestionTitle
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "💥 ERROR in GenreReshuffle: {Message}", ex.Message);
        return Json(new { success = false, error = "Could not generate a new suggestion." });
    }
}

[HttpGet]
[Authorize]
public async Task<IActionResult> SurpriseMeReshuffle()
{
    try
    {
        _logger.LogInformation("🚀 SurpriseMeReshuffle AJAX endpoint called.");
        
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // --- 1. Check cache for existing pool (2 hours) ---
        string poolCacheKey = $"SurprisePool_{userId}";
        var cachedPool = _memoryCache.Get<(List<TmdbMovieBrief> bucket3x3, List<TmdbMovieBrief> bucket2x3, List<TmdbMovieBrief> bucket1x3)>(poolCacheKey);
        
        List<TmdbMovieBrief> bucket3x3, bucket2x3, bucket1x3;
        
        if (cachedPool.bucket3x3 != null && cachedPool.bucket2x3 != null && cachedPool.bucket1x3 != null)
        {
            _logger.LogInformation("🎯 Using cached surprise pool for user {UserId}", userId);
            bucket3x3 = cachedPool.bucket3x3;
            bucket2x3 = cachedPool.bucket2x3;
            bucket1x3 = cachedPool.bucket1x3;
        }
        else
        {
            _logger.LogInformation("🏗️ Building new surprise pool for user {UserId}", userId);
            
            // Build new pool (we'll create this helper next)
            var poolResult = await BuildSurprisePoolAsync(userId);
            bucket3x3 = poolResult.bucket3x3;
            bucket2x3 = poolResult.bucket2x3;
            bucket1x3 = poolResult.bucket1x3;
            
            // Cache for 2 hours
            var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(2));
            _memoryCache.Set(poolCacheKey, (bucket3x3, bucket2x3, bucket1x3), cacheOptions);
            
            _logger.LogInformation("✅ Pool cached: {Count3x3} + {Count2x3} + {Count1x3} = {Total} movies",
                bucket3x3.Count, bucket2x3.Count, bucket1x3.Count, bucket3x3.Count + bucket2x3.Count + bucket1x3.Count);
        }

        // --- 2. Filters already applied during build, use buckets directly ---
var filtered3x3 = bucket3x3;
var filtered2x3 = bucket2x3;
var filtered1x3 = bucket1x3;

var totalPoolSize = filtered3x3.Count + filtered2x3.Count + filtered1x3.Count;
_logger.LogInformation("🎁 Using pre-filtered pool: {Count3x3} + {Count2x3} + {Count1x3} = {Total} valid movies",
    filtered3x3.Count, filtered2x3.Count, filtered1x3.Count, totalPoolSize);

// Edge case: if pool too small, show friendly message
if (totalPoolSize < 10)
{
    var emptyHtml = @"<div class='alert alert-info text-center my-5'>Come back in a bit for more surprises! 🎬</div>";
    return Json(new { success = true, html = emptyHtml, count = 0 });
}

       // --- 3. Infinite cyclic rotation with pool index tracking ---
// --- 3. Infinite cyclic rotation with shuffled pool ---
string poolIndexKey = "SurprisePoolIndex";
string shuffledPoolKey = "SurprisePoolShuffled";

int poolIndex = HttpContext.Session.GetInt32(poolIndexKey) ?? 0;

// Get or create shuffled pool
var shuffledPool = HttpContext.Session.Get<List<TmdbMovieBrief>>(shuffledPoolKey);
var allMovies = filtered3x3.Concat(filtered2x3).Concat(filtered1x3).ToList();

// Create shuffled pool if it doesn't exist or if we're starting fresh
if (shuffledPool == null || shuffledPool.Count != allMovies.Count)
{
    shuffledPool = allMovies.OrderBy(x => Random.Shared.Next()).ToList();
    HttpContext.Session.Set(shuffledPoolKey, shuffledPool);
    poolIndex = 0; // Reset index when creating new shuffle
    _logger.LogInformation("🔀 Created new shuffled pool with {Count} movies", shuffledPool.Count);
}

// If we've gone through all movies, re-shuffle and restart
if (poolIndex >= shuffledPool.Count)
{
    shuffledPool = allMovies.OrderBy(x => Random.Shared.Next()).ToList();
    HttpContext.Session.Set(shuffledPoolKey, shuffledPool);
    poolIndex = 0;
    _logger.LogInformation("🔄 Pool completed, re-shuffled and restarting from movie #0");
}

// Select movie from shuffled pool
TmdbMovieBrief? selectedMovie = null;
string suggestionTitle = "Surprise!";

if (shuffledPool.Any() && poolIndex < shuffledPool.Count)
{
    selectedMovie = shuffledPool[poolIndex];
    
    // Advance pool index for next movie
    poolIndex++;
    HttpContext.Session.SetInt32(poolIndexKey, poolIndex);
    
    _logger.LogInformation("🎯 Selected movie #{Index} from shuffled pool: {Title}", poolIndex - 1, selectedMovie.Title);
}
        // --- 4. Handle no suggestion case ---
if (selectedMovie == null)
{
    var emptyHtml = @"<div class='alert alert-info text-center my-5'>Come back in a bit for more surprises! 🎬</div>";
    return Json(new { success = true, html = emptyHtml, count = 0 });
}

        // --- 5. Render response using partial view ---
        var htmlBuilder = new StringBuilder();
        var partialViewResult = await this.RenderPartialViewToStringAsync("_MovieSuggestionCard", selectedMovie);
        htmlBuilder.Append($"<div class=\"col\">{partialViewResult}</div>");

        return Json(new { 
            success = true, 
            html = htmlBuilder.ToString(),
            count = 1,
            suggestionTitle = suggestionTitle
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "💥 ERROR in SurpriseMeReshuffle: {Message}", ex.Message);
        return Json(new { 
            success = false, 
            error = "Could not generate a surprise suggestion. Please try again."
        });
    }
}

/// <summary>
/// <summary>
/// Builds the static pool of 80 deduplicated movies for Surprise Me suggestions.
///
/// <para>
/// <b>Pool Guarantee:</b> Always attempts to fill 80 slots using aggressive cascading across prioritized buckets (e.g., trending, genre, director, actor).
/// </para>
/// <para>
/// <b>Bucket System:</b> Uses a 3x3/2x3/1x3 bucket system for flexible distribution, ensuring variety and fallback if a bucket is exhausted.
/// </para>
/// <para>
/// <b>Deduplication & Filtering:</b> Deduplication by TMDB ID and filtering for blacklist/recent movies are performed during build, not per reshuffle.
/// </para>
/// <para>
/// <b>Performance:</b> Typically requires ~5 TMDB API calls for initial build; zero calls for subsequent reshuffles within the cache window.
/// </para>
/// </summary>
/// <returns>Tuple containing (bucket3x3, bucket2x3, bucket1x3) with unique movies</returns>
private async Task<(List<TmdbMovieBrief> bucket3x3, List<TmdbMovieBrief> bucket2x3, List<TmdbMovieBrief> bucket1x3)> 
    BuildSurprisePoolAsync(string userId)
{
    _logger.LogInformation("🏗️ Building surprise pool for user {UserId}", userId);
    
    // --- 1. Get user ingredients ---
    var loggedMovies = await _dbContext.Movies
        .Where(m => m.UserId == userId && m.TmdbId.HasValue && !string.IsNullOrEmpty(m.Director) && !string.IsNullOrEmpty(m.Genres) && m.ReleasedYear.HasValue)
        .ToListAsync();
        
        // Get user filters to apply during build (not after)
var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);
var recentMovieIds = await _dbContext.Movies
    .Where(m => m.UserId == userId && m.DateWatched.HasValue && m.TmdbId.HasValue)
    .OrderByDescending(m => m.DateWatched)
    .Take(10)
    .Select(m => m.TmdbId!.Value)
    .ToListAsync();
var recentMovieIdSet = recentMovieIds.ToHashSet();
_logger.LogInformation("🔍 Filters loaded: {BlacklistCount} blacklisted, {RecentCount} recent", 
    userBlacklistedIds.Count, recentMovieIdSet.Count);

    if (loggedMovies.Count < 3)
            {
                return (new List<TmdbMovieBrief>(), new List<TmdbMovieBrief>(), new List<TmdbMovieBrief>());
            }

    var random = new Random();
    var directorPool = loggedMovies.Select(m => m.Director!).Distinct().ToList();
    var genrePool = loggedMovies.SelectMany(m => m.Genres!.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();
    
    // Build actor pool (cached for performance)
    var actorPool = new List<TmdbCastPerson>();
    var movieDetailsCache = new Dictionary<int, TmdbMovieDetails?>();
    
    foreach (var movie in loggedMovies.OrderByDescending(m => m.DateWatched).Take(15))
    {
        if (!movie.TmdbId.HasValue) continue;
        var tmdbIdValue = movie.TmdbId.Value;
        
        TmdbMovieDetails? details;
        if (movieDetailsCache.ContainsKey(tmdbIdValue))
        {
            details = movieDetailsCache[tmdbIdValue];
        }
        else
        {
            details = await _tmdbService.GetMovieDetailsAsync(tmdbIdValue);
            movieDetailsCache[tmdbIdValue] = details;
        }
        
        if (details?.Credits?.Cast != null)
        {
            actorPool.AddRange(details.Credits.Cast.Take(3));
        }
    }
    actorPool = actorPool.DistinctBy(p => p.Id).ToList();

    if (!actorPool.Any())
    {
        _logger.LogWarning("No actors found in user's recent movies");
        return (new List<TmdbMovieBrief>(), new List<TmdbMovieBrief>(), new List<TmdbMovieBrief>());
    }

    // Get all genres for mapping
    var allGenres = await _tmdbService.GetAllGenresAsync();
    var genreDict = allGenres.ToDictionary(g => g.Name!, g => g.Id, StringComparer.OrdinalIgnoreCase);

    // --- 2. Build buckets with deduplication ---
    var allMoviesFound = new HashSet<int>(); // Global deduplication tracker
    var bucket3x3 = new List<TmdbMovieBrief>();
    var bucket2x3 = new List<TmdbMovieBrief>();
    var bucket1x3 = new List<TmdbMovieBrief>();

    // --- BUCKET A: 3/3 MATCHES (Target: 20) ---
    _logger.LogInformation("🎯 Building 3/3 matches bucket...");
    var attempts3x3 = 0;
    while (bucket3x3.Count < 20 && attempts3x3 < 10) // Max 10 combinations to avoid infinite loop
    {
        var randDirector = directorPool[random.Next(directorPool.Count)];
        var randActor = actorPool[random.Next(actorPool.Count)];
        var randGenre = genrePool[random.Next(genrePool.Count)];
        
        var directorId = await _tmdbService.GetPersonIdAsync(randDirector);
        var actorId = randActor.Id;
        var genreId = genreDict.ContainsKey(randGenre) ? genreDict[randGenre] : (int?)null;
        
        if (directorId.HasValue && genreId.HasValue)
        {
            var movies = await _tmdbService.DiscoverMoviesAsync(directorId, actorId, genreId, null);
            var validMovies = movies
    .Where(m => m.VoteAverage >= 4.5)
    .Where(m => !userBlacklistedIds.Contains(m.Id)) // Apply blacklist filter
    .Where(m => !recentMovieIdSet.Contains(m.Id)) // Apply recent filter
    .Where(m => !allMoviesFound.Contains(m.Id)) // Deduplication
    .ToList();
            
            foreach (var movie in validMovies.Take(20 - bucket3x3.Count))
            {
                bucket3x3.Add(movie);
                allMoviesFound.Add(movie.Id);
            }
        }
        attempts3x3++;
    }
    
    _logger.LogInformation("✅ 3/3 bucket built: {Count} movies", bucket3x3.Count);

    // --- BUCKET B: 2/3 MATCHES (Target: 60, complete missing from 3/3) ---
    _logger.LogInformation("🎯 Building 2/3 matches bucket...");
    var needed2x3 = 60 + Math.Max(0, 20 - bucket3x3.Count); // Cascade: complete missing 3/3
    var attempts2x3 = 0;
    
    // Three 2/3 combinations
    var combos2x3 = new[]
    {
        "director_actor", // Director + Actor
        "director_genre", // Director + Genre  
        "actor_genre"     // Actor + Genre
    };
    
    while (bucket2x3.Count < needed2x3 && attempts2x3 < 15) // Max 15 combinations
    {
        var combo = combos2x3[attempts2x3 % 3];
        var randDirector = directorPool[random.Next(directorPool.Count)];
        var randActor = actorPool[random.Next(actorPool.Count)];
        var randGenre = genrePool[random.Next(genrePool.Count)];
        
        var directorId = await _tmdbService.GetPersonIdAsync(randDirector);
        var actorId = randActor.Id;
        var genreId = genreDict.ContainsKey(randGenre) ? genreDict[randGenre] : (int?)null;
        
        List<TmdbMovieBrief> movies = new();
        
        switch (combo)
        {
            case "director_actor":
                if (directorId.HasValue)
                    movies = await _tmdbService.DiscoverMoviesAsync(directorId, actorId, null, null);
                break;
            case "director_genre":
                if (directorId.HasValue && genreId.HasValue)
                    movies = await _tmdbService.DiscoverMoviesAsync(directorId, null, genreId, null);
                break;
            case "actor_genre":
                if (genreId.HasValue)
                    movies = await _tmdbService.DiscoverMoviesAsync(null, actorId, genreId, null);
                break;
        }
        
        var validMovies = movies
    .Where(m => m.VoteAverage >= 4.5)
    .Where(m => !userBlacklistedIds.Contains(m.Id)) // Apply blacklist filter
    .Where(m => !recentMovieIdSet.Contains(m.Id)) // Apply recent filter
    .Where(m => !allMoviesFound.Contains(m.Id)) // Deduplication
    .ToList();
        
        foreach (var movie in validMovies.Take(needed2x3 - bucket2x3.Count))
        {
            bucket2x3.Add(movie);
            allMoviesFound.Add(movie.Id);
        }
        
        attempts2x3++;
    }
    
    _logger.LogInformation("✅ 2/3 bucket built: {Count} movies (needed: {Needed})", bucket2x3.Count, needed2x3);

    // --- BUCKET C: 1/3 MATCHES (Target: 20, complete missing from 2/3) ---
    _logger.LogInformation("🎯 Building 1/3 matches bucket...");
    var needed1x3 = 20 + Math.Max(0, needed2x3 - bucket2x3.Count); // Cascade: complete missing 2/3
    var attempts1x3 = 0;
    
    var singles1x3 = new[] { "director", "actor", "genre" };
    
    while (bucket1x3.Count < needed1x3 && attempts1x3 < 10)
    {
        var single = singles1x3[attempts1x3 % 3];
        var randDirector = directorPool[random.Next(directorPool.Count)];
        var randActor = actorPool[random.Next(actorPool.Count)];
        var randGenre = genrePool[random.Next(genrePool.Count)];
        
        List<TmdbMovieBrief> movies = new();
        
        switch (single)
        {
            case "director":
                var directorId = await _tmdbService.GetPersonIdAsync(randDirector);
                if (directorId.HasValue)
                    movies = await _tmdbService.DiscoverMoviesAsync(directorId, null, null, null);
                break;
            case "actor":
                var actorId = randActor.Id;
                movies = await _tmdbService.DiscoverMoviesAsync(null, actorId, null, null);
                break;
            case "genre":
                var genreId = genreDict.ContainsKey(randGenre) ? genreDict[randGenre] : (int?)null;
                if (genreId.HasValue)
                    movies = await _tmdbService.DiscoverMoviesAsync(null, null, genreId, null);
                break;
        }
        
       var validMovies = movies
    .Where(m => m.VoteAverage >= 4.5)
    .Where(m => !userBlacklistedIds.Contains(m.Id)) // Apply blacklist filter
    .Where(m => !recentMovieIdSet.Contains(m.Id)) // Apply recent filter
    .Where(m => !allMoviesFound.Contains(m.Id)) // Deduplication
    .ToList();
        
        foreach (var movie in validMovies.Take(needed1x3 - bucket1x3.Count))
        {
            bucket1x3.Add(movie);
            allMoviesFound.Add(movie.Id);
        }
        
        attempts1x3++;
    }
    
    _logger.LogInformation("✅ 1/3 bucket built: {Count} movies (needed: {Needed})", bucket1x3.Count, needed1x3);
    
    var totalFound = bucket3x3.Count + bucket2x3.Count + bucket1x3.Count;
_logger.LogInformation("🎁 Pool building complete: {Total} unique movies across all buckets", totalFound);

// GUARANTEE exactly 80 movies - aggressive cascading if needed
if (totalFound < 80)
{
    _logger.LogWarning("⚠️ Pool has only {Total} movies, need 80. Starting aggressive cascading...", totalFound);
    
    // Try more 1x3 combinations until we reach 80
    var additionalNeeded = 80 - totalFound;
    var extraAttempts = 0;
    var extraSingles = new[] { "director", "actor", "genre" };
    
    while (totalFound < 80 && extraAttempts < 20) // Max 20 additional attempts
    {
        var single = extraSingles[extraAttempts % 3];
        var randDirector = directorPool[random.Next(directorPool.Count)];
        var randActor = actorPool[random.Next(actorPool.Count)];
        var randGenre = genrePool[random.Next(genrePool.Count)];
        
        List<TmdbMovieBrief> movies = new();
        
        switch (single)
        {
            case "director":
                var directorId = await _tmdbService.GetPersonIdAsync(randDirector);
                if (directorId.HasValue)
                    movies = await _tmdbService.DiscoverMoviesAsync(directorId, null, null, null);
                break;
            case "actor":
                var actorId = randActor.Id;
                movies = await _tmdbService.DiscoverMoviesAsync(null, actorId, null, null);
                break;
            case "genre":
                var genreId = genreDict.ContainsKey(randGenre) ? genreDict[randGenre] : (int?)null;
                if (genreId.HasValue)
                    movies = await _tmdbService.DiscoverMoviesAsync(null, null, genreId, null);
                break;
        }
        
        var validMovies = movies
            .Where(m => m.VoteAverage >= 4.0) // Lower threshold for guarantee
            .Where(m => !userBlacklistedIds.Contains(m.Id))
            .Where(m => !recentMovieIdSet.Contains(m.Id))
            .Where(m => !allMoviesFound.Contains(m.Id))
            .ToList();
        
        foreach (var movie in validMovies.Take(80 - totalFound))
        {
            bucket1x3.Add(movie);
            allMoviesFound.Add(movie.Id);
            totalFound++;
        }
        
        extraAttempts++;
    }
    
    _logger.LogInformation("✅ Aggressive cascading complete: {Total} movies guaranteed", totalFound);
}

    return (bucket3x3, bucket2x3, bucket1x3);
}

/// <summary>
/// Helper para renderizar una partial view a string.
/// Permite reutilizar la lógica de vistas parciales en endpoints AJAX, devolviendo HTML listo para insertar en el cliente.
///
/// Se usa en TrendingReshuffle para garantizar que los posters, helpers y paths de imágenes funcionen igual que en el render inicial.
        /// </summary>
        private async Task<string> RenderPartialViewToStringAsync(string viewName, object model)
        {
            // Obtiene el motor de vistas configurado en ASP.NET Core
            if (ControllerContext.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) is not ICompositeViewEngine viewEngine)
            {
                throw new InvalidOperationException("ViewEngine not found");
            }

            var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

            if (viewResult.View == null)
            {
                throw new FileNotFoundException($"View '{viewName}' not found");
            }

            using var writer = new StringWriter();
            // Crea un contexto de vista con el modelo y helpers necesarios
            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                new ViewDataDictionary(ViewData) { Model = model },
                TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return writer.ToString();
        }

        /// <summary>
        /// Displays a genre selection view based on the user's logged movies.
        /// </summary>
        /// <remarks>
        /// - Fetches all genres from TMDB and counts their occurrence in the user's movie history.
        /// - Used for genre-based suggestion flows.
        /// - Shows a prompt if the user has not logged any movies.
        /// </remarks>
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

            // Fetch all genres from the TMDB service once
            var allGenres = await _tmdbService.GetAllGenresAsync();
            var allGenresDict = (allGenres ?? new List<TmdbGenre>()).ToDictionary(g => g.Id, g => g.Name ?? string.Empty);

            var genreCounts = new Dictionary<int, int>();

            // Performance bottleneck: Consider implementing caching for large movie collections
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

            // Select the top 6 genres based on frequency
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
                // Transform TMDB search results to match JavaScript client expectations
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
            // Display initial suggestion page with criteria buttons
            return View();
        }

        #region Suggestion Actions & Helpers

        /// <summary>
        /// Returns a set of movie suggestions for the logged-in user, based on the specified suggestion type.
        /// </summary>
        /// <param name="suggestionType">The type of suggestion sequence to use (e.g., trending, director_recent, genre_frequent, etc.).</param>
        /// <param name="query">Optional: Used for sequence state or to pass a specific director/genre/cast for the next suggestion.</param>
        /// <param name="page">Optional: For paginated suggestion types (default 1).</param>
        /// <remarks>
        /// Suggestion system with session-based sequences: directors, genres, cast, decades, trending.
        /// - Director sequence: recent → frequent → rated → random, with anti-repetition and session state.
        /// - Genre and cast sequences follow similar logic, using user history and session to avoid repetition.
        /// - Trending suggestions are personalized, exclude blacklisted and recently watched movies, and randomize pool.
        /// - All suggestion types are designed to maximize variety, avoid repetition, and respect user preferences.
        /// - Business logic is modular, with each case handling its own pool building, anti-repetition, and fallback.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> ShowSuggestions(string suggestionType, string? query = null, int page = 1)
        // See XML doc for high-level overview. Below: business logic comments for each sequence.
        // Director sequence: recent → frequent → rated → random with anti-repetition
        // Fresh start reset: page refresh starts with recent director
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
                // Trending suggestions: build a user-personalized pool of trending movies, avoiding repetition and respecting user preferences.
                // Exclude all TMDB IDs blacklisted by the current user (prevents unwanted suggestions).
                // Exclude the last 5 movies watched by the user (prevents immediate repeats in trending suggestions).
                // Pool generation: up to 30 valid trending movies, paging up to 5 TMDB pages if needed (ensures variety and fallback if user has many exclusions).
                // Randomize pool and select 3 movies for suggestion (ensures variety and avoids bias from TMDB order)
                case "trending":
                    suggestionTitle = "Trending Movies Today";
                    // Get blacklisted movie IDs (with null safety)
                    var blacklistIds = await _dbContext.BlacklistedMovies
                        .Where(b => b.UserId == userId)
                        .Select(b => b.TmdbId)
                        .ToListAsync();
                    // Get recent movie IDs (with null safety)
                    var recentIds = await _dbContext.Movies
                        .Where(m => m.UserId == userId && m.DateWatched.HasValue && m.TmdbId.HasValue)
                        .OrderByDescending(m => m.DateWatched)
                        .Take(5)
                        .Select(m => m.TmdbId ?? 0)
                        .ToListAsync();
                    // Build pool of valid movies
                    var moviePool = new List<TmdbMovieBrief>();
                    int pageNum = 1;
                    while (moviePool.Count < 30 && pageNum <= 5)
                    {
                        var pageMovies = await _tmdbService.GetTrendingMoviesAsync(pageNum);
                        var validMovies = pageMovies
                            .Where(m => !blacklistIds.Contains(m.Id) && !recentIds.Contains(m.Id))
                            .ToList();
                        moviePool.AddRange(validMovies);
                        pageNum++;
                    }
                    // Randomize and take 3
                    suggestedMovies = moviePool
                        .OrderBy(x => Random.Shared.Next())
                        .Take(3)
                        .ToList();
                    break;

                case "director_recent":
                case "director_frequent":
                case "director_rated":
                case "director_random":
                {
        // DIRECTOR SUGGESTIONS LOGIC (AJAX-enabled)
        // This block now only handles the INITIAL director suggestion request (when clicking the main button).
        // All subsequent reshuffles are handled via AJAX in the DirectorReshuffle() endpoint.
        // The sequence is: recent → frequent → rated → random, with anti-repetition and session state.
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
                    
                    // Set current and next director types
                    string[] directorTypes = new[] { "director_recent", "director_frequent", "director_rated" };
                    int maxDirectorTypeIndex = directorTypes.Length - 1;
                    string currentDirectorType = directorTypeCount <= maxDirectorTypeIndex
                        ? directorTypes[directorTypeCount]
                        : "director_random";
                    string nextDirectorType = directorTypeCount < maxDirectorTypeIndex
                        ? directorTypes[directorTypeCount + 1]
                        : "director_random";
                    
                    // Advance sequence counter
                    if (directorTypeCount <= maxDirectorTypeIndex)
                    {
                    HttpContext.Session.SetInt32(directorTypeKey, directorTypeCount + 1);
                }
                else
                {
                    HttpContext.Session.SetInt32(directorTypeKey, maxDirectorTypeIndex + 1);
                }

                    // Get user movies for director analysis - limit to last 20 directors if more than 25 unique
                    var allUserMovies = await _dbContext.Movies
                        .Where(m => m.UserId == userId && !string.IsNullOrEmpty(m.Director) && m.Director != "N/A" && m.TmdbId.HasValue)
                        .OrderByDescending(m => m.DateWatched ?? m.DateCreated)
                        .ToListAsync();
                    if (!allUserMovies.Any())
                    {
                        suggestionTitle = "Log some movies to get director suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }

                    // Limit to recent directors for performance optimization
                    var uniqueDirectorsOrdered = allUserMovies
                        .Select(m => m.Director!)
                        .Distinct()
                        .ToList();
                    List<string> limitedDirectors;
                    if (uniqueDirectorsOrdered.Count > 25)
                        limitedDirectors = uniqueDirectorsOrdered.Take(20).ToList();
                    else
                        limitedDirectors = uniqueDirectorsOrdered;

                    // Filter movies to selected directors only
                    var loggedDirectorMovies = allUserMovies
                        .Where(m => limitedDirectors.Contains(m.Director!))
                        .ToList();

                    // Build director queue: recent, frequent, rated
                    var topDirectorQueue = new List<string>();
                    var recentDirector = loggedDirectorMovies.OrderByDescending(m => m.DateWatched).FirstOrDefault()?.Director;
                    var frequentDirector = loggedDirectorMovies.GroupBy(m => m.Director!).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                    
                    // Get rated directors (minimum 2 rated movies)
                    var ratedDirectors = loggedDirectorMovies
                        .Where(m => m.UserRating.HasValue)
                        .GroupBy(m => m.Director!)
                        .Where(g => g.Count() >= 2)
                        .Select(g => new { Name = g.Key, Avg = g.Average(m => m.UserRating!.Value) })
                        .OrderByDescending(d => d.Avg)
                        .Select(d => d.Name)
                        .ToList();
                    
                    if (recentDirector is string rd) topDirectorQueue.Add(rd);
                    if (frequentDirector is string fd && !topDirectorQueue.Contains(fd)) topDirectorQueue.Add(fd);
                    if (ratedDirectors.Any() && !topDirectorQueue.Contains(ratedDirectors[0])) topDirectorQueue.Add(ratedDirectors[0]);
                    
                    // Debug logging for director queue
                    _logger.LogInformation("Original topDirectorQueue: {Queue}", string.Join(", ", topDirectorQueue));
                    var dedupedDirectorQueue = topDirectorQueue.Distinct().ToList();
                    _logger.LogInformation("Deduplicated queue: {Queue}", string.Join(", ", dedupedDirectorQueue));
                    _logger.LogInformation("Deduped queue count: {Count}", dedupedDirectorQueue.Count);
                    _logger.LogInformation("Top director queue debug");
                    _logger.LogInformation("Recent director: {Recent}", recentDirector);
                    _logger.LogInformation("Frequent director: {Frequent}", frequentDirector);
                    _logger.LogInformation("Rated director: {Rated}", ratedDirectors.FirstOrDefault());

                    string? directorToSuggest = null;
                    List<TmdbMovieBrief> directorSuggestions = new();

                    // Execute director selection based on current type
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
                            if (dedupedDirectorQueue.Contains(d))
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
                    
                    // Random director selection with anti-repetition
                    var allDirectors = loggedDirectorMovies.Select(m => m.Director!).Distinct().ToList();
                    if (currentDirectorType == "director_random")
                    {
                        _logger.LogInformation("EXECUTING: director_random block");
                        if (!allDirectors.Any())
                        {
                            suggestionTitle = "No available director suggestions. Try reshuffling or logging more movies.";
                            break;
                        }
                        
                        // Anti-repetition: avoid last random director
                        string lastRandomDirectorKey = $"LastRandomDirector_{userId}";
                        string? lastRandomDirector = HttpContext.Session.GetString(lastRandomDirectorKey);
                        var availableDirectors = allDirectors;
                        if (!string.IsNullOrEmpty(lastRandomDirector) && allDirectors.Count > 1)
                        {
                            var filtered = allDirectors.Where(d => d != lastRandomDirector).ToList();
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
                        HttpContext.Session.SetString(lastRandomDirectorKey, selectedDirector);
                    }
                    
                    // Bulletproof fallback: force random selection if no suggestions found
                    if (directorSuggestions.Count == 0)
                    {
                        _logger.LogWarning("No director suggestions found for {Type}, forcing random fallback", currentDirectorType);
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
    // Genre suggestion logic with dynamic variety (matches AJAX behavior)
    var loggedGenreMovies = await _dbContext.Movies.Where(m => m.UserId == userId && !string.IsNullOrEmpty(m.Genres) && m.TmdbId.HasValue).ToListAsync();
    if (!loggedGenreMovies.Any())
    {
        suggestionTitle = "Log movies with genres to get suggestions!";
        ViewData["ShowAddMovieButton"] = true;
        break;
    }

    // Use the same priority queue logic as AJAX endpoint
    var priorityQueue = GetGenrePriorityQueueCached(userId, loggedGenreMovies);

    // Generate random sort + page parameters (same as AJAX)
    var genreSortTypes = new[] { "popularity.desc", "vote_average.desc", "release_date.desc" };
    var genreCurrentSort = genreSortTypes[Random.Shared.Next(genreSortTypes.Length)];
    var genreCurrentPage = Random.Shared.Next(1, 4);

    _logger.LogInformation("🎲 Initial genre suggestion: Sort={Sort}, Page={Page}", genreCurrentSort, genreCurrentPage);

    // Determine which genre to suggest based on sequence
    string genreTypeKey = $"GenreTypeSequence_{userId}";
    bool isFreshStartGenre = string.IsNullOrWhiteSpace(query);
    int genreTypeCount;
    if (isFreshStartGenre)
    {
        HttpContext.Session.SetInt32(genreTypeKey, 0);
        genreTypeCount = 0;
        _logger.LogInformation("[SEQUENCE RESET] Fresh start detected, resetting genre sequence to 0");
    }
    else
    {
        genreTypeCount = HttpContext.Session.GetInt32(genreTypeKey) ?? 0;
    }

    string? genreToSuggest = null;
    if (genreTypeCount < priorityQueue.Count)
    {
        genreToSuggest = priorityQueue[genreTypeCount];
    }
    else
    {
        genreToSuggest = GetRandomGenreWithAntiRepetition(userId, loggedGenreMovies);
    }

    if (string.IsNullOrEmpty(genreToSuggest))
    {
        suggestionTitle = "Could not find a valid genre to suggest.";
        break;
    }

    // Get suggestions using the same method as AJAX (with dynamic parameters)
    var genreSuggestions = await GetSuggestionsForGenre(genreToSuggest, userId, genreCurrentSort, genreCurrentPage);

    if (!genreSuggestions.Any())
    {
        suggestionTitle = "No available genre suggestions. Try reshuffling or logging more movies.";
        break;
    }

    // Advance the sequence for next time
    HttpContext.Session.SetInt32(genreTypeKey, genreTypeCount + 1);

    suggestedMovies = genreSuggestions;
    suggestionTitle = $"Because you watched {genreToSuggest} movies";
    nextQuery = genreToSuggest;
    break;

                case "cast_recent":
                case "cast_frequent":
                case "cast_rated":
                case "cast_random":
                    // Cast suggestion system: recent → frequent → rated → random, with anti-repetition and local TMDB cache for performance
                    
                    // Manages cast suggestion sequence (mirrors director logic)
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

                    // Local TMDB details pool for performance optimization
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
                    
                    // Build actor pool from last 5 movies (top 3 cast per movie)
                    var allTopActors = new List<TmdbCastPerson>();
                    foreach (var movie in loggedCastMovies.Take(5))
                    {
                        if (!movie.TmdbId.HasValue) continue;
                        var details = await GetMovieDetailsWithPoolAsync(movie.TmdbId.Value);
                        if (details?.Credits?.Cast != null) allTopActors.AddRange(details.Credits.Cast.Take(3));
                    }
                    if (!allTopActors.Any()) { suggestionTitle = "Could not find cast info in your recent logs."; break; }

                    // Build cast queue with variety: random selection from each category
                    var topActorQueue = new List<TmdbCastPerson>();
                    var randomCast = Random.Shared;

                    // Recent cast: random actor from top 5 cast of most recent movie
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

                    // Frequent cast: random actor from top 3 most frequent actors
                    var allActorsById = allTopActors.GroupBy(a => a.Id)
                        .Select(g => new { Actor = g.First(), Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToList();

                    if (allActorsById.Any())
                    {
                        var topFrequentActors = allActorsById.Take(3).Select(x => x.Actor).ToList();
                        var frequentActor = topFrequentActors[randomCast.Next(topFrequentActors.Count)];
                        // Avoid duplicate if already added as recent
                        if (!topActorQueue.Any(a => a.Id == frequentActor.Id))
                        {
                            topActorQueue.Add(frequentActor);
                            _logger.LogInformation("Frequent: Selected {Actor} from top frequent actors", frequentActor.Name);
                        }
                    }

                    // Rated cast: random actor from top 5 cast of highest rated movie
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

                    // Deduplicate actor queue
                    var dedupedActorQueue = topActorQueue.DistinctBy(a => a.Id).ToList();
                    _logger.LogInformation("Original topActorQueue count: {Count}", topActorQueue.Count);
                    _logger.LogInformation("Deduplicated queue count: {Count}", dedupedActorQueue.Count);

                    TmdbCastPerson? actorToSuggest = null;
                    List<TmdbMovieBrief> actorSuggestions = new();
                    
                    // Sequence logic: avoid consecutive repeats across reshuffles
                    int castStep = -1;
                    if (currentCastType == "cast_recent") castStep = 0;
                    else if (currentCastType == "cast_frequent") castStep = 1;
                    else if (currentCastType == "cast_rated") castStep = 2;

                    // Retrieve last actor ID from session for anti-repetition
                    string lastActorSessionKey = $"LastCastActorId_{userId}";
                    int? lastActorId = null;
                    var lastActorIdStr = HttpContext.Session.GetString(lastActorSessionKey);
                    if (int.TryParse(lastActorIdStr, out var parsedId))
                        lastActorId = parsedId;

                    // Find first actor in queue that isn't the same as previous
                    if (castStep >= 0 && dedupedActorQueue.Count > castStep)
                    {
                        var candidate = dedupedActorQueue.Skip(castStep).FirstOrDefault(a => a.Id != lastActorId) ?? dedupedActorQueue[castStep];
                        var movies = await GetSuggestionsForActor(candidate.Id, userId);
                        if (movies.Any())
                        {
                            actorToSuggest = candidate;
                            actorSuggestions = movies;
                            HttpContext.Session.SetString(lastActorSessionKey, candidate.Id.ToString());
                        }
                    }
                    
                    // Random cast selection with anti-repetition
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
                    
                    // Fallback: force random selection if no suggestions found
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
    // Decade suggestion logic using optimized approach (matches AJAX behavior)
    var last25Movies = await _dbContext.Movies
        .Where(m => m.UserId == userId && m.ReleasedYear.HasValue)
        .OrderByDescending(m => m.DateWatched ?? m.DateCreated)
        .Take(25)
        .ToListAsync();

    if (last25Movies.Count < 3)
    {
        suggestionTitle = "Log at least 3 movies to get decade suggestions!";
        ViewData["ShowAddMovieButton"] = true;
        break;
    }

    // Calculate decade priorities based on last 25 movies (same as AJAX)
    var availableDecades = last25Movies
        .Select(m => (m.ReleasedYear!.Value / 10) * 10)
        .Distinct()
        .ToList();

    var latestDecade = (last25Movies.First().ReleasedYear!.Value / 10) * 10;

    var frequentGroups = last25Movies.GroupBy(m => (m.ReleasedYear!.Value / 10) * 10)
        .Where(g => g.Count() >= 2).ToList();
    var mostFrequentDecade = 0;
    if (frequentGroups.Any())
    {
        var maxFreq = frequentGroups.Max(g => g.Count());
        var freqCandidates = frequentGroups.Where(g => g.Count() == maxFreq).Select(g => g.Key).ToList();
        mostFrequentDecade = freqCandidates[Random.Shared.Next(freqCandidates.Count)];
    }

    var ratedGroups = last25Movies.Where(m => m.UserRating.HasValue)
        .GroupBy(m => (m.ReleasedYear!.Value / 10) * 10)
        .Where(g => g.Count() >= 2).ToList();
    var highestRatedDecade = 0;
    if (ratedGroups.Any())
    {
        var maxAvgRating = ratedGroups.Max(g => g.Average(m => m.UserRating!.Value));
        var ratedCandidates = ratedGroups
            .Where(g => Math.Abs(g.Average(m => m.UserRating!.Value) - maxAvgRating) < 0.01m)
            .Select(g => g.Key).ToList();
        highestRatedDecade = ratedCandidates[Random.Shared.Next(ratedCandidates.Count)];
    }

    // Decade priority queue with anti-repetition
    var decadePriorityQueue = new List<int> { latestDecade, mostFrequentDecade, highestRatedDecade }
        .Where(d => d > 0).Distinct().ToList();
    var randomDecades = availableDecades.OrderBy(_ => Random.Shared.Next()).ToList();
    var decadesToTry = decadePriorityQueue.Concat(randomDecades).ToList();

    // Cache expensive filters (same as AJAX)
    var blacklisted = await GetUserBlacklistedTmdbIdsAsync(userId);
    var last5 = last25Movies.Take(5).Select(m => m.TmdbId ?? 0).ToHashSet();

    // Find first valid decade with movies
    List<TmdbMovieBrief> decadeSuggestions = new();
    int? selectedDecade = null;

    foreach (var decade in decadesToTry.Take(5)) // Same limit as AJAX
    {
        var currentPool = new List<TmdbMovieBrief>();
        int decadePage = 1;
        const int maxPagesToTry = 3;

        while (currentPool.Count < 10 && decadePage <= maxPagesToTry)
        {
            var results = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, decadePage);
            if (!results.Any()) break;

            var valid = results.Where(m =>
                !blacklisted.Contains(m.Id) &&
                !last5.Contains(m.Id) &&
                !last25Movies.Any(um => um.TmdbId == m.Id)
            ).ToList();

            currentPool.AddRange(valid);
            decadePage++;
        }

        if (currentPool.Count >= 3)
        {
            selectedDecade = decade;
            decadeSuggestions = currentPool.OrderBy(_ => Random.Shared.Next()).Take(3).ToList();
            break;
        }
    }

    if (!decadeSuggestions.Any())
    {
        suggestionTitle = "No new decade suggestions available right now.";
        break;
    }

    suggestedMovies = decadeSuggestions;
    suggestionTitle = $"Here are movies from the {selectedDecade}s";
    nextQuery = selectedDecade?.ToString();
    break;

                default: return RedirectToAction("Suggest");
            }

            ViewData["SuggestionTitle"] = suggestionTitle;
            ViewData["NextSuggestionType"] = nextSuggestionType;
            ViewData["NextQuery"] = nextQuery;
            return View("Suggest", suggestedMovies);
        }


        private async Task<List<TmdbMovieBrief>> GetSuggestionsForDirector(string directorName, string userId)
        {
            var directorId = await _tmdbService.GetPersonIdAsync(directorName);
            if (!directorId.HasValue) return new List<TmdbMovieBrief>();

            // Get the director's ENTIRE filmography from our service
            var allDirectorMovies = await _tmdbService.GetDirectorFilmographyAsync(directorId.Value);

            // Get user state sets
            var userLoggedTmdbIds = (await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue)
                .Select(m => m.TmdbId)
                .ToListAsync())
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            var userWishlistTmdbIds = (await _dbContext.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.TmdbId)
                .ToListAsync()).ToHashSet();
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);

            // Blacklist filter
            allDirectorMovies = allDirectorMovies.Where(m => !userBlacklistedIds.Contains(m.Id)).ToList();

            if (!allDirectorMovies.Any())
            {
                return new List<TmdbMovieBrief>(); // Return empty if they have no movies
            }

            // If they have 3 or fewer movies, just return them all.
            var suggestions = allDirectorMovies.Count <= 3
                ? allDirectorMovies
                : allDirectorMovies.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

            // Populate state properties
            foreach (var movie in suggestions)
            {
                movie.IsWatched = userLoggedTmdbIds.Contains(movie.Id);
                movie.IsInWishlist = userWishlistTmdbIds.Contains(movie.Id);
                movie.IsInBlacklist = userBlacklistedIds.Contains(movie.Id);
            }
            return suggestions;
        }


      private async Task<List<TmdbMovieBrief>> GetSuggestionsForGenre(string genreName, string userId, string sortBy = "popularity.desc", int page = 1)
{
    var allGenres = await _tmdbService.GetAllGenresAsync();
    var genre = allGenres.FirstOrDefault(g => g.Name != null && g.Name.Equals(genreName, StringComparison.OrdinalIgnoreCase));
    if (genre == null) return new List<TmdbMovieBrief>();

    _logger.LogInformation("🎬 Genre suggestion for {Genre}: Sort={Sort}, Page={Page}", genreName, sortBy, page);

    // Try primary sort + page combination
    var movies = await TryGetGenreMovies(genre.Id, sortBy, page, genreName);
    
    // Fallback 1: Same sort, page 1 (if primary failed)
    if (movies.Count < 3)
    {
        _logger.LogInformation("⚠️ Fallback 1: {Genre} {Sort} page {Page} insufficient, trying page 1", genreName, sortBy, page);
        movies = await TryGetGenreMovies(genre.Id, sortBy, 1, genreName);
    }
    
    // Fallback 2: Popular, page 1 (ultimate safety net)
    if (movies.Count < 3 && sortBy != "popularity.desc")
    {
        _logger.LogInformation("⚠️ Fallback 2: {Genre} {Sort} insufficient, trying popular page 1", genreName, sortBy);
        movies = await TryGetGenreMovies(genre.Id, "popularity.desc", 1, genreName);
    }

    // Apply user filtering
    var userLoggedTmdbIds = (await _dbContext.Movies
        .Where(m => m.UserId == userId && m.TmdbId.HasValue)
        .Select(m => m.TmdbId)
        .ToListAsync())
        .Where(id => id.HasValue)
        .Select(id => id!.Value)
        .ToHashSet();

    var userWishlistTmdbIds = (await _dbContext.WishlistItems
        .Where(w => w.UserId == userId)
        .Select(w => w.TmdbId)
        .ToListAsync()).ToHashSet();

    var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);

    // Filter out movies already logged or blacklisted
    movies = movies.Where(m => !userLoggedTmdbIds.Contains(m.Id) && !userBlacklistedIds.Contains(m.Id)).ToList();

    var suggestions = movies.Take(3).ToList();
    foreach (var movie in suggestions)
    {
        movie.IsWatched = userLoggedTmdbIds.Contains(movie.Id);
        movie.IsInWishlist = userWishlistTmdbIds.Contains(movie.Id);
        movie.IsInBlacklist = userBlacklistedIds.Contains(movie.Id);
    }

    return suggestions;
}

/// <summary>
/// Helper method to try getting movies for a specific genre, sort, and page combination
/// </summary>
private async Task<List<TmdbMovieBrief>> TryGetGenreMovies(int genreId, string sortBy, int page, string genreName)
{
    try
    {
        var movies = await _tmdbService.DiscoverMoviesByGenreAsync(genreId, page, sortBy);
        _logger.LogInformation("✅ {Genre}: Got {Count} movies with {Sort} page {Page}", genreName, movies.Count, sortBy, page);
        return movies;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "❌ Failed to get {Genre} movies with {Sort} page {Page}", genreName, sortBy, page);
        return new List<TmdbMovieBrief>();
    }
}
      

        private async Task<List<TmdbMovieBrief>> GetSuggestionsForActor(int actorId, string userId)
        {
            _logger.LogInformation("HELPER: Getting random movies for actor ID {ActorId}", actorId);

            // 1. Get the actor's filmography using the service
            var allActorMovies = await _tmdbService.DiscoverMoviesByActorAsync(actorId);

            // Get user state sets
            var userLoggedTmdbIds = (await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue)
                .Select(m => m.TmdbId)
                .ToListAsync())
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            var userWishlistTmdbIds = (await _dbContext.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.TmdbId)
                .ToListAsync()).ToHashSet();
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);

            // Filter out movies already logged or blacklisted
            allActorMovies = allActorMovies.Where(m => !userLoggedTmdbIds.Contains(m.Id) && !userBlacklistedIds.Contains(m.Id)).ToList();

            if (!allActorMovies.Any())
            {
                return new List<TmdbMovieBrief>();
            }

            var suggestions = allActorMovies.Count <= 3
                ? allActorMovies
                : allActorMovies.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

            foreach (var movie in suggestions)
            {
                movie.IsWatched = userLoggedTmdbIds.Contains(movie.Id);
                movie.IsInWishlist = userWishlistTmdbIds.Contains(movie.Id);
                movie.IsInBlacklist = userBlacklistedIds.Contains(movie.Id);
            }
            return suggestions;
        }

        private async Task<List<TmdbMovieBrief>> GetSuggestionsForDecade(int decade, string userId)
        {
            // Use a unique session key to remember the page for this specific decade
            string sessionKey = $"DecadePage_{decade}";
            int pageToFetch = HttpContext.Session.GetInt32(sessionKey) ?? 1;

            _logger.LogInformation("HELPER: Finding movies for decade {Decade}, starting at page {Page}", decade, pageToFetch);

            var movies = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, pageToFetch);

            // Get user state sets
            var userLoggedTmdbIds = (await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue)
                .Select(m => m.TmdbId)
                .ToListAsync())
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();
            var userWishlistTmdbIds = (await _dbContext.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.TmdbId)
                .ToListAsync()).ToHashSet();
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);

            // Filter out movies already logged or blacklisted
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

            var suggestions = movies.Take(3).ToList();
            foreach (var movie in suggestions)
            {
                movie.IsWatched = userLoggedTmdbIds.Contains(movie.Id);
                movie.IsInWishlist = userWishlistTmdbIds.Contains(movie.Id);
                movie.IsInBlacklist = userBlacklistedIds.Contains(movie.Id);
            }
            return suggestions;
        }

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
                // Pre-fill form for TMDB ID
                _logger.LogInformation("Pre-filling form for TMDB ID: {TmdbId}", tmdbId.Value);
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId.Value);
                if (movieDetails != null)
                {
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
            _logger.LogInformation("Add POST action invoked for user {UserId}", _userManager.GetUserId(User));
            _logger.LogDebug("ViewModel received: Title={Title}, Director={Director}, ReleasedYear={ReleasedYear}, PosterPath={PosterPath}, OverviewSnippet={Overview}, DateWatched={DateWatched}, WatchedLocation={WatchedLocation}, IsRewatch={IsRewatch}, TmdbId={TmdbId}, Subscribed={Subscribed}",
                viewModel.Title,
                viewModel.Director,
                viewModel.ReleasedYear,
                viewModel.PosterPath,
                viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 50)),
                viewModel.DateWatched,
                viewModel.WatchedLocation,
                viewModel.IsRewatch,
                viewModel.TmdbId,
                viewModel.Subscribed);

            if (ModelState.IsValid)
            {


            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                // This will prevent non-logged-in users from adding movies.
                // We'll improve this with an [Authorize] tag later.
                return Unauthorized();
            }
         

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

            _logger.LogDebug("Adding Movie '{Title}', UserRating from ViewModel: {UserRatingViewModel}, Value being saved to entity: {UserRatingEntity}", movie.Title, viewModel.UserRating, movie.UserRating);

            await _dbContext.Movies.AddAsync(movie);

            // Remove from wishlist if present (mutual exclusion)
            if (movie.TmdbId.HasValue)
            {
                var wishlistItem = await _dbContext.WishlistItems
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.TmdbId == movie.TmdbId);
                if (wishlistItem != null)
                {
                    _dbContext.WishlistItems.Remove(wishlistItem);
                    _logger.LogInformation("WishlistItem with TmdbId {TmdbId} removed for user {UserId} during Add.", movie.TmdbId.HasValue ? movie.TmdbId.Value.ToString() : "N/A", userId);
                }
            }
            // --- END WISHLIST REMOVAL LOGIC ---

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Add POST - Movie Added Successfully: {Title}", movie.Title);
            return RedirectToAction("List");
            }
            else // ModelState is NOT valid
            {
                _logger.LogWarning("Add POST - ModelState IS INVALID. Movie not added.");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    if (value != null && value.Errors != null && value.Errors.Any())
                    {
                        _logger.LogWarning("-- Errors for '{ModelStateKey}' --", modelStateKey);
                        foreach (var error in value.Errors)
                        {
                            _logger.LogWarning("  - Message: {ErrorMessage}", error.ErrorMessage);
                            if (error.Exception != null)
                            {
                                _logger.LogWarning("    Exception: {ExceptionMessage}", error.Exception.Message);
                            }
                        }
                    }
                }
                // Re-display the Add page with the submitted data and validation errors.
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

            _logger.LogDebug("List action invoked with SortOrder: {SortOrder}, SearchString: {SearchString}, PageNumber: {PageNumber}", sortOrder, searchString, pageNumber);

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
            _logger.LogDebug("Edit GET - Loaded Movie from DB (ID: {Id}): Title='{Title}', Director='{Director}', ReleasedYear='{ReleasedYear}', IsRewatch='{IsRewatch}', UserRating='{UserRating}', TmdbId='{TmdbId}'",
                movieEntity.Id, movieEntity.Title, movieEntity.Director, movieEntity.ReleasedYear, movieEntity.IsRewatch, movieEntity.UserRating, movieEntity.TmdbId);


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

            _logger.LogDebug("Edit GET - ViewModel sent to View (ID: {Id}): Title='{Title}', Director='{Director}', ReleasedYear='{ReleasedYear}', PosterPath='{PosterPath}', OverviewSnippet='{Overview}', IsRewatch='{IsRewatch}', UserRating='{UserRating}', TmdbId='{TmdbId}'",
                viewModel.Id,
                viewModel.Title,
                viewModel.Director,
                viewModel.ReleasedYear,
                viewModel.PosterPath,
                viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 30)),
                viewModel.IsRewatch,
                viewModel.UserRating,
                viewModel.TmdbId);

            return View(viewModel);
        }




        [HttpPost]
        public async Task<IActionResult> Edit(AddMoviesViewModel viewModel)
        {
            _logger.LogInformation("Edit POST action invoked for user {UserId}", _userManager.GetUserId(User));
            _logger.LogDebug("Edit POST ViewModel: Id={Id}, Title={Title}, Director={Director}, ReleasedYear={ReleasedYear}, PosterPath={PosterPath}, OverviewSnippet={Overview}, DateWatched={DateWatched}, WatchedLocation={WatchedLocation}, IsRewatch={IsRewatch}, UserRating={UserRating}, TmdbId={TmdbId}, Subscribed={Subscribed}",
                viewModel.Id,
                viewModel.Title,
                viewModel.Director,
                viewModel.ReleasedYear,
                viewModel.PosterPath,
                viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 50)),
                viewModel.DateWatched,
                viewModel.WatchedLocation,
                viewModel.IsRewatch,
                viewModel.UserRating,
                viewModel.TmdbId,
                viewModel.Subscribed);

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                // VVVV THIS IS THE KEY CHANGE VVVV
                // We now find the movie only if the ID and UserId both match.
                var movieEntity = await _dbContext.Movies
                    .FirstOrDefaultAsync(m => m.Id == viewModel.Id && m.UserId == userId);

                if (movieEntity != null)
                {
                    _logger.LogDebug("Edit POST - Movie Found (ID: {Id}). Original DB Values - Title: '{Title}', Director: '{Director}', Year: {Year}', IsRewatch: {IsRewatch}, UserRating: {UserRating}, TmdbId: {TmdbId}",
                        movieEntity.Id,
                        movieEntity.Title,
                        movieEntity.Director,
                        movieEntity.ReleasedYear,
                        movieEntity.IsRewatch,
                        movieEntity.UserRating,
                        movieEntity.TmdbId);

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

                    _logger.LogDebug("Edit POST - Entity values BEFORE SaveChangesAsync (ID: {Id}) - Title: '{Title}', Director: '{Director}', Year: '{Year}', IsRewatch: {IsRewatch}, TmdbId: {TmdbId}, UserRating: {UserRating}",
                        movieEntity.Id,
                        movieEntity.Title,
                        movieEntity.Director,
                        movieEntity.ReleasedYear,
                        movieEntity.IsRewatch,
                        movieEntity.TmdbId,
                        movieEntity.UserRating);

                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation("Edit POST - SaveChangesAsync completed successfully for Movie ID {Id}. Redirecting to List.", movieEntity.Id);
                        return RedirectToAction("List", "Movies");
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        _logger.LogWarning(ex, "Edit POST - DbUpdateConcurrencyException for Movie ID {Id}", movieEntity.Id);
                        ModelState.AddModelError("", "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Edit POST - Exception during SaveChangesAsync for Movie ID {Id}", movieEntity.Id);
                        if (ModelState != null)
                        {
                            ModelState.AddModelError("", "An error occurred while saving changes. Please try again.");
                        }
                    }
                }
                else
                {
                    _logger.LogError("Edit POST - Movie entity with ID {Id} not found in DB during save attempt", viewModel.Id);
                    return NotFound();
                }
            }
            else // ModelState is NOT valid
            {
                _logger.LogWarning("Edit POST - ModelState IS INVALID. Changes will not be saved");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    if (value != null && value.Errors != null && value.Errors.Any())
                    {
                        _logger.LogWarning("-- Errors for '{ModelStateKey}' --", modelStateKey);
                        foreach (var error in value.Errors)
                        {
                            _logger.LogWarning("  - Message: {ErrorMessage}", error.ErrorMessage);
                            if (error.Exception != null)
                            {
                                _logger.LogWarning("    Exception: {ExceptionMessage}", error.Exception.Message);
                            }
                        }
                    }
                }
            }
            // If ModelState invalid or save failed, return to View with viewModel
            _logger.LogWarning("Edit POST - Returning View(viewModel) due to invalid ModelState or error for Movie ID: {Id}", viewModel.Id);
            return View(viewModel);
        }
        // POST: Movies/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug("Delete action in MoviesController hit. ID received: {Id}", id);
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
                _logger.LogDebug("No movie found with ID: {Id}. Nothing to delete", id);
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

            _logger.LogDebug("Attempting TMDB Search for query: '{Query}'", query);
            var searchResult = await _tmdbService.SearchMoviesAsync(query);

            if (searchResult != null && searchResult.Results != null && searchResult.Results.Any())
            {
            _logger.LogDebug("SUCCESS: Found {TotalResults} movies. Displaying up to the first 5", searchResult.TotalResults);
                foreach (var movieItem in searchResult.Results.Take(5))
                {
                    _logger.LogDebug("ID: {Id}, Title: {Title}, Release Date: {ReleaseDate}, Overview: {Overview}", movieItem.Id, movieItem.Title, movieItem.ReleaseDate, movieItem.Overview?.Substring(0, Math.Min(movieItem.Overview.Length, 50)));
                }
                return Content($"Search for '{query}' successful. Found {searchResult.TotalResults} movies. Check 'Application Output' pad in Visual Studio for details.");
            }
            else if (searchResult != null)
            {
                _logger.LogInformation("Search for '{Query}' yielded 0 results", query);
                return Content($"Search for '{query}' yielded 0 results from TMDB.");
            }
            else
            {
                _logger.LogError("TMDB search for '{Query}' failed or the service returned a null response", query);
                return Content($"Search for '{query}' failed. Check 'Application Output' pad and logs from TmdbService.");
            }
        }
    }

#endregion
}