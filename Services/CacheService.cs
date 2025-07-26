using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Ezequiel_Movies.Data;
using Ezequiel_Movies1.Models.Entities;

namespace Ezequiel_Movies.Services
{
    /// <summary>
    /// Centralized caching service for user-specific data to improve performance
    /// </summary>
    public class CacheService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

        public CacheService(ApplicationDbContext dbContext, IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets user blacklist IDs from cache or database
        /// </summary>
        public async Task<List<int>> GetUserBlacklistIdsAsync(string userId)
        {
            var cacheKey = $"BlacklistIds_{userId}";
            
            if (_memoryCache.TryGetValue(cacheKey, out List<int> cachedIds))
            {
                return cachedIds;
            }

            var ids = await _dbContext.BlacklistedMovies
                .Where(b => b.UserId == userId)
                .Select(b => b.TmdbId)
                .ToListAsync();

            _memoryCache.Set(cacheKey, ids, _cacheExpiration);
            return ids;
        }

        /// <summary>
        /// Gets user wishlist IDs from cache or database
        /// </summary>
        public async Task<List<int>> GetUserWishlistIdsAsync(string userId)
        {
            var cacheKey = $"WishlistIds_{userId}";
            
            if (_memoryCache.TryGetValue(cacheKey, out List<int> cachedIds))
            {
                return cachedIds;
            }

            var ids = await _dbContext.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.TmdbId)
                .ToListAsync();

            _memoryCache.Set(cacheKey, ids, _cacheExpiration);
            return ids;
        }

        /// <summary>
        /// Invalidates user blacklist cache
        /// </summary>
        public void InvalidateUserBlacklistCache(string userId)
        {
            var cacheKey = $"BlacklistIds_{userId}";
            _memoryCache.Remove(cacheKey);
        }

        /// <summary>
        /// Invalidates user wishlist cache
        /// </summary>
        public void InvalidateUserWishlistCache(string userId)
        {
            var cacheKey = $"WishlistIds_{userId}";
            _memoryCache.Remove(cacheKey);
        }
    }
}
