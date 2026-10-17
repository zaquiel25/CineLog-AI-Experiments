using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using Ezequiel_Movies.Models;

namespace Ezequiel_Movies.Controllers;

public class HomeController : Controller
{
    private const int HeroPosterCount = 3;

    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly TmdbService _tmdbService;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager,
        TmdbService tmdbService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _userManager = userManager;
        _tmdbService = tmdbService;
    }

    /// <summary>
    /// FEATURE: Home hero shows the signed-in user's last three logged movies.
    /// Visitors (or users with fewer than three posters) get TMDB trending movies to fill the gap.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var posters = new List<HomePoster>();

        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            posters = await _dbContext.Movies
                .AsNoTracking()
                .Where(m => m.UserId == userId && m.PosterPath != null && m.PosterPath != "")
                .OrderByDescending(m => m.DateCreated)
                .Take(HeroPosterCount)
                .Select(m => new HomePoster(m.Title, m.PosterPath!))
                .ToListAsync();
        }

        if (posters.Count < HeroPosterCount)
        {
            // Trending is cached by TmdbService; on failure it returns an empty list and the hero renders without posters.
            var trending = await _tmdbService.GetTrendingMoviesAsync();
            posters.AddRange(trending
                .Where(t => !string.IsNullOrEmpty(t.PosterPath) && posters.All(p => p.PosterPath != t.PosterPath))
                .Take(HeroPosterCount - posters.Count)
                .Select(t => new HomePoster(t.Title ?? string.Empty, t.PosterPath!)));
        }

        return View(new HomeIndexViewModel { Posters = posters });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// FEATURE: PWA installation instructions page — publicly accessible.
    /// </summary>
    public IActionResult FreeApp()
    {
        return View();
    }

    /// <summary>
    /// FEATURE: Offline fallback page for PWA service worker.
    /// </summary>
    public IActionResult Offline()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

