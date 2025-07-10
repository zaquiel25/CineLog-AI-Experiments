
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
            try
            {
                // Define the start and end dates for the decade
                string startDate = $"{decade}-01-01";
                string endDate = $"{decade + 9}-12-31";

                // Ask for movies within the date range, sorted by rating, with a minimum number of votes.
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
                    $"discover/movie?primary_release_date.gte={startDate}&primary_release_date.lte={endDate}&sort_by=vote_average.desc&vote_count.gte=500&language=en-US&page={page}");

                return response?.Results ?? new List<TmdbMovieBrief>();
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
            _logger.LogInformation("Requesting TMDB API for person details for ID: {PersonId}", personId);
            try
            {
                return await _httpClient.GetFromJsonAsync<TmdbPersonDetails>($"person/{personId}?language=en-US");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch person details for ID {PersonId}", personId);
                return null;
            }
        }



        public async Task<List<TmdbMovieBrief>> DiscoverMoviesByActorAsync(int actorId, int page = 1)
        {
            _logger.LogInformation("Requesting TMDB API for movies with actor ID: {ActorId}, page: {Page}", actorId, page);
            try
            {
                // VVVV THIS IS THE ONLY CHANGE VVVV
                // We've added "&vote_average.gte=1" to filter out movies with a 0 rating.
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
                    $"discover/movie?with_cast={actorId}&sort_by=popularity.desc&vote_count.gte=50&vote_average.gte=1&language=en-US&page={page}");

                return response?.Results ?? new List<TmdbMovieBrief>();
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

        public TmdbService(HttpClient httpClient, ILogger<TmdbService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

            var requestUri = $"movie/{tmdbMovieId}?append_to_response=credits";
            _logger.LogInformation("Requesting TMDB API for movie details: {RequestUri}", requestUri);

            try
            {
                var movieDetails = await _httpClient.GetFromJsonAsync<TmdbMovieDetails>(requestUri);
                _logger.LogInformation("Successfully fetched details for movie ID {MovieId}.", tmdbMovieId);
                return movieDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting TMDB details for ID {MovieId}.", tmdbMovieId);
                return null;
            }
        }

        public async Task<int?> GetPersonIdAsync(string personName)
        {
            _logger.LogInformation("Requesting TMDB API to find person ID for: {PersonName}", personName);
            try
            {
                var searchResponse = await _httpClient.GetFromJsonAsync<TmdbPersonSearchResponse>($"search/person?query={Uri.EscapeDataString(personName)}");
                var person = searchResponse?.Results?.OrderByDescending(p => p.Popularity).FirstOrDefault();
                if (person != null)
                {
                    _logger.LogInformation("Found person ID {PersonId} for name {PersonName}", person.Id, personName);
                    return person.Id;
                }
                _logger.LogWarning("Could not find a person ID for name {PersonName}", personName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception finding person ID for name {PersonName}", personName);
                return null;
            }
        }


        // In TmdbService.cs

        public async Task<List<TmdbMovieBrief>> GetDirectorFilmographyAsync(int directorId)
        {
            _logger.LogInformation("Requesting TMDB API for movie credits for person ID: {PersonId}", directorId);
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

                // VVVV THIS IS THE ONLY CHANGE: A NEW FILTER VVVV
                // 3. From that accurate list, remove movies with too few votes to be considered "rated".
                var ratedFilmography = filmography
                    .Where(m => m.VoteCount > 25) // Only keeps movies with more than 25 votes
                    .ToList();
                // ^^^^ END OF THE ONLY CHANGE ^^^^

                // 4. Sort this accurate, rated list by vote average, as you wanted.
                var sortedFilmography = ratedFilmography
                    .OrderByDescending(m => m.VoteAverage)
                    .ThenByDescending(m => m.VoteCount)
                    .ToList();

                _logger.LogInformation("Successfully fetched and sorted {Count} directed movies for person ID {PersonId}", sortedFilmography.Count, directorId);

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
            _logger.LogInformation("Requesting TMDB API for trending movies (day).");
            try
            {
                var searchResponse = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>($"trending/movie/day?language=en-US&page={page}");
                return searchResponse?.Results ?? new List<TmdbMovieBrief>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch trending movies.");
                return new List<TmdbMovieBrief>();
            }
        }
    }
}