## 🚀 Hybrid AJAX+HTML Suggestion System (2025-07-18)

- **Trending Reshuffle AJAX:** El reshuffle de sugerencias trending ahora se realiza vía AJAX y el endpoint devuelve HTML renderizado del servidor (partial views), no JSON puro.
- **Justificación técnica:** El renderizado server-side asegura que los posters y paths de imágenes funcionen correctamente, evitando problemas de rutas y CORS.
- **Patrón extensible:** El resto de tipos de sugerencia siguen usando navegación tradicional, pero el patrón es extensible a más tipos si se desea AJAXizar.
- **Mantenimiento:** El botón trending usa data-suggestion-type y event delegation en JS para disparar el fetch AJAX. Tras reemplazar el grid, siempre se re-adjuntan los event listeners para mantener la funcionalidad AJAX de los formularios internos.
- **Documentación:** Los comentarios en C# y JS explican el propósito, el porqué del enfoque y las mejores prácticas de mantenimiento. Ver ejemplos en `MoviesController.cs` y `Views/Movies/Suggest.cshtml`.
### 🔄 Intelligent List Management
- **Mutual Exclusion Logic**: Movies cannot exist in both wishlist and blacklist
- **Preventive UI**: Visual states prevent conflicting actions before they occur
- **Seamless Experience**: No error messages - clear visual indicators instead
## 📝 Code Quality & Documentation

- All controller comments (especially in `MoviesController.cs`) follow a professional, business-logic-focused style.
- Development artifacts and redundant comments have been removed for clarity and maintainability.
- Please maintain this standard for all future contributions.
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
- **Robust Fallback System**: Always provides suggestions, even when edge cases occur
- **Trending Movies**: Discover what's popular right now
    - Now uses backend IMemoryCache (90 min per page) for all trending API calls
    - Suggestions are filtered to exclude your blacklist and last 5 watched
    - Pool of 30 trending movies is built from up to 5 TMDB pages, then randomized
    - Always provides 3 unique, user-relevant trending suggestions
- **Surprise Me**: Get random suggestions based on your taste profile
- **Generalized AJAX Reshuffle**: The "Reshuffle" button now works for all suggestion types using event delegation, always maintaining context and providing instant feedback.
- **Dual Caching**: IMemoryCache is used for API data, and Session State is used for user-specific anti-repetition and sequencing.

### 🔄 Seamless Experience
- **No Page Reloads**: AJAX-powered interactions for smooth user experience
- **Instant Feedback**: Visual confirmation of all actions
- **Consistent UI/UX**: All suggestion cards and reshuffle actions are visually and behaviorally consistent across categories.
- **Mobile Responsive**: Works perfectly on all devices
