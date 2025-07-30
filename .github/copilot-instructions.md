
---
**Best Practice:**
> For all AJAX POST requests (especially for Blacklist/Wishlist removal), **always include** the `X-Requested-With: XMLHttpRequest` header. This guarantees the backend returns JSON for AJAX, not HTML error pages, and prevents frontend parsing errors. This is required for robust, user-friendly error handling in all AJAX-powered UI actions.


## ⚠️ CRITICAL INSTRUCTIONS

### 🚫 NEVER Auto-Commit
- **NEVER** stage and commit files automatically
- Only commit when explicitly asked by the user

### 🔨 Build Requirements
- **CRITICAL**: A task is NEVER complete if the application cannot build successfully
- **ALWAYS** run `dotnet build` to verify compilation before marking tasks finished
- Build failures MUST be resolved as part of implementation, not left for later

### 📋 Task Management
- **ALWAYS** use TodoWrite tool for complex multi-step tasks
- **MUST** keep working until ALL todo items are checked off
- **NEVER** end your turn until problem is completely solved and verified

### 🎯 Autonomous Operation
- You are an agent - keep going until user's query is completely resolved
- You have everything needed to solve problems autonomously
- When user says "resume" or "continue", check todo list for next incomplete step

### 💬 Communication
- **ALWAYS** tell user what you're going to do before making tool calls
- Be concise but thorough - avoid unnecessary repetition
- When you say "I will do X", you **MUST** actually do X

### 🤔 Question Before Replicating
**Critical thinking approach:**
- **ALWAYS** question why existing patterns might fail in new contexts
- Verbalize fundamental differences (e.g., "Director" is unique entity, "Decade" is paginated group)
- Adapt solutions to new complexity from the start
- **NEVER** assume old patterns will work without analysis

### 📐 Literal Implementation Over Creative Interpretation
**Stick to requirements:**
- Implement **ONLY** explicitly requested functionality and logic
- **NEVER** assume, infer, or add "improvements" unless asked
- Creativity is limited to accomplishing what's requested, not expanding requirements
- When in doubt, ask for clarification rather than assume

---

<!-- AJAX REMOVAL PATTERN: Blacklist & Wishlist -->
<button type="button" class="btn btn-soft-danger btn-sm remove-from-blacklist" data-tmdb-id="12345">Remove</button>
<button type="button" class="btn btn-soft-danger btn-sm remove-from-wishlist" data-tmdb-id="12345">Remove</button>
```

#### **AJAX Removal Pattern (Blacklist/Wishlist)**
// Best Practice: Always use 'credentials: same-origin' and include the anti-forgery token in the body for secure, authenticated requests.
```javascript
// Always include X-Requested-With header to guarantee JSON response from backend
fetch('/Movies/RemoveFromBlacklist', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'X-Requested-With': 'XMLHttpRequest'
    },
    body: `tmdbId=${encodeURIComponent(tmdbId)}&__RequestVerificationToken=${encodeURIComponent(token)}`,
    credentials: 'same-origin'
});
// Robust error handling: always parse response as text, then try JSON, fallback to alert
```

#### **Troubleshooting**
- If you see a "Non-JSON response" error in the UI, ensure your AJAX request includes the `X-Requested-With: XMLHttpRequest` header and the backend action returns JSON for all AJAX cases.
+ If you see a "Non-JSON response" error in the UI, ensure your AJAX request includes the `X-Requested-With: XMLHttpRequest` header and the backend action returns JSON for all AJAX cases.
+ Quick tip: You can inspect network requests in your browser's dev tools to confirm the header is present and the response is valid JSON.
- If you see a "Non-JSON response" error in the UI, ensure your AJAX request includes the `X-Requested-With: XMLHttpRequest` header and the backend action returns JSON for all AJAX cases.

## 🔄 Development Workflow

### 1. 🧠 Problem Analysis
**Understand the problem deeply before coding:**
- Carefully read the issue and think critically about requirements
- Consider expected behavior, edge cases, and potential pitfalls
- Understand how it fits into the larger codebase context
- Identify dependencies and interactions with other components

### 2. 🔍 Codebase Investigation
**Explore and understand the existing code:**
- Explore relevant files and directories
- Search for key functions, classes, or variables related to the issue
- Read and understand relevant code snippets (2000 lines at a time for context)
- Identify the root cause of the problem
- Continuously validate and update understanding
- **Look for unified helper methods** that implement consistent business logic
- **Check for existing patterns** before implementing new solutions

### 3. 📋 Planning & Todo Management
**Create a detailed, step-by-step plan:**
- Outline specific, simple, and verifiable sequence of steps
- **ALWAYS** create todo list using TodoWrite tool for complex tasks
- Check off steps using [x] syntax as you complete them
- **MUST** continue to next step after checking off previous step
- Never end turn until ALL todo items are completed

### 4. ⚙️ Implementation
**Make incremental, testable changes:**
- Always read relevant file contents before editing
- Make small, logical changes that follow from investigation
- When detecting environment variables needed, proactively create .env file
- Test frequently after each change
- **ALWAYS** run `dotnet build` to verify compilation
- **Follow CineLog patterns** for user data isolation, caching, and API usage
- **Use unified helper methods** for consistent business logic

### 5. 🐛 Debugging & Testing
**Ensure robust solutions:**
- Use debugging tools to check for problems
- Determine root causes, not just symptoms
- Use structured `_logger` calls instead of console output
- Test edge cases rigorously - this is the #1 failure mode
- Iterate until solution is perfect and all tests pass
- **Verify user data isolation** and proper filtering by UserId
---

## 📝 Communication & Documentation Guidelines

### 💬 Communication Style
**Be clear, direct, and professional:**
- Use casual but professional tone
- Communicate what you're doing before tool calls
- Respond with clear, direct answers using bullet points
- Avoid unnecessary explanations, repetition, and filler
- Only elaborate when essential for accuracy

### 📋 Todo List Format
**When using TodoWrite tool, follow these patterns:**
```markdown
- [ ] Step 1: Description of the first step
- [ ] Step 2: Description of the second step
- [ ] Step 3: Description of the third step
```
- **NEVER** use HTML tags for todo lists
- Always wrap in triple backticks for proper formatting
- Show completed todo list to user at end of messages

### 💻 Code Handling
**Direct file editing approach:**
- **ALWAYS** write code directly to files (don't display unless asked)
- Read relevant file contents before editing for complete context
- Make incremental changes with clear commit messages when requested

---
## 🔧 Tool Usage & File Management

### 🧠 Memory Management
**User preference storage:**
- Memory stored in `.github/instructions/memory.instruction.md`
- **MUST** include front matter when creating memory file:
```yaml
---
applyTo: '**'
---
```
- Update memory when user asks to remember preferences

### 📁 File Reading Efficiency
**Avoid redundant file reads:**
- Check if file already read before re-reading
- Only re-read if:
  - Content suspected to have changed
  - You made edits to the file
  - Error suggests stale context
- Use internal memory to avoid redundant operations

### ✍️ Writing & Prompts
**Markdown formatting standards:**
- Generate prompts in markdown format
- Wrap prompts in triple backticks for copying
- Todo lists MUST be markdown format in triple backticks

### 🔄 Git Operations
**Version control guidelines:**
- Only stage and commit when explicitly told by user
- **NEVER** auto-commit (see Critical Instructions above)

---

## 🎯 CineLog-Specific Development Principles

### 🏗️ Architecture Consistency
**Maintain existing patterns:**
- ✅ Maintain consistency with existing architecture
- ✅ Implement appropriate error handling
- Always follow established CineLog patterns and conventions

### 🤔 Question Before Replicating
**Critical thinking approach:**
- **ALWAYS** question why existing patterns might fail in new contexts
- Verbalize fundamental differences (e.g., "Director" is unique entity, "Decade" is paginated group)
- Adapt solutions to new complexity from the start
- **NEVER** assume old patterns will work without analysis

### 📐 Literal Implementation Over Creative Interpretation
**Stick to requirements:**
- Implement **ONLY** explicitly requested functionality and logic
- **NEVER** assume, infer, or add "improvements" unless asked
- Creativity is limited to accomplishing what's requested, not expanding requirements
- When in doubt, ask for clarification rather than assume

### 📝 Documentation & Comments Standards

#### 🎯 XML Documentation (Required)
- **All public methods** must have comprehensive XML documentation
- **Include purpose, parameters, returns, and remarks** when applicable
- **Document edge cases** and special behavior
- **Example format**:
```csharp
/// <summary>
/// Brief description of what the method does and its purpose.
/// 
/// FIX/FEATURE: Add context for significant changes or new functionality.
/// </summary>
/// <param name="paramName">Description of parameter and its constraints</param>
/// <returns>Description of return value and possible states</returns>
/// <remarks>
/// Additional context about implementation details, performance considerations,
/// or architectural decisions that future developers should understand.
/// </remarks>
```

#### 🔧 Inline Comments (Professional Standards)
- **Explain "why" not "what"** - focus on business logic and reasoning
- **Use FIX/FEATURE/ENHANCEMENT prefixes** for significant changes
- **Add context for complex logic** or non-obvious solutions
- **Comment architectural decisions** and trade-offs
- **English only** - replace any Spanish comments with English equivalents
- **Examples**:
```csharp
// FIX: Check if director has available movies before adding to queue
// This prevents showing "No more suggestions available" message
if (await HasAvailableMoviesForDirector(trimmed, userId))

// PERFORMANCE: Use batch API calls to prevent N+1 queries
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

// ARCHITECTURE: Session-based sequencing for anti-repetition
string directorTypeKey = $"DirectorTypeSequence_{userId}";
```

#### 🚫 Avoid These Comment Patterns
- Development artifacts like "ADD THIS", "NUEVAS", "TODO"
- Shallow comments that just repeat the code
- Spanish comments (replace with English)
- Comments without business justification or technical value

#### 📋 Documentation Requirements
- **English only** for international collaboration
- **Professional tone** - avoid casual or development-only comments
- **Business-focused** - explain impact and purpose
- **Maintainable** - help future developers understand decisions
- **Consistent formatting** - follow established patterns in codebase
- **Structured logging** - use `_logger.LogInformation("English message")` instead of console output

---


### Director Suggestion Sequencing (2025-07-24)
- DirectorReshuffle implements intelligent sequencing with case-insensitive deduplication
- Priority order: recent director → frequent director → top-rated director → random
- Deduplication prevents the same director appearing twice when they qualify for multiple categories
- Example: If Steven Spielberg is both "recent" and "frequent", he only appears once in the sequence
- Session state tracks sequence position per user, advancing with each reshuffle
- Random phase includes anti-repetition to avoid immediately repeating the last random selection
- The deduplication approach is elegant: solve at data level (HashSet) rather than logic level (skip patterns)

### Dynamic Variety and Consistency Improvements
- **Dynamic Variety System**: Decade suggestions now use randomized sort criteria (`popularity.desc`, `vote_average.desc`, `release_date.desc`) and page (1-3) for every suggestion, matching the genre system.
- **Triple Fallback Logic**: Robust fallback system for each decade:
  - Primary: Random sort + random page combination
  - Fallback 1: Same sort, page 1 (if original page insufficient)
  - Fallback 2: Popular, page 1 (ultimate safety net)
- **Helper Method**: `TryGetDecadeMovies` provides consistent error handling, user filtering, and fallback logic.
- **Consistency**: Both initial load and AJAX reshuffles use identical dynamic variety logic, ensuring a unified user experience.
- **Performance**: Maintains ~1-2 TMDB API calls per user interaction, with early exit optimization and 24-hour caching per sort+page+decade combo.
- **User Filtering**: Blacklist and watched movies are filtered consistently, with all expensive operations cached outside evaluation loops.
- **User Experience**: Decade suggestions now provide varied, reliable content from the very first click, with bulletproof fallback for edge cases.

- **Deduplication**: Prevents same decade appearing multiple times in results

### Code Patterns for DecadeReshuffle
- Always use `last25Movies` for decade calculations.
- Use dynamic sort and page parameters for every suggestion, both initial and reshuffle.
- Apply triple fallback logic for reliability.
- Use `TryGetDecadeMovies` for all decade movie retrievals.
- Maintain session state for anti-repetition (immediate only).
- Cache blacklist and recent movies once per request.

### Consistency Achievement
- Decade and genre suggestion systems now share a unified dynamic variety and fallback approach, providing a consistent and reliable user experience across all suggestion types.

- Si un usuario intenta blacklistear la mayoría o totalidad de las películas trending (por ejemplo, más de 20 en una sola sesión), puede encontrar que los últimos títulos no se pueden blacklistear por limitaciones de estado, caché o validación.
- Este es un caso extremo y poco realista en el uso normal de la app. La mayoría de los usuarios nunca intentarán blacklistear tantas películas trending de una vez.
- Se decidió no priorizar la solución de este edge case para optimizar el tiempo de desarrollo y enfocarse en flujos que impactan a la mayoría de los usuarios.
- El sistema ya muestra un mensaje amigable cuando no hay más sugerencias trending disponibles, por lo que el usuario nunca queda "atrapado".
## UI/UX Patterns
- Mutual exclusion implemented preventively via conditional rendering
- Error states avoided through visual state management
- Consistent behavior across Details and Preview pages

---
**Best Practice:**
> For all AJAX POST requests (especially for Blacklist/Wishlist removal), **always include** the `X-Requested-With: XMLHttpRequest` header. This guarantees the backend returns JSON for AJAX, not HTML error pages, and prevents frontend parsing errors. This is required for robust, user-friendly error handling in all AJAX-powered UI actions.
---
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

## 2025-07-27+ Performance, Architecture, and UX Standards

### Performance & Architecture
- Always use batch API calls (e.g., `GetMultipleMovieDetailsAsync`) for Blacklist and Wishlist to avoid N+1 query problems. Never call TMDB for each movie individually.
- Use the centralized `CacheService` for user-specific data (blacklist/wishlist IDs), with 15-minute expiration and automatic invalidation on add/remove.
- Pagination for Blacklist and Wishlist must be 20 items per page, preserving search/sort across pages.
- All expensive filtering (blacklist, watched, deduplication) must be cached per request or per pool build, not per reshuffle.
- All TMDB movie details are cached for 24 hours in IMemoryCache.

### Suggestion System
- All suggestion types (Trending, Director, Genre, Cast, Decade, Surprise Me) must use unified helper methods for filtering, pool building, and randomization.
- Both initial loads and AJAX reshuffles must use the same business logic and dynamic variety systems (random sort/page, triple fallback, deduplication).
- Genre and Decade suggestions always use randomized sort criteria (`popularity.desc`, `vote_average.desc`, `release_date.desc`) and page (1-3), with triple fallback logic.
- Director and Cast suggestions use deduplicated, session-tracked priority queues and anti-repetition logic.
- "Surprise Me" uses a static, deduplicated pool (50–80 movies), built in parallel, cached for 2 hours, with cyclic rotation and anti-repetition.

### UI/UX
- All suggestion section titles use `.cinelog-gold-title` for Cinema Gold color.
- Suggestion card titles and descriptions are 1pt larger for readability.
- All AJAX-powered UI actions use event delegation and server-rendered HTML for consistency.
- Pagination controls must be consistent across Blacklist and Wishlist.

### Code Quality & Documentation
- All controller and service comments must be in English, business-logic-focused, and use XML documentation for public methods.
- All logging uses structured `_logger` calls.
- All mutual exclusion logic (wishlist/blacklist) must be enforced both in backend and UI, with clear visual feedback.
- Maintain the new commenting/documentation standards for all future contributions.

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
- **Cast Reshuffle ahora implementa una secuencia robusta:** rota entre sugerir por actor más reciente, actor más frecuente, actor de la película mejor puntuada y, si se agotan, un actor aleatorio. El paso actual se almacena en Session y avanza en cada reshuffle.
- Anti-repetition: el mismo actor nunca será sugerido dos veces seguidas (anti-repetición inmediata vía Session).
- Track session state to avoid repetitive suggestions (Session sequencing is only used on the initial suggestion click; all reshuffles use client parameters, excepto en Cast donde la secuencia es gestionada por Session).
- Handle empty result sets gracefully with actionable next steps
- The "Reshuffle" button is implemented via event delegation and always maintains the correct context for all suggestion types.
- IMemoryCache is used for TMDB API data; Session State is used for user-specific anti-repetition and sequencing (y para la secuencia de Cast).

### Genre Suggestion Variety System (2025-07-24)
- GenreReshuffle implements dynamic content variety through randomized sort criteria and pagination
- **Random Parameters**: Each reshuffle uses random combination of sort type (popular/top-rated/latest) and page (1-3)
- **Quality Filtering**: All suggestions filtered to 6.5+ rating with minimum vote counts for reliability
- **Triple Fallback System**: Primary sort+page → Same sort, page 1 → Popular, page 1 (never shows empty results)
- **Genre Sequencing**: Maintains intelligent sequence (recent → frequent → rated → random) while varying content within each genre
- **User Filtering**: Excludes user's watched movies, wishlist items, and blacklisted content
- **Performance**: Same API usage as previous system but with significantly improved content variety
- **User Experience**: Consistent "Because you watched [GENRE] movies" titles regardless of underlying sort criteria

---
### 2025-07-24 Genre Suggestion Consistency Fix

Initial genre suggestions now use the same dynamic variety system as AJAX reshuffles
Both initial load and reshuffles generate random sort criteria (popularity.desc, vote_average.desc, release_date.desc) and page (1-3)
Unified title format: "Because you watched [GENRE] movies" for both initial and reshuffles
Session state is reset on fresh start to ensure correct sequence
User experience is now consistent and varied from the very first click
No impact on caching or performance optimizations

# 2025-07-25 Trending Suggestion Unification

- **Unified Trending Logic**: Both initial `ShowSuggestions` and AJAX `TrendingReshuffle` now use the same helper method `GetTrendingMoviesWithFiltering()`
- **Consistent Filtering**: Same blacklist and recent movie exclusion logic across both endpoints
- **Consistent Pool Building**: Same 30-movie pool generation from up to 5 TMDB pages
- **Consistent Randomization**: Same shuffling algorithm for variety in both flows
- **Code Maintenance**: Single source of truth for trending movie logic, eliminating duplication
- **Performance**: Consistent caching behavior using TMDB service's built-in 90-minute cache

## Trending Suggestion Pattern
When implementing trending suggestions, always use the unified helper method:
```csharp
// For both initial and AJAX suggestions
var trendingResult = await GetTrendingMoviesWithFiltering(userId);
var suggestedMovies = trendingResult.Take(3).ToList();
```

## Comment and Documentation Standards (Updated 2025-07-25)

### Code Comments in English
- All new code comments must be written in English for international collaboration
- Use XML documentation (`///`) for public methods with clear business purpose explanations  
- Inline comments should explain "why" not "what" - focus on business logic and decision rationale
- Remove any Spanish comments and replace with English equivalents
- Use structured logging with English messages: `_logger.LogInformation("English message here")`

### Method Documentation Pattern
```csharp
/// <summary>
/// [Brief description of what the method does]
/// </summary>
/// <param name="paramName">Description of parameter purpose</param>
/// <returns>Description of return value and when it's used</returns>
/// <remarks>
/// Business rationale, performance notes, or usage patterns.
/// </remarks>
```

## Unified Logic Pattern (2025-07-25)

When creating suggestion endpoints that have both initial and AJAX variants:

### 1. Create Shared Helper Method
```csharp
private async Task<List<TmdbMovieBrief>> Get[Type]MoviesWithFiltering(string userId)
{
  // Unified filtering and pool building logic
  // Include user blacklist, recent movies, and other filters
  // Return consistent, shuffled results
}
```

### 2. Use Helper in Both Endpoints
```csharp
// In ShowSuggestions switch case:
case "trending":
  var result = await GetTrendingMoviesWithFiltering(userId);
  suggestedMovies = result.Take(3).ToList();
  break;

// In AJAX endpoint:
public async Task<IActionResult> TrendingReshuffle()
{
  var moviePool = await GetTrendingMoviesWithFiltering(userId);
  var suggestedMovies = moviePool.Take(3).ToList();
  // ... render and return JSON
}
```

### 3. Benefits of This Pattern
- **Single Source of Truth**: One method contains all business logic
- **Consistency**: Same filtering, caching, and randomization across endpoints  
- **Maintainability**: Changes only need to be made in one place
- **Testing**: Easier to unit test the core logic separately

# (Business Rules update)
- Genre suggestion system always uses dynamic variety logic, regardless of initial load or reshuffle
- Consistent user-facing behavior and titles for all genre suggestions

---

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

### Surprise Me System (2025-01-26 Major Optimization)
- Pool size reduced from 80 to 50 movies, matching real user interaction patterns
- All pool queries are now constructed and executed in parallel (up to 15 concurrent calls, throttled)
- Build time reduced from ~2,800ms to ~400-450ms (85% faster)
- Anti-repetition system tracks 3 previous pool rotations (6-hour windows) for better variety
- After initial build, all suggestions are instant (zero API calls per reshuffle)
- API usage: 15 parallel calls per build (was 25+ sequential)
- System is robust to TMDB rate limits and supports high concurrency
- The old 4-cycle system is fully replaced by this unified, scalable approach

---

## 🤖 Advanced Claude Code Agent System (2025-07-29)

### 🎭 Master Agent Director
The project now includes an intelligent **Master Agent Director** that analyzes task complexity and routes requests to optimal specialized agents:

**Intelligence Framework:**
- **Task Analysis Engine**: Parses requests, analyzes complexity, detects domains
- **Complexity Assessment**: Automatically classifies tasks as Simple/Medium/Complex/Strategic
- **Strategic Planning**: Auto-triggered 5-step planning process for complex tasks
- **Multi-Agent Orchestration**: Coordinates sequential and parallel agent workflows

**Complexity-Based Routing:**
- **Simple Tasks** (bug fixes) → Direct execution to specialist agent
- **Medium Tasks** (enhancements) → Light planning → Execute
- **Complex Tasks** (new features) → Strategic planning → Multi-agent execution
- **Strategic Tasks** (major changes) → Deep planning → Phased execution

### 🎬 Core CineLog Subagents

#### `cinelog-movie-specialist`
**Domain Expert** for movie-specific features and suggestion algorithms:
- MoviesController patterns and CRUD operations
- Unified helper methods for consistent business logic across initial/AJAX calls
- Triple fallback systems ensuring suggestions are never empty
- Session-based anti-repetition and sequencing logic
- Dynamic variety systems with randomized sort criteria and pagination
- Proactive director filtering to prevent empty suggestion states
- Mutual exclusion logic (movies cannot be in both wishlist and blacklist)

#### `tmdb-api-expert`
**External API Integration Specialist**:
- TmdbService architecture with 24-hour caching
- Batch operations using `GetMultipleMovieDetailsAsync()` to avoid N+1 queries
- Parallel execution for pool building (up to 15 concurrent calls)
- Rate limiting with SemaphoreSlim and error handling for API failures
- Data mapping between TMDB API responses and CineLog models

#### `performance-optimizer`
**Performance & Caching Specialist**:
- IMemoryCache optimization for TMDB data (24-hour expiration)
- CacheService for user-specific data (15-minute expiration)
- Batch processing patterns to eliminate N+1 query problems
- Parallel execution strategies (85% performance improvement)
- Database query optimization with proper indexing

#### `aspnet-feature-developer`
**Full-Stack Development Specialist**:
- ASP.NET Core MVC patterns with Bootstrap 5 (Cyborg theme)
- AJAX + Server-Side Rendering hybrid architecture
- Event delegation for dynamic UI elements
- Progressive enhancement (works without JavaScript)
- Cinema Gold branding with `.cinelog-gold-title` classes
- User data isolation patterns with UserId filtering

### 🚀 Enhanced Development Subagents

#### `test-writer-fixer` (Proactive)
**Comprehensive Testing Specialist** - Auto-triggers after code changes:
- Unit, integration, and end-to-end testing for ASP.NET Core
- Movie-specific test scenarios (suggestions, CRUD, API integration)
- Test failure analysis and repair without compromising test intent

#### `ui-designer` (Proactive)
**Visual Design Enhancement Specialist** - Auto-triggers after UI updates:
- Movie-centric UI component design beyond Bootstrap
- Cinema Gold branding implementation and design systems
- Responsive design patterns for movie discovery interfaces
- Screenshot-worthy design moments for social sharing

#### `whimsy-injector` (Proactive)
**User Engagement Specialist** - Auto-triggers after UI/UX changes:
- Movie discovery and logging micro-interactions
- Achievement celebrations and playful animations
- Personality-filled copy and error states
- Shareable moments that encourage user evangelism

#### `backend-architect`
**Scalable Architecture Specialist**:
- ASP.NET Core architecture patterns and scalability design
- Database optimization and performance architecture
- API design with proper authentication and rate limiting
- System architecture for movie data management at scale

#### `performance-benchmarker`
**Performance Testing Specialist**:
- TMDB API integration performance and rate limiting testing
- Suggestion system performance profiling and optimization
- Database query performance analysis and recommendations
- Frontend rendering optimization for movie-rich interfaces

### 🎯 Agent Usage Patterns

**Automatic Delegation:**
```
"Add movie feature X" → aspnet-feature-developer + test-writer-fixer + ui-designer
"Fix suggestion bug" → cinelog-movie-specialist + test-writer-fixer
"Optimize performance" → performance-optimizer + performance-benchmarker
"TMDB API issue" → tmdb-api-expert + api-tester
"Deploy to production" → deployment-project-manager + multi-agent coordination
"Plan production deployment" → deployment-project-manager
"Choose hosting platform" → deployment-project-manager
```

**Proactive Invocation:**
```
Code changes made → test-writer-fixer (ensures test coverage)
UI/feature updates → ui-designer (enhances visual appeal)
UI/UX changes → whimsy-injector (adds personality and delight)
```

#### `deployment-project-manager`
**Strategic production deployment coordination and educational guidance**:
- **Strategic Decision Making**: Infrastructure sizing, platform selection (Azure/AWS), technology stack recommendations with cost optimization
- **Educational Guidance**: Patient explanations of complex deployment concepts with clear decision rationale and best practices
- **Cross-Agent Coordination**: Orchestrates deployment phases across all specialized agents with risk management and emergency response
- **Production Architecture**: Distributed caching (Redis), session state management, security configuration, and performance monitoring
- **Infrastructure Design**: Load balancing, monitoring setup (APM), backup/recovery strategies, and scalability planning
- **Deployment Phases**: Foundation setup → Performance infrastructure → Production deployment → Optimization & monitoring

### 📊 Development Benefits
- **Intelligent Orchestration**: Master Director routes tasks to optimal agents automatically
- **Proactive Quality**: Automatic testing, UI enhancement, and delight injection
- **Strategic Planning**: Complex features receive proper planning before implementation
- **Comprehensive Testing**: Built-in test coverage ensures robust, reliable features
- **Enhanced User Experience**: Automatic UI enhancement and personality injection
- **Performance Excellence**: Built-in performance analysis and optimization recommendations
- **Production Deployment Expertise**: Strategic deployment guidance with educational approach and cross-agent coordination

### 🔑 Key Principles for Agent Coordination
- **Domain Expertise**: Each agent has deep knowledge of specific CineLog patterns
- **Consistency**: All agents follow the same architectural conventions and coding standards
- **Quality First**: Built-in quality gates and performance considerations
- **User-Centric**: Every feature considers the complete user experience
- **Performance-Aware**: All implementations consider scalability and optimization

---

## 🎯 Agent Invocation & Coordination Guide

### 📋 **Explicit Agent Invocation Guidance**

| User Request Pattern | Primary Agent(s) | Rationale |
|---------------------|------------------|-----------|
| "Add movie feature X" | `aspnet-feature-developer` → `test-writer-fixer` → `ui-designer` | Full-stack development with testing and UI enhancement |
| "Fix suggestion bug" | `cinelog-movie-specialist` → `test-writer-fixer` | Domain expertise + test coverage |
| "Make the app faster" | `performance-benchmarker` → `performance-optimizer` | Analysis first, then optimization |
| "Database changes needed" | `ef-migration-manager` → `backend-architect` | Schema changes + architecture review |
| "TMDB API not working" | `tmdb-api-expert` → `api-tester` | Integration expertise + reliability testing |
| "Code is messy/complex" | `code-refactoring-specialist` → `test-writer-fixer` | Refactoring + maintained functionality |
| "UI needs improvement" | `ui-designer` → `whimsy-injector` | Visual design + personality injection |
| "Tests are failing" | `test-writer-fixer` + Domain expert | Fix tests + address root cause |
| "Deploy to production" | `deployment-project-manager` → Multi-agent coordination | Strategic deployment planning + execution |
| "Plan production deployment" | `deployment-project-manager` | Infrastructure decisions + educational guidance |
| "Choose hosting platform" | `deployment-project-manager` | Platform selection with cost/complexity analysis |
| "Users complaining about X" | `feedback-synthesizer` → Relevant domain agent | Analyze feedback + implement solution |

### 🔄 **Simple Planning for Complex Tasks**

For complex tasks only, quickly think through:
- **What needs to be done?** (objective)
- **Which agent(s)?** (primary + follow-up)  
- **What could break?** (risks)

Keep it simple - no need for formal templates unless the task truly spans multiple domains.

### ⚡ **Agent Escalation/Delegation Rules**

**CRITICAL RULE**: If a task is ambiguous or spans multiple domains, **always escalate to the Master Agent Director** for orchestration and planning.

**Escalation Triggers**:
- Task affects 3+ architectural components
- Requirements are unclear or conflicting
- Multiple domain expertise needed simultaneously
- Risk of breaking existing functionality
- User mentions "comprehensive" or "major" changes

**Example**: *"Redesign the entire suggestion system"* → **Master Agent Director** (Strategic planning required)

### 🎭 **Multi-Agent Coordination Example**

**User Prompt**: *"Add a movie rating system with stars, save to database, and make it look good"*

**Master Agent Director Analysis**:
```
🎯 DOMAINS: Database, Backend, Frontend, UI/UX
⚡ COMPLEXITY: COMPLEX → Strategic planning activated
🚀 AGENTS: Sequential multi-agent workflow
```

**Execution Sequence**:
1. **`backend-architect`**: Design rating system schema and API structure
2. **`ef-migration-manager`**: Create database migration for ratings table
3. **`aspnet-feature-developer`**: Implement MVC components and rating logic
4. **`ui-designer`**: Create star rating component with Bootstrap integration
5. **`whimsy-injector`**: Add hover effects and rating submission animations
6. **`test-writer-fixer`**: Write comprehensive tests for rating functionality
7. **`docs-architect`**: Update documentation with new rating feature

**Coordination Benefits**: Each agent builds on previous work, ensuring cohesive implementation.

### 📚 **Documentation Updates**

Update docs when you introduce new patterns or fix significant bugs. Key files: `CLAUDE.md`, `README.md`, `CHANGELOG.md`.

### 🚨 **Error Handling**

Always be specific and actionable: "TMDB API rate limited - try again in 60 seconds" not "API error".

### ✏️ **Documentation Edits**

Don't remove working patterns or guidance unless explicitly asked. Add and enhance, don't delete.

---

## 🔍 CineLog Development Knowledge Base

*Quick reference for GitHub Copilot to access specialized knowledge when working on specific CineLog components*

### 🎬 Movie Suggestions **[WHEN: MoviesController, suggestion algorithms, AJAX reshuffles, empty states]**

#### **Core Patterns:**
```csharp
// UNIFIED HELPER METHOD PATTERN - Use for all suggestion types
private async Task<List<TmdbMovieBrief>> Get[Type]MoviesWithFiltering(string userId)
{
    // Get user blacklist and recent movies for filtering (cached per request)
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    var last25Movies = await GetLast25MoviesAsync(userId); // Cache this call
    
    // Build movie pool with variety and pagination
    // Apply deduplication using HashSet<string> for TMDB IDs
    // Return consistent results for both initial and AJAX calls
}

// USER DATA ISOLATION (CRITICAL) - Always filter by UserId
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);

// TRIPLE FALLBACK SYSTEM - Ensure suggestions are never empty
// Primary: Dynamic variety (random sort + random page)
// Fallback 1: Same sort, page 1
// Fallback 2: Popular, page 1 (ultimate safety net)
```

#### **Common Problems → Solutions:**

**Problem:** "No suggestions available for [Director]"
```csharp
// FIX: Proactive director filtering before suggestion
private async Task<bool> HasAvailableMoviesForDirector(string directorName, string userId)
{
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    // Lightweight check without fetching full movie details
    // Only include directors with at least one non-blacklisted movie
}
```

**Problem:** Repetitive suggestions / No variety
```csharp
// SOLUTION: Dynamic variety system
var randomSort = new[] { "popularity.desc", "vote_average.desc", "release_date.desc" }
    .OrderBy(x => Guid.NewGuid()).First();
var randomPage = Random.Shared.Next(1, 4);

// Use different parameters for each suggestion
var movies = await _tmdbService.GetMoviesByGenreAsync(genreId, randomSort, randomPage);
```

**Problem:** Session anti-repetition not working
```csharp
// SOLUTION: Session-based sequencing
string sequenceKey = $"DirectorTypeSequence_{userId}";
var currentStep = HttpContext.Session.GetInt32(sequenceKey) ?? 0;
HttpContext.Session.SetInt32(sequenceKey, (currentStep + 1) % 4);

// Sequencing: Recent → Frequent → Top-rated → Random
```

#### **Suggestion Type Specifics:**

**Trending:** Pool of 30 movies from multiple TMDB pages, 90-minute cache, exclude last 5 watched
**Director/Cast:** Sequenced rotation with case-insensitive deduplication, session tracking
**Genre/Decade:** Dynamic variety with 6.5+ rating filter, randomized sort/page, triple fallback
**Surprise Me:** 50-movie deduplicated pool, parallel build (15 concurrent), 2-hour cache, instant reshuffles

#### **AJAX Implementation:**
```csharp
// Return server-rendered HTML, not JSON
return PartialView("_MovieSuggestionCard", suggestedMovies);

// Event delegation pattern in JavaScript
document.addEventListener('click', function(e) {
    if (e.target.matches('[data-suggestion-type]')) {
        // Handle all reshuffle buttons dynamically
    }
});
```

### 🌐 TMDB API Integration **[WHEN: External API calls, rate limiting, caching, data mapping]**

#### **Core Patterns:**
```csharp
// CENTRALIZED SERVICE USAGE - Always use TmdbService
var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
var searchResults = await _tmdbService.SearchMoviesAsync(query);

// BATCH OPERATIONS - Avoid N+1 queries
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

// PARALLEL EXECUTION - For pool building (Surprise Me)
var poolTasks = buckets.Select(async bucket => 
    await _tmdbService.GetMoviesAsync(bucket.endpoint)).ToArray();
var results = await Task.WhenAll(poolTasks);

// RATE LIMITING - SemaphoreSlim throttling
private readonly SemaphoreSlim _semaphore = new(6, 6); // Max 6 concurrent
await _semaphore.WaitAsync();
try { /* API call */ } finally { _semaphore.Release(); }
```

#### **Common Problems → Solutions:**

**Problem:** TMDB API rate limiting
```csharp
// SOLUTION: Built-in throttling + retry logic
await _semaphore.WaitAsync();
try {
    var response = await _httpClient.GetAsync(url);
    if (response.StatusCode == HttpStatusCode.TooManyRequests) {
        await Task.Delay(1000); // Wait and retry
        response = await _httpClient.GetAsync(url);
    }
} finally { _semaphore.Release(); }
```

**Problem:** Slow API performance
```csharp
// SOLUTION: 24-hour caching + parallel execution
_memoryCache.Set(cacheKey, result, TimeSpan.FromHours(24));

// For multiple calls, use parallel execution
var tasks = tmdbIds.Select(id => _tmdbService.GetMovieDetailsAsync(id));
var results = await Task.WhenAll(tasks);
```

**Problem:** API failures breaking user experience
```csharp
// SOLUTION: Robust error handling with fallbacks
try {
    return await _tmdbService.GetMovieDetailsAsync(tmdbId);
} catch (HttpRequestException) {
    _logger.LogWarning("TMDB API unavailable, using cached data");
    return GetCachedMovieDetails(tmdbId) ?? CreateFallbackMovieDetails(tmdbId);
}
```

#### **Caching Strategy:**
- **TMDB Movie Details:** 24 hours in IMemoryCache
- **Search Results:** 1 hour (more dynamic)
- **Trending Data:** 90 minutes (balances freshness with performance)
- **Suggestion Pools:** 2 hours for Surprise Me, varies by type

### ⚡ Performance Optimization **[WHEN: Slow queries, cache misses, N+1 problems, memory issues]**

#### **Core Patterns:**
```csharp
// CACHING SERVICE - User-specific data
var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
// 15-minute expiration, automatic invalidation on add/remove

// BATCH PROCESSING - Eliminate N+1 queries
// BAD: Individual API calls
foreach (var item in wishlistItems) {
    var details = await _tmdbService.GetMovieDetailsAsync(item.TmdbId);
}

// GOOD: Batch API call
var tmdbIds = wishlistItems.Select(i => i.TmdbId).ToList();
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

// PAGINATION - 20 items per page with proper indexing
var paginatedItems = await PaginatedList<T>.CreateAsync(
    query.Where(m => m.UserId == userId), pageNumber, 20);
```

#### **Common Problems → Solutions:**

**Problem:** Database queries are slow
```csharp
// SOLUTION: Composite indexes + proper filtering
// Migration: Add index on (UserId, Title) for fast user-specific searches
migrationBuilder.CreateIndex(
    name: "IX_Movies_UserId_Title",
    table: "Movies",
    columns: new[] { "UserId", "Title" });

// Always filter by UserId first (uses index)
var userMovies = _dbContext.Movies
    .Where(m => m.UserId == userId)  // Index hit
    .Where(m => m.Title.Contains(searchTerm));
```

**Problem:** Cache misses causing performance hits
```csharp
// SOLUTION: Request-level caching for expensive operations
private Dictionary<string, object> _requestCache = new();

private async Task<List<Movies>> GetLast25MoviesAsync(string userId)
{
    var cacheKey = $"last25_{userId}";
    if (_requestCache.TryGetValue(cacheKey, out var cached))
        return (List<Movies>)cached;
        
    var result = await _dbContext.Movies
        .Where(m => m.UserId == userId)
        .OrderByDescending(m => m.DateWatched)
        .Take(25).ToListAsync();
        
    _requestCache[cacheKey] = result;
    return result;
}
```

**Problem:** Surprise Me pool building is slow
```csharp
// SOLUTION: Parallel execution (85% faster)
var buckets = CreateSurprisePoolBuckets(); // Different API endpoints
var poolTasks = buckets.Select(async bucket => {
    var semaphore = new SemaphoreSlim(1);
    await semaphore.WaitAsync();
    try {
        return await _tmdbService.GetMoviesFromBucket(bucket);
    } finally { semaphore.Release(); }
}).ToArray();

var poolResults = await Task.WhenAll(poolTasks);
// Build time: ~400ms (was ~2800ms)
```

#### **Performance Benchmarks:**
- API calls: Batch 20 movies in ~200ms vs 20 individual calls in ~4000ms
- Database pagination: 20 items with index in <50ms
- Surprise Me build: 50 movies in ~400ms with parallel execution
- Cache hit rate: >90% for TMDB data, >80% for user blacklist/wishlist

### 🏗️ ASP.NET Core Development **[WHEN: Controllers, Views, AJAX, Authentication, Routing]**

#### **Core Patterns:**
```csharp
// CONTROLLER STRUCTURE - Standard CineLog pattern
[Authorize] // Always require authentication
public class MoviesController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly TmdbService _tmdbService;
    private readonly CacheService _cacheService;
    private readonly ILogger<MoviesController> _logger;

    // Always get userId first in actions
    var userId = _userManager.GetUserId(User);
    
    // Always filter data by userId
    var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);
}

// AJAX ENDPOINTS - Return server-rendered HTML
[HttpPost]
public async Task<IActionResult> TrendingReshuffle()
{
    var userId = _userManager.GetUserId(User);
    var movies = await GetTrendingMoviesWithFiltering(userId);
    return PartialView("_MovieSuggestionCard", movies.Take(3));
}

// MUTUAL EXCLUSION - Prevent wishlist/blacklist conflicts
private async Task<bool> IsMovieInWishlist(string userId, string tmdbId)
{
    return await _dbContext.WishlistItems
        .AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
}
```

#### **Common Problems → Solutions:**

**Problem:** User data exposed across accounts
```csharp
// SOLUTION: Always filter by UserId (CRITICAL SECURITY)
// BAD: Exposes all users' data
var movies = _dbContext.Movies.Where(m => m.Title.Contains(search));

// GOOD: User isolation
var userId = _userManager.GetUserId(User);
var movies = _dbContext.Movies
    .Where(m => m.UserId == userId)
    .Where(m => m.Title.Contains(search));
```

**Problem:** AJAX not working with dynamic content
```csharp
// SOLUTION: Event delegation pattern
// JavaScript: Handle dynamically added buttons
document.addEventListener('click', function(e) {
    if (e.target.matches('.reshuffle-btn')) {
        handleReshuffle(e.target);
    }
});

// Controller: Return HTML, not JSON for consistent styling
return PartialView("_MovieSuggestionCard", suggestedMovies);
```

**Problem:** Anti-forgery token validation failing
```csharp
// SOLUTION: Include tokens in AJAX requests
// Razor view:
@Html.AntiForgeryToken()

// JavaScript:
var token = $('input[name="__RequestVerificationToken"]').val();
$.post(url, { __RequestVerificationToken: token, data: data });
```

#### **View Patterns:**
```html
<!-- CINEMA GOLD BRANDING -->
<h3 class="cinelog-gold-title">Trending Movies</h3>

<!-- BOOTSTRAP 5 CYBORG THEME -->
<div class="card bg-dark border-secondary">
    <div class="card-body">
        <!-- Movie content -->
    </div>
</div>

<!-- EVENT DELEGATION ATTRIBUTES -->
<button class="btn btn-outline-warning" 
        data-suggestion-type="trending"
        data-action="reshuffle">
    Reshuffle
</button>
```

### 🗄️ Database & Entity Framework **[WHEN: Migrations, queries, performance indexes, data models]**

#### **Core Patterns:**
```csharp
// USER DATA ISOLATION - Critical for all queries
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);

// COMPOSITE INDEXES - For fast user-specific searches
migrationBuilder.CreateIndex(
    name: "IX_Movies_UserId_Title",
    table: "Movies", 
    columns: new[] { "UserId", "Title" });

// PAGINATION PATTERN
var paginatedList = await PaginatedList<Movies>.CreateAsync(
    _dbContext.Movies.Where(m => m.UserId == userId), pageNumber, 20);

// Use TotalCount for pagination (NOT viewModels.Count)
var totalCount = paginatedList.TotalCount; // Total database records
```

#### **Common Problems → Solutions:**

**Problem:** Pagination navigation broken
```csharp
// PROBLEM: Using current page count instead of total
var totalItems = viewModels.Count; // WRONG - only current page items (max 20)

// SOLUTION: Use total database count
var totalItems = paginatedList.TotalCount; // CORRECT - all user's records
var viewModel = new ViewModel {
    TotalCount = paginatedList.TotalCount,
    HasNextPage = paginatedList.HasNextPage,
    HasPreviousPage = paginatedList.HasPreviousPage
};
```

**Problem:** Slow user-specific queries
```csharp
// SOLUTION: Always filter by UserId first (uses index)
// BAD: Full table scan
var movies = _dbContext.Movies.Where(m => m.Title.Contains(search));

// GOOD: Index-optimized
var movies = _dbContext.Movies
    .Where(m => m.UserId == userId)  // Uses IX_Movies_UserId
    .Where(m => m.Title.Contains(search));
```

**Problem:** N+1 query problems in Entity Framework
```csharp
// BAD: N+1 queries
var movies = _dbContext.Movies.Where(m => m.UserId == userId);
foreach (var movie in movies) {
    // This creates additional queries
    var relatedData = movie.SomeRelatedEntity;
}

// GOOD: Include related data
var movies = _dbContext.Movies
    .Where(m => m.UserId == userId)
    .Include(m => m.SomeRelatedEntity)
    .ToListAsync();
```

#### **Entity Models:**
```csharp
// Standard CineLog entity pattern
public class Movies
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } // Always required
    [Required] public string Title { get; set; }
    public string? Director { get; set; }
    public int ReleasedYear { get; set; }
    public decimal? UserRating { get; set; }
    public DateTime DateWatched { get; set; }
    public string? WatchedLocation { get; set; }
    public bool IsRewatch { get; set; }
    public string? TmdbId { get; set; }
    public string? PosterPath { get; set; }
    public string? Overview { get; set; }
    public string? Genres { get; set; }
    public DateTime DateCreated { get; set; }
}
```

#### **Migration Best Practices:**
```csharp
// Always include UserId indexes for new user-specific tables
migrationBuilder.CreateIndex(
    name: "IX_TableName_UserId",
    table: "TableName",
    column: "UserId");

// Composite indexes for common query patterns
migrationBuilder.CreateIndex(  
    name: "IX_TableName_UserId_CommonField",
    table: "TableName",
    columns: new[] { "UserId", "CommonField" });
```

### 🎨 UI/UX & AJAX Patterns **[WHEN: Views, styling, JavaScript, responsive design, user interactions]**

> **⚡ AJAX Quick Reference**: All AJAX POSTs must include `X-Requested-With: XMLHttpRequest`, `credentials: 'same-origin'`, and antiforgery token.

#### **Core Patterns:**
```html
<!-- CINEMA GOLD BRANDING -->
<h2 class="cinelog-gold-title mb-4">Movie Suggestions</h2>

<!-- BOOTSTRAP 5 CYBORG THEME -->
<div class="container-fluid bg-dark text-light">
    <div class="card bg-dark border-secondary mb-3">
        <div class="card-body">
            <h5 class="card-title text-warning">Movie Title</h5>
            <p class="card-text">Movie description...</p>
        </div>
    </div>
</div>

<!-- EVENT DELEGATION FOR DYNAMIC CONTENT -->
<script>
document.addEventListener('click', function(e) {
    if (e.target.matches('[data-action="reshuffle"]')) {
        handleReshuffle(e.target.dataset.suggestionType);
    }
    
    if (e.target.matches('.add-to-wishlist')) {
        addToWishlist(e.target.dataset.tmdbId);
    }
});
</script>
```

#### **Common Problems → Solutions:**

**Problem:** AJAX buttons stop working after content updates
```javascript
// PROBLEM: Direct event binding breaks with dynamic content
$('.reshuffle-btn').click(function() { /* Won't work for new buttons */ });

// SOLUTION: Event delegation
$(document).on('click', '.reshuffle-btn', function() {
    // Works for all buttons, including dynamically added ones
    handleReshuffle($(this).data('suggestion-type'));
});
```

**Problem:** Inconsistent styling after AJAX updates
```csharp
// SOLUTION: Return server-rendered HTML from AJAX endpoints
[HttpPost]
public async Task<IActionResult> TrendingReshuffle()
{
    var movies = await GetTrendingMovies(userId);
    // Return partial view with consistent styling
    return PartialView("_MovieSuggestionCard", movies);
}
```

**Problem:** Mobile responsiveness issues
```html
<!-- SOLUTION: Bootstrap responsive classes -->
<div class="container-fluid">
    <div class="row">
        <div class="col-12 col-md-6 col-lg-4 mb-3">
            <!-- Movie card - stacks on mobile, 2 per row on tablet, 3 on desktop -->
            <div class="card">...</div>
        </div>
    </div>
</div>
```

#### **JavaScript Patterns:**
```javascript
// AJAX with anti-forgery tokens
function makeAjaxCall(url, data) {
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    return $.ajax({
        url: url,
        type: 'POST',
        data: { ...data, __RequestVerificationToken: token },
        success: function(html) {
            // Replace content with server-rendered HTML
            $('#suggestion-container').html(html);
        },
        error: function() {
            showErrorMessage('Failed to load suggestions');
        }
    });
}

// Progressive enhancement - works without JavaScript
function enhanceForm(formSelector) {
    $(formSelector).on('submit', function(e) {
        e.preventDefault();
        const form = $(this);
        makeAjaxCall(form.attr('action'), form.serialize());
    });
}
```

#### **CSS Patterns:**
```css
/* Cinema Gold branding */
.cinelog-gold-title {
    color: #FFD700 !important;
    font-weight: 600;
}

/* Hover effects for interactive elements */
.movie-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(255, 215, 0, 0.3);
    transition: all 0.2s ease;
}

/* Responsive spacing */
@media (max-width: 768px) {
    .movie-card {
        margin-bottom: 1rem;
    }
    
    .suggestion-grid {
        grid-template-columns: 1fr;
    }
}
```

#### **Accessibility Patterns:**
```html
<!-- Screen reader support -->
<button class="btn btn-outline-warning" 
        aria-label="Reshuffle trending movie suggestions"
        data-suggestion-type="trending">
    <i class="fas fa-refresh" aria-hidden="true"></i>
    Reshuffle
</button>

<!-- Loading states -->
<div id="loading-spinner" class="d-none" role="status" aria-live="polite">
    <span class="sr-only">Loading suggestions...</span>
    <div class="spinner-border text-warning"></div>
</div>
```

### 🔧 Testing & Debugging **[WHEN: Test failures, debugging issues, performance problems]**

#### **Core Patterns:**
```csharp
// STRUCTURED LOGGING - Use throughout controllers
_logger.LogInformation("Generating {SuggestionType} suggestions for user {UserId}", 
    suggestionType, userId);

_logger.LogWarning("Director {DirectorName} has no available movies for user {UserId}", 
    directorName, userId);

_logger.LogError(ex, "TMDB API failed for user {UserId}: {ErrorMessage}", 
    userId, ex.Message);

// DEFENSIVE PROGRAMMING - Always validate user data
private async Task<bool> ValidateUserAccess(string userId, string resourceId)
{
    return await _dbContext.SomeEntity
        .AnyAsync(e => e.Id == resourceId && e.UserId == userId);
}

// PERFORMANCE TIMING
using var activity = _logger.BeginScope("SuggestionBuild_{SuggestionType}", type);
var stopwatch = Stopwatch.StartNew();
var result = await BuildSuggestions(userId, type);
_logger.LogInformation("Built {Count} suggestions in {ElapsedMs}ms", 
    result.Count, stopwatch.ElapsedMilliseconds);
```

#### **Common Debugging Scenarios:**

**Issue:** "Suggestions are empty"
```csharp
// DEBUG: Check each filter step
_logger.LogDebug("Pool before blacklist filter: {Count} movies", poolMovies.Count);
var filteredPool = poolMovies.Where(m => !blacklistIds.Contains(m.TmdbId));
_logger.LogDebug("Pool after blacklist filter: {Count} movies", filteredPool.Count());

var finalPool = filteredPool.Where(m => !recentTmdbIds.Contains(m.TmdbId));
_logger.LogDebug("Final pool: {Count} movies", finalPool.Count());
```

**Issue:** "User seeing other users' data"
```csharp
// DEBUG: Verify UserId filtering
_logger.LogDebug("Querying movies for user {UserId}", userId);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);
_logger.LogDebug("Found {Count} movies for user {UserId}", 
    await userMovies.CountAsync(), userId);
```

**Issue:** "Performance is slow"
```csharp
// DEBUG: Profile individual operations
var sw = Stopwatch.StartNew();

var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
_logger.LogDebug("Blacklist fetch: {ElapsedMs}ms", sw.ElapsedMilliseconds);
sw.Restart();

var apiResult = await _tmdbService.GetTrendingMoviesAsync();
_logger.LogDebug("TMDB API call: {ElapsedMs}ms", sw.ElapsedMilliseconds);
```

#### **Test Patterns:**
```csharp
// User isolation testing
[Test]
public async Task GetUserMovies_ShouldOnlyReturnCurrentUserMovies()
{
    // Arrange
    var user1Id = "user1";
    var user2Id = "user2";
    
    await _dbContext.Movies.AddRangeAsync(
        new Movies { UserId = user1Id, Title = "User1 Movie" },
        new Movies { UserId = user2Id, Title = "User2 Movie" }
    );
    await _dbContext.SaveChangesAsync();
    
    // Act
    var result = await _controller.GetUserMovies(user1Id);
    
    // Assert
    Assert.That(result.All(m => m.UserId == user1Id));
}
```

### 🔧 Code Refactoring & Technical Debt **[WHEN: Complex methods, duplicate code, legacy patterns, code smells]**

#### **Core Patterns:**
```csharp
// EXTRACT METHOD PATTERN - Break down large methods
// Before: 200+ line method
public async Task<IActionResult> ShowSuggestions(string type)
{
    // Complex logic mixing concerns
}

// After: Focused, single-responsibility methods
public async Task<IActionResult> ShowSuggestions(string type)
{
    var userId = _userManager.GetUserId(User);
    var suggestionStrategy = _suggestionFactory.CreateStrategy(type);
    var viewModel = await suggestionStrategy.GenerateAsync(userId);
    return View(viewModel);
}

// ELIMINATE DUPLICATION - Common filtering logic
private async Task<List<TmdbMovieBrief>> ApplyUserFiltering(
    List<TmdbMovieBrief> movies, string userId)
{
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    var recentTmdbIds = await GetRecentMovieTmdbIds(userId);
    
    return movies
        .Where(m => !blacklistIds.Contains(m.TmdbId))
        .Where(m => !recentTmdbIds.Contains(m.TmdbId))
        .ToList();
}

// SIMPLIFY COMPLEX CONDITIONS
// Before: Complex nested conditions
if (suggestionType == "trending" && 
    (userPreferences?.IncludeTrending == true || userPreferences == null) && 
    !userMovies.Any(m => m.Genre?.Contains("Action") == true && m.Rating > 4.0))

// After: Extracted to meaningful methods
if (ShouldShowTrendingSuggestions(suggestionType, userPreferences, userMovies))
```

#### **CineLog-Specific Refactoring Patterns:**

**MoviesController Simplification:**
```csharp
// BEFORE: Mixed concerns in controller
public async Task<IActionResult> DirectorReshuffle()
{
    var userId = _userManager.GetUserId(User);
    var directors = await GetDirectorPriorityQueue(userId);
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    // ... 50+ lines of filtering, API calls, and business logic
}

// AFTER: Delegated to services
public async Task<IActionResult> DirectorReshuffle()
{
    var userId = _userManager.GetUserId(User);
    var suggestions = await _directorSuggestionService.GetNextSuggestionsAsync(userId);
    return PartialView("_MovieSuggestionCard", suggestions);
}
```

**Suggestion Algorithm Unification:**
```csharp
// STRATEGY PATTERN for suggestion types
public interface ISuggestionStrategy
{
    Task<List<TmdbMovieBrief>> GenerateAsync(string userId);
    string GetSuggestionTitle();
}

public class TrendingSuggestionStrategy : ISuggestionStrategy
{
    public async Task<List<TmdbMovieBrief>> GenerateAsync(string userId)
    {
        return await GetTrendingMoviesWithFiltering(userId);
    }
}

// Factory for clean controller logic
public class SuggestionStrategyFactory
{
    public ISuggestionStrategy CreateStrategy(string type) => type switch
    {
        "trending" => new TrendingSuggestionStrategy(_tmdbService, _cacheService),
        "director" => new DirectorSuggestionStrategy(_tmdbService, _cacheService),
        _ => new DefaultSuggestionStrategy()
    };
}
```

#### **Common Refactoring Scenarios:**

**Problem:** Large, complex controller methods
```csharp
// SOLUTION: Service layer extraction
// Move business logic to dedicated services
// Controller only handles HTTP concerns and delegates to services
// Each service has single responsibility (e.g., DirectorSuggestionService)
```

**Problem:** Duplicate filtering logic across suggestion types
```csharp
// SOLUTION: Common base class or shared service
public abstract class BaseSuggestionService
{
    protected async Task<List<TmdbMovieBrief>> ApplyStandardFiltering(
        List<TmdbMovieBrief> movies, string userId)
    {
        // Common blacklist, wishlist, recent movie filtering
    }
}
```

**Problem:** Complex LINQ queries mixed with business logic
```csharp
// SOLUTION: Repository pattern with domain-specific queries
public class MovieRepository
{
    public async Task<List<Movies>> GetUserMoviesWithFilters(
        string userId, MovieFilter filters)
    {
        var query = _dbContext.Movies.Where(m => m.UserId == userId);
        
        if (filters.MinRating.HasValue)
            query = query.Where(m => m.Rating >= filters.MinRating);
            
        return await query.ToListAsync();
    }
}
```

#### **Quality Metrics to Monitor:**
```csharp
// CYCLOMATIC COMPLEXITY - Methods should have < 10 branches
// METHOD LENGTH - Keep methods under 50 lines
// CLASS SIZE - Controllers should focus on HTTP concerns only
// DUPLICATION - DRY principle, extract common patterns

// Performance impact measurement
var stopwatch = Stopwatch.StartNew();
var result = await RefactoredMethod();
_logger.LogInformation("Refactored method completed in {ElapsedMs}ms", 
    stopwatch.ElapsedMilliseconds);
```

#### **Test-Safe Refactoring:**
```csharp
// PRESERVE BEHAVIOR - Ensure existing functionality works
[Test]
public async Task RefactoredMethod_ShouldReturnSameResults()
{
    // Arrange - same test data
    var userId = "test-user";
    
    // Act - call both old and new implementations
    var oldResult = await OldSuggestionMethod(userId);
    var newResult = await NewSuggestionService.GenerateAsync(userId);
    
    // Assert - verify identical behavior
    Assert.That(newResult, Is.EqualTo(oldResult));
}
```
