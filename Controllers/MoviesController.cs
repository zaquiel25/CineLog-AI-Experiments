using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using Ezequiel_Movies.Models;
using Ezequiel_Movies.Helpers;
using Ezequiel_Movies.Models.TmdbApi;
using Microsoft.Extensions.Logging;

namespace Ezequiel_Movies.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly TmdbService _tmdbService;
        private readonly ILogger<MoviesController> _logger;

        
        public MoviesController(ApplicationDbContext dbContext, TmdbService tmdbService, ILogger<MoviesController> logger)
        {
            _dbContext = dbContext;
            _tmdbService = tmdbService;
            _logger = logger;
        }


        // In MoviesController.cs, replace your SearchTmdbApi method with this one:

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
                // This part is the fix: It creates a clean list of results
                // in the exact format the JavaScript expects.
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
            // This action is for displaying the initial suggestion page.
            // It doesn't need to fetch any data yet, it just shows the view with the criteria buttons.
            // When it returns View(), it will look for Views/Movies/Suggest.cshtml.
            return View();
        }

        // In MoviesController.cs

        // In MoviesController.cs

        // In MoviesController.cs

        // In MoviesController.cs

        [HttpGet]
        public async Task<IActionResult> ShowSuggestions(string suggestionType, string? query = null, int page = 1)
        {
            _logger.LogInformation("ShowSuggestions invoked with type: {Type}, query: {Query}, page: {Page}", suggestionType, query, page);

            List<TmdbMovieBrief> suggestedMovies = new List<TmdbMovieBrief>();
            string suggestionTitle = "Suggestions";

            // Default next suggestion type
            ViewData["NextSuggestionType"] = suggestionType;
            ViewData["NextQuery"] = query;
            ViewData["NextPage"] = page + 1;

            switch (suggestionType?.ToLower())
            {
                case "trending":
                    suggestionTitle = "Trending Movies Today";
                    var allTrendingMovies = await _tmdbService.GetTrendingMoviesAsync(page);
                    suggestedMovies = allTrendingMovies.Take(3).ToList();
                    break;

                case "director_recent":
                case "director_frequent":
                case "director_rated":
                case "director_random":
                    var loggedMovies = await _dbContext.Movies
                        .Where(m => !string.IsNullOrEmpty(m.Director) && m.Director != "N/A" && m.TmdbId.HasValue)
                        .OrderByDescending(m => m.DateWatched)
                        .ToListAsync();

                    if (loggedMovies.Count == 0)
                    {
                        suggestionTitle = "Log some movies to get personalized suggestions!";
                        break;
                    }

                    // --- Find all our candidate directors ---
                    string? recentDirector = loggedMovies.FirstOrDefault()?.Director;
                    string? frequentDirector = loggedMovies.GroupBy(m => m.Director!).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                    string? ratedDirector = loggedMovies.Where(m => m.UserRating.HasValue).GroupBy(m => m.Director!).Select(g => new { Name = g.Key, Avg = g.Average(m => m.UserRating!.Value) }).OrderByDescending(d => d.Avg).Select(d => d.Name).FirstOrDefault();

                    // --- Build a de-duplicated queue of top directors ---
                    var topDirectorQueue = new List<string?>();
                    if (recentDirector != null) topDirectorQueue.Add(recentDirector);
                    if (frequentDirector != null && !topDirectorQueue.Contains(frequentDirector)) topDirectorQueue.Add(frequentDirector);
                    if (ratedDirector != null && !topDirectorQueue.Contains(ratedDirector)) topDirectorQueue.Add(ratedDirector);

                    string? directorToSuggest = null;

                    if (suggestionType == "director_recent")
                    {
                        directorToSuggest = topDirectorQueue.ElementAtOrDefault(0);
                        ViewData["NextSuggestionType"] = "director_frequent";
                    }
                    else if (suggestionType == "director_frequent")
                    {
                        directorToSuggest = topDirectorQueue.ElementAtOrDefault(1);
                        ViewData["NextSuggestionType"] = "director_rated";
                    }
                    else if (suggestionType == "director_rated")
                    {
                        directorToSuggest = topDirectorQueue.ElementAtOrDefault(2);
                        ViewData["NextSuggestionType"] = "director_random"; // Next click starts exploration
                    }
                    else if (suggestionType == "director_random")
                    {
                        var allLoggedDirectors = loggedMovies.Select(m => m.Director!).Distinct().ToList();
                        directorToSuggest = allLoggedDirectors[new Random().Next(allLoggedDirectors.Count)];
                        ViewData["NextSuggestionType"] = "director_random"; // Stay in random mode
                    }

                    if (directorToSuggest != null)
                    {
                        suggestionTitle = $"Because you like {directorToSuggest}...";
                        var loggedTmdbIds = new HashSet<int>(loggedMovies.Select(m => m.TmdbId!.Value));
                        suggestedMovies = await GetSuggestionsForDirector(directorToSuggest, loggedTmdbIds);
                    }
                    else
                    {
                        // This happens if the queue runs out (e.g., user only has 1-2 directors logged)
                        // We'll just enter random mode early
                        return RedirectToAction("ShowSuggestions", new { suggestionType = "director_random" });
                    }
                    break;

                default:
                    return RedirectToAction("Suggest");
            }

            ViewData["SuggestionTitle"] = suggestionTitle;
            return View("Suggest", suggestedMovies);
        }

        // In MoviesController.cs, paste this inside the class

        private async Task<List<TmdbMovieBrief>> GetSuggestionsForDirector(string directorName, HashSet<int> loggedTmdbIds, int page = 1)
        {
            var directorId = await _tmdbService.GetPersonIdAsync(directorName);
            if (!directorId.HasValue)
            {
                _logger.LogWarning("Helper method could not find director ID for {DirectorName}", directorName);
                return new List<TmdbMovieBrief>();
            }

            // Pass the page number to the service to get different sets of movies for reshuffling
            var allDirectorMovies = await _tmdbService.GetDirectorFilmographyAsync(directorId.Value, page);

            var newSuggestions = allDirectorMovies
                .Where(movie => !loggedTmdbIds.Contains(movie.Id))
                .OrderByDescending(m => m.Popularity)
                .ToList();

            // If this page of results gave us no NEW movies, try the next page automatically.
            // This makes "Reshuffle" more effective.
            if (!newSuggestions.Any() && allDirectorMovies.Any())
            {
                _logger.LogInformation("No new suggestions on page {Page} for {Director}, trying next page.", page, directorName);
                return await GetSuggestionsForDirector(directorName, loggedTmdbIds, page + 1);
            }

            return newSuggestions.Take(3).ToList();
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
                // A TMDB ID was provided (e.g., from the Suggestion page), so pre-fill the ViewModel
                Console.WriteLine($"Add GET - Pre-filling form for TMDB ID: {tmdbId.Value}");
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId.Value);
                if (movieDetails != null)
                {
                    // Overwrite TMDB fields, but DateWatched remains as DateTime.Today
                    viewModel.Title = movieDetails.Title;
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
            // Optional: Log received ViewModel values at the start for debugging
            Console.WriteLine(""); // Blank line for readability
            Console.WriteLine("--- Add POST Action Invoked ---");
            Console.WriteLine($"ViewModel Title Received: [{viewModel.Title}]");
            Console.WriteLine($"ViewModel Director Received: [{viewModel.Director}]");
            Console.WriteLine($"ViewModel ReleasedYear Received: [{viewModel.ReleasedYear}]");
            Console.WriteLine($"ViewModel PosterPath Received: [{viewModel.PosterPath}]");
            Console.WriteLine($"ViewModel Overview Snippet: [{viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 50))}...]");
            Console.WriteLine($"ViewModel DateWatched Received: {viewModel.DateWatched}");
            Console.WriteLine($"ViewModel WatchedLocation Received: {viewModel.WatchedLocation}");
            Console.WriteLine($"ViewModel IsRewatch Received: {viewModel.IsRewatch}"); // Log IsRewatch
            Console.WriteLine($"ViewModel TmdbId Received: [{viewModel.TmdbId}]"); // <<< ENSURE THIS LINE IS PRESENT
            Console.WriteLine($"ViewModel Subscribed Received: {viewModel.Subscribed}"); // Log Subscribed
            Console.WriteLine("-----------------------------");

            if (ModelState.IsValid)
            {
                var movie = new Ezequiel_Movies1.Models.Entities.Movies // Ensure this namespace is correct
                {
                    Id = Guid.NewGuid(),
                    Title = viewModel.Title,
                    Director = viewModel.Director,
                    ReleasedYear = viewModel.ReleasedYear,
                    DateWatched = viewModel.DateWatched,
                    WatchedLocation = viewModel.WatchedLocation!.Value, // Assuming WatchedLocation is required
                    PosterPath = viewModel.PosterPath,
                    Overview = viewModel.Overview,
                    IsRewatch = viewModel.IsRewatch,       // Assign IsRewatch
                    Subscribed = viewModel.Subscribed,
                    UserRating = viewModel.UserRating,
                    TmdbId = viewModel.TmdbId
                }; // Object initializer for 'movie' ends here

                // Logging line for IsRewatch, after 'movie' object is created
                Console.WriteLine($"Adding Movie '{movie.Title}', UserRating from ViewModel: {viewModel.UserRating}, Value being saved to entity: {movie.UserRating}");

                await _dbContext.Movies.AddAsync(movie);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"Add POST - Movie Added Successfully: {movie.Title}");
                return RedirectToAction("List");
            }
            else // ModelState is NOT valid
            {
                Console.WriteLine("Add POST - ModelState IS INVALID. Movie not added.");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    if (value.Errors.Any())
                    {
                        Console.WriteLine($"-- Errors for '{modelStateKey}' --");
                        foreach (var error in value.Errors)
                        {
                            Console.WriteLine($"  - Message: {error.ErrorMessage}");
                            if (error.Exception != null)
                            {
                                Console.WriteLine($"    Exception: {error.Exception.Message}");
                            }
                        }
                    }
                }
                // Re-display the Add page with the submitted data and validation errors.
                // The JavaScript's showPreviewCardFromModelOnLoad() should handle re-showing the TMDB preview.
                Console.WriteLine("Add POST - Returning View(viewModel) due to invalid ModelState.");
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> List(string sortOrder, string searchString, int? pageNumber)
        {
            Console.WriteLine($"--- List Action Invoked --- SortOrder: [{sortOrder}], SearchString: [{searchString}], PageNumber: [{pageNumber}]");

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

            var moviesQuery = _dbContext.Movies.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                moviesQuery = moviesQuery.Where(m =>
                    m.Title.Contains(searchString) ||
                    m.Director.Contains(searchString) ||
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
            var movieEntity = await _dbContext.Movies.FindAsync(id);
            if (movieEntity == null)
            {
                Console.WriteLine($"Edit GET - Movie with ID {id} not found in DB.");
                return NotFound();
            }

            // Enhanced log after loading the entity from the database
            Console.WriteLine($"Edit GET - Loaded Movie from DB (ID: {movieEntity.Id}): " +
                              $"Title='{movieEntity.Title}', " +
                              $"Director='{movieEntity.Director}', " +
                              $"ReleasedYear='{movieEntity.ReleasedYear}', " +
                              $"PosterPath='{movieEntity.PosterPath}', " +
                              $"Overview (snippet)='{movieEntity.Overview?.Substring(0, Math.Min(movieEntity.Overview?.Length ?? 0, 30))}...', " +
                              $"IsRewatch='{movieEntity.IsRewatch}', " +
                              $"UserRating='{movieEntity.UserRating}', " +
                              $"TmdbId='{movieEntity.TmdbId}'");


            var viewModel = new AddMoviesViewModel
            {
                Id = movieEntity.Id,
                Title = movieEntity.Title,
                Director = movieEntity.Director,
                ReleasedYear = movieEntity.ReleasedYear,
                DateWatched = movieEntity.DateWatched,
                WatchedLocation = movieEntity.WatchedLocation,
                Subscribed = movieEntity.Subscribed,
                PosterPath = movieEntity.PosterPath,
                Overview = movieEntity.Overview,
                IsRewatch = movieEntity.IsRewatch,
                UserRating = movieEntity.UserRating,
                TmdbId = movieEntity.TmdbId
            };

            // Enhanced log for the ViewModel being sent to the View
            Console.WriteLine($"Edit GET - ViewModel being sent to View (ID: {viewModel.Id}): " +
                              $"Title='{viewModel.Title}', " +
                              $"Director='{viewModel.Director}', " +
                              $"ReleasedYear='{viewModel.ReleasedYear}', " +
                              $"PosterPath='{viewModel.PosterPath}', " +
                              $"Overview (snippet)='{viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 30))}...', " +
                              $"IsRewatch='{viewModel.IsRewatch}', " +
                              $"UserRating='{viewModel.UserRating}', " +
                              $"TmdbId='{viewModel.TmdbId}'");

            return View(viewModel);
        }




        [HttpPost]
        public async Task<IActionResult> Edit(AddMoviesViewModel viewModel)
        {
            Console.WriteLine(""); // Blank line for readability in logs
            Console.WriteLine("--- Edit POST Action Invoked ---");
            Console.WriteLine($"ViewModel ID Received: {viewModel.Id}");
            Console.WriteLine($"ViewModel Title Received: [{viewModel.Title}]");
            Console.WriteLine($"ViewModel Director Received: [{viewModel.Director}]");
            Console.WriteLine($"ViewModel ReleasedYear Received FROM FORM: [{viewModel.ReleasedYear}]");
            Console.WriteLine($"ViewModel PosterPath Received: [{viewModel.PosterPath}]");
            Console.WriteLine($"ViewModel Overview Snippet: [{viewModel.Overview?.Substring(0, Math.Min(viewModel.Overview?.Length ?? 0, 50))}...]");
            Console.WriteLine($"ViewModel DateWatched Received: {viewModel.DateWatched}");
            Console.WriteLine($"ViewModel WatchedLocation Received: {viewModel.WatchedLocation}");
            Console.WriteLine($"ViewModel IsRewatch Received: {viewModel.IsRewatch}");
            Console.WriteLine($"ViewModel UserRating Received: [{viewModel.UserRating}]"); // Added brackets for consistency
            Console.WriteLine($"ViewModel TmdbId Received: [{viewModel.TmdbId}]");     // <<< ENSURE THIS IS PRESENT AND LOGS VIEWMODEL TmdbId
            Console.WriteLine($"ViewModel Subscribed Received: {viewModel.Subscribed}");
            Console.WriteLine("-----------------------------");

            if (ModelState.IsValid)
            {
                Console.WriteLine("Edit POST - ModelState IS VALID. Proceeding to save.");
                var movieEntity = await _dbContext.Movies.FindAsync(viewModel.Id);

                if (movieEntity is not null)
                {
                    // Log original values from DB, including TmdbId
                    Console.WriteLine($"Edit POST - Movie Found (ID: {movieEntity.Id}). Original DB Values - Title: '{movieEntity.Title}', Director: '{movieEntity.Director}', Year: {movieEntity.ReleasedYear}, IsRewatch: {movieEntity.IsRewatch}, UserRating: {movieEntity.UserRating}, TmdbId: {movieEntity.TmdbId}");

                    // Assign values from ViewModel to the Entity
                    movieEntity.Title = viewModel.Title;
                    movieEntity.Director = viewModel.Director;
                    movieEntity.ReleasedYear = viewModel.ReleasedYear;
                    movieEntity.DateWatched = viewModel.DateWatched;
                    movieEntity.WatchedLocation = viewModel.WatchedLocation!.Value;
                    movieEntity.PosterPath = viewModel.PosterPath;
                    movieEntity.Overview = viewModel.Overview;
                    movieEntity.IsRewatch = viewModel.IsRewatch;
                    movieEntity.Subscribed = viewModel.Subscribed;
                    movieEntity.UserRating = viewModel.UserRating;
                    movieEntity.TmdbId = viewModel.TmdbId; // <<< This assignment is crucial and already in your code

                    // Log entity values just before saving, including TmdbId
                    Console.WriteLine($"Edit POST - Entity values BEFORE SaveChangesAsync (ID: {movieEntity.Id}) - Title: '{movieEntity.Title}', Director: '{movieEntity.Director}', Year: '{movieEntity.ReleasedYear}', IsRewatch: {movieEntity.IsRewatch}, TmdbId: {movieEntity.TmdbId}, UserRating: {movieEntity.UserRating}");

                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"Edit POST - SaveChangesAsync completed successfully for Movie ID {movieEntity.Id}. Redirecting to List.");
                        return RedirectToAction("List", "Movies");
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        Console.WriteLine($"Edit POST - DbUpdateConcurrencyException for Movie ID {movieEntity.Id}: {ex.Message}");
                        ModelState.AddModelError("", "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Edit POST - Exception during SaveChangesAsync for Movie ID {movieEntity.Id}: {ex.Message}");
                        ModelState.AddModelError("", "An error occurred while saving changes. Please try again.");
                    }
                }
                else
                {
                    Console.WriteLine($"Edit POST - ERROR: Movie entity with ID {viewModel.Id} not found in DB during save attempt.");
                    return NotFound();
                }
            }
            else // ModelState is NOT valid
            {
                Console.WriteLine("Edit POST - ModelState IS INVALID. Changes will not be saved.");
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    if (value.Errors.Any())
                    {
                        Console.WriteLine($"-- Errors for '{modelStateKey}' --");
                        foreach (var error in value.Errors)
                        {
                            Console.WriteLine($"  - Message: {error.ErrorMessage}");
                            if (error.Exception != null)
                            {
                                Console.WriteLine($"    Exception: {error.Exception.Message}");
                            }
                        }
                    }
                }
            }
            // If ModelState invalid or save failed, return to View with viewModel
            Console.WriteLine("Edit POST - Returning View(viewModel) due to invalid ModelState or error for Movie ID: {viewModel.Id}.");
            return View(viewModel);
        }
        // POST: Movies/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            Console.WriteLine($"DEBUG: Delete action in MoviesController hit. ID received: {id}");
            var movie = await _dbContext.Movies.FindAsync(id);
            if (movie != null)
            {
                Console.WriteLine($"DEBUG: Movie found with Title: {movie.Title}. Preparing to remove.");
                _dbContext.Movies.Remove(movie);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"DEBUG: Changes saved. Movie should be deleted.");
            }
            else
            {
                Console.WriteLine($"DEBUG: No movie found with ID: {id}. Nothing to delete.");
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

            System.Diagnostics.Debug.WriteLine($"--- Attempting TMDB Search for query: '{query}' ---");
            var searchResult = await _tmdbService.SearchMoviesAsync(query);

            if (searchResult != null && searchResult.Results != null && searchResult.Results.Any())
            {
                System.Diagnostics.Debug.WriteLine($"SUCCESS: Found {searchResult.TotalResults} movies. Displaying up to the first 5:");
                foreach (var movieItem in searchResult.Results.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"  ID: {movieItem.Id}, Title: {movieItem.Title}, Release Date: {movieItem.ReleaseDate}, Overview: {movieItem.Overview?.Substring(0, Math.Min(movieItem.Overview.Length, 50))}...");
                }
                return Content($"Search for '{query}' successful. Found {searchResult.TotalResults} movies. Check 'Application Output' pad in Visual Studio for details.");
            }
            else if (searchResult != null)
            {
                System.Diagnostics.Debug.WriteLine($"INFO: Search for '{query}' yielded 0 results.");
                return Content($"Search for '{query}' yielded 0 results from TMDB.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: TMDB search for '{query}' failed or the service returned a null response. Check previous logs from TmdbService.");
                return Content($"Search for '{query}' failed. Check 'Application Output' pad and logs from TmdbService.");
            }
        }
    }
}