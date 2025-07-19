## Edge Case: Blacklisting Many Trending Movies

- Si un usuario intenta blacklistear la mayoría o totalidad de las películas trending (por ejemplo, más de 20 en una sola sesión), puede encontrar que los últimos títulos no se pueden blacklistear por limitaciones de estado, caché o validación.
- Este es un caso extremo y poco realista en el uso normal de la app. La mayoría de los usuarios nunca intentarán blacklistear tantas películas trending de una vez.
- Se decidió no priorizar la solución de este edge case para optimizar el tiempo de desarrollo y enfocarse en flujos que impactan a la mayoría de los usuarios.
- El sistema ya muestra un mensaje amigable cuando no hay más sugerencias trending disponibles, por lo que el usuario nunca queda "atrapado".
## UI/UX Patterns
- Mutual exclusion implemented preventively via conditional rendering
- Error states avoided through visual state management
- Consistent behavior across Details and Preview pages
#
# Model Comment Standards (2025-07-18)
#
# - Todos los modelos (`Models/`) han sido limpiados de comentarios de desarrollo, temporales y anotaciones vagas.
# - Solo se mantienen comentarios técnicos, explicaciones de validación y documentación relevante para producción.
# - No se permiten comentarios tipo "ADD THIS", "NUEVAS", ni notas de importancia sin justificación técnica.
# - Los atributos de validación y la lógica funcional permanecen intactos.
# - Los futuros cambios en modelos deben seguir este estándar de documentación.
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
## Commenting & Documentation Standards
- All controller comments (especially in `MoviesController.cs`) now follow a professional, business-logic-focused style.
- Development artifacts, redundant, and shallow comments have been removed.
- Comments should explain "why" for business rules, suggestion systems, and session/anti-repetition logic.
- Future contributions must maintain this standard: avoid obsolete, non-English, or low-value comments.
- See `CHANGELOG.md` (2025-07-17) for details on the latest comment refactor.
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

## Business Rules

### Mutual Exclusion Policy
- A movie cannot exist in both wishlist and blacklist for the same user
- When adding to one list, check if movie exists in the other
- Provide clear error messages guiding user to resolve conflicts

### UI/UX Principles
- Always provide a way to reshuffle suggestions, even if no results available
- Use AJAX for non-navigational actions (add/remove from lists)
- Maintain visual feedback for all user interactions
- Preserve user's place in suggestion flow during list management

### Suggestion System Behavior
- Implement bulletproof fallbacks for edge cases
- Track session state to avoid repetitive suggestions (Session sequencing is only used on the initial suggestion click; all reshuffles use client parameters.)
- Handle empty result sets gracefully with actionable next steps
- The "Reshuffle" button is implemented via event delegation and always maintains the correct context for all suggestion types.
- IMemoryCache is used for TMDB API data; Session State is used for user-specific anti-repetition and sequencing.


## AJAX & Hybrid Suggestion Implementation (2025-07-18)

- El sistema de sugerencias ahora implementa un patrón híbrido:
  - Para el tipo "Trending", el reshuffle se realiza vía AJAX y el endpoint devuelve HTML renderizado del servidor (partial views), no JSON puro.
  - Esto garantiza que los posters y paths de imágenes funcionen correctamente, ya que el renderizado server-side respeta la lógica de rutas y helpers de ASP.NET MVC.
  - El resto de tipos de sugerencia siguen usando navegación tradicional, pero el patrón es extensible a más tipos si se desea AJAXizar.
- Justificación técnica:
  - El renderizado HTML server-side evita problemas de rutas relativas/absolutas y CORS con imágenes de TMDB.
  - Permite reutilizar la misma partial view que en el render inicial, manteniendo DRY y consistencia visual.
  - Facilita el mantenimiento y la extensión futura del sistema de sugerencias.
- Notas de implementación:
  - El botón de reshuffle para trending usa data-suggestion-type y event delegation en JS para disparar el fetch AJAX.
  - Tras reemplazar el grid de sugerencias, siempre se re-adjuntan los event listeners para mantener la funcionalidad AJAX de los formularios internos.
  - Los comentarios en C# y JS deben documentar el propósito, el porqué del enfoque y las mejores prácticas de mantenimiento.
  - Ver ejemplos y convenciones en `MoviesController.cs` y `Views/Movies/Suggest.cshtml`.

## AJAX Implementation Notes

### Anti-Forgery Protection
- All AJAX requests include anti-forgery tokens via Razor helpers
- Use `@Html.AntiForgeryToken()` in forms submitted via JavaScript

### User Security
- All POST actions are filtered by `UserId` to ensure data isolation
- Authorization attributes protect all user-specific operations
- No user data exposure across accounts

### Error Handling
- AJAX operations include comprehensive error handling
- Failed operations provide user feedback and restore UI state
- Network errors are handled gracefully with retry options

---

For new features, always:
- Use session sequencing only on the initial suggestion click; trust client parameters for all reshuffles.
- Implement AJAX-powered UI actions using event delegation for all dynamic elements.
- Use IMemoryCache for API data and Session State for user-specific anti-repetition and sequencing.
- Follow the established patterns in `MoviesController.cs` and use `TmdbService` for all TMDB interactions.
- Filter all user data queries by user ID for privacy and correctness.

---

For new features, follow the established patterns in `MoviesController.cs` and use `TmdbService` for all TMDB interactions. Always filter data by user ID for privacy and correctness.
