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
        public async Task<List<TmdbMovieBrief>> GetDirectorFilmographyAsync(int directorId, int page = 1)
        {
            _logger.LogInformation("Requesting TMDB API for movie credits for person ID: {PersonId}, page: {Page}", directorId, page);
            try
            {
                var creditsResponse = await _httpClient.GetFromJsonAsync<TmdbPersonMovieCreditsResponse>($"person/{directorId}/movie_credits?language=en-US&page={page}");
                var filmography = creditsResponse?.Crew?.Where(movie => movie.Job == "Director").ToList();
                return filmography ?? new List<TmdbMovieBrief>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch filmography for person ID {PersonId}", directorId);
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