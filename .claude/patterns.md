# CineLog Detailed Code Patterns

Reference file for detailed implementation patterns. Read this when working on specific areas.

## Two-Layer Authentication

```csharp
// Identity as default scheme, PasswordGate as named scheme
builder.Services.AddAuthentication()
    .AddCookie("PasswordGate", options =>
    {
        options.LoginPath = "/PasswordGate";
    });
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// CORRECT: Explicit authentication against named scheme
var result = await HttpContext.AuthenticateAsync("PasswordGate");
var isAuthenticated = result.Succeeded &&
    result.Principal.HasClaim("PasswordGate", "granted");

// WRONG: Only checks default scheme
var isAuthenticated = HttpContext.User.HasClaim("PasswordGate", "granted");
```

Supports multiple password key formats: SitePassword, Sitepassword, SiteAccess:Password.

## TMDB Director Validation

```csharp
// 1. Check known directors cache first (0 API calls)
if (KnownDirectors.TryGetValue(personName, out int knownId))
    return knownId;

// 2. Validate with director credits for disambiguation
var directorCredits = creditsResponse?.Crew?.Count(c => c.Job == "Director") ?? 0;
if (directorCredits > 0) return candidate.Id;
```

70-90% reduction in TMDB API usage through intelligent caching and validation.

## Azure Production Configuration

```csharp
// Automatic placeholder replacement in production
if (builder.Environment.IsProduction() && connectionString.Contains("{DatabasePassword}"))
{
    var databasePassword = builder.Configuration["DatabasePassword"];
    connectionString = connectionString.Replace("{DatabasePassword}", databasePassword);
}
```

## URL Parameter Handling

```csharp
// RouteValueDictionary for conditional parameter inclusion
object CreateRouteValues(object baseValues, bool includeFirstWatch = true)
{
    var routeDict = new RouteValueDictionary(baseValues);
    if (includeFirstWatch && isFirstWatchOnly)
    {
        routeDict["firstWatchOnly"] = "true";
    }
    return routeDict;
}
```

Rules: Never include boolean params as empty strings. Use RouteValueDictionary for complex scenarios.

## AJAX Layout Stability

```javascript
// Preserve Bootstrap structure during AJAX replacement
const currentContainer = currentMain.querySelector('.container');
const newContainer = newMain.querySelector('.container');
if (currentContainer && newContainer) {
    currentContainer.innerHTML = newContainer.innerHTML;
}
```

- Target `.container` elements, never replace `main` completely
- Use invisible placeholders for consistent column widths
- `html { overflow-y: scroll; }` prevents scrollbar width changes
- Reserve minimum height for dynamic components (Timeline Navigator)

## TMDB Service Patterns

```csharp
// Rate limiting
await _tmdbSemaphore.WaitAsync();
try { /* API call */ }
finally { _tmdbSemaphore.Release(); }

// Caching
var cacheKey = $"tmdb_movie_{movieId}";
if (_memoryCache.TryGetValue(cacheKey, out TmdbMovieDetails? cached))
    return cached;
_memoryCache.Set(cacheKey, result, TimeSpan.FromHours(24));

// Batch calls
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);
```

## Debugging Methodology

1. Consider user workflow before assuming technical bugs
2. Trace parameter flow through entire request pipeline
3. Compare dev vs production behavior
4. Simplest explanation first — user workflow issues often masquerade as bugs

```csharp
// Add structured logging at each step
_logger.LogInformation("Parameter received: {Value}", value);
_logger.LogInformation("Query filter applied: {HasFilter}", hasFilter);
_logger.LogInformation("Results: {Count} items", count);
```

## Code Review Checklist

- Parameter consistency across all implementations
- Professional XML docs on all new methods
- English-only comments (replace any Spanish)
- FIX/FEATURE/ENHANCEMENT prefixes on significant changes
- UserId filtering on all user data queries
- Async/await on all DB and API calls
- No hardcoded secrets or credentials
