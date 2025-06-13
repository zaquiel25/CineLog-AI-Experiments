using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Ezequiel_Movies.Models.TmdbApi;
using Microsoft.Extensions.Logging;

namespace Ezequiel_Movies
{
    public class TmdbService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TmdbService> _logger;



        // In TmdbService.cs

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

        // This method discovers top-rated movies starring a specific actor.
        public async Task<List<TmdbMovieBrief>> DiscoverMoviesByActorAsync(int actorId, int page = 1)
        {
            _logger.LogInformation("Requesting TMDB API for movies with actor ID: {ActorId}, page: {Page}", actorId, page);
            try
            {
                // We use the 'discover' endpoint, filtering by 'with_cast' and sorting by rating.
                var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(
                    $"discover/movie?with_cast={actorId}&sort_by=vote_average.desc&vote_count.gte=500&language=en-US&page={page}");

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