## [Version X.X.X] - 2025-07-19
### Cinema Gold Branding & UI Polish
- **Navbar:** Ahora el borde inferior del navbar es dorado (Cinema Gold) y se fuerza con `!important` para máxima consistencia visual.
- **Títulos de sección:** Los títulos de sección de sugerencias usan Cinema Gold y mantienen el tamaño y peso visual original.
- **Tarjetas de sugerencias:** El texto descriptivo dentro de cada tarjeta es un punto más grande para mejor legibilidad.
- **Consistencia visual:** Todos los cambios de color y jerarquía tipográfica están alineados con la identidad visual de CineLog y documentados en `site.css`.
- **No se alteraron clases Bootstrap ni tamaños base, solo color y detalles visuales clave.

## [Version X.X.X] - 2025-07-18
### Hybrid AJAX+HTML Suggestion System
- **Nuevo patrón híbrido en sugerencias:**
  - El reshuffle de tipo "Trending" ahora usa AJAX y devuelve HTML renderizado del servidor (partial views), no JSON puro.
  - Justificación: El renderizado server-side asegura que los posters y paths de imágenes funcionen correctamente, evitando problemas de rutas y CORS.
  - El resto de tipos de sugerencia siguen usando navegación tradicional, pero el patrón es extensible a más tipos si se desea AJAXizar.
  - El botón de reshuffle trending usa data-suggestion-type y event delegation en JS para disparar el fetch AJAX.
  - Tras reemplazar el grid de sugerencias, siempre se re-adjuntan los event listeners para mantener la funcionalidad AJAX de los formularios internos.
  - Comentarios en C# y JS documentan el propósito, el porqué del enfoque y las mejores prácticas de mantenimiento.
  - Ver ejemplos y convenciones en `MoviesController.cs` y `Views/Movies/Suggest.cshtml`.
### Suggestion Engine, AJAX, and Caching Overhaul
- Robust session-vs-client logic: Session sequencing is only used on the initial suggestion click; all reshuffles trust client parameters.
- Generalized AJAX "Reshuffle" button: Now works for all suggestion types using event delegation, always maintaining context.
- Trending suggestions: Now use backend IMemoryCache (90 min per page), exclude blacklisted and recently watched movies, and always provide 3 unique, user-relevant cards.
- Dual caching: IMemoryCache for API data, Session State for user-specific anti-repetition and sequencing.
- UI/UX: All suggestion cards and reshuffle actions are visually and behaviorally consistent, with instant feedback and no page reloads.
- Codebase: All controller and model comments are professional, business-logic-focused, and DRY. No development artifacts remain.
## [Version X.X.X] - 2025-07-18
### Trending Suggestions & Caching Improvements
- **Trending Movies**: Now uses backend cache (90 min per page) for all trending API calls
- **User Filtering**: Trending suggestions exclude blacklisted movies and your last 5 watched
- **Pool Generation**: Up to 30 valid trending movies are pooled from 5 TMDB pages, then randomized for variety
- **Consistent UX**: Trending suggestions now always reflect user preferences and avoid repetition
### 🎯 Enhanced User Experience
- **Preventive Mutual Exclusion**: Implemented visual state management for wishlist/blacklist
- **Eliminated Error Banners**: Replaced reactive error messages with preventive UI states
- **Consistent UX**: Unified mutual exclusion behavior across Details and Preview pages
# 2025-07-18
### Mutual Exclusion UI en Suggestion Cards
- Ahora, al agregar una película a wishlist vía AJAX en la página de sugerencias, el botón "Add to Blacklist" de la misma tarjeta se desactiva/oculta automáticamente en el frontend.
- No se modificó la lógica backend ni la estructura HTML; el cambio es solo JavaScript para mejorar la experiencia y consistencia visual.
# 2025-07-18
### Final Model Comments Cleanup
- Eliminados todos los comentarios de desarrollo, temporales y anotaciones vagas en los modelos (`Models/`).
- Mejoradas las descripciones de propiedades y comentarios de validación para mayor claridad y profesionalismo.
- Se mantuvieron todos los atributos de validación y lógica funcional intactos.
- Los modelos ahora cumplen con los estándares de documentación para producción: solo comentarios técnicos, sin artefactos de desarrollo.
# 2025-07-17
### Codebase Quality Improvements
- Comprehensive code comment refactor in `Controllers/MoviesController.cs`:
  - Removed all remaining development artifact comments and obsolete notes.
  - Improved clarity and professionalism of all controller comments.
  - Preserved and clarified business logic, suggestion system, and anti-repetition documentation.
  - No functional code changes; all logic and structure remain intact.
  - All future comment changes should maintain this standard.
# CHANGELOG - CineLog-AI-Experiments

## 2025-07-15
### Movie Preview Card (Add/Edit Movie)
- Solucionado problema de especificidad CSS usando selector ultra específico para la tarjeta de preview.
- Aplicados colores profesionales: fondo gris oscuro, borde azul, sombra negra pronunciada.
- Efectos hover con elevación y transición suave.
- Jerarquía tipográfica mejorada: título grande, labels medianos azules, valores normales blancos, overview más pequeño.
- Overview ahora se muestra completo, sin scrollbar, y con texto justificado para mejor legibilidad.
- Comentarios agregados en site.css explicando cada mejora y recomendaciones de mantenimiento.

### Notas de mantenimiento
- Si se modifica la estructura HTML o clases Bootstrap de la tarjeta, revisar y actualizar los selectores ultra específicos.
- Probar visualmente en todos los navegadores y dispositivos tras cualquier cambio visual.
- Documentar futuras mejoras UX en este archivo y en los comentarios del CSS.

## [Version X.X.X] - 2025-07-16

### 🎯 Enhanced User Experience
- **AJAX Blacklist/Wishlist**: Added instant feedback for blacklist and wishlist actions without page reloads
- **Mutual Exclusion Logic**: Movies cannot be in both wishlist and blacklist simultaneously
- **Smart Reshuffle Fallback**: Improved suggestion system with bulletproof fallback when all results are blacklisted
- **UI Edge Case Handling**: Reshuffle button always provides a way forward, even with empty suggestion results

### 🔧 Technical Improvements
- Implemented AJAX handlers for seamless movie list management
- Enhanced suggestion sequence logic with robust error handling
- Added strategic comments for complex business rules and UI interactions
