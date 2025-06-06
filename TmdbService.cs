using System;
using System.Collections.Generic; // For List<T>
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Ezequiel_Movies.Models.TmdbApi; // For your TMDB DTOs
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
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            var requestUri = $"search/movie?query={Uri.EscapeDataString(query)}";
            _logger.LogInformation("Requesting TMDB API: {RequestUri}", requestUri);

            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var searchResult = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
                    _logger.LogInformation("Successfully fetched {Count} movies for query '{Query}'.", searchResult?.Results?.Count ?? 0, query);
                    return searchResult;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("TMDB API request failed with status code {StatusCode}. Query: {Query}. Response: {ErrorContent}",
                                     response.StatusCode, query, errorContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling TMDB API for query '{Query}'.", query);
                return null;
            }
        }

        public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbMovieId)
        {
            if (tmdbMovieId <= 0)
            {
                _logger.LogWarning("GetMovieDetailsAsync called with invalid tmdbMovieId: {TmdbMovieId}", tmdbMovieId);
                return null;
            }

            var requestUri = $"movie/{tmdbMovieId}?append_to_response=credits";
            _logger.LogInformation("Requesting TMDB API for movie details: {RequestUri}", requestUri);

            try
            {
                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    var movieDetails = await response.Content.ReadFromJsonAsync<TmdbMovieDetails>();
                    _logger.LogInformation("Successfully fetched details for movie ID {MovieId}.", tmdbMovieId);
                    return movieDetails;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("TMDB API request for movie details failed with status code {StatusCode}. Movie ID: {MovieId}. Response: {ErrorContent}",
                                     response.StatusCode, tmdbMovieId, errorContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling TMDB API for movie details with ID {MovieId}.", tmdbMovieId);
                return null;
            }
        }

        // VVVV HERE IS THE NEW METHOD, NOW INSIDE THE CLASS VVVV
        public async Task<List<TmdbMovieBrief>> GetTrendingMoviesAsync(int page = 1)
        {
            _logger.LogInformation("Requesting TMDB API for trending movies (day).");
            var response = await _httpClient.GetAsync($"trending/movie/day?language=en-US&page={page}");

            if (response.IsSuccessStatusCode)
            {
                var searchResponse = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
                if (searchResponse?.Results != null)
                {
                    _logger.LogInformation($"Successfully fetched {searchResponse.Results.Count} trending movies.");
                    return searchResponse.Results;
                }
            }
            else
            {
                _logger.LogError($"Failed to fetch trending movies. Status code: {response.StatusCode}");
            }
            return new List<TmdbMovieBrief>(); // Return empty list on failure
        }
        // ^^^^ END OF NEW METHOD ^^^^

    } // <<< This is the correct closing brace for the TmdbService class
}