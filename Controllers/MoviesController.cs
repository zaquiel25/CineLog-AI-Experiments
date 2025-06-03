using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using Ezequiel_Movies.Models;
using Ezequiel_Movies.Helpers;

namespace Ezequiel_Movies.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly TmdbService _tmdbService; // Added TmdbService field

        // Updated constructor to inject both DbContext and TmdbService
        public MoviesController(ApplicationDbContext dbContext, TmdbService tmdbService)
        {
            _dbContext = dbContext;
            _tmdbService = tmdbService;
        }

        // In MoviesController.cs

        [HttpGet] // This action will respond to GET requests
        public async Task<IActionResult> SearchTmdbApi(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { results = new List<object>() }); // Return empty list if query is empty
            }

            var searchResult = await _tmdbService.SearchMoviesAsync(query);

            if (searchResult != null && searchResult.Results != null)
            {
                // We only need to send back the relevant parts for the search result display
                // Let's select just a few properties from TmdbMovieBrief for the JSON response
                var simplifiedResults = searchResult.Results.Select(m => new
                {
                    id = m.Id,
                    title = m.Title,
                    releaseDate = m.ReleaseDate, // Format YYYY-MM-DD from TMDB
                    overview = m.Overview != null && m.Overview.Length > 150 ? m.Overview.Substring(0, 150) + "..." : m.Overview, // Truncate overview
                    posterPath = m.PosterPath // We'll need the base URL for images later to display this
                }).ToList();

                return Json(new { results = simplifiedResults });
            }

            // Return an empty list or an error indicator if the search failed or returned no results
            return Json(new { results = new List<object>(), error = "Search failed or no results found." });
        }


        [HttpGet] // This action will respond to GET requests
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

        // GET: Movies/Add
        [HttpGet]
        public IActionResult Add()
        {
            var viewModel = new AddMoviesViewModel
            {
                DateWatched = DateTime.Today
            };
            return View(viewModel);
        }

        // POST: Movies/Add
        // In MoviesController.cs

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
                    UserRating = viewModel.UserRating
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

        // GET: Movies/List
        // In MoviesController.cs

        // In MoviesController.cs

        // In MoviesController.cs

        // In MoviesController.cs

        // In MoviesController.cs

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
        // GET: Movies/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var movieEntity = await _dbContext.Movies.FindAsync(id);
            if (movieEntity == null)
            {
                Console.WriteLine($"Edit GET - Movie with ID {id} not found in DB."); // <<< DEBUG
                return NotFound();
            }
            Console.WriteLine($"Edit GET - Loaded movie ID {movieEntity.Id} from DB. PosterPath from DB: [{movieEntity.PosterPath}]"); // <<< DEBUG

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
                UserRating = movieEntity.UserRating
            };
            Console.WriteLine($"Edit GET - ViewModel PosterPath being sent to view: [{viewModel.PosterPath}]");
            Console.WriteLine($"Edit GET - ViewModel UserRating being sent to view: [{viewModel.UserRating}]");// <<< DEBUG
            return View(viewModel);
        }

        // POST: Movies/Edit/5
        // In MoviesController.cs

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
            Console.WriteLine($"ViewModel IsRewatch Received: {viewModel.IsRewatch}"); // <<< LOG IsRewatch FROM VIEWMODEL
            Console.WriteLine($"ViewModel Subscribed Received: {viewModel.Subscribed}"); // Log Subscribed if you're using it
            Console.WriteLine("-----------------------------");

            if (ModelState.IsValid)
            {
                Console.WriteLine("Edit POST - ModelState IS VALID. Proceeding to save.");
                var movieEntity = await _dbContext.Movies.FindAsync(viewModel.Id);

                if (movieEntity is not null)
                {
                    Console.WriteLine($"Edit POST - Movie Found. Original DB Values - ReleasedYear: [{movieEntity.ReleasedYear}], IsRewatch: [{movieEntity.IsRewatch}] for Movie ID {viewModel.Id}");

                    // Assign values from ViewModel to the Entity
                    movieEntity.Title = viewModel.Title;
                    movieEntity.Director = viewModel.Director;
                    movieEntity.ReleasedYear = viewModel.ReleasedYear;
                    movieEntity.DateWatched = viewModel.DateWatched;
                    movieEntity.WatchedLocation = viewModel.WatchedLocation!.Value;
                    movieEntity.PosterPath = viewModel.PosterPath;
                    movieEntity.Overview = viewModel.Overview;
                    movieEntity.IsRewatch = viewModel.IsRewatch;       // Assign IsRewatch
                    movieEntity.Subscribed = viewModel.Subscribed;
                    movieEntity.UserRating = viewModel.UserRating;

                    Console.WriteLine($"Edit POST - Entity values BEFORE SaveChangesAsync - Director: '{movieEntity.Director}', Year: '{movieEntity.ReleasedYear}', IsRewatch: {movieEntity.IsRewatch} for Movie ID {movieEntity.Id}"); // <<< LOG IsRewatch ON ENTITY
                    Console.WriteLine($"Edit POST - Entity values BEFORE SaveChangesAsync - UserRating: [{movieEntity.UserRating}]");

                    try
                    {
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"Edit POST - SaveChangesAsync completed successfully for Movie ID {movieEntity.Id}. Redirecting to List.");
                        return RedirectToAction("List", "Movies");
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        Console.WriteLine($"Edit POST - DbUpdateConcurrencyException: {ex.Message}");
                        ModelState.AddModelError("", "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Edit POST - Exception during SaveChangesAsync: {ex.Message}");
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
            Console.WriteLine("Edit POST - Returning View(viewModel) due to invalid ModelState or save error.");
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

        // GET: Movies/TestTmdbSearch?query=YourQuery
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