## Movie Preview Card (Add/Edit Movie) - Notas de Estilo y Mantenimiento (2025-07-15)

- Se utiliza un selector ultra específico en el CSS de la tarjeta de movie preview para garantizar que los estilos personalizados prevalezcan sobre Bootstrap y el tema Cyborg.
- No reducir la especificidad del selector ni cambiar la estructura HTML/clases de la tarjeta sin revisar los estilos, ya que puede romper la visual.
- Los colores, jerarquía tipográfica y efectos hover están documentados en la cabecera de `site.css`.
- El overview ahora se muestra completo, sin scrollbar, y con texto justificado para mejor legibilidad.
- Todas las mejoras visuales y de UX se documentan en `CHANGELOG.md`.
- Tras cualquier cambio visual, probar la tarjeta en todos los navegadores y dispositivos para asegurar consistencia.

# Copilot Instructions for CineLog-AI-Experiments

## Project Overview
- **CineLog-AI-Experiments** is an ASP.NET Core MVC web application for logging, suggesting, and managing movies, with deep integration to TMDB (The Movie Database) APIs.
- The main business logic is in `Controllers/MoviesController.cs`, which handles user movie lists, suggestions, and TMDB interactions.
- Data access is via Entity Framework Core, with models in `Models/` and database context in `Data/ApplicationDbContext.cs`.
- TMDB API communication is abstracted in `TmdbService.cs`.

## Key Architectural Patterns
- **Separation of Concerns:**
  - Controllers handle HTTP and user logic.
  - Data models/entities are in `Models/` and `Ezequiel_Movies1.Models.Entities`.
  - External API logic is in `TmdbService.cs`.
- **User-specific Data:**
  - All movie and wishlist actions are filtered by the current user's ID (via ASP.NET Identity).
  - Example: `MoviesController` always queries with `.Where(m => m.UserId == userId)`.
- **Suggestion System:**
  - Movie suggestions are generated based on user's logged data (directors, genres, actors, decades).
  - Helper methods like `GetSuggestionsForDirector`, `GetSuggestionsForGenre`, etc., encapsulate this logic.

## Developer Workflows
- **Build:**
  - Use `dotnet build` in the project root.
- **Run:**
  - Use `dotnet run` or launch via Visual Studio/VS Code.
- **Migrations:**
  - Use `dotnet ef migrations add <Name>` and `dotnet ef database update` for schema changes.
- **Debugging:**
  - Console and logger output is used extensively in controllers for tracing data flow and debugging.

## Project Conventions
- **Model Validation:**
  - Uses custom attributes (e.g., `NoFutureDateAttribute`, `ValidReleasedYearAttribute`) for model validation.
- **Session Usage:**
  - Session is used to store temporary state for suggestions (e.g., `ShownSurpriseIds`).
- **ViewModels:**
  - `AddMoviesViewModel` is used for add/edit forms, mapping to/from entity models.
- **TMDB Integration:**
  - All TMDB lookups and suggestions go through `TmdbService.cs`.
  - Example: `await _tmdbService.GetMovieDetailsAsync(tmdbId)`.

  **UI & Styling:** The project uses **Bootstrap 5** and the dark **'Cyborg' Bootswatch theme**. New UI elements should match this style, using standard Bootstrap classes (card, btn, list-group, etc.).
  - The "Sort By" dropdown on the "My Movies" page must always be visible, with a static grey background (`#6c757d`), white text, and no hover/focus effects. Use inline styles or a dedicated CSS class to ensure consistent appearance.

## UI/UX Consistency
- Always verify that UI changes are visually correct, accessible, and consistent with the app's design system.
- The "Sort By" dropdown must remain always visible and styled as described above.

## Code Efficiency & Quality
- Regularly review for DRYness, async/await usage, null safety, and error handling.
- Optimize database queries and TMDB API calls to avoid unnecessary work (e.g., use helpers, avoid redundant loops, and consider caching for repeated API data).
- Ensure all user data queries are filtered by UserId for security and privacy.
- Document any new architectural or UI conventions in this file after major changes.

## Notable Files & Directories
- `Controllers/MoviesController.cs`: Main controller for movie logic and suggestions.
- `Models/`: Contains view models, TMDB models, and validation attributes.
- `Data/ApplicationDbContext.cs`: Entity Framework Core DB context.
- `TmdbService.cs`: Handles all TMDB API calls.
- `Migrations/`: Entity Framework migration files.

## Patterns & Examples
- **User Filtering:**
  ```csharp
  var userId = _userManager.GetUserId(User);
  var movies = _dbContext.Movies.Where(m => m.UserId == userId);
  ```
- **TMDB API Usage:**
  ```csharp
  var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
  ```
- **Suggestion Helper:**
  ```csharp
  private async Task<List<TmdbMovieBrief>> GetSuggestionsForDirector(string directorName) { ... }
  ```

## Integration Points
- **TMDB API:** All external movie data and suggestions are fetched via TMDB.
- **ASP.NET Identity:** Used for user authentication and per-user data isolation.
- **Data Mapping:** Data from the TMDB API is mapped to C# classes in the 'Models/TmdbApi/' folder, such as TmdbMovieDetails and TmdbMovieBrief.


---

For new features, follow the established patterns in `MoviesController.cs` and use `TmdbService` for all TMDB interactions. Always filter data by user ID for privacy and correctness.
