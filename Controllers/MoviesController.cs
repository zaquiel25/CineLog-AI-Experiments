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
using Microsoft.AspNetCore.Http;
using Ezequiel_Movies.Extensions;

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

        // In MoviesController.cs

        [HttpGet]
        public async Task<IActionResult> SelectGenre()
        {
            _logger.LogInformation("SelectGenre action invoked.");

            var loggedMovies = await _dbContext.Movies
                .Where(m => m.TmdbId.HasValue)
                .ToListAsync();

            if (!loggedMovies.Any())
            {
                ViewData["SuggestionTitle"] = "Log some movies to get genre suggestions!";
                ViewData["ShowAddMovieButton"] = true;
                return View("Suggest");
            }

            // Fetch all genres from our service one time
            var allGenres = await _tmdbService.GetAllGenresAsync();
            var allGenresDict = allGenres.ToDictionary(g => g.Id, g => g.Name);

            var genreCounts = new Dictionary<int, int>();

            // This can be slow if there are many movies. We can optimize it later.
            foreach (var movie in loggedMovies)
            {
                var movieDetails = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId.Value);
                if (movieDetails?.Genres != null)
                {
                    foreach (var genre in movieDetails.Genres)
                    {
                        if (genreCounts.ContainsKey(genre.Id))
                        {
                            genreCounts[genre.Id]++;
                        }
                        else
                        {
                            genreCounts[genre.Id] = 1;
                        }
                    }
                }
            }

            // Get the top 6 genres based on frequency
            var topGenreIds = genreCounts.OrderByDescending(kvp => kvp.Value).Take(6).Select(kvp => kvp.Key).ToList();

            var topGenres = allGenres.Where(g => topGenreIds.Contains(g.Id)).ToList();

            ViewData["GenreSuggestions"] = topGenres;
            ViewData["SuggestionTitle"] = "Choose Your Favorite Genre";

            return View("Suggest");
        }

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


        // In MoviesController.cs - PASTE THIS ENTIRE BLOCK OF CODE

        #region Suggestion Actions & Helpers

        [HttpGet]
        public async Task<IActionResult> ShowSuggestions(string suggestionType, string? query = null, int page = 1)
        {
            _logger.LogInformation("ShowSuggestions invoked with type: {Type}, query: {Query}, page: {Page}", suggestionType, query, page);

            List<TmdbMovieBrief> suggestedMovies = new List<TmdbMovieBrief>();
            string suggestionTitle = "Suggestions";
            string nextSuggestionType = suggestionType;
            string? nextQuery = query;

            switch (suggestionType?.ToLower())
            {
                case "trending":
                    suggestionTitle = "Trending Movies Today";
                    suggestedMovies = (await _tmdbService.GetTrendingMoviesAsync(page)).Take(3).ToList();
                    ViewData["NextPage"] = page + 1;
                    break;

                case "director_recent":
                case "director_frequent":
                case "director_rated":
                case "director_random":
                    // This entire block handles the director suggestion cycle
                    #region Director Suggestion Logic
                    var loggedDirectorMovies = await _dbContext.Movies.Where(m => !string.IsNullOrEmpty(m.Director) && m.Director != "N/A" && m.TmdbId.HasValue).ToListAsync();
                    if (!loggedDirectorMovies.Any())
                    {
                        suggestionTitle = "Log some movies to get director suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }

                    var topDirectorQueue = new List<string>();
                    if (loggedDirectorMovies.OrderByDescending(m => m.DateWatched).FirstOrDefault()?.Director is string rd) topDirectorQueue.Add(rd);
                    if (loggedDirectorMovies.GroupBy(m => m.Director!).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() is string fd && !topDirectorQueue.Contains(fd)) topDirectorQueue.Add(fd);
                    if (loggedDirectorMovies.Where(m => m.UserRating.HasValue).GroupBy(m => m.Director!).Select(g => new { Name = g.Key, Avg = g.Average(m => m.UserRating!.Value) }).OrderByDescending(d => d.Avg).Select(d => d.Name).FirstOrDefault() is string trd && !topDirectorQueue.Contains(trd)) topDirectorQueue.Add(trd);

                    string? directorToSuggest = null;
                    if (suggestionType == "director_recent") { directorToSuggest = topDirectorQueue.ElementAtOrDefault(0); nextSuggestionType = "director_frequent"; }
                    else if (suggestionType == "director_frequent") { directorToSuggest = topDirectorQueue.ElementAtOrDefault(1); nextSuggestionType = "director_rated"; }
                    else if (suggestionType == "director_rated") { directorToSuggest = topDirectorQueue.ElementAtOrDefault(2); nextSuggestionType = "director_random"; }
                    else if (suggestionType == "director_random")
                    {
                        var allDirectors = loggedDirectorMovies.Select(m => m.Director!).Distinct().ToList();
                        var potentialDirectors = allDirectors.Where(d => d != query).ToList();
                        if (!potentialDirectors.Any()) potentialDirectors = allDirectors;
                        directorToSuggest = potentialDirectors.Any() ? potentialDirectors[new Random().Next(potentialDirectors.Count)] : null;
                        nextSuggestionType = "director_random";
                    }

                    if (string.IsNullOrEmpty(directorToSuggest)) return RedirectToAction("ShowSuggestions", new { suggestionType = "director_random" });

                    suggestedMovies = await GetSuggestionsForDirector(directorToSuggest); suggestionTitle = $"Because you like {directorToSuggest}...";
                    nextQuery = directorToSuggest;
                    if (!suggestedMovies.Any()) suggestionTitle = $"You've seen all available movies by {directorToSuggest}! Try reshuffling.";
                    #endregion
                    break;

                case "genre_recent":
                case "genre_frequent":
                case "genre_rated":
                case "genre_random":
                    // This entire block handles the genre suggestion cycle
                    #region Genre Suggestion Logic
                    var loggedGenreMovies = await _dbContext.Movies.Where(m => !string.IsNullOrEmpty(m.Genres) && m.TmdbId.HasValue).ToListAsync();
                    if (!loggedGenreMovies.Any())
                    {
                        suggestionTitle = "Log movies with genres to get suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }

                    var allUserGenres = loggedGenreMovies.SelectMany(m => m.Genres!.Split(new[] { ", " }, StringSplitOptions.None)).ToList();

                    var topGenreQueue = new List<string>();
                    if (loggedGenreMovies.OrderByDescending(m => m.DateWatched).FirstOrDefault()?.Genres?.Split(new[] { ", " }, StringSplitOptions.None).FirstOrDefault() is string rg) topGenreQueue.Add(rg.Trim());
                    if (allUserGenres.GroupBy(g => g).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() is string fg && !topGenreQueue.Contains(fg)) topGenreQueue.Add(fg);
                    var highestRatedGenres = loggedGenreMovies.Where(m => m.UserRating >= 4.0m).SelectMany(m => m.Genres!.Split(new[] { ", " }, StringSplitOptions.None)).ToList();
                    if (highestRatedGenres.GroupBy(g => g).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() is string hg && !topGenreQueue.Contains(hg)) topGenreQueue.Add(hg);

                    string? genreToSuggest = null;
                    if (suggestionType == "genre_recent") { genreToSuggest = topGenreQueue.ElementAtOrDefault(0); nextSuggestionType = "genre_frequent"; }
                    else if (suggestionType == "genre_frequent") { genreToSuggest = topGenreQueue.ElementAtOrDefault(1); nextSuggestionType = "genre_rated"; }
                    else if (suggestionType == "genre_rated") { genreToSuggest = topGenreQueue.ElementAtOrDefault(2); nextSuggestionType = "genre_random"; }
                    else if (suggestionType == "genre_random")
                    {
                        var potentialGenres = allUserGenres.Distinct().Where(g => g != query).ToList();
                        if (!potentialGenres.Any()) potentialGenres = allUserGenres.Distinct().ToList();
                        genreToSuggest = potentialGenres.Any() ? potentialGenres[new Random().Next(potentialGenres.Count)] : null;
                        nextSuggestionType = "genre_random";
                    }

                    if (string.IsNullOrEmpty(genreToSuggest)) { return RedirectToAction("ShowSuggestions", new { suggestionType = "genre_random" }); }

                    suggestedMovies = await GetSuggestionsForGenre(genreToSuggest);
                    suggestionTitle = $"Popular {genreToSuggest} Movies";
                    nextQuery = genreToSuggest;
                    if (!suggestedMovies.Any()) { suggestionTitle = $"Couldn't find new suggestions for {genreToSuggest}. Try reshuffling."; }
                    #endregion
                    break;

                case "cast_recent":
                case "cast_frequent":
                case "cast_rated": // We'll use TMDB popularity as a proxy for "rated"
                case "cast_random":
                    var loggedCastMovies = await _dbContext.Movies.Where(m => m.TmdbId.HasValue).OrderByDescending(m => m.DateWatched).ToListAsync();
                    if (!loggedCastMovies.Any())
                    {
                        suggestionTitle = "Log some movies to get cast suggestions!";
                        ViewData["ShowAddMovieButton"] = true;
                        break;
                    }

                    // NOTE: This part is slow. We can optimize it later by storing cast info locally.
                    var allTopActors = new List<TmdbCastPerson>();
                    foreach (var movie in loggedCastMovies.Take(15)) // Analyze last 15 movies for speed
                    {
                        var details = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId.Value);
                        if (details?.Credits?.Cast != null) allTopActors.AddRange(details.Credits.Cast.Take(5));
                    }
                    if (!allTopActors.Any()) { suggestionTitle = "Could not find cast info in your recent logs."; break; }

                    var topActorQueue = new List<TmdbCastPerson>();
                    if (allTopActors.FirstOrDefault() is TmdbCastPerson ra) topActorQueue.Add(ra);
                    if (allTopActors.GroupBy(a => a.Id).OrderByDescending(g => g.Count()).Select(g => g.First()).FirstOrDefault() is TmdbCastPerson fa && !topActorQueue.Any(p => p.Id == fa.Id)) topActorQueue.Add(fa);
                    if (allTopActors.OrderByDescending(a => a.Popularity).FirstOrDefault() is TmdbCastPerson pa && !topActorQueue.Any(p => p.Id == pa.Id)) topActorQueue.Add(pa);

                    TmdbCastPerson? actorToSuggest = null;
                    if (suggestionType == "cast_recent") { actorToSuggest = topActorQueue.ElementAtOrDefault(0); nextSuggestionType = "cast_frequent"; }
                    else if (suggestionType == "cast_frequent") { actorToSuggest = topActorQueue.ElementAtOrDefault(1); nextSuggestionType = "cast_rated"; }
                    else if (suggestionType == "cast_rated") { actorToSuggest = topActorQueue.ElementAtOrDefault(2); nextSuggestionType = "cast_random"; }
                    else if (suggestionType == "cast_random")
                    {
                        var potentialActors = allTopActors.DistinctBy(p => p.Id).Where(p => p.Name != query).ToList();
                        if (!potentialActors.Any()) potentialActors = allTopActors.DistinctBy(p => p.Id).ToList();
                        actorToSuggest = potentialActors.Any() ? potentialActors[new Random().Next(potentialActors.Count)] : null;
                        nextSuggestionType = "cast_random";
                    }

                    if (actorToSuggest == null) { return RedirectToAction("ShowSuggestions", new { suggestionType = "cast_random" }); }

                    var actorDetails = await _tmdbService.GetPersonDetailsAsync(actorToSuggest.Id);
                    suggestedMovies = await GetSuggestionsForActor(actorToSuggest.Id);

                    suggestionTitle = $"Because you like movies with {actorToSuggest.Name}";
                    ViewData["ActorProfilePath"] = actorDetails?.ProfilePath;
                    nextQuery = actorToSuggest.Name;
                    break;

                default:
                    return RedirectToAction("Suggest");
            }

            ViewData["SuggestionTitle"] = suggestionTitle;
            ViewData["NextSuggestionType"] = nextSuggestionType;
            ViewData["NextQuery"] = nextQuery;
            return View("Suggest", suggestedMovies);
        }



        // THIS IS THE NEW, SIMPLIFIED HELPER METHOD
        private async Task<List<TmdbMovieBrief>> GetSuggestionsForDirector(string directorName)
        {
            var directorId = await _tmdbService.GetPersonIdAsync(directorName);
            if (!directorId.HasValue) return new List<TmdbMovieBrief>();

            // Get the director's ENTIRE filmography from our service
            var allDirectorMovies = await _tmdbService.GetDirectorFilmographyAsync(directorId.Value);

            if (!allDirectorMovies.Any())
            {
                return new List<TmdbMovieBrief>(); // Return empty if they have no movies
            }

            // If they have 3 or fewer movies, just return them all.
            if (allDirectorMovies.Count <= 3)
            {
                return allDirectorMovies;
            }

            // Otherwise, shuffle the entire list and take 3 random ones.
            var random = new Random();
            var randomSuggestions = allDirectorMovies.OrderBy(x => random.Next()).Take(3).ToList();

            return randomSuggestions;
        }

        private async Task<List<TmdbMovieBrief>> GetSuggestionsForGenre(string genreName)
        {
            var allGenres = await _tmdbService.GetAllGenresAsync();
            var genre = allGenres.FirstOrDefault(g => g.Name != null && g.Name.Equals(genreName, StringComparison.OrdinalIgnoreCase));
            if (genre == null) return new List<TmdbMovieBrief>();

            string sessionKey = $"GenrePage_{genreName.Replace(" ", "")}";
            int pageToFetch = HttpContext.Session.GetInt32(sessionKey) ?? 1;
            _logger.LogInformation("HELPER: Finding movies for genre {Genre}, starting at page {Page}", genreName, pageToFetch);

            var movies = await _tmdbService.DiscoverMoviesByGenreAsync(genre.Id, pageToFetch);

            if (movies.Any())
            {
                HttpContext.Session.SetInt32(sessionKey, pageToFetch + 1);
            }
            else
            {
                // If we ran out of pages, reset and fetch page 1 again as a fallback.
                HttpContext.Session.SetInt32(sessionKey, 1);
                movies = await _tmdbService.DiscoverMoviesByGenreAsync(genre.Id, 1);
            }

            return movies.Take(3).ToList();
        }

        private async Task<List<TmdbMovieBrief>> GetSuggestionsForActor(int actorId)
        {
            // This uses the same session-based paging logic as our working Genre helper
            string sessionKey = $"ActorPage_{actorId}";
            int pageToFetch = HttpContext.Session.GetInt32(sessionKey) ?? 1;

            _logger.LogInformation("HELPER: Finding movies for actor {ActorId}, starting at page {Page}", actorId, pageToFetch);

            var movies = await _tmdbService.DiscoverMoviesByActorAsync(actorId, pageToFetch);

            if (movies.Any())
            {
                // If we found movies, update the session to get the next page next time
                HttpContext.Session.SetInt32(sessionKey, pageToFetch + 1);
            }
            else
            {
                // If we ran out of movies for this actor, reset the page tracker for next time and fetch page 1 again
                _logger.LogWarning("No movies found on page {Page} for actor {ActorId}. Resetting to page 1.", pageToFetch, actorId);
                HttpContext.Session.SetInt32(sessionKey, 1);
                movies = await _tmdbService.DiscoverMoviesByActorAsync(actorId, 1);
            }

            return movies.Take(3).ToList();
        }

        #endregion

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

                // VVVV NEW LOGIC TO FETCH AND SAVE GENRES VVVV
                if (movie.TmdbId.HasValue)
                {
                    var movieDetails = await _tmdbService.GetMovieDetailsAsync(movie.TmdbId.Value);
                    if (movieDetails?.Genres != null && movieDetails.Genres.Any())
                    {
                        // Join the list of genre names into a single, comma-separated string
                        movie.Genres = string.Join(", ", movieDetails.Genres.Select(g => g.Name));
                        _logger.LogInformation("Saving genres for movie '{Title}': {Genres}", movie.Title, movie.Genres);
                    }
                }
                // ^^^^ END OF NEW LOGIC ^^^^

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
                TmdbId = movieEntity.TmdbId,
                Genres = movieEntity.Genres
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
                    movieEntity.TmdbId = viewModel.TmdbId;
                    movieEntity.Genres = viewModel.Genres;// <<< This assignment is crucial and already in your code

                    // VVVV NEW LOGIC TO FETCH AND SAVE GENRES IF THEY ARE MISSING VVVV
                    if (movieEntity.TmdbId.HasValue && string.IsNullOrEmpty(movieEntity.Genres))
                    {
                        var movieDetails = await _tmdbService.GetMovieDetailsAsync(movieEntity.TmdbId.Value);
                        if (movieDetails?.Genres != null && movieDetails.Genres.Any())
                        {
                            movieEntity.Genres = string.Join(", ", movieDetails.Genres.Select(g => g.Name));
                            _logger.LogInformation("Populated missing genres for movie '{Title}': {Genres}", movieEntity.Title, movieEntity.Genres);
                        }
                    }
                    // ^^^^ END OF NEW LOGIC ^^^^

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