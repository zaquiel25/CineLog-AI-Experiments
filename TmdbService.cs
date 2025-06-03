using System;
using System.Net.Http;
using System.Net.Http.Json; // For ReadFromJsonAsync
using System.Text.Json; // If you were using JsonSerializer.Deserialize manually
using System.Threading.Tasks;
using Ezequiel_Movies.Models.TmdbApi; // <<< IMPORTANT: For TmdbMovieDetails, TmdbSearchResponse etc.
using Microsoft.Extensions.Logging; // If you're using ILogger

namespace Ezequiel_Movies 
{
    public class TmdbService


    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TmdbService> _logger; // Optional: for logging

        // Updated constructor to include ILogger (optional)
        public TmdbService(HttpClient httpClient, ILogger<TmdbService> logger)
        {
            _httpClient = httpClient;
            _logger = logger; // Store the logger
        }

        public async Task<TmdbSearchResponse?> SearchMoviesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null; // Or return an empty response object
            }

            // The API key/token is already in DefaultRequestHeaders configured in Program.cs
            var requestUri = $"search/movie?query={Uri.EscapeDataString(query)}";
            _logger.LogInformation("Requesting TMDB API: {RequestUri}", requestUri); // Log the request

            try
            {
                // Using System.Net.Http.Json extension method ReadFromJsonAsync for convenience
                var response = await _httpClient.GetAsync(requestUri);

                if (response.IsSuccessStatusCode)
                {
                    // This is a more direct way to deserialize with System.Net.Http.Json
                    var searchResult = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
                    _logger.LogInformation("Successfully fetched {Count} movies for query '{Query}'.", searchResult?.Results?.Count ?? 0, query);
                    return searchResult;
                }
                else
                {
                    // Log the error status and content if any
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("TMDB API request failed with status code {StatusCode}. Query: {Query}. Response: {ErrorContent}",
                                     response.StatusCode, query, errorContent);
                    return null; // Or throw an exception, or return a response indicating error
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while calling TMDB API for query '{Query}'.", query);
                return null; // Or rethrow or handle as appropriate
            }
        }

        // This method goes INSIDE your TmdbService class,
    // for example, after the SearchMoviesAsync method.

    public async Task<TmdbMovieDetails?> GetMovieDetailsAsync(int tmdbMovieId)
    {
        if (tmdbMovieId <= 0) 
        {
            // Using _logger if you have it initialized in your constructor
            _logger.LogWarning("GetMovieDetailsAsync called with invalid tmdbMovieId: {TmdbMovieId}", tmdbMovieId);
            return null;
        }

        // Request movie details and append "credits" to get cast/crew information
        // The API key/token is already in DefaultRequestHeaders of _httpClient
        var requestUri = $"movie/{tmdbMovieId}?append_to_response=credits"; 
        _logger.LogInformation("Requesting TMDB API for movie details: {RequestUri}", requestUri);

        try
        {
            var response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                // Deserialize the JSON response into our TmdbMovieDetails object
                // This requires the TmdbMovieDetails DTO class we created.
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
    }
}