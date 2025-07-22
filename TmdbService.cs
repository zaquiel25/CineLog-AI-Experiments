using Microsoft.Extensions.Caching.Memory;

using Ezequiel_Movies.Models.TmdbApi;

namespace Ezequiel_Movies
{
    public class TmdbService
    {

        // --- TMDB Watch URL Simplification ---
        private const string TMDB_BASE_URL = "https://www.themoviedb.org";
        private readonly HashSet<string> _allowedRegions = new() { "IE", "US", "GB", "CA", "AU" };

        private string GetTmdbWatchUrl(int tmdbId, string region = "IE")
        {
            if (tmdbId <= 0) throw new ArgumentException("Invalid TMDB ID");
            if (string.IsNullOrWhiteSpace(region) || region.Length != 2 || !_allowedRegions.Contains(region.ToUpper()))
                region = "IE";
            var url = $"{TMDB_BASE_URL}/movie/{tmdbId}/watch?locale={region.ToUpper()}";
            _logger.LogInformation("TMDB watch URL generated for movie {TmdbId} in region {Region}", tmdbId, region);
            return url;
        }


        private readonly HttpClient _httpClient;
        private readonly ILogger<TmdbService> _logger;
        private static readonly Random _random = new Random();

        // Whitelist of allowed provider domains (add more as needed)
        private static readonly HashSet<string> AllowedProviderDomains = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "netflix.com",
            "disneyplus.com",
            "primevideo.com",
            "hulu.com",
            "hbomax.com",
            "apple.com",
            "paramountplus.com",
            "peacocktv.com",
            "mubi.com",
            "starz.com",
            "amcplus.com",
            "crunchyroll.com",
            "curiositystream.com",
            "shudder.com",
            "kanopy.com",
            "plex.tv",
            "youtube.com",
            "google.com",
            "vudu.com",
            "rakuten.tv",
            "nowtv.com",
            "itv.com",
            "bbc.co.uk",
            "all4.com",
            "iplayer.com",
            "viaplay.com",
            "viu.com",
            "viaplay.se",
            "viaplay.dk",
            "viaplay.no",
            "viaplay.fi",
            "viaplay.pl",
            "viaplay.is",
            "viaplay.lv",
            "viaplay.lt",
            "viaplay.ee",
            "viaplay.nl",
            "viaplay.de",
            "viaplay.com",
            // Add more as needed
        };

        // Helper: Validate and sanitize a provider link
        private static string? ValidateProviderLink(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
            if (uri.Scheme != Uri.UriSchemeHttps) return null; // Only allow HTTPS
            var host = uri.Host.StartsWith("www.") ? uri.Host.Substring(4) : uri.Host;
            if (!AllowedProviderDomains.Any(domain => host.EndsWith(domain, StringComparison.OrdinalIgnoreCase))) return null;
            return uri.ToString();
        }




        public async Task<TmdbMovieBrief?> GetRandomPopularMovieAsync()
        {
            _logger.LogInformation("Requesting a random popular movie for fallback.");
            try
            {
                int randomPage = _random.Next(1, 21); // Use the static random instance
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>("movie/popular?language=en-US&page="+randomPage);
                var movies = response?.Results;
                if (movies != null && movies.Any())
                {
                    return movies[_random.Next(movies.Count)]; // Use the static random instance again
                }
            }
            catch (Exception) { }
            return null;
        }
        // Gets the watch providers for a given movie from TMDB

        // In TmdbService.cs
        public async Task<List<TmdbMovieBrief>> DiscoverMoviesAsync(int? directorId, int? actorId, int? genreId, int? decade)
        {
            var queryBuilder = new System.Text.StringBuilder("discover/movie?language=en-US&sort_by=popularity.desc&vote_count.gte=100");
            if (directorId.HasValue) queryBuilder.Append($"&with_crew={directorId.Value}");
            if (actorId.HasValue) queryBuilder.Append($"&with_cast={actorId.Value}");
            if (genreId.HasValue) queryBuilder.Append($"&with_genres={genreId.Value}");
            if (decade.HasValue)
            {
                queryBuilder.Append($"&primary_release_date.gte={decade.Value}-01-01");
                queryBuilder.Append($"&primary_release_date.lte={decade.Value + 9}-12-31");
            }
            var requestUri = queryBuilder.ToString();
            _logger.LogInformation("DISCOVER: Requesting TMDB API: {RequestUri}", requestUri);
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(requestUri);
                return response?.Results ?? new List<TmdbMovieBrief>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed during discover API call for URI: {RequestUri}", requestUri);
                return new List<TmdbMovieBrief>();
            }
        }

        public async Task<List<TmdbMovieBrief>> DiscoverMoviesByDecadeAsync(int decade, int page = 1)
        {
            _logger.LogInformation("Requesting TMDB API for movies from decade: {Decade}, page: {Page}", decade, page);
            string cacheKey = $"decade_movies_{decade}_{page}";
            if (_memoryCache.TryGetValue(cacheKey, out List<TmdbMovieBrief>? cachedMovies) && cachedMovies != null)
            {
                _logger.LogWarning("CACHE HIT: Resultados para la clave '{CacheKey}' encontrados en memoria.", cacheKey);
                return cachedMovies;
            }
            try
            {
                _logger.LogInformation("CACHE MISS: Resultados para la clave '{CacheKey}' no encontrados. Realizando llamada a la API.", cacheKey);
                // Define the start and end dates for the decade
                string startDate = $"{decade}-01-01";
                string endDate = $"{decade + 9}-12-31";

                // Ask for movies within the date range, sorted by rating, with a minimum number of votes.
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
                    $"discover/movie?primary_release_date.gte={startDate}&primary_release_date.lte={endDate}&sort_by=vote_average.desc&vote_count.gte=500&language=en-US&page={page}");

                var results = response?.Results ?? new List<TmdbMovieBrief>();
                _memoryCache.Set(cacheKey, results, TimeSpan.FromHours(24));
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discover movies by decade {Decade}", decade);
                return new List<TmdbMovieBrief>();
            }
        }

        // In TmdbService.cs

        public async Task<List<TmdbMovieBrief>> DiscoverMoviesByYearAsync(int year, int page = 1)
        {
            _logger.LogInformation("Requesting TMDB API for movies from year: {Year}, page: {Page}", year, page);
            try
            {
                // VVVV THIS IS THE CORRECTED LINE VVVV
                // We now sort by vote_average and require a minimum vote count for quality results.
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
                    $"discover/movie?primary_release_year={year}&sort_by=vote_average.desc&vote_count.gte=300&language=en-US&page={page}");

                return response?.Results ?? new List<TmdbMovieBrief>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discover movies by year {Year}", year);
                return new List<TmdbMovieBrief>();
            }
        }


        // This method gets the details for a single person, including their profile picture path.
        public async Task<TmdbPersonDetails?> GetPersonDetailsAsync(int personId)
        {
            /// <summary>
            /// Fetches the details for a single person (director, actor, etc.) from TMDB.
            /// Results are cached for 24 hours to minimize redundant API calls and improve performance.
            /// </summary>
            string cacheKey = $"person_details_{personId}";
            if (_memoryCache.TryGetValue(cacheKey, out TmdbPersonDetails? cachedDetails) && cachedDetails != null)
            {
                return cachedDetails;
            }
            try
            {
                var details = await _httpClient.GetFromJsonAsync<TmdbPersonDetails>($"person/{personId}?language=en-US");
                if (details != null)
                {
                    _memoryCache.Set(cacheKey, details, TimeSpan.FromHours(24));
                }
                return details;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch person details for ID {PersonId}", personId);
                return null;
            }
        }



        public async Task<List<TmdbMovieBrief>> DiscoverMoviesByActorAsync(int actorId, int page = 1)
        {
            /// <summary>
            /// Fetches a list of movies for a given actor from TMDB, with results cached for 24 hours.
            /// </summary>
            string cacheKey = $"actor_movies_{actorId}";
            if (_memoryCache.TryGetValue(cacheKey, out List<TmdbMovieBrief>? cachedMovies) && cachedMovies != null)
            {
                return cachedMovies;
            }
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
                    $"discover/movie?with_cast={actorId}&sort_by=popularity.desc&vote_count.gte=50&vote_average.gte=1&language=en-US&page={page}");
                var results = response?.Results ?? new List<TmdbMovieBrief>();
                _memoryCache.Set(cacheKey, results, TimeSpan.FromHours(24));
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discover movies by actor ID {ActorId}", actorId);
                return new List<TmdbMovieBrief>();
            }
        }

        // Method to get the master list of all official genres from TMDB
        public async Task<List<TmdbGenre>> GetAllGenresAsync()
        {
            _logger.LogInformation("Requesting TMDB API for all movie genres.");
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TmdbGenreListResponse>("genre/movie/list?language=en-US");
                return response?.Genres ?? new List<TmdbGenre>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch genre list from TMDB.");
                return new List<TmdbGenre>();
            }
        }

        private readonly IMemoryCache _memoryCache;

        public TmdbService(HttpClient httpClient, ILogger<TmdbService> logger, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        // Gets the watch providers for a given movie from TMDB
    public async Task<WatchProviderResponse?> GetWatchProvidersAsync(int tmdbMovieId, string? region = "IE")
        {
            if (tmdbMovieId <= 0) return null;
            var requestUri = $"movie/{tmdbMovieId}/watch/providers";
            _logger.LogInformation("Requesting TMDB API for watch providers: {RequestUri}", requestUri);
            try
            {
                var response = await _httpClient.GetFromJsonAsync<WatchProviderResponse>(requestUri);
                _logger.LogInformation("Successfully fetched watch providers for movie ID {MovieId}.", tmdbMovieId);
                if (response?.Results != null)
                {
                    foreach (var country in response.Results.Values)
                    {
                        string regionCode = country.Link?.Split("locale=").LastOrDefault()?.Substring(0,2) ?? region ?? "IE";
                        string tmdbWatchUrl;
                        try
                        {
                            tmdbWatchUrl = GetTmdbWatchUrl(tmdbMovieId, regionCode);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error generating TMDB watch URL");
                            tmdbWatchUrl = string.Empty;
                        }
                        void SetProviderLinks(List<ProviderInfo>? providers)
                        {
                            if (providers == null) return;
                            foreach (var provider in providers)
                            {
                                provider.Link = tmdbWatchUrl;
                            }
                        }
                        SetProviderLinks(country.Streaming);
                        SetProviderLinks(country.Buy);
                        SetProviderLinks(country.Rent);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting TMDB watch providers for ID {MovieId}.", tmdbMovieId);
                return null;
            }
        }

        public async Task<TmdbSearchResponse?> SearchMoviesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            var requestUri = $"search/movie?query={Uri.EscapeDataString(query)}";
            _logger.LogInformation("Requesting TMDB API: {RequestUri}", requestUri);

            try
            {
                var searchResult = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(requestUri);
                _logger.LogInformation("Successfully fetched {Count} movies for query '{Query}'.", searchResult?.Results?.Count ?? 0, query);
                return searchResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching TMDB for query '{Query}'.", query);
                return null;
            }
        }

        public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbMovieId)
        {
            if (tmdbMovieId <= 0) return null;

            string cacheKey = $"movie_details_{tmdbMovieId}";
            if (_memoryCache.TryGetValue(cacheKey, out TmdbMovieDetails? cachedDetails) && cachedDetails != null)
            {
                _logger.LogWarning("CACHE HIT: La clave '{CacheKey}' fue encontrada en memoria.", cacheKey);
                return cachedDetails;
            }
            _logger.LogInformation("CACHE MISS: La clave '{CacheKey}' no fue encontrada. Realizando llamada a la API de TMDB.", cacheKey);

            var requestUri = $"movie/{tmdbMovieId}?append_to_response=credits";
            _logger.LogInformation("Requesting TMDB API for movie details: {RequestUri}", requestUri);

            try
            {
                var movieDetails = await _httpClient.GetFromJsonAsync<TmdbMovieDetails>(requestUri);
                if (movieDetails != null)
                {
                    _memoryCache.Set(cacheKey, movieDetails, TimeSpan.FromHours(24));
                }
                _logger.LogInformation("Successfully fetched details for movie ID {MovieId}.", tmdbMovieId);
                return movieDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting TMDB details for ID {MovieId}.", tmdbMovieId);
                return null;
            }
        }

        /// <summary>
        /// Retrieves the TMDB person ID for a given name (e.g., director or actor).
        /// Results are cached for 24 hours to avoid redundant API calls and improve performance.
        /// </summary>
        /// <param name="personName">The name of the person to look up.</param>
        /// <returns>The TMDB person ID if found, otherwise null.</returns>
        public async Task<int?> GetPersonIdAsync(string personName)
        {
            if (string.IsNullOrWhiteSpace(personName)) return null;

            string cacheKey = $"person_id_{personName.ToLower().Replace(" ", "_")}";

            if (_memoryCache.TryGetValue(cacheKey, out int? cachedPersonId))
            {
                return cachedPersonId;
            }

            try
            {
                var searchResponse = await _httpClient.GetFromJsonAsync<TmdbPersonSearchResponse>($"search/person?query={Uri.EscapeDataString(personName)}");
                var person = searchResponse?.Results?.OrderByDescending(p => p.Popularity).FirstOrDefault();
                if (person != null)
                {
                    _memoryCache.Set(cacheKey, person.Id, TimeSpan.FromHours(24));
                    return person.Id;
                }
                _memoryCache.Set<int?>(cacheKey, null, TimeSpan.FromMinutes(5));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception finding person ID for name {PersonName}", personName);
                return null;
            }
        }


        // In TmdbService.cs

        /// <summary>
        /// Retrieves a list of movies directed by the specified person (director) from TMDB.
        /// Results are cached for 24 hours to minimize API usage and improve performance.
        /// Only movies with more than 25 votes are included, sorted by vote average and count.
        /// </summary>
        /// <param name="directorId">The TMDB person ID of the director.</param>
        /// <returns>A list of movies directed by the person, sorted by rating and popularity.</returns>
        public async Task<List<TmdbMovieBrief>> GetDirectorFilmographyAsync(int directorId)
        {
            string cacheKey = $"director_filmography_{directorId}";
            if (_memoryCache.TryGetValue(cacheKey, out var obj) && obj is List<TmdbMovieBrief> cached && cached != null)
            {
                return cached;
            }
            try
            {
                // 1. Call the accurate '/person/{id}/movie_credits' endpoint. This is correct.
                var creditsResponse = await _httpClient.GetFromJsonAsync<TmdbPersonMovieCreditsResponse>($"person/{directorId}/movie_credits?language=en-US");

                if (creditsResponse?.Crew == null)
                {
                    return new List<TmdbMovieBrief>();
                }

                // 2. Filter to get ONLY the movies where the job was "Director". This is correct.
                var filmography = creditsResponse.Crew
                    .Where(movie => movie.Job == "Director")
                    .ToList();

                // 3. From that accurate list, remove movies with too few votes to be considered "rated".
                var ratedFilmography = filmography
                    .Where(m => m.VoteCount > 25)
                    .ToList();

                // 4. Sort this accurate, rated list by vote average, as you wanted.
                var sortedFilmography = ratedFilmography
                    .OrderByDescending(m => m.VoteAverage)
                    .ThenByDescending(m => m.VoteCount)
                    .ToList();

                _memoryCache.Set(cacheKey, sortedFilmography, TimeSpan.FromHours(24));
                return sortedFilmography;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch filmography for person ID {PersonId}", directorId);
                return new List<TmdbMovieBrief>();
            }
        }



        public async Task<List<TmdbMovieBrief>> DiscoverMoviesByGenreAsync(int genreId, int page = 1)
        {
            _logger.LogInformation("Requesting TMDB API for movies by genre ID: {GenreId}, page: {Page}", genreId, page);
            try
            {
                // VVVV THIS IS THE ONLY LINE WE ARE CHANGING VVVV
                // We changed sort_by=popularity.desc to sort_by=vote_average.desc
                // and added vote_count.gte=500 to ensure quality.
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>($"discover/movie?with_genres={genreId}&sort_by=vote_average.desc&vote_count.gte=500&language=en-US&page={page}");

                return response?.Results ?? new List<TmdbMovieBrief>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to discover movies by genre ID {GenreId}", genreId);
                return new List<TmdbMovieBrief>();
            }
        }

        public async Task<List<TmdbMovieBrief>> GetTrendingMoviesAsync(int page = 1)
        {
            // Caches trending movies from TMDB for 90 minutes per page to minimize API calls and improve user-perceived performance.
            // Cache key pattern: "trending_movies_day_page_{page}" (ensures unique cache per TMDB page).
            // On cache hit: returns cached results instantly. On miss: fetches from TMDB, stores in cache, and returns.
            var cacheKey = $"trending_movies_day_page_{page}";
            if (_memoryCache.TryGetValue(cacheKey, out var obj) && obj is List<TmdbMovieBrief> cached && cached != null)
            {
                _logger.LogWarning("CACHE HIT: La clave '{CacheKey}' fue encontrada en memoria.", cacheKey);
                return cached;
            }
            _logger.LogInformation("CACHE MISS: La clave '{CacheKey}' no fue encontrada. Realizando llamada a la API de TMDB.", cacheKey);
            try
            {
                var searchResponse = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>($"trending/movie/day?language=en-US&page={page}");
                var results = searchResponse?.Results ?? new List<TmdbMovieBrief>();
                // Cache por 90 minutos
                _memoryCache.Set(cacheKey, results, TimeSpan.FromMinutes(90));
                _logger.LogInformation("Cached trending movies for page {Page}", page);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch trending movies.");
                return new List<TmdbMovieBrief>();
            }
        }
    }
}