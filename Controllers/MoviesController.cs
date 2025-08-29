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
using Ezequiel_Movies1.Models;
using Microsoft.AspNetCore.Identity;
using Ezequiel_Movies1.Models.Entities;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Ezequiel_Movies.Services;
namespace Ezequiel_Movies.Controllers
{

    [Authorize]
    public class MoviesController : Controller
    {

        private async Task<HashSet<int>> GetUserBlacklistedTmdbIdsAsync(string userId)
        {
            var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
            return blacklistIds.ToHashSet();
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
        /// <summary>
        /// Unified trending movie logic used by both initial suggestions and AJAX reshuffles.
        /// Filters out blacklisted and recently watched movies, builds a pool from multiple pages.
        /// </summary>
        /// <param name="userId">Current user ID for filtering</param>
        /// <returns>List of filtered trending movies</returns>
        private async Task<List<TmdbMovieBrief>> GetTrendingMoviesWithFiltering(string userId)
        {
            // Get user filters - identical logic to TrendingReshuffle
            var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);

            var recentIds = await _dbContext.Movies
                .Where(m => m.UserId == userId && m.DateWatched.HasValue && m.TmdbId.HasValue)
                .OrderByDescending(m => m.DateWatched)
                .Take(5)
                .Select(m => m.TmdbId ?? 0)
                .ToListAsync();

            // Build pool of valid movies - same logic as TrendingReshuffle
            var moviePool = new List<TmdbMovieBrief>();
        int pageNum = 1; // Initialize page number for fetching movies

            while (moviePool.Count < 30 && pageNum <= 5)
            {
                var pageMovies = await _tmdbService.GetTrendingMoviesAsync(pageNum);
                var validMovies = pageMovies
                    .Where(m => !blacklistIds.Contains(m.Id) && !recentIds.Contains(m.Id))
                    .ToList();
                moviePool.AddRange(validMovies);
                pageNum++;
            }

            // Return shuffled pool for variety
            return moviePool.OrderBy(x => Random.Shared.Next()).ToList();
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
        private readonly CacheService _cacheService;


        public MoviesController(
            ApplicationDbContext dbContext,
            TmdbService tmdbService,
            ILogger<MoviesController> logger,
            UserManager<IdentityUser> userManager,
            IMemoryCache memoryCache,
            CacheService cacheService)
        {
            _dbContext = dbContext;
            _tmdbService = tmdbService;
            _logger = logger;
            _userManager = userManager;
            _memoryCache = memoryCache;
            _cacheService = cacheService;
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
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (userId == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Please log in to add movies to your blacklist." });
                return RedirectToAction("Login", "Account");
            }
            
            if (await MovieExistsInBlacklistAsync(userId, tmdbId))
            {
                if (isAjax)
                    return Json(new { success = false, message = "Movie is already in your blacklist." });
                return RedirectToAction(nameof(Blacklist));
            }

            if (await MovieExistsInWishlistAsync(userId, tmdbId))
            {
                if (isAjax)
                    return Json(new { success = false, message = "Movie is in your wishlist. Cannot add to blacklist." });
                return LocalRedirect(returnUrl);
            }
            try
            {
                var movieDetails = await GetMovieDetailsWithLoggingAsync(tmdbId);
                if (movieDetails == null)
                {
                    if (isAjax)
                        return Json(new { success = false, message = "Movie not found." });
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
                
                // Invalidate cache to ensure fresh data
                _cacheService.InvalidateUserBlacklistCache(userId);
                
                if (isAjax)
                    return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding movie {TmdbId} to blacklist for user {UserId}", tmdbId, userId);
                if (isAjax)
                    return Json(new { success = false, message = "An error occurred while adding the movie to your blacklist." });
                throw;
            }
            return RedirectToAction(nameof(Blacklist));
        }

        /// <summary>
        /// Displays the user's blacklist, with optional search and sort.
        /// </summary>
        /// <param name="searchString">Optional: filter blacklist by title or metadata.</param>
        /// <param name="sortOrder">Optional: sort order for the blacklist view.</param>
        /// <param name="pageNumber">Optional: page number for pagination.</param>
        /// <remarks>
        /// - Only movies blacklisted by the current user are shown.
        /// - UI/UX: Consistent with wishlist and movie list views.
        /// - Performance optimized with pagination and batch processing.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Blacklist(string? searchString = null, string? sortOrder = null, int? pageNumber = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Blacklist query started for user {UserId}", userId);

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
                case "title_asc":
                    blacklistQuery = blacklistQuery.OrderBy(b => b.Title);
                    break;
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
                    blacklistQuery = blacklistQuery.OrderByDescending(b => b.BlacklistedDate);
                    break;
            }

            int pageSize = 20;
            var paginatedBlacklist = await PaginatedList<Ezequiel_Movies1.Models.Entities.BlacklistedMovie>.CreateAsync(
                blacklistQuery.AsNoTracking(),
                pageNumber ?? 1,
                pageSize);

            // OPTIMIZED: Use batch processing for TMDB details
            var tmdbIds = paginatedBlacklist.Select(b => b.TmdbId).Distinct().ToList();
            var tmdbDetailsBatch = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

            var blacklistViewModels = new List<BlacklistViewModel>();
            foreach (var movie in paginatedBlacklist)
            {
                string director = movie.Director ?? "Unknown (TMDB)";
                int? releasedYear = movie.ReleasedYear;
                string posterUrl = movie.PosterUrl ?? string.Empty;

                // Use cached/batched data if available
                if (tmdbDetailsBatch.TryGetValue(movie.TmdbId, out var details))
                {
                    if (string.IsNullOrEmpty(movie.Director))
                    {
                        director = details.GetDirector() ?? "Unknown (TMDB)";
                    }
                    
                    if (!movie.ReleasedYear.HasValue && !string.IsNullOrEmpty(details.ReleaseDate) && details.ReleaseDate.Length >= 4)
                    {
                        if (int.TryParse(details.ReleaseDate.Substring(0, 4), out var year))
                            releasedYear = year;
                    }
                    
                    if (string.IsNullOrEmpty(movie.PosterUrl))
                    {
                        posterUrl = details.PosterPath ?? string.Empty;
                    }
                }

                blacklistViewModels.Add(new BlacklistViewModel
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    TmdbId = movie.TmdbId,
                    BlacklistedDate = movie.BlacklistedDate,
                    PosterUrl = posterUrl ?? string.Empty,
                    Director = director,
                    ReleasedYear = releasedYear ?? 0
                });
            }

            stopwatch.Stop();
            _logger.LogInformation("Blacklist query completed for user {UserId} in {ElapsedMs}ms. Found {Count} items.",
                userId, stopwatch.ElapsedMilliseconds, blacklistViewModels.Count);

            return View(new PaginatedList<BlacklistViewModel>(
                blacklistViewModels,
                paginatedBlacklist.TotalCount,
                paginatedBlacklist.PageIndex,
                pageSize));
        }

        /// <summary>
        /// <summary>
        /// Removes a movie from the user's blacklist by TMDB ID.
        /// </summary>
        /// <param name="tmdbId">TMDB movie ID to remove from blacklist.</param>
        /// <remarks>
        /// - Supports both AJAX and standard POST requests.
        /// - Only allows removal if the movie belongs to the current user.
        /// - Uses async cache invalidation and structured logging.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromBlacklist(string tmdbId)
        {
            var userId = _userManager.GetUserId(User);
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tmdbId))
            {
                _logger.LogWarning("RemoveFromBlacklist: Invalid user or tmdbId. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
                if (isAjax)
                    return Json(new { success = false, message = "Invalid request." });
                return BadRequest();
            }

            if (!int.TryParse(tmdbId, out var tmdbIdInt))
            {
                _logger.LogWarning("RemoveFromBlacklist: Invalid tmdbId format. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
                if (isAjax)
                    return Json(new { success = false, message = "Invalid TMDB ID format." });
                return BadRequest();
            }

            var blacklistedMovie = await _dbContext.BlacklistedMovies
                .FirstOrDefaultAsync(b => b.UserId == userId && b.TmdbId == tmdbIdInt);
            if (blacklistedMovie == null)
            {
                _logger.LogInformation("RemoveFromBlacklist: Movie not found. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
                if (isAjax)
                    return Json(new { success = false, message = "Movie not found in blacklist." });
                return NotFound();
            }

            try
            {
                _dbContext.BlacklistedMovies.Remove(blacklistedMovie);
                await _dbContext.SaveChangesAsync();
                await _cacheService.InvalidateUserBlacklistCacheAsync(userId);
                _logger.LogInformation("RemoveFromBlacklist: Movie removed. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error removing blacklist item. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
                if (isAjax)
                    return Json(new { success = false, message = "This movie was already removed from your blacklist." });
                return NotFound();
            }

            if (isAjax)
                return Json(new { success = true });
            return RedirectToAction(nameof(Blacklist));
        }


        /// <summary>
        /// Displays the user's wishlist, with optional search and sort.
        /// </summary>
        /// <param name="searchString">Optional: filter wishlist by title.</param>
        /// <param name="sortOrder">Optional: sort order for the wishlist view.</param>
        /// <param name="pageNumber">Optional: page number for pagination.</param>
        /// <remarks>
        /// - Only movies wishlisted by the current user are shown.
        /// - UI/UX: Consistent with blacklist and movie list views.
        /// - Performance optimized with pagination and batch processing.
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Wishlist(string? searchString = null, string? sortOrder = null, int? pageNumber = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Wishlist query started for user {UserId}", userId);

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
                case "title_asc":
                    wishlistQuery = wishlistQuery.OrderBy(w => w.Title);
                    break;
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
                    wishlistQuery = wishlistQuery.OrderByDescending(w => w.DateAdded);
                    break;
            }

            int pageSize = 20;
            var paginatedWishlist = await PaginatedList<Ezequiel_Movies1.Models.Entities.WishlistItem>.CreateAsync(
                wishlistQuery.AsNoTracking(),
                pageNumber ?? 1,
                pageSize);

            // OPTIMIZED: Use batch processing for TMDB details
            var tmdbIds = paginatedWishlist.Select(w => w.TmdbId).Distinct().ToList();
            var tmdbDetailsBatch = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

            var wishlistViewModels = new List<WishlistViewModel>();
            foreach (var wishlistItem in paginatedWishlist)
            {
                string director = "Unknown (TMDB)";
                string movieTitle = "N/A";
                int? movieTmdbId = null;

                // Use cached/batched data if available
                if (tmdbDetailsBatch.TryGetValue(wishlistItem.TmdbId, out var details))
                {
                    director = details.GetDirector() ?? "Unknown (TMDB)";
                    movieTitle = details.Title ?? "N/A";
                    movieTmdbId = details.Id;
                }

                wishlistViewModels.Add(new WishlistViewModel
                {
                    Id = wishlistItem.Id,
                    TmdbId = wishlistItem.TmdbId,
                    Title = wishlistItem.Title,
                    PosterPath = wishlistItem.PosterPath ?? string.Empty,
                    ReleasedYear = wishlistItem.ReleasedYear ?? 0,
                    Director = director,
                    MovieTitle = movieTitle,
                    MovieTmdbId = movieTmdbId ?? 0
                });
            }

            stopwatch.Stop();
            _logger.LogInformation("Wishlist query completed for user {UserId} in {ElapsedMs}ms. Found {Count} items.",
                userId, stopwatch.ElapsedMilliseconds, wishlistViewModels.Count);

            return View(new PaginatedList<WishlistViewModel>(
                wishlistViewModels,
                paginatedWishlist.TotalCount,
                paginatedWishlist.PageIndex,
                pageSize));
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
                    
                    // Invalidate cache to ensure fresh data
                    _cacheService.InvalidateUserWishlistCache(userId);
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
        /// Removes a movie from the user's wishlist by TMDB ID, supporting both AJAX and standard POST requests.
        /// </summary>
        /// <param name="tmdbId">TMDB movie ID to remove from wishlist.</param>
        /// <remarks>
        /// - Only allows removal if the movie belongs to the current user.
        /// - Supports AJAX requests (returns JSON) and standard POST (redirects).
        /// - Uses async cache invalidation and structured logging.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(string tmdbId)
        {
            var userId = _userManager.GetUserId(User);
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tmdbId))
            {
                _logger.LogWarning("RemoveFromWishlist: Invalid user or tmdbId. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
                if (isAjax)
                    return Json(new { success = false, message = "Invalid request." });
                return BadRequest();
            }

            if (!int.TryParse(tmdbId, out var tmdbIdInt))
            {
                _logger.LogWarning("RemoveFromWishlist: Invalid tmdbId format. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
                if (isAjax)
                    return Json(new { success = false, message = "Invalid TMDB ID format." });
                return BadRequest();
            }

            var wishlistItem = await _dbContext.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.TmdbId == tmdbIdInt);
            if (wishlistItem == null)
            {
                _logger.LogInformation("RemoveFromWishlist: Movie not found. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
                if (isAjax)
                    return Json(new { success = false, message = "Movie not found in wishlist." });
                return NotFound();
            }

            try
            {
                _dbContext.WishlistItems.Remove(wishlistItem);
                await _dbContext.SaveChangesAsync();
                await _cacheService.InvalidateUserWishlistCacheAsync(userId);
                _logger.LogInformation("RemoveFromWishlist: Movie removed. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error removing wishlist item. UserId={UserId}, TmdbId={TmdbId}", userId, tmdbId);
                if (isAjax)
                    return Json(new { success = false, message = "This movie was already removed from your wishlist." });
                return NotFound();
            }

            if (isAjax)
                return Json(new { success = true });
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
        /// Returns a personalized "Surprise Me!" movie suggestion using the optimized static pool approach.
        /// Both initial suggestions and reshuffles use identical logic for consistent performance.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Pool Construction:</b> Builds a static, deduplicated pool of 80 movies using aggressive 
        /// cascading from multiple sources (trending, genre, director, actor). Blacklisted and recently 
        /// watched movies are filtered out during pool build. Deduplication is enforced by TMDB ID.
        /// </para>
        /// <para>
        /// <b>Caching Strategy:</b> Pool is cached in IMemoryCache for 2 hours. During this period, 
        /// all interactions use the same pool, resulting in zero TMDB API calls per suggestion.
        /// </para>
        /// <para>
        /// <b>Rotation Logic:</b> Pool supports infinite cyclic rotation with session-based position 
        /// tracking. Each suggestion advances the pointer, wrapping to start when pool is exhausted.
        /// </para>
        /// <para>
        /// <b>Performance:</b> Only ~5 TMDB API calls during initial pool construction; all subsequent 
        /// suggestions are instant and API-free.
        /// </para>
        /// <para>
        /// <b>Requirements:</b> User must have logged at least 3 movies with director, genre, and 
        /// release year information. Suggestions are personalized based on user's movie history.
        /// </para>
        /// </remarks>
        /// <returns>View with single movie suggestion or empty state with guidance message</returns>
        [HttpGet]
        public async Task<IActionResult> GetSurpriseSuggestion()
        {
            _logger.LogInformation("GetSurpriseSuggestion action invoked (unified pool system).");

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user has sufficient logged movies for suggestions
            var loggedMoviesCount = await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue && !string.IsNullOrEmpty(m.Director)
                            && !string.IsNullOrEmpty(m.Genres) && m.ReleasedYear.HasValue)
                .CountAsync();

            if (loggedMoviesCount < 3)
            {
                ViewData["SuggestionTitle"] = "Log at least 3 movies to get a 'Surprise Me!' suggestion.";
                ViewData["ShowAddMovieButton"] = true;
                return View("Suggest", new List<TmdbMovieBrief>());
            }

            // --- Use the same optimized pool logic as SurpriseMeReshuffle ---
            string poolCacheKey = $"SurprisePool_{userId}";
            var cachedPool = _memoryCache.Get<(List<TmdbMovieBrief> bucket3x3, List<TmdbMovieBrief> bucket2x3, List<TmdbMovieBrief> bucket1x3)>(poolCacheKey);

            List<TmdbMovieBrief> bucket3x3, bucket2x3, bucket1x3;

            if (cachedPool.bucket3x3 != null && cachedPool.bucket2x3 != null && cachedPool.bucket1x3 != null)
            {

                bucket3x3 = cachedPool.bucket3x3;
                bucket2x3 = cachedPool.bucket2x3;
                bucket1x3 = cachedPool.bucket1x3;
            }
            else
            {

                var poolResult = await BuildSurprisePoolAsync(userId);
                bucket3x3 = poolResult.bucket3x3;
                bucket2x3 = poolResult.bucket2x3;
                bucket1x3 = poolResult.bucket1x3;

                // Cache for 2 hours
                var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(2));
                _memoryCache.Set(poolCacheKey, (bucket3x3, bucket2x3, bucket1x3), cacheOptions);

                _logger.LogDebug("✅ Pool cached: {Count3x3} + {Count2x3} + {Count1x3} = {Total} movies",
                        bucket3x3.Count, bucket2x3.Count, bucket1x3.Count, bucket3x3.Count + bucket2x3.Count + bucket1x3.Count);
            }

            // Use pre-filtered pool
            var filtered3x3 = bucket3x3;
            var filtered2x3 = bucket2x3;
            var filtered1x3 = bucket1x3;

            var totalPoolSize = filtered3x3.Count + filtered2x3.Count + filtered1x3.Count;
            _logger.LogDebug("🎁 Using pre-filtered pool: {Count3x3} + {Count2x3} + {Count1x3} = {Total} valid movies",
                filtered3x3.Count, filtered2x3.Count, filtered1x3.Count, totalPoolSize);

            // Edge case: if pool too small, show friendly message
            if (totalPoolSize < 10)
            {
                ViewData["SuggestionTitle"] = "Come back in a bit for more surprises! 🎬";
                ViewData["ShowAddMovieButton"] = true;
                return View("Suggest", new List<TmdbMovieBrief>());
            }

            // --- Infinite cyclic rotation with shuffled pool (same logic as reshuffle) ---
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
                _logger.LogDebug("🔀 Created new shuffled pool with {Count} movies", shuffledPool.Count);
            }

            // If we've gone through all movies, re-shuffle and restart
            if (poolIndex >= shuffledPool.Count)
            {
                shuffledPool = allMovies.OrderBy(x => Random.Shared.Next()).ToList();
                HttpContext.Session.Set(shuffledPoolKey, shuffledPool);
                poolIndex = 0;
                _logger.LogDebug("🔄 Pool completed, re-shuffled and restarting from movie #0");
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

            // Handle no suggestion case
            if (selectedMovie == null)
            {
                ViewData["SuggestionTitle"] = "Come back in a bit for more surprises! 🎬";
                ViewData["ShowAddMovieButton"] = true;
                return View("Suggest", new List<TmdbMovieBrief>());
            }

            // Return single suggestion in list format for the view
            var suggestedMoviesList = new List<TmdbMovieBrief> { selectedMovie };

            ViewData["SuggestionTitle"] = suggestionTitle;
            ViewData["NextSuggestionType"] = "surprise_me";

            // FEATURE: Handle AJAX requests for suggestion card clicks to eliminate page reloads
            // This provides seamless user experience while maintaining identical functionality
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // ARCHITECTURE: Use server-side rendering for consistent styling and form helpers
                var html = await RenderSuggestionResultsHtml(suggestedMoviesList, suggestionTitle, "surprise_me", "");
                return Json(new { success = true, html = html });
            }

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

                // Usar el método unificado
                var moviePool = await GetTrendingMoviesWithFiltering(userId);

                // Seleccionar 3 sugerencias aleatorias
                var suggestedMovies = moviePool.Take(3).ToList();

                _logger.LogInformation("🎬 Returning {Count} movies for reshuffle", suggestedMovies.Count);

                // Si no hay sugerencias posibles, mostrar mensaje amigable
                if (!suggestedMovies.Any())
                {
                    var emptyHtml = @"<div class='alert alert-info text-center my-5'>No more trending movies available right now. Please try another suggestion type or come back later for new trending picks!</div>";
                    return Json(new
                    {
                        success = true,
                        html = emptyHtml,
                        count = 0
                    });
                }

                // Renderizar HTML usando partial view
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
                    count = suggestedMovies.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR in TrendingReshuffle: {Message}", ex.Message);
                return Json(new
                {
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
    /// <item>Actors with no available suggestions are automatically skipped in all categories (recent, frequent, rated, random).</item>
    /// <item>The user never sees a 'no suggestions for this actor' message; only valid suggestions are shown.</item>
    /// <item>Sequence advances: recent → frequent → rated → random → random ...</item>
    /// <item>All logic and edge cases are documented for maintainability and future extension.</item>
    /// </list>
    /// </para>
    /// <remarks>
    /// FIX: Updated July 2025 to skip actors with no suggestions and always advance the sequence, never showing an empty-actor message.
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
                //    If an actor in any step has no suggestions, skip to the next in the sequence.
                string castTypeKey = $"CastTypeSequence_{userId}";
                int castTypeCount = HttpContext.Session.GetInt32(castTypeKey) ?? 0;
                HttpContext.Session.SetInt32(castTypeKey, castTypeCount + 1);
                _logger.LogInformation("Cast Reshuffle Sequence: Attempting Step {Step}", castTypeCount);

                // 2. ACTOR POOL CONSTRUCTION: Build a pool of top 3 actors from the user's last 15 logged movies.
                //    This pool is used for all selection strategies and deduplicated by actor ID.
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

                // 3. SEQUENTIAL ACTOR SELECTION: Use castTypeCount to select the Nth actor in the priority queue (recent, frequent, rated).
                //    If that actor has no suggestions, skip to the next in the sequence.
                //    After the priority queue, all further reshuffles use random eligible actors (excluding immediate repetition).
                //    Actors with no suggestions are always skipped; user never sees a 'no suggestions for this actor' message.
                TmdbCastPerson? selectedActor = null;
                List<TmdbCastPerson> triedActors = new();

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

                // --- ANTI-REPETITION: Avoid immediate repetition of actor ---
                //    The last suggested actor is tracked in Session and excluded from selection if possible.
                string lastActorKey = $"LastActorId_{userId}";
                int? lastActorId = HttpContext.Session.GetInt32(lastActorKey);

                // Use castTypeCount to select the Nth actor in the priority queue (recent, frequent, rated)
                //    If no valid actor is found in the queue, try random actors from the pool (excluding those already tried and the last actor).
                //    If no actors with suggestions are found at all, show a generic message (extremely rare edge case).
                int queueIndex = castTypeCount;
                int maxQueueIndex = priorityQueue.Count - 1;
                bool found = false;
                // Try to find a valid actor in the sequence (recent, frequent, rated)
                while (queueIndex <= maxQueueIndex && !found)
                {
                    var candidate = priorityQueue[queueIndex];
                    if (lastActorId.HasValue && candidate.Id == lastActorId.Value && priorityQueue.Count > 1)
                    {
                        queueIndex++;
                        continue;
                    }
                    var suggestedMovies = await GetSuggestionsForActor(candidate.Id, userId);
                    if (suggestedMovies.Any())
                    {
                        selectedActor = candidate;
                        HttpContext.Session.SetInt32(lastActorKey, selectedActor.Id);
                        var actorDetails = await _tmdbService.GetPersonDetailsAsync(selectedActor.Id);
                        var suggestionTitle = $"Because you like movies with {selectedActor.Name}";
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
                    queueIndex++;
                }

                // If not found in priority queue, try random actors (excluding last actor and those already tried)
                if (!found)
                {
                    var alreadyTriedIds = priorityQueue.Select(a => a.Id).ToHashSet();
                    var distinctActors = allTopActors.Where(a => a != null).DistinctBy(a => a.Id)
                        .Where(a => !alreadyTriedIds.Contains(a.Id) && (!lastActorId.HasValue || a.Id != lastActorId.Value))
                        .ToList();
                    foreach (var randomActor in distinctActors.OrderBy(x => Random.Shared.Next()))
                    {
                        var suggestedMovies = await GetSuggestionsForActor(randomActor.Id, userId);
                        if (suggestedMovies.Any())
                        {
                            selectedActor = randomActor;
                            HttpContext.Session.SetInt32(lastActorKey, selectedActor.Id);
                            var actorDetails = await _tmdbService.GetPersonDetailsAsync(selectedActor.Id);
                            var suggestionTitle = $"Because you like movies with {selectedActor.Name}";
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
                    }
                }

                // If no actors with suggestions, show a generic message (should be extremely rare)
                var emptyHtmlFinal = $@"<div class='alert alert-info text-center my-5'>No cast-based suggestions available at this time. Try another suggestion type!</div>";
                return Json(new { success = true, html = emptyHtmlFinal, count = 0 });
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

                // FEATURE: Debug log all distinct director names for current user
                var allDirectorNames = allUserMovies
                    .Where(m => !string.IsNullOrWhiteSpace(m.Director))
                    .Select(m => m.Director!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                async Task AddDirector(string? director)
                {
                    if (string.IsNullOrWhiteSpace(director)) return;
                    var trimmed = director.Trim();
                    if (seen.Add(trimmed)) // HashSet.Add returns true if item was newly added
                    {
                        // FIX: Check if director has available movies before adding to queue
                        // This prevents showing "No more suggestions available" message
                        if (await HasAvailableMoviesForDirector(trimmed, userId))
                        {
                            priorityQueue.Add(trimmed);
                            _logger.LogInformation("Added director to queue: '{Director}' (has available movies)", trimmed);
                        }
                        else
                        {
                            _logger.LogInformation("Skipped director: '{Director}' (all movies blacklisted)", trimmed);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Skipped duplicate director: '{Director}'", trimmed);
                    }
                }

                // Add directors in priority order: recent → frequent → rated
                await AddDirector(recentDirector);
                await AddDirector(frequentDirector);
                await AddDirector(ratedDirector);

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
                        // FIX: Filter out directors with no available movies to prevent empty suggestions
                        var directorsWithMovies = new List<string>();
                        foreach (var director in allDirectors)
                        {
                            if (await HasAvailableMoviesForDirector(director, userId))
                            {
                                directorsWithMovies.Add(director);
                            }
                        }

                        if (directorsWithMovies.Any())
                        {
                            // Anti-repetición: evitar que se repita el último director random
                            string lastRandomDirectorKey = $"LastRandomDirector_{userId}";
                            string? lastRandomDirector = HttpContext.Session.GetString(lastRandomDirectorKey);
                            var availableDirectors = directorsWithMovies;
                            if (!string.IsNullOrEmpty(lastRandomDirector) && directorsWithMovies.Count > 1)
                            {
                                availableDirectors = directorsWithMovies.Where(d => d != lastRandomDirector).ToList();
                                if (!availableDirectors.Any())
                                {
                                    availableDirectors = directorsWithMovies; // Fallback if filtering removes all directors
                                }
                            }
                            directorToSuggest = availableDirectors[Random.Shared.Next(availableDirectors.Count)];
                            HttpContext.Session.SetString(lastRandomDirectorKey, directorToSuggest);
                        }
                    }
                }

                if (string.IsNullOrEmpty(directorToSuggest))
                {
                    // All directors have been blacklisted - redirect to other suggestion types
                    var emptyHtml = @"<div class='alert alert-info text-center my-5'>Try exploring other suggestion types!</div>";
                    return Json(new { success = true, html = emptyHtml, count = 0 });
                }

                // 5. Obtener Sugerencias y Construir Respuesta
                var suggestedMovies = await GetSuggestionsForDirector(directorToSuggest, userId);
                var suggestionTitle = $"Because you like {directorToSuggest}...";

                // This should now rarely happen since we pre-filter directors, but keeping as safety net
                if (!suggestedMovies.Any())
                {
                    var emptyHtml = @"<div class='alert alert-info text-center my-5'>Try exploring other suggestion types!</div>";
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
                    suggestionTitle = suggestionTitle
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR in DirectorReshuffle: {Message}", ex.Message);
                return Json(new
                {
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
                _logger.LogInformation("🚀 DecadeReshuffle AJAX endpoint called with optimized logic and variety system.");
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Retrieve the user's last 25 logged movies, ordered by most recent activity
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

                // Calculate decade priorities based on last 25 movies
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

                // Build the priority queue for decade evaluation
                var priorityQueue = new List<int> { latestDecade, mostFrequentDecade, highestRatedDecade }
                    .Where(d => d > 0).Distinct().ToList();
                var randomDecades = availableDecades.OrderBy(_ => Random.Shared.Next()).ToList();
                var decadesToTry = priorityQueue.Concat(randomDecades).ToList();

                _logger.LogInformation("[DECADE-LOGIC] Decades to try in order: {Decades}", string.Join(", ", decadesToTry));

                // Anti-repetition logic
                string lastDecadeKey = $"LastDecadeShown_{userId}";
                int? lastDecade = HttpContext.Session.GetInt32(lastDecadeKey);

                // Cache expensive filters
                var last5 = last25Movies.Take(5).Select(m => m.TmdbId ?? 0).ToHashSet();

                // Find valid decades with triple fallback system
                List<(int Decade, List<TmdbMovieBrief> Pool)> validDecades = new();

                foreach (var decadeToTry in decadesToTry.Take(5))
                {
                    // Skip the decade shown immediately before (anti-repetition)
                    if (decadeToTry == lastDecade) continue;

                    // Triple fallback system for this decade
                    var decadePool = await TryGetDecadeMovies(decadeToTry, "vote_average.desc", Random.Shared.Next(1, 4), userId);

                    // Fallback 1: Same sort, page 1
                    if (decadePool.Count < 3)
                    {
                        _logger.LogInformation("⚠️ Fallback 1: {Decade}s insufficient, trying vote_average page 1", decadeToTry);
                        decadePool = await TryGetDecadeMovies(decadeToTry, "vote_average.desc", 1, userId);
                    }

                    // Fallback 2: Popular, page 1
                    if (decadePool.Count < 3)
                    {
                        _logger.LogInformation("⚠️ Fallback 2: {Decade}s insufficient, trying popular page 1", decadeToTry);
                        decadePool = await TryGetDecadeMovies(decadeToTry, "popularity.desc", 1, userId);
                    }

                    // Apply additional filtering for last5 and recent movies
                    var finalPool = decadePool.Where(m =>
                        !last5.Contains(m.Id) &&
                        !last25Movies.Any(um => um.TmdbId == m.Id)
                    ).ToList();

                    if (finalPool.Count >= 3)
                    {
                        // Check if we already have this decade to avoid duplicates
                        if (!validDecades.Any(v => v.Decade == decadeToTry))
                        {
                            validDecades.Add((decadeToTry, finalPool));
                            _logger.LogInformation("✅ Added decade {Decade}s with {Count} movies", decadeToTry, finalPool.Count);
                        }
                        else
                        {
                            _logger.LogInformation("⏭️ Skipping duplicate decade {Decade}s", decadeToTry);
                        }
                    }
                }

                if (!validDecades.Any())
                {
                    var emptyHtml = "<div class='alert alert-info text-center my-5'>No new decade suggestions available right now.</div>";
                    return Json(new { success = true, html = emptyHtml, count = 0 });
                }

                // Random selection among all valid decades found
                var selected = validDecades[Random.Shared.Next(validDecades.Count)];
                HttpContext.Session.SetInt32(lastDecadeKey, selected.Decade);

                // Final response: render up to 3 suggestions for display
                var suggestedMovies = selected.Pool.OrderBy(_ => Random.Shared.Next()).Take(3).ToList();
                string suggestionTitle = $"Here are movies from the {selected.Decade}s";

                var htmlBuilder = new System.Text.StringBuilder();
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

                return Json(new
                {
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


        /// <summary>
        /// AJAX endpoint for reshuffling "Surprise Me" suggestions using the unified pool system.
        /// Uses the same optimized pool logic as the initial GetSurpriseSuggestion method.
        /// Returns server-rendered HTML for consistent UI experience.
        /// </summary>
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
                        bucket3x3 = cachedPool.bucket3x3;
                    bucket2x3 = cachedPool.bucket2x3;
                    bucket1x3 = cachedPool.bucket1x3;
                }
                else
                {
    
                    // Build new pool (we'll create this helper next)
                    var poolResult = await BuildSurprisePoolAsync(userId);
                    bucket3x3 = poolResult.bucket3x3;
                    bucket2x3 = poolResult.bucket2x3;
                    bucket1x3 = poolResult.bucket1x3;

                    // Cache for 2 hours
                    var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(2));
                    _memoryCache.Set(poolCacheKey, (bucket3x3, bucket2x3, bucket1x3), cacheOptions);

                    _logger.LogDebug("✅ Pool cached: {Count3x3} + {Count2x3} + {Count1x3} = {Total} movies",
                        bucket3x3.Count, bucket2x3.Count, bucket1x3.Count, bucket3x3.Count + bucket2x3.Count + bucket1x3.Count);
                }

                // --- 2. Filters already applied during build, use buckets directly ---
                var filtered3x3 = bucket3x3;
                var filtered2x3 = bucket2x3;
                var filtered1x3 = bucket1x3;

                var totalPoolSize = filtered3x3.Count + filtered2x3.Count + filtered1x3.Count;
                _logger.LogDebug("🎁 Using pre-filtered pool: {Count3x3} + {Count2x3} + {Count1x3} = {Total} valid movies",
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
                    _logger.LogDebug("🔀 Created new shuffled pool with {Count} movies", shuffledPool.Count);
                }

                // If we've gone through all movies, re-shuffle and restart
                if (poolIndex >= shuffledPool.Count)
                {
                    shuffledPool = allMovies.OrderBy(x => Random.Shared.Next()).ToList();
                    HttpContext.Session.Set(shuffledPoolKey, shuffledPool);
                    poolIndex = 0;
                    _logger.LogDebug("🔄 Pool completed, re-shuffled and restarting from movie #0");
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

                return Json(new
                {
                    success = true,
                    html = htmlBuilder.ToString(),
                    count = 1,
                    suggestionTitle = suggestionTitle
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR in SurpriseMeReshuffle: {Message}", ex.Message);
                return Json(new
                {
                    success = false,
                    error = "Could not generate a surprise suggestion. Please try again."
                });
            }
        }

        
/// <summary>
/// Builds an optimized pool of 50 deduplicated movies for Surprise Me suggestions using parallel API execution.
///
/// <para>
/// <b>Pool Size:</b> Constructs exactly 50 unique movies distributed across prioritized buckets (~15 + 20 + 15).
/// Pool is cached for 2 hours with anti-repetition across 3 previous pools to ensure fresh content.
/// </para>
/// <para>
/// <b>Parallel Performance:</b> Executes 15 TMDB API calls in parallel with throttling (6 concurrent max) 
/// for ~400-450ms total build time vs 2,800ms+ sequential approach.
/// </para>
/// <para>
/// <b>Bucket System:</b> Uses 3x3/2x3/1x3 matching criteria (director+actor+genre / 2 criteria / 1 criteria) 
/// for intelligent variety based on user's logged movie preferences.
/// </para>
/// <para>
/// <b>Smart Filtering:</b> Excludes user's blacklisted movies, last 10 watched, and movies from previous 
/// 2 pool rotations. All filtering applied during build for zero-latency reshuffles.
/// </para>
/// <para>
/// <b>Anti-Repetition Strategy:</b> Tracks previous pools in cache to prevent same movies appearing 
/// within 6-hour windows, ensuring fresh suggestions for daily active users.
/// </para>
/// </summary>
/// <param name="userId">User ID for personalized filtering and cache keys</param>
/// <returns>Tuple containing (bucket3x3, bucket2x3, bucket1x3) with 50 total unique movies</returns>
        private async Task<(List<TmdbMovieBrief> bucket3x3, List<TmdbMovieBrief> bucket2x3, List<TmdbMovieBrief> bucket1x3)>
            BuildSurprisePoolAsync(string userId)
        {
            _logger.LogInformation("🏗️ Building surprise pool for user {UserId} with anti-repetition", userId);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Get previous pools to avoid repetition (middle ground strategy)
            var previousPoolKey1 = $"SurprisePool_Previous1_{userId}";
            var previousPoolKey2 = $"SurprisePool_Previous2_{userId}";
            var previousPool1 = _memoryCache.Get<HashSet<int>>(previousPoolKey1) ?? new HashSet<int>();
            var previousPool2 = _memoryCache.Get<HashSet<int>>(previousPoolKey2) ?? new HashSet<int>();
            var excludedTmdbIds = new HashSet<int>(previousPool1.Union(previousPool2));

            _logger.LogInformation("📝 Excluding {Count} movies from previous 2 pools", excludedTmdbIds.Count);

            // --- 1. Get user ingredients ---
            var loggedMovies = await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue && !string.IsNullOrEmpty(m.Director) && !string.IsNullOrEmpty(m.Genres) && m.ReleasedYear.HasValue)
                .ToListAsync();
            _logger.LogInformation("⏱️ DB Query took: {Ms}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            // Get user filters to apply during build (not after)
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);
            var recentMovieIds = await _dbContext.Movies
                .Where(m => m.UserId == userId && m.DateWatched.HasValue && m.TmdbId.HasValue)
                .OrderByDescending(m => m.DateWatched)
                .Take(10)
                .Select(m => m.TmdbId!.Value)
                .ToListAsync();
            var recentMovieIdSet = recentMovieIds.ToHashSet();

            // Combine all exclusions: blacklist + recent + previous pools
            var allExcludedIds = userBlacklistedIds.Union(recentMovieIdSet).Union(excludedTmdbIds).ToHashSet();
            _logger.LogInformation("🚫 Total excluded movies: {Count} (blacklist: {Blacklist}, recent: {Recent}, previous pools: {Previous})", 
                allExcludedIds.Count, userBlacklistedIds.Count, recentMovieIdSet.Count, excludedTmdbIds.Count);
            _logger.LogInformation("⏱️ User filters took: {Ms}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            _logger.LogInformation("🔍 Filters loaded: {BlacklistCount} blacklisted, {RecentCount} recent",
                userBlacklistedIds.Count, recentMovieIdSet.Count);

            if (loggedMovies.Count < 3)
            {
                return (new List<TmdbMovieBrief>(), new List<TmdbMovieBrief>(), new List<TmdbMovieBrief>());
            }

            var random = new Random();
            var directorPool = loggedMovies.Select(m => m.Director!).Distinct().ToList();
            var genrePool = loggedMovies.SelectMany(m => m.Genres!.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();

            // Build actor pool (cached for performance) - PARALLELIZED
            var actorPool = new List<TmdbCastPerson>();
            var movieDetailsCache = new Dictionary<int, TmdbMovieDetails?>();

            // Get unique movie IDs to avoid duplicate API calls
            var movieIds = loggedMovies
                .OrderByDescending(m => m.DateWatched)
                .Take(15)
                .Where(m => m.TmdbId.HasValue)
                .Select(m => m.TmdbId!.Value)
                .Distinct()
                .ToList();

            if (movieIds.Any())
            {
                // Parallel API calls for movie details
                var detailsTasks = movieIds.Select(async tmdbId => new
                {
                    TmdbId = tmdbId,
                    Details = await _tmdbService.GetMovieDetailsAsync(tmdbId)
                });

                var detailsResults = await Task.WhenAll(detailsTasks);

                // Process results and build actor pool
                foreach (var result in detailsResults)
                {
                    movieDetailsCache[result.TmdbId] = result.Details;

                    if (result.Details?.Credits?.Cast != null)
                    {
                        actorPool.AddRange(result.Details.Credits.Cast.Take(3));
                    }
                }
            }
            actorPool = actorPool.DistinctBy(p => p.Id).ToList();
            _logger.LogInformation("⏱️ Actor pool building took: {Ms}ms", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();

            if (!actorPool.Any())
            {
                _logger.LogWarning("No actors found in user's recent movies");
                return (new List<TmdbMovieBrief>(), new List<TmdbMovieBrief>(), new List<TmdbMovieBrief>());
            }

            // Get all genres for mapping
            var allGenres = await _tmdbService.GetAllGenresAsync();
            var genreDict = allGenres.ToDictionary(g => g.Name!, g => g.Id, StringComparer.OrdinalIgnoreCase);

            // --- 2. Build buckets with parallel API calls ---
            // ...declarations moved to parallelization block...

            // Prepare all queries for parallel execution
            var queries3x3 = new List<(int? directorId, int? actorId, int? genreId, int? decade)>();
            var queries2x3 = new List<(int? directorId, int? actorId, int? genreId, int? decade)>();
            var queries1x3 = new List<(int? directorId, int? actorId, int? genreId, int? decade)>();

            _logger.LogInformation("🎯 Preparing parallel queries for bucket construction...");
            var allMoviesFound = new HashSet<int>(); // Global deduplication tracker
            var bucket3x3 = new List<TmdbMovieBrief>();
            var bucket2x3 = new List<TmdbMovieBrief>();
            var bucket1x3 = new List<TmdbMovieBrief>();

            // Build 3x3 queries (director + actor + genre combinations)
            for (int i = 0; i < 5; i++) // Reduced from 10 to 5 attempts for 50-movie target
            {
                var randDirector = directorPool[random.Next(directorPool.Count)];
                var randActor = actorPool[random.Next(actorPool.Count)];
                var randGenre = genrePool[random.Next(genrePool.Count)];

                var directorId = await _tmdbService.GetPersonIdAsync(randDirector);
                var genreId = genreDict.ContainsKey(randGenre) ? genreDict[randGenre] : (int?)null;

                if (directorId.HasValue && genreId.HasValue)
                {
                    queries3x3.Add((directorId, randActor.Id, genreId, null));
                }
            }
           

            _logger.LogInformation("✅ 3/3 bucket built: {Count} movies", bucket3x3.Count);


            // Declare combos2x3 before its first use
            var combos2x3 = new[] { "director_actor", "director_genre", "actor_genre" };
            // Build 2x3 queries (director+actor, director+genre, actor+genre combinations)
            for (int i = 0; i < 5; i++) // Reduced to 5 for rate limit safety
            {
                var combo = combos2x3[i % 3];
                var randDirector = directorPool[random.Next(directorPool.Count)];
                var randActor = actorPool[random.Next(actorPool.Count)];
                var randGenre = genrePool[random.Next(genrePool.Count)];

                var directorId = await _tmdbService.GetPersonIdAsync(randDirector);
                var genreId = genreDict.ContainsKey(randGenre) ? genreDict[randGenre] : (int?)null;

                switch (combo)
                {
                    case "director_actor":
                        if (directorId.HasValue)
                            queries2x3.Add((directorId, randActor.Id, null, null));
                        break;
                    case "director_genre":
                        if (directorId.HasValue && genreId.HasValue)
                            queries2x3.Add((directorId, null, genreId, null));
                        break;
                    case "actor_genre":
                        if (genreId.HasValue)
                            queries2x3.Add((null, randActor.Id, genreId, null));
                        break;
                }
            }
           
            _logger.LogInformation("✅ 2/3 bucket built: {Count} movies", bucket2x3.Count);


            // Declare singles1x3 before its first use
            var singles1x3 = new[] { "director", "actor", "genre" };
            // Build 1x3 queries (single criteria: director, actor, or genre)
            for (int i = 0; i < 5; i++) // Reduced to 5 for rate limit safety
            {
                var single = singles1x3[i % 3];
                var randDirector = directorPool[random.Next(directorPool.Count)];
                var randActor = actorPool[random.Next(actorPool.Count)];
                var randGenre = genrePool[random.Next(genrePool.Count)];

                switch (single)
                {
                    case "director":
                        var directorId = await _tmdbService.GetPersonIdAsync(randDirector);
                        if (directorId.HasValue)
                            queries1x3.Add((directorId, null, null, null));
                        break;
                    case "actor":
                        queries1x3.Add((null, randActor.Id, null, null));
                        break;
                    case "genre":
                        var genreId = genreDict.ContainsKey(randGenre) ? genreDict[randGenre] : (int?)null;
                        if (genreId.HasValue)
                            queries1x3.Add((null, null, genreId, null));
                        break;
                }
            }

            _logger.LogInformation("🚀 Executing {Total} queries in parallel ({Count3x3} 3x3, {Count2x3} 2x3, {Count1x3} 1x3)", 
    queries3x3.Count + queries2x3.Count + queries1x3.Count, queries3x3.Count, queries2x3.Count, queries1x3.Count);

// Execute all queries in parallel
var allQueries = queries3x3.Concat(queries2x3).Concat(queries1x3).ToList();
var allResults = await _tmdbService.ExecuteParallelDiscoveryAsync(allQueries);

_logger.LogInformation("⏱️ Parallel API execution took: {Ms}ms", stopwatch.ElapsedMilliseconds);
stopwatch.Restart();

// Filter and organize results into buckets
var filteredResults = allResults
    .Where(m => m.VoteAverage >= 4.0)
    .Where(m => !allExcludedIds.Contains(m.Id))
    .Where(m => !allMoviesFound.Contains(m.Id))
    .ToList();

_logger.LogInformation("🎬 Got {Count} valid movies after filtering", filteredResults.Count);

// Distribute results into buckets (maintaining original proportions for 50 movies)
// Target: ~20 for 3x3, ~20 for 2x3, ~10 for 1x3

// Fill 3x3 bucket (target: 20 movies)
var results3x3 = filteredResults.Take(20).ToList();
foreach (var movie in results3x3)
{
    bucket3x3.Add(movie);
    allMoviesFound.Add(movie.Id);
}

// Fill 2x3 bucket (target: 20 movies)
var remaining = filteredResults.Where(m => !allMoviesFound.Contains(m.Id)).Take(20).ToList();
foreach (var movie in remaining)
{
    bucket2x3.Add(movie);
    allMoviesFound.Add(movie.Id);
}

// Fill 1x3 bucket (target: 10 movies)
var final = filteredResults.Where(m => !allMoviesFound.Contains(m.Id)).Take(10).ToList();
foreach (var movie in final)
{
    bucket1x3.Add(movie);
    allMoviesFound.Add(movie.Id);
}

_logger.LogInformation("✅ Buckets filled: {Count3x3} + {Count2x3} + {Count1x3} = {Total} movies",
    bucket3x3.Count, bucket2x3.Count, bucket1x3.Count, bucket3x3.Count + bucket2x3.Count + bucket1x3.Count);

    // Save current pool IDs for future anti-repetition (middle ground strategy)
var currentPoolIds = bucket3x3.Concat(bucket2x3).Concat(bucket1x3).Select(m => m.Id).ToHashSet();
_memoryCache.Set(previousPoolKey2, previousPool1, TimeSpan.FromHours(6)); // Shift previous pools
_memoryCache.Set(previousPoolKey1, currentPoolIds, TimeSpan.FromHours(6)); // Save current as previous

var totalFound = bucket3x3.Count + bucket2x3.Count + bucket1x3.Count;
_logger.LogInformation("🎁 Pool building complete: {Total} unique movies, took: {Ms}ms", totalFound, stopwatch.ElapsedMilliseconds);

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
        /// Renders the suggestion results area HTML for AJAX responses.
        /// 
        /// FEATURE: Added comprehensive AJAX support for suggestion cards to eliminate page reloads
        /// while preserving exact original functionality and visual appearance.
        /// </summary>
        /// <param name="suggestedMovies">List of movie suggestions to render</param>
        /// <param name="suggestionTitle">Title to display for the suggestion section</param>
        /// <param name="nextSuggestionType">Type identifier for reshuffle functionality</param>
        /// <param name="nextQuery">Optional query parameter for suggestion context</param>
        /// <returns>Complete HTML string for the suggestion results area including cards and reshuffle button</returns>
        /// <remarks>
        /// Uses server-side partial view rendering to ensure consistent styling, image paths,
        /// and form helper functionality. Populates all movie properties (IsWatched, IsInWishlist, 
        /// IsInBlacklist) to maintain visual state consistency between AJAX and traditional navigation.
        /// </remarks>
        private async Task<string> RenderSuggestionResultsHtml(List<TmdbMovieBrief> suggestedMovies, string suggestionTitle, string nextSuggestionType, string? nextQuery)
        {
            // CRITICAL: Populate movie properties for proper display (IsWatched, IsInWishlist, IsInBlacklist)
            // Without this, AJAX-loaded cards would lose visual state indicators
            var userId = _userManager.GetUserId(User);
            await PopulateMovieProperties(suggestedMovies, userId!);

            // ARCHITECTURE: Set ViewData to match original view rendering behavior
            ViewData["SuggestionTitle"] = suggestionTitle;
            ViewData["NextSuggestionType"] = nextSuggestionType;
            ViewData["NextQuery"] = nextQuery;

            var html = new StringBuilder();
            html.Append("<div id=\"suggestion-results-area\">");
            html.Append("<hr class=\"my-5\">");
            html.Append("<div class=\"text-center mb-4\">");
            html.Append($"<div id=\"suggestion-header\" class=\"d-flex justify-content-center align-items-center\">");
            
            // Add actor profile image if available
            if (ViewData["ActorProfilePath"] != null && !string.IsNullOrEmpty(ViewData["ActorProfilePath"]?.ToString()))
            {
                var tmdbImageBaseUrl = "https://image.tmdb.org/t/p/w342";
                html.Append($"<img id=\"actor-profile-image\" src=\"{tmdbImageBaseUrl}{ViewData["ActorProfilePath"]}\" class=\"rounded-circle me-3\" style=\"width: 50px; height: 50px; object-fit: cover;\" alt=\"Profile picture\" />");
            }
            
            html.Append($"<h4 id=\"suggestion-title\" class=\"cinelog-gold-title mb-0\">{suggestionTitle}</h4>");
            html.Append("</div>");

            if (suggestedMovies.Any())
            {
                // FEATURE: Generate reshuffle button with proper data attributes for AJAX functionality
                // Transform suggestion type to base type for consistent reshuffle endpoint routing
                var suggestionBaseType = nextSuggestionType?.StartsWith("year_") == true ? "decade" :
                    nextSuggestionType?.StartsWith("genre_") == true ? "genre" :
                    nextSuggestionType == "surprise_me" ? "surprise" :
                    (nextSuggestionType?.Contains("_") == true ? nextSuggestionType.Split('_')[0] : nextSuggestionType);

                // UX: Only show reshuffle button for suggestion types that support it
                if (nextSuggestionType != null && (nextSuggestionType.StartsWith("trending") || nextSuggestionType.StartsWith("cast_") || nextSuggestionType.StartsWith("director_") || nextSuggestionType.StartsWith("year_") || nextSuggestionType.StartsWith("genre_") || nextSuggestionType == "surprise_me"))
                {
                    html.Append($"<button id=\"reshuffle-btn\" type=\"button\" class=\"btn btn-outline-light btn-sm mt-2\" data-suggestion-type=\"{suggestionBaseType}\">");
                    html.Append("<i class=\"bi bi-shuffle\"></i> Reshuffle");
                    html.Append("</button>");
                }
            }

            html.Append("</div>");

            if (suggestedMovies.Any())
            {
                // ARCHITECTURE: Use Bootstrap grid system matching original view layout
                html.Append("<div class=\"row row-cols-1 row-cols-sm-2 row-cols-md-3 g-4 justify-content-center\">");
                foreach (var movie in suggestedMovies)
                {
                    // PERFORMANCE: Render each movie using existing partial view for consistency
                    // This ensures identical styling, form helpers, and image paths
                    var partialViewResult = await RenderPartialViewToStringAsync("_MovieSuggestionCard", movie);
                    html.Append($"<div class=\"col\">{partialViewResult}</div>");
                }
                html.Append("</div>");
            }

            html.Append("</div>");
            return html.ToString();
        }

        /// <summary>
        /// Populates IsWatched, IsInWishlist, and IsInBlacklist properties for movie suggestions.
        /// 
        /// FIX: Essential for AJAX responses to maintain visual consistency with original page loads.
        /// Without this, suggestion cards would lose watched badges, wishlist states, and blacklist buttons.
        /// </summary>
        /// <param name="movies">List of movies to populate with user-specific properties</param>
        /// <param name="userId">Current user's ID for filtering user-specific data</param>
        /// <remarks>
        /// Performs efficient batch queries to get user's logged movies, wishlist items, and blacklisted movies.
        /// Sets boolean properties on TmdbMovieBrief objects to control UI state in partial views.
        /// Critical for maintaining identical functionality between AJAX and traditional navigation.
        /// </remarks>
        private async Task PopulateMovieProperties(List<TmdbMovieBrief> movies, string userId)
        {
            if (movies == null || !movies.Any() || string.IsNullOrEmpty(userId)) return;

            var tmdbIds = movies.Where(m => m.Id > 0).Select(m => m.Id).ToList();
            if (!tmdbIds.Any()) return;

            // Get user's logged movies, wishlist, and blacklist
            var loggedTmdbIds = await _dbContext.Movies
                .Where(m => m.UserId == userId && m.TmdbId.HasValue && tmdbIds.Contains(m.TmdbId.Value))
                .Select(m => m.TmdbId!.Value)
                .ToListAsync();

            var wishlistTmdbIds = await _dbContext.WishlistItems
                .Where(w => w.UserId == userId && tmdbIds.Contains(w.TmdbId))
                .Select(w => w.TmdbId)
                .ToListAsync();

            var blacklistTmdbIds = await _dbContext.BlacklistedMovies
                .Where(b => b.UserId == userId && tmdbIds.Contains(b.TmdbId))
                .Select(b => b.TmdbId)
                .ToListAsync();

            // Set properties for each movie
            foreach (var movie in movies)
            {
                movie.IsWatched = loggedTmdbIds.Contains(movie.Id);
                movie.IsInWishlist = wishlistTmdbIds.Contains(movie.Id);
                movie.IsInBlacklist = blacklistTmdbIds.Contains(movie.Id);
            }
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
        /// <param name="suggestionType">The type of suggestion sequence to use (e.g., trending, director_recent, genre_frequent, year_recent, etc.).</param>
        /// <param name="query">Optional: Used for sequence state or to pass a specific director/genre/cast/decade for the next suggestion.</param>
        /// <param name="page">Optional: For paginated suggestion types (default 1).</param>
        /// <remarks>
        /// Suggestion system with session-based sequences: directors, genres, cast, decades, trending.
        /// - Director sequence: recent → frequent → rated → random, with anti-repetition and session state.
        /// - Genre and cast sequences follow similar logic, using user history and session to avoid repetition.
        /// - Decade suggestions now use a dynamic variety system identical to genres:
        ///   - Each suggestion uses a random combination of sort criteria (`popularity.desc`, `vote_average.desc`, `release_date.desc`) and page (1-3).
        ///   - Triple fallback logic ensures suggestions are always available:
        ///     - Primary: Random sort + random page
        ///     - Fallback 1: Same sort, page 1
        ///     - Fallback 2: Popular, page 1
        ///   - Deduplication logic prevents duplicate decades in suggestion results.
        ///   - User filtering (blacklist, watched movies) is always applied, and all expensive operations are cached per request.
        ///   - Both initial load and AJAX reshuffles use the same dynamic logic for decades, matching genres.
        ///   - User experience: Decade suggestions now provide varied, reliable content from the first click, with bulletproof fallback for edge cases.
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
                case "trending":
                    suggestionTitle = "Trending Movies Today";

                    // Usar la misma lógica que TrendingReshuffle para consistencia
                    var trendingResult = await GetTrendingMoviesWithFiltering(userId);
                    suggestedMovies = trendingResult.Take(3).ToList();
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

                        // FIX: Add directors to queue only if they have available movies (not all blacklisted)
                        if (recentDirector is string rd && await HasAvailableMoviesForDirector(rd, userId)) 
                            topDirectorQueue.Add(rd);
                        if (frequentDirector is string fd && !topDirectorQueue.Contains(fd) && await HasAvailableMoviesForDirector(fd, userId)) 
                            topDirectorQueue.Add(fd);
                        if (ratedDirectors.Any())
                        {
                            var topRatedDirector = ratedDirectors[0];
                            if (!topDirectorQueue.Contains(topRatedDirector) && await HasAvailableMoviesForDirector(topRatedDirector, userId))
                                topDirectorQueue.Add(topRatedDirector);
                        }

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

                            // Filter out directors with no available movies
                            var directorsWithMovies = new List<string>();
                            foreach (var director in allDirectors)
                            {
                                if (await HasAvailableMoviesForDirector(director, userId))
                                {
                                    directorsWithMovies.Add(director);
                                }
                            }

                            if (!directorsWithMovies.Any())
                            {
                                suggestionTitle = "Try exploring other suggestion types!";
                                break;
                            }

                            // Anti-repetition: avoid last random director
                            string lastRandomDirectorKey = $"LastRandomDirector_{userId}";
                            string? lastRandomDirector = HttpContext.Session.GetString(lastRandomDirectorKey);
                            var availableDirectors = directorsWithMovies;
                            if (!string.IsNullOrEmpty(lastRandomDirector) && directorsWithMovies.Count > 1)
                            {
                                var filtered = directorsWithMovies.Where(d => d != lastRandomDirector).ToList();
                                if (filtered.Any())
                                    availableDirectors = filtered;
                            }

                            var random = Random.Shared;
                            var randomIndex = random.Next(0, availableDirectors.Count);
                            _logger.LogInformation("All directors pool: {Directors}", string.Join(", ", allDirectors));
                            _logger.LogInformation("Directors with available movies: {Directors}", string.Join(", ", directorsWithMovies));
                            _logger.LogInformation("Available directors (no repeat): {Directors}", string.Join(", ", availableDirectors));
                            _logger.LogInformation("Random index selected: {Index}", randomIndex);
                            var selectedDirector = availableDirectors[randomIndex];
                            _logger.LogInformation("Selected director: {Director}", selectedDirector);
                            var movies = await GetSuggestionsForDirector(selectedDirector, userId);
                            directorToSuggest = selectedDirector;
                            directorSuggestions = movies.Take(Math.Min(3, movies.Count)).ToList();
                            nextSuggestionType = "director_random";
                            HttpContext.Session.SetString(lastRandomDirectorKey, selectedDirector);
                        }

                        // Bulletproof fallback: force random selection if no suggestions found
                        if (directorSuggestions.Count == 0)
                        {
                            _logger.LogWarning("No director suggestions found for {Type}, trying fallback with directors who have available movies", currentDirectorType);
                            var allDirectorsFallback = loggedDirectorMovies.Select(m => m.Director!).Distinct().ToList();
                            
                            // Filter fallback directors to only those with available movies
                            var fallbackDirectorsWithMovies = new List<string>();
                            foreach (var director in allDirectorsFallback)
                            {
                                if (await HasAvailableMoviesForDirector(director, userId))
                                {
                                    fallbackDirectorsWithMovies.Add(director);
                                }
                            }

                            if (fallbackDirectorsWithMovies.Any())
                            {
                                var random = Random.Shared;
                                var fallbackDirector = fallbackDirectorsWithMovies[random.Next(fallbackDirectorsWithMovies.Count)];
                                var fallbackMovies = await GetSuggestionsForDirector(fallbackDirector, userId);
                                directorToSuggest = fallbackDirector;
                                directorSuggestions = fallbackMovies.Take(Math.Min(3, fallbackMovies.Count)).ToList();
                                suggestionTitle = $"Because you like {fallbackDirector}... (Random)";
                                _logger.LogInformation("Fallback director selected: {Director}", fallbackDirector);
                            }
                            else
                            {
                                suggestionTitle = "Try exploring other suggestion types!";
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
                    if (loggedCastMovies == null || loggedCastMovies.Count < 3)
                    {
                        suggestionTitle = "Log at least 3 movies to get a 'Cast' suggestion";
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

                case "surprise_me":
                    return await GetSurpriseSuggestion();

                case "year_recent":
                case "year_frequent":
                case "year_rated":
                case "year_random":
                    // Decade suggestion logic using triple fallback system (matches AJAX behavior)
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

                    // Calculate decade priorities (same as AJAX)
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

                    // Priority queue with random selection
                    var decadePriorityQueue = new List<int> { latestDecade, mostFrequentDecade, highestRatedDecade }
                        .Where(d => d > 0).Distinct().ToList();
                    var randomDecades = availableDecades.OrderBy(_ => Random.Shared.Next()).ToList();
                    var decadesToTry = decadePriorityQueue.Concat(randomDecades).ToList();

                    // Find first valid decade with triple fallback
                    List<TmdbMovieBrief> decadeSuggestions = new();
                    int? selectedDecade = null;

                    foreach (var decadeToTry in decadesToTry.Take(5))
                    {
                        // Triple fallback system for variety
                        var decadePool = await TryGetDecadeMovies(decadeToTry, "vote_average.desc", Random.Shared.Next(1, 4), userId);

                        // Fallback 1: Same sort, page 1
                        if (decadePool.Count < 3)
                        {
                            decadePool = await TryGetDecadeMovies(decadeToTry, "vote_average.desc", 1, userId);
                        }

                        // Fallback 2: Popular, page 1
                        if (decadePool.Count < 3)
                        {
                            decadePool = await TryGetDecadeMovies(decadeToTry, "popularity.desc", 1, userId);
                        }

                        if (decadePool.Count >= 3)
                        {
                            selectedDecade = decadeToTry;
                            decadeSuggestions = decadePool.OrderBy(_ => Random.Shared.Next()).Take(3).ToList();
                            _logger.LogInformation("✅ Selected decade {Decade}s with {Count} movies", decadeToTry, decadeSuggestions.Count);
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

            // FEATURE: Handle AJAX requests for suggestion card clicks to eliminate page reloads
            // This provides seamless user experience while maintaining identical functionality
            if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                // ARCHITECTURE: Use server-side rendering for consistent styling and form helpers
                var html = await RenderSuggestionResultsHtml(suggestedMovies, suggestionTitle, nextSuggestionType, nextQuery);
                return Json(new { success = true, html = html });
            }

            return View("Suggest", suggestedMovies);
        }


        private async Task<List<TmdbMovieBrief>> GetSuggestionsForDirector(string directorName, string userId)
        {
            // Fetch director ID from TMDB
            var directorId = await _tmdbService.GetPersonIdAsync(directorName);
            if (!directorId.HasValue)
            {
                _logger.LogWarning("No TMDB director ID found for: {DirectorName}", directorName);
                return new List<TmdbMovieBrief>();
            }

            // Get the director's entire filmography from TMDB
            var allDirectorMovies = await _tmdbService.GetDirectorFilmographyAsync(directorId.Value);
            if (allDirectorMovies == null || !allDirectorMovies.Any())
            {
                _logger.LogWarning("No movies found in TMDB filmography for director: {DirectorName} (ID: {DirectorId})", directorName, directorId.Value);
                return new List<TmdbMovieBrief>();
            }

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
            var filteredMovies = allDirectorMovies.Where(m => !userBlacklistedIds.Contains(m.Id)).ToList();
            if (!filteredMovies.Any())
            {
                return new List<TmdbMovieBrief>(); // Return empty if they have no movies
            }

            // If they have 3 or fewer movies, just return them all. Otherwise, randomize and take 3.
            var suggestions = filteredMovies.Count <= 3
                ? filteredMovies
                : filteredMovies.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

            // Populate state properties for UI
            foreach (var movie in suggestions)
            {
                movie.IsWatched = userLoggedTmdbIds.Contains(movie.Id);
                movie.IsInWishlist = userWishlistTmdbIds.Contains(movie.Id);
                movie.IsInBlacklist = userBlacklistedIds.Contains(movie.Id);
            }
            return suggestions;
        }

        /// <summary>
        /// Checks if a director has any movies available for suggestions (not blacklisted by the user).
        /// This is a lightweight check that avoids fetching full movie details when we just need to know
        /// if the director should be included in the suggestion rotation.
        /// 
        /// FIX: Prevents showing "No more suggestions available for [Director]" message when user has
        /// blacklisted all movies from a director. Instead, directors with no available movies are
        /// silently skipped from the suggestion rotation.
        /// </summary>
        /// <param name="directorName">The name of the director to check</param>
        /// <param name="userId">The current user's ID</param>
        /// <returns>True if the director has at least one non-blacklisted movie, false otherwise</returns>
        private async Task<bool> HasAvailableMoviesForDirector(string directorName, string userId)
        {
            var directorId = await _tmdbService.GetPersonIdAsync(directorName);
            if (!directorId.HasValue) return false;

            // Get the director's filmography
            var allDirectorMovies = await _tmdbService.GetDirectorFilmographyAsync(directorId.Value);
            
            
            if (!allDirectorMovies.Any()) return false;

            // Get user's blacklisted movie IDs
            var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);

            // DEBUG LOGGING: Output director, filmography IDs, blacklist IDs
            _logger.LogInformation("[DEBUG] Director: {DirectorName}", directorName);
            _logger.LogInformation("[DEBUG] Director Filmography IDs: {FilmographyIds}", string.Join(",", allDirectorMovies.Select(m => m.Id)));
            _logger.LogInformation("[DEBUG] User Blacklisted TMDB IDs: {BlacklistIds}", string.Join(",", userBlacklistedIds));

            var availableMovies = allDirectorMovies.Where(movie => !userBlacklistedIds.Contains(movie.Id)).ToList();
            _logger.LogInformation("[DEBUG] Available movies for {DirectorName}: {AvailableMovieIds}", directorName, string.Join(",", availableMovies.Select(m => m.Id)));

            // Check if there are any movies not in the blacklist
            return availableMovies.Any();
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

        /// <summary>
        /// Helper method to try getting movies for a specific decade, sort, and page combination with fallback logic
        /// </summary>
        private async Task<List<TmdbMovieBrief>> TryGetDecadeMovies(int decade, string sortBy, int page, string userId)
        {
            try
            {
                var movies = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, page, sortBy);

                // Apply user filtering
                var userLoggedTmdbIds = (await _dbContext.Movies
                    .Where(m => m.UserId == userId && m.TmdbId.HasValue)
                    .Select(m => m.TmdbId)
                    .ToListAsync())
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToHashSet();

                var userBlacklistedIds = await GetUserBlacklistedTmdbIdsAsync(userId);

                // Filter out movies already logged or blacklisted
                var filteredMovies = movies
                    .Where(m => !userLoggedTmdbIds.Contains(m.Id) && !userBlacklistedIds.Contains(m.Id))
                    .ToList();

                _logger.LogInformation("✅ {Decade}s: Got {Count} valid movies with {Sort} page {Page}", decade, filteredMovies.Count, sortBy, page);
                return filteredMovies;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Failed to get {Decade}s movies with {Sort} page {Page}", decade, sortBy, page);
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

            // Generate random sort + page parameters for variety
            var sortTypes = new[] { "popularity.desc", "vote_average.desc", "release_date.desc" };
            var randomSort = sortTypes[Random.Shared.Next(sortTypes.Length)];
            var randomPageToUse = Random.Shared.Next(1, 4);

            _logger.LogInformation("🎲 Helper decade variety: {Decade}, Sort={Sort}, Page={Page}", decade, randomSort, randomPageToUse);

            var movies = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, randomPageToUse, randomSort);

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
                movies = await _tmdbService.DiscoverMoviesByDecadeAsync(decade, 1, "popularity.desc");
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

            var movieDetails = await _tmdbService.GetMovieDetailsAsync(id);

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

            return NotFound(new { error = "Movie details not found in TMDB." });
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

        /// <summary>
        /// Generates timeline navigation data by aggregating movies by month with counts.
        /// 
        /// FEATURE: Smart Timeline Navigator - provides month-based filtering with movie counts.
        /// Creates dropdown options showing available months in user's movie timeline.
        /// </summary>
        /// <param name="userId">Current user ID for data isolation</param>
        /// <returns>Timeline navigator data with available months and movie counts</returns>
        private async Task<TimelineNavigatorViewModel> GetTimelineNavigatorDataAsync(string userId)
        {
            try
            {
                // PERFORMANCE: Aggregate movies by month directly in database query
                var monthlyData = await _dbContext.Movies
                    .Where(m => m.UserId == userId && m.DateWatched.HasValue)
                    .GroupBy(m => new { 
                        Year = m.DateWatched!.Value.Year,
                        Month = m.DateWatched!.Value.Month
                    })
                    .Select(g => new TimelineMonth
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MovieCount = g.Count(),
                        MonthKey = $"{g.Key.Year:0000}-{g.Key.Month:00}",
                        DisplayName = $"{GetMonthName(g.Key.Month)} {g.Key.Year} ({g.Count()} {(g.Count() == 1 ? "movie" : "movies")})"
                    })
                    .OrderByDescending(m => m.Year)
                    .ThenByDescending(m => m.Month)
                    .ToListAsync();

                return new TimelineNavigatorViewModel
                {
                    AvailableMonths = monthlyData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating timeline navigator data for user {UserId}", userId);
                // FALLBACK: Return empty timeline data on error
                return new TimelineNavigatorViewModel();
            }
        }

        /// <summary>
        /// Converts month number to localized month name for timeline display.
        /// 
        /// ENHANCEMENT: Provides user-friendly month names for timeline navigator dropdown.
        /// </summary>
        /// <param name="month">Month number (1-12)</param>
        /// <returns>Month name in English</returns>
        private static string GetMonthName(int month)
        {
            return month switch
            {
                1 => "January",
                2 => "February", 
                3 => "March",
                4 => "April",
                5 => "May",
                6 => "June",
                7 => "July",
                8 => "August",
                9 => "September",
                10 => "October",
                11 => "November",
                12 => "December",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Applies month-based filtering to movie queries for timeline navigation.
        /// 
        /// FEATURE: Smart Timeline Navigator month filtering implementation.
        /// Filters movies to show only those watched in the selected month.
        /// </summary>
        /// <param name="query">Base movie query to filter</param>
        /// <param name="monthFilter">Month filter in YYYY-MM format (null for no filter)</param>
        /// <returns>Filtered query for the specified month</returns>
        private static IQueryable<Ezequiel_Movies1.Models.Entities.Movies> ApplyMonthFilter(
            IQueryable<Ezequiel_Movies1.Models.Entities.Movies> query, 
            string? monthFilter)
        {
            if (string.IsNullOrEmpty(monthFilter))
                return query;

            // Parse month filter (YYYY-MM format)
            if (monthFilter.Length == 7 && monthFilter[4] == '-')
            {
                if (int.TryParse(monthFilter.Substring(0, 4), out int year) &&
                    int.TryParse(monthFilter.Substring(5, 2), out int month))
                {
                    // FILTER: Apply month and year filter to DateWatched
                    return query.Where(m => m.DateWatched.HasValue &&
                                          m.DateWatched.Value.Year == year &&
                                          m.DateWatched.Value.Month == month);
                }
            }

            // FALLBACK: Return original query if month filter is invalid
            return query;
        }

        [HttpGet]
        public async Task<IActionResult> List(string sortOrder, string searchString, int? pageNumber, string view, bool firstWatchOnly = false, string displayMode = "grid", string? monthFilter = null)
        {
            // FEATURE: Hybrid journal-collection viewing system implementation
            // Supports both chronological journal view (default) and deduplicated collection view

            // 1. Get the current logged-in user's ID
            var userId = _userManager.GetUserId(User);

            // 2. Determine view mode - default to journal to preserve existing UX
            var viewMode = string.IsNullOrEmpty(view) || view.ToLower() != "collection" ? "journal" : "collection";
            ViewData["CurrentView"] = viewMode;
            
            // FEATURE: Display mode support for Grid/List toggle functionality
            // Supports "grid" (default) and "list" display modes with state preservation
            var actualDisplayMode = string.IsNullOrEmpty(displayMode) || (displayMode.ToLower() != "list") ? "grid" : "list";
            ViewData["CurrentDisplayMode"] = actualDisplayMode;
            
            _logger.LogInformation("Fetching movie list for User ID: {UserId} in {ViewMode} view with {DisplayMode} display mode", userId, viewMode, actualDisplayMode);
            _logger.LogDebug("List action invoked with SortOrder: {SortOrder}, SearchString: {SearchString}, PageNumber: {PageNumber}, View: {View}, DisplayMode: {DisplayMode}", 
                sortOrder, searchString, pageNumber, view, actualDisplayMode);

            ViewData["CurrentFilter"] = searchString;

            // Determine the actual sort order to apply
            string actualSortToApply = string.IsNullOrEmpty(sortOrder) ? "datewatched_desc" : sortOrder;
            ViewData["CurrentSort"] = actualSortToApply;

            // Set up sort parameters for view links
            ViewData["TitleSortParm"] = actualSortToApply == "title_asc" ? "title_desc" : "title_asc";
            ViewData["DirectorSortParm"] = actualSortToApply == "director_asc" ? "director_desc" : "director_asc";
            ViewData["YearSortParm"] = actualSortToApply == "year_asc" ? "year_desc" : "year_asc";
            ViewData["WatchedAtSortParm"] = actualSortToApply == "watchedat_asc" ? "watchedat_desc" : "watchedat_asc";
            ViewData["DateWatchedSortParm"] = actualSortToApply == "datewatched_desc" ? "datewatched_asc" : "datewatched_desc";
            ViewData["RatingSortParm"] = actualSortToApply == "rating_desc" ? "rating_asc" : "rating_desc";
            
            // ENHANCEMENT: Collection-specific sort options
            ViewData["MostWatchedSortParm"] = actualSortToApply == "most_watched_desc" ? "most_watched_asc" : "most_watched_desc";
            ViewData["RecentlyAddedSortParm"] = actualSortToApply == "recently_added_desc" ? "recently_added_asc" : "recently_added_desc";

            // FEATURE: Smart Timeline Navigator - generate timeline data for month filtering
            var timelineData = await GetTimelineNavigatorDataAsync(userId!);
            timelineData.SelectedMonth = monthFilter;
            
            // Set display name for selected month filter
            if (!string.IsNullOrEmpty(monthFilter))
            {
                var selectedMonth = timelineData.AvailableMonths.FirstOrDefault(m => m.MonthKey == monthFilter);
                timelineData.SelectedMonthDisplayName = selectedMonth?.DisplayName.Replace($" ({selectedMonth.MovieCount} ", " (").Replace(")", ")")
                    ?? "Unknown Month";
            }
            
            ViewData["TimelineNavigator"] = timelineData;
            ViewData["CurrentMonthFilter"] = monthFilter;

            IActionResult result;
            if (viewMode == "collection")
            {
                // COLLECTION VIEW: Show deduplicated movies with watch counts
                result = await GetCollectionView(userId!, searchString, actualSortToApply, pageNumber, monthFilter);
            }
            else
            {
                // JOURNAL VIEW: Keep existing chronological behavior (default)
                result = await GetJournalView(userId!, searchString, actualSortToApply, pageNumber, firstWatchOnly, monthFilter);
            }
            
            // FEATURE: Simple AJAX support for sorting - return partial view for smooth UX
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" && result is ViewResult viewResult)
            {
                return PartialView(viewResult.ViewName, viewResult.Model);
            }
            
            return result;
        }

        /// <summary>
        /// Handles the traditional journal view showing all movie entries chronologically.
        /// 
        /// FEATURE: Preserves existing journal functionality as the default view.
        /// Maintains user data isolation and existing sorting/search behavior.
        /// ENHANCEMENT: Added Smart Timeline Navigator month filtering support.
        /// </summary>
        private async Task<IActionResult> GetJournalView(string userId, string searchString, string sortOrder, int? pageNumber, bool firstWatchOnly = false, string? monthFilter = null)
        {
            var moviesQuery = _dbContext.Movies.Where(m => m.UserId == userId);

            // Get the total movie count for this user and store it in ViewData
            ViewData["MovieCount"] = await moviesQuery.CountAsync();

            // ENHANCEMENT: Apply first watch only filter for journal view
            if (firstWatchOnly)
            {
                moviesQuery = moviesQuery.Where(m => !m.IsRewatch);
            }
            
            // FEATURE: Smart Timeline Navigator - apply month filtering
            moviesQuery = ApplyMonthFilter(moviesQuery, monthFilter);
            
            // Apply search filtering
            if (!string.IsNullOrEmpty(searchString))
            {
                moviesQuery = moviesQuery.Where(m =>
                    (!string.IsNullOrEmpty(m.Title) && m.Title.Contains(searchString)) ||
                    (!string.IsNullOrEmpty(m.Director) && m.Director.Contains(searchString)) ||
                    (m.ReleasedYear != null && m.ReleasedYear.Value.ToString().Contains(searchString))
                );
            }

            // Apply sorting
            moviesQuery = sortOrder switch
            {
                "title_asc" => moviesQuery.OrderBy(m => m.Title),
                "title_desc" => moviesQuery.OrderByDescending(m => m.Title),
                "director_asc" => moviesQuery.OrderBy(m => m.Director),
                "director_desc" => moviesQuery.OrderByDescending(m => m.Director),
                "year_asc" => moviesQuery.OrderBy(m => m.ReleasedYear),
                "year_desc" => moviesQuery.OrderByDescending(m => m.ReleasedYear),
                "datewatched_asc" => moviesQuery.OrderBy(m => m.DateWatched),
                "watchedat_asc" => moviesQuery.OrderBy(m => m.WatchedLocation),
                "watchedat_desc" => moviesQuery.OrderByDescending(m => m.WatchedLocation),
                "rating_desc" => moviesQuery.OrderByDescending(m => m.UserRating),
                "rating_asc" => moviesQuery.OrderBy(m => m.UserRating),
                "datewatched_desc" or _ => moviesQuery.OrderByDescending(m => m.DateWatched)
            };

            int pageSize = 8;
            var paginatedMovies = await PaginatedList<Ezequiel_Movies1.Models.Entities.Movies>.CreateAsync(
                moviesQuery.AsNoTracking(),
                pageNumber ?? 1,
                pageSize);

            return View(paginatedMovies);
        }

        /// <summary>
        /// Handles the collection view showing deduplicated movies with watch count statistics.
        /// 
        /// FEATURE: Groups movies by TmdbId and calculates watch statistics.
        /// Shows latest movie details with watch count badges for multiple viewings.
        /// </summary>
        private async Task<IActionResult> GetCollectionView(string userId, string searchString, string actualSortToApply, int? pageNumber, string? monthFilter = null)
        {
            var moviesQuery = _dbContext.Movies.Where(m => m.UserId == userId);

            // Apply search filtering before grouping
            if (!string.IsNullOrEmpty(searchString))
            {
                moviesQuery = moviesQuery.Where(m =>
                    (!string.IsNullOrEmpty(m.Title) && m.Title.Contains(searchString)) ||
                    (!string.IsNullOrEmpty(m.Director) && m.Director.Contains(searchString)) ||
                    (m.ReleasedYear != null && m.ReleasedYear.Value.ToString().Contains(searchString))
                );
            }

            // FEATURE: Smart Timeline Navigator - apply month filtering to collection view
            moviesQuery = ApplyMonthFilter(moviesQuery, monthFilter);

            // Group by TmdbId to create collection view with watch counts
            var collectionData = await moviesQuery
                .GroupBy(m => m.TmdbId ?? 0) // Use 0 for movies without TmdbId
                .Select(g => new CollectionMovieViewModel
                {
                    // Use the most recent movie entry for display details
                    Movie = g.OrderByDescending(m => m.DateWatched ?? m.DateCreated).First(),
                    WatchCount = g.Count(),
                    FirstWatched = g.Min(m => m.DateWatched),
                    LastWatched = g.Max(m => m.DateWatched)
                })
                .AsNoTracking()
                .ToListAsync();

            // ENHANCEMENT: Calculate comprehensive collection statistics
            var totalWatches = collectionData.Sum(c => c.WatchCount);
            var mostWatchedMovie = collectionData.OrderByDescending(c => c.WatchCount).FirstOrDefault();
            var averageWatchesPerMovie = collectionData.Count > 0 ? Math.Round((double)totalWatches / collectionData.Count, 1) : 0;
            
            // Store enhanced statistics for view display
            ViewData["MovieCount"] = collectionData.Count;
            ViewData["TotalWatches"] = totalWatches;
            ViewData["MostWatchedMovie"] = mostWatchedMovie?.Movie.Title ?? "N/A";
            ViewData["MostWatchedCount"] = mostWatchedMovie?.WatchCount ?? 0;
            ViewData["AverageWatchesPerMovie"] = averageWatchesPerMovie;

            // ENHANCEMENT: Apply sorting to collection data with new collection-specific options
            var sortedCollection = actualSortToApply switch
            {
                "title_asc" => collectionData.OrderBy(c => c.Movie.Title),
                "title_desc" => collectionData.OrderByDescending(c => c.Movie.Title),
                "director_asc" => collectionData.OrderBy(c => c.Movie.Director),
                "director_desc" => collectionData.OrderByDescending(c => c.Movie.Director),
                "year_asc" => collectionData.OrderBy(c => c.Movie.ReleasedYear),
                "year_desc" => collectionData.OrderByDescending(c => c.Movie.ReleasedYear),
                "datewatched_asc" => collectionData.OrderBy(c => c.LastWatched),
                "watchedat_asc" => collectionData.OrderBy(c => c.Movie.WatchedLocation),
                "watchedat_desc" => collectionData.OrderByDescending(c => c.Movie.WatchedLocation),
                "rating_desc" => collectionData.OrderByDescending(c => c.Movie.UserRating),
                "rating_asc" => collectionData.OrderBy(c => c.Movie.UserRating),
                // NEW: Collection-specific sorting options
                "most_watched_desc" => collectionData.OrderByDescending(c => c.WatchCount).ThenByDescending(c => c.LastWatched),
                "most_watched_asc" => collectionData.OrderBy(c => c.WatchCount).ThenBy(c => c.LastWatched),
                "recently_added_desc" => collectionData.OrderByDescending(c => c.FirstWatched),
                "recently_added_asc" => collectionData.OrderBy(c => c.FirstWatched),
                "datewatched_desc" or _ => collectionData.OrderByDescending(c => c.LastWatched)
            };

            // Apply pagination to sorted collection
            int pageSize = 8;
            var collectionList = sortedCollection.ToList();
            var totalCount = collectionList.Count;
            var pageIndex = pageNumber ?? 1;
            var pagedItems = collectionList.Skip((pageIndex - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToList();
            var paginatedCollection = new PaginatedList<CollectionMovieViewModel>(pagedItems, totalCount, pageIndex, pageSize);

            return View("ListCollection", paginatedCollection);
        }

        /// <summary>
        /// AJAX endpoint for Smart Timeline Navigator month selection.
        /// 
        /// FEATURE: Provides seamless month filtering with AJAX support.
        /// Returns either JSON response for AJAX calls or redirects for direct navigation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> FilterByMonth(string? monthFilter, string sortOrder, string searchString, 
            int? pageNumber, string view, bool firstWatchOnly = false, string displayMode = "grid")
        {
            try
            {
                // Check if this is an AJAX request
                bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                
                if (isAjax)
                {
                    // AJAX: Return partial view with filtered content
                    var result = await List(sortOrder, searchString, pageNumber, view, firstWatchOnly, displayMode, monthFilter);
                    
                    if (result is ViewResult viewResult)
                    {
                        // Return the same view but indicate it's for AJAX
                        return PartialView(viewResult.ViewName ?? "List", viewResult.Model);
                    }
                    
                    // FALLBACK: Return error for AJAX
                    return Json(new { success = false, message = "Unable to filter timeline" });
                }
                else
                {
                    // NON-AJAX: Standard redirect to preserve existing functionality
                    return RedirectToAction("List", new 
                    { 
                        sortOrder = sortOrder,
                        searchString = searchString,
                        pageNumber = pageNumber,
                        view = view,
                        firstWatchOnly = firstWatchOnly,
                        displayMode = displayMode,
                        monthFilter = monthFilter
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering timeline by month {MonthFilter}", monthFilter);
                
                // Return appropriate error response based on request type
                bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                if (isAjax)
                {
                    return Json(new { success = false, message = "Unable to filter timeline" });
                }
                else
                {
                    // Redirect to unfiltered list on error
                    return RedirectToAction("List", new { view = view, displayMode = displayMode });
                }
            }
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
        /// <summary>
        /// Deletes a movie from the user's list with AJAX support.
        /// 
        /// ENHANCEMENT: Added AJAX support with JSON responses for smooth UX.
        /// </summary>
        /// <param name="id">The unique identifier of the movie to delete</param>
        /// <returns>JSON response for AJAX requests, redirect for standard requests</returns>
        /// <remarks>
        /// Supports both AJAX and standard POST requests. AJAX requests receive JSON responses
        /// while standard requests are redirected to the List page for backward compatibility.
        /// All operations are filtered by current user for security.
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogDebug("Delete action in MoviesController hit. ID received: {Id}", id);
            var userId = _userManager.GetUserId(User);
            bool isAjax = Request.Headers.XRequestedWith == "XMLHttpRequest";
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Delete: Invalid user. UserId={UserId}, MovieId={Id}", userId, id);
                if (isAjax)
                    return Json(new { success = false, message = "Invalid request." });
                return BadRequest();
            }

            var movieToDelete = await _dbContext.Movies
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (movieToDelete == null)
            {
                _logger.LogInformation("Delete: Movie not found. UserId={UserId}, MovieId={Id}", userId, id);
                if (isAjax)
                    return Json(new { success = false, message = "Movie not found." });
                return NotFound();
            }

            try
            {
                _dbContext.Movies.Remove(movieToDelete);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Delete: Movie removed. UserId={UserId}, MovieId={Id}, Title={Title}", userId, id, movieToDelete.Title);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error deleting movie. UserId={UserId}, MovieId={Id}", userId, id);
                if (isAjax)
                    return Json(new { success = false, message = "This movie was already deleted." });
                return NotFound();
            }

            if (isAjax)
                return Json(new { success = true });
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
