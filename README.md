# Cast Suggestions (Actor)

- Suggests movies based on actors from your recent movie history, rotating between most recent, most frequent, highest-rated, and random.
- Anti-repetition: the same actor will never be suggested twice in a row (immediate repetition is prevented).

# Genre Suggestion Priority Queue and AJAX Reshuffle (2025-07-23)
### Surprise Me (Optimized Pool-Based Approach)

- The "Surprise Me" feature now uses a static, deduplicated pool of 80 movies, built with aggressive cascading from prioritized buckets (trending, genre, director, etc.).


- The pool is cached for 2 hours (IMemoryCache), ensuring instant reshuffles and consistent suggestions.
- Infinite cyclic rotation: each reshuffle advances the pointer, wrapping around as needed.
- Blacklist and recent filters are applied during pool build, not per reshuffle.
- Deduplication by TMDB ID is enforced during pool construction.
- Performance: Only ~5 TMDB API calls are made during initial pool build; all reshuffles are API-free.

#### Performance Comparison
| Suggestion Type      | API Calls (Initial) | API Calls (Reshuffle) | Latency (Reshuffle) |
|----------------------|--------------------|----------------------|---------------------|
| Surprise Me (Pool)   | ~5                 | 0                    | Instant             |
| Director/Genre/etc.  | 1-2                | 1-2                  | 1-2s                |


## Genre Priority Queue
- The backend now provides a prioritized queue of genres for each user, based on their logged movies.
- The queue is ordered by:
  1. Most recent genre (from the latest logged movie)
  2. Most frequent genre (across all logged movies)
  3. Highest-rated genre (from movies rated 4.0 or higher)
- The queue is cached per user for 1 hour to optimize performance and reduce redundant calculations.
- This queue is used for AJAX-powered genre reshuffles and anti-repetition logic in the UI.

## AJAX Genre Reshuffle
- The "By Genre" suggestion type now supports AJAX reshuffling, returning server-rendered HTML for seamless UI updates.
- All user-specific filtering and anti-repetition logic is enforced server-side.

See `MoviesController.cs` for implementation details and business logic comments.
# ✨ Cinema Gold Branding & UI Polish (2025-07-19)
- **Navbar:** The bottom border of the navbar is now gold (Cinema Gold) and enforced with `!important` for maximum visual consistency.
- **Section Titles:** Suggestion section titles use Cinema Gold and retain their original size and visual weight.
- **Suggestion Cards:** Descriptive text inside each card is one point larger for better readability.
- **Visual Consistency:** All color and typographic hierarchy changes are aligned with the CineLog visual identity and documented in `site.css`.
- **No Bootstrap classes or base sizes were altered, only color and key visual details.


# 🎭 Sequential Cast Reshuffle (2025-07-20)
- **Cast Reshuffle now rotates between strategies:**
  - Suggests by most recent actor, most frequent actor, top-rated movie actor, and, if exhausted, a random actor.
  - The current step is stored in Session and advances with each reshuffle, ensuring variety and personalization.
  - If a step has no valid actor, it automatically skips to the next.
  - The endpoint continues to return server-rendered HTML (partial views) for maximum visual and routing consistency.
  - Documentation and XML comments updated to reflect the new logic and edge cases.



# 🚀 Hybrid AJAX+HTML Suggestion System (2025-07-18)
- **Trending Reshuffle AJAX:** Trending suggestions reshuffle is now performed via AJAX, and the endpoint returns server-rendered HTML (partial views), not raw JSON.
- **Technical Rationale:** Server-side rendering ensures posters and image paths work correctly, avoiding routing and CORS issues.
- **Extensible Pattern:** Other suggestion types still use traditional navigation, but the pattern is extensible to more types if AJAX is desired.
- **Maintenance:** The trending button uses data-suggestion-type and event delegation in JS to trigger the AJAX fetch. After replacing the grid, event listeners are always reattached to maintain AJAX functionality for internal forms.
- **Documentation:** C# and JS comments explain the purpose, rationale, and best maintenance practices. See examples in `MoviesController.cs` and `Views/Movies/Suggest.cshtml`.
### 🔄 Intelligent List Management
- **Mutual Exclusion Logic**: Movies cannot exist in both wishlist and blacklist
- **Preventive UI**: Visual states prevent conflicting actions before they occur
- **Seamless Experience**: No error messages - clear visual indicators instead



# 📝 Code Quality & Documentation
- All controller comments (especially in `MoviesController.cs`) now follow a professional, business-logic-focused style.
- All redundant, development-only, and shallow comments have been removed for clarity and maintainability.
- All `Console.WriteLine` and `System.Diagnostics.Debug.WriteLine` statements have been replaced with structured `_logger` calls.
- All major public methods in `MoviesController.cs` now have professional XML documentation and clarified business logic comments.
- Mutual exclusion logic for wishlist/blacklist is now clearly documented and visually enforced.
- Please maintain this standard for all future contributions.
- **Preventive UI**: Visual states prevent conflicting actions before they occur
- **Seamless Experience**: No error messages - clear visual indicators instead

# CineLog-AI-Experiments

## ✨ Key Features

### 🎬 Movie Management
- **Personal Movie Log**: Track what you've watched with ratings, dates, and locations
- **Smart Search Integration**: Powered by The Movie Database (TMDB) API
- **Rich Movie Details**: Automatic director, year, poster, and genre information

### 📋 Lists & Organization
- **Dynamic Wishlist**: AJAX-enabled instant adding/removing without page reloads
- **Smart Blacklist**: Block unwanted suggestions with mutual exclusion logic
- **Advanced Filtering**: Search and sort by title, director, year, rating, and more



### 🎯 Intelligent Suggestions
- **Personalized Recommendations**: Based on your directors, genres, cast, and decades
- **By Director (AJAX-powered)**: Director suggestions now use the same fast, no-reload AJAX system as Trending and Cast. Reshuffles are instant, server-rendered, and always maintain context and UI consistency.
- **Robust Fallback System**: Always provides suggestions, even when edge cases occur
- **Trending Movies**: Discover what's popular right now
    - Now uses backend IMemoryCache (90 min per page) for all trending API calls
    - Suggestions are filtered to exclude your blacklist and last 5 watched
    - Pool of 30 trending movies is built from up to 5 TMDB pages, then randomized
    - Always provides 3 unique, user-relevant trending suggestions
- **Surprise Me**: Get random suggestions based on your taste profile
- **Generalized AJAX Reshuffle**: The "Reshuffle" button now works for all suggestion types (Trending, Cast, Director) using event delegation, always maintaining context and providing instant feedback.
- **Dual Caching**: IMemoryCache is used for API data, and Session State is used for user-specific anti-repetition and sequencing.

### 🎯 By Decade (Optimized AJAX)
- **Recent Activity Focus**: Based on your last 25 movies instead of entire history for more relevant suggestions
- **Intelligent Prioritization**: 
  - **Latest**: Decade from your most recently added movie
  - **Frequent**: Most common decade (minimum 2 movies from last 25)
  - **Rated**: Highest average rated decade (minimum 2 rated movies from last 25)
- **Equitable Selection**: Any decade from your last 25 movies can appear in suggestions
- **Performance Optimized**: Maximum 15 API calls per request (reduced from 25+)
- **Smart Anti-Repetition**: Only avoids the immediately previous decade, ensuring variety
- **Fallback System**: Automatically adjusts criteria if no decades meet the 3+ movie threshold

### 🔄 Seamless Experience
- **No Page Reloads**: AJAX-powered interactions for smooth user experience
- **Instant Feedback**: Visual confirmation of all actions
- **Consistent UI/UX**: All suggestion cards and reshuffle actions are visually and behaviorally consistent across categories.
- **Mobile Responsive**: Works perfectly on all devices
