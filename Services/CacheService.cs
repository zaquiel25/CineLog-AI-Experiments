using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ezequiel_Movies.Data;
using Ezequiel_Movies1.Models.Entities;
using System.Diagnostics;

namespace Ezequiel_Movies.Services
{
    /// <summary>
    /// Centralized caching service for user-specific data to improve performance.
    /// Enhanced with Application Insights telemetry for monitoring cache effectiveness
    /// and validating recent 70-90% database performance improvements.
    /// </summary>
    public class CacheService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly CineLogTelemetryService? _telemetryService;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

        public CacheService(ApplicationDbContext dbContext, IMemoryCache memoryCache, CineLogTelemetryService? telemetryService = null)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _telemetryService = telemetryService;
        }

        /// <summary>
        /// Gets user blacklist IDs from cache or database with performance monitoring.
        /// Tracks cache hit rates and database query performance for optimization validation.
        /// </summary>
        public async Task<List<int>> GetUserBlacklistIdsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            
            var stopwatch = Stopwatch.StartNew();
            var cacheKey = $"BlacklistIds_{userId}";
            
            // Check cache first
            if (_memoryCache.TryGetValue(cacheKey, out List<int>? cachedIds) && cachedIds != null)
            {
                stopwatch.Stop();
                _telemetryService?.TrackCacheOperation("UserBlacklist", "Get", true, stopwatch.Elapsed);
                return cachedIds;
            }

            // Cache miss - query database
            var ids = await _dbContext.BlacklistedMovies
                .Where(b => b.UserId == userId)
                .Select(b => b.TmdbId)
                .ToListAsync();

            stopwatch.Stop();
            
            // Cache the results
            _memoryCache.Set(cacheKey, ids, _cacheExpiration);
            
            // Track cache miss and database performance
            _telemetryService?.TrackCacheOperation("UserBlacklist", "Set", false, stopwatch.Elapsed);
            _telemetryService?.TrackDatabaseQuery("BlacklistQuery", stopwatch.Elapsed, ids.Count, userId);
            
            return ids;
        }

        /// <summary>
        /// Gets user wishlist IDs from cache or database with performance monitoring.
        /// Tracks cache hit rates and database query performance for optimization validation.
        /// </summary>
        public async Task<List<int>> GetUserWishlistIdsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            
            var stopwatch = Stopwatch.StartNew();
            var cacheKey = $"WishlistIds_{userId}";
            
            // Check cache first
            if (_memoryCache.TryGetValue(cacheKey, out List<int>? cachedIds) && cachedIds != null)
            {
                stopwatch.Stop();
                _telemetryService?.TrackCacheOperation("UserWishlist", "Get", true, stopwatch.Elapsed);
                return cachedIds;
            }

            // Cache miss - query database
            var ids = await _dbContext.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.TmdbId)
                .ToListAsync();

            stopwatch.Stop();
            
            // Cache the results
            _memoryCache.Set(cacheKey, ids, _cacheExpiration);
            
            // Track cache miss and database performance
            _telemetryService?.TrackCacheOperation("UserWishlist", "Set", false, stopwatch.Elapsed);
            _telemetryService?.TrackDatabaseQuery("WishlistQuery", stopwatch.Elapsed, ids.Count, userId);
            
            return ids;
        }

        /// <summary>
        /// Invalidates user blacklist cache
        /// </summary>
        public void InvalidateUserBlacklistCache(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            var cacheKey = $"BlacklistIds_{userId}";
            _memoryCache.Remove(cacheKey);
        }

        /// <summary>
        /// Invalidates user wishlist cache
        /// </summary>
        public void InvalidateUserWishlistCache(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            var cacheKey = $"WishlistIds_{userId}";
            _memoryCache.Remove(cacheKey);
        }

        /// <summary>
        /// Invalidates user blacklist cache asynchronously
        /// </summary>
        public async Task InvalidateUserBlacklistCacheAsync(string userId)
        {
            InvalidateUserBlacklistCache(userId);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Invalidates user wishlist cache asynchronously
        /// </summary>
        public async Task InvalidateUserWishlistCacheAsync(string userId)
        {
            InvalidateUserWishlistCache(userId);
            await Task.CompletedTask;
        }
    }
}
