# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## ⚠️ CRITICAL INSTRUCTIONS

### 🚨 GOLDEN RULE: DO NOT INVENT THINGS
**MOST IMPORTANT - User explicitly emphasized:**
- **NEVER** add features, improvements, or enhancements unless explicitly requested
- **NEVER** add loading states, animations, or visual changes unless asked
- **NEVER** assume user wants "better" UX or performance improvements
- **REMEMBER**: "don't add things we don't ask for" - this is the #1 rule
- Implement ONLY what is specifically requested, nothing more

### 📝 MANDATORY PROFESSIONAL COMMENTS
**ALWAYS add comments when making significant changes:**
- **ALL** new methods MUST have comprehensive XML documentation
- **ALL** complex logic MUST have inline comments explaining "why"
- Use FEATURE/FIX/ENHANCEMENT prefixes for significant changes
- Comments MUST be in English and follow professional standards

### 🔨 Build Requirements
- **CRITICAL**: A task is NEVER complete if the application cannot build successfully
- **ALWAYS** run `dotnet build` to verify compilation before marking tasks finished
- Build failures MUST be resolved as part of implementation, not left for later

### 📋 Task Management
- **ALWAYS** use TodoWrite tool for complex multi-step tasks
- **MUST** keep working until ALL todo items are checked off
- **NEVER** end your turn until problem is completely solved and verified

### 🚫 Never Auto-Commit
- **NEVER** stage and commit files automatically
- Only commit when explicitly asked by the user

---

## 🔄 Development Workflow

### 1. 🧠 Problem Analysis
- Carefully read the issue and think critically about requirements 
- **Question existing patterns** - analyze if they apply to new contexts
- Consider expected behavior, edge cases, and potential pitfalls

### 2. 🔍 Codebase Investigation
- Search for key functions, classes, or variables related to the issue
- Read and understand relevant code snippets for context
- **Look for unified helper methods** that implement consistent business logic

### 3. 📋 Planning & Todo Management
- **ALWAYS** create todo list using TodoWrite tool for complex tasks
- Check off steps as you complete them
- Never end turn until ALL todo items are completed

### 4. ⚙️ Implementation
- Always read relevant file contents before editing
- **MANDATORY**: Add professional comments to ALL new code
- **ALWAYS** run `dotnet build` to verify compilation
- **Follow CineLog patterns** for user data isolation, caching, and API usage

### 5. 🐛 Debugging & Testing
- Use structured `_logger` calls instead of console output
- Test edge cases rigorously
- **Verify user data isolation** and proper filtering by UserId

---

## 🤖 Claude Code Subagents System

> **📚 For detailed agent documentation, see [AGENTS.md](./.claude/agents/AGENTS.md)**

### 🎯 Agent Selection Guide
| Task Type | Primary Agent | Use Case |
|-----------|---------------|----------|
| Movie features/suggestions | `cinelog-movie-specialist` | Domain expertise |
| TMDB API integration | `tmdb-api-expert` | External API operations |
| Performance issues | `performance-optimizer` | Caching & optimization |
| Database changes | `ef-migration-manager` | Schema & migrations |
| Full-stack features | `aspnet-feature-developer` | Complete MVC development |
| Production deployment | `deployment-project-manager` | Strategic deployment coordination |

---

## 🛠️ Development Commands

### 🔨 Build and Run
```bash
dotnet build                    # Build the project
dotnet run                      # Run the application
dotnet watch run               # Run with hot reload during development
```

### 🗄️ Database Commands
```bash
dotnet ef migrations add <Name>        # Create new migration
dotnet ef database update              # Apply migrations to database
dotnet ef database drop                # Drop database (development only)
```

### 📄 Documentation Management
```bash
/update-docs [description]      # Comprehensive documentation update after changes
/docs [description]             # Quick documentation sync
```

---

## 🏛️ Architecture Overview

### 🔧 Tech Stack
- **🚀 Framework**: ASP.NET Core 8.0 with MVC pattern
- **🗄️ Database**: SQL Server with Entity Framework Core (25 migrations)
- **🔐 Authentication**: ASP.NET Core Identity with robust user isolation
- **🌐 External API**: TMDB API integration with rate limiting and caching
- **🎨 Frontend**: Bootstrap 5 with Cyborg dark theme, jQuery for AJAX
- **⚡ Caching**: IMemoryCache for TMDB data, custom CacheService for user-specific data

### 🏗️ Core Architecture Patterns

#### 🗃️ Data Layer
- **`ApplicationDbContext`**: EF Core context with Identity integration
- **Entity models**: Movies, WishlistItem, BlacklistedMovie with foreign key relationships
- **User isolation**: All user data isolated by `UserId` with excellent security model

#### ⚙️ Service Layer
- **`TmdbService`**: External TMDB API calls with caching and rate limiting (24-hour cache)
- **`CacheService`**: Centralized caching for user blacklist/wishlist IDs (15-minute expiration)
- **Batch processing**: N+1 query prevention with `GetMultipleMovieDetailsAsync`

#### 🎮 Controller Layer
- **`MoviesController`**: Main business logic with comprehensive AJAX architecture
- **AJAX Detection**: `X-Requested-With` header detection for seamless navigation
- **Unified Rendering**: `RenderSuggestionResultsHtml()` for server-side HTML in AJAX responses
- **State Management**: `PopulateMovieProperties()` preserves movie states in AJAX interactions
- **Security**: All data queries filtered by current user ID

#### 🎯 Suggestion System
**🎬 Suggestion Types (All AJAX-Enabled):**
- **📈 Trending**: TMDB trending API with user filtering
- **🎬 By Director**: Based on directors from user's movie history
- **🎭 By Genre**: Prioritized queue based on recent/frequent/highly-rated genres
- **⭐ By Cast**: Rotates through actors from user's watched movies
- **📅 By Decade**: Dynamic variety system with randomized parameters
- **🎲 Surprise Me**: Optimized pool-based system with parallel API calls

**🔑 Key AJAX Patterns:**
- **Unified Logic**: Same helper methods for initial load and AJAX reshuffles
- **Server-Side Rendering**: AJAX responses return server-rendered HTML
- **Graceful Fallback**: Automatic fallback to page navigation if AJAX fails
- **Session Tracking**: Anti-repetition and sequencing via session state

### 📊 Data Models
- **`Movies`**: User's logged movies with ratings, dates, locations
- **`WishlistItem`**: Movies user wants to watch
- **`BlacklistedMovie`**: Movies user wants to exclude from suggestions
- **`TmdbMovieDetails`**: Full movie data from TMDB API
- **`TmdbMovieBrief`**: Simplified movie data for suggestions

---

## 🎯 CineLog-Specific Development Patterns

### 🔒 User Data Security & Isolation
**ALWAYS filter by current user:**
```csharp
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);
```
**Critical Rules:**
- **ALL** user data queries MUST include `UserId` filtering
- **NEVER** expose data across user accounts
- Use ASP.NET Identity for authentication and authorization

### 🏭 Production Deployment Security
**CRITICAL for production deployment:**
```csharp
// ❌ NEVER do this in production:
var conString = "Server=localhost,1433;Database=Ezequiel_Movies;User Id=sa;Password=***REMOVED***;TrustServerCertificate=True";

// ✅ ALWAYS use secure configuration:
var conString = builder.Configuration.GetConnectionString("DefaultConnection");
```
**Production Security Requirements:**
- **NEVER** hardcode passwords or connection strings in source code
- **ALWAYS** use Azure Key Vault or secure secret management
- **ALWAYS** create dedicated database user (not sa) with minimal permissions
- **ALWAYS** apply production-performance-indexes.sql for optimal query performance

### 🌐 TMDB Service Integration
**Use centralized service for all external API calls:**
```csharp
var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
var searchResults = await _tmdbService.SearchMoviesAsync(query);
```
**Best Practices:**
- **24-hour caching** for TMDB data in IMemoryCache
- **Batch operations**: Use `GetMultipleMovieDetailsAsync()` to avoid N+1 queries
- **Rate limiting**: Respect TMDB API limits with SemaphoreSlim

### 🎭 AJAX-Enhanced Suggestion System
**Unified helper methods for AJAX and traditional navigation:**
```csharp
private async Task<List<TmdbMovieBrief>> Get[Type]MoviesWithFiltering(string userId)
{
    // Get user blacklist and recent movies for filtering
    // Build movie pool with pagination and variety
    // Return consistent results for both initial and AJAX calls
}
```

**Key AJAX Implementation Requirements:**
- **X-Requested-With Header**: Essential for backend to detect AJAX requests
- **Graceful Fallback**: Always implement fallback to regular navigation if AJAX fails
- **State Preservation**: Use `PopulateMovieProperties()` to ensure all movie states are maintained
- **Server-Side Rendering**: Return server-rendered HTML in JSON responses
- **Clean Implementation**: Avoid loading overlays or visual changes that create jarring experiences

### 🚀 Enhanced AJAX Patterns
**Robust AJAX with comprehensive error handling:**
```javascript
// CRITICAL: Always include X-Requested-With header
const response = await fetch('/Movies/RemoveFromBlacklist', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'X-Requested-With': 'XMLHttpRequest'  // REQUIRED for JSON response
    },
    body: `tmdbId=${encodeURIComponent(tmdbId)}&__RequestVerificationToken=${encodeURIComponent(token)}`,
    credentials: 'same-origin'
});

// Text-first parsing with JSON fallback for robust error handling
const rawText = await response.text();
let data;
try {
    data = JSON.parse(rawText);
} catch (jsonErr) {
    showTempAlert('Non-JSON response: ' + rawText, 'danger');
    return;
}
```

### 📄 Pagination Implementation
**⚠️ CRITICAL**: Always use `TotalCount` property for pagination calculations:
```csharp
// ✅ CORRECT - Use TotalCount for pagination logic
var paginatedList = await PaginatedList<EntityType>.CreateAsync(query, pageNumber, pageSize);
var viewModel = new ViewModel
{
    TotalCount = paginatedList.TotalCount,  // Total database count
    HasPreviousPage = paginatedList.HasPreviousPage,
    HasNextPage = paginatedList.HasNextPage
};

// ❌ INCORRECT - Never use Count of current page items
// var totalCount = viewModels.Count; // This breaks pagination navigation!
```

### 🗃️ Business Rules Implementation
```csharp
// A movie cannot exist in both wishlist and blacklist
var existsInWishlist = await _dbContext.WishlistItems
    .AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
```

### ⚡ Performance Patterns
- **Batch Processing**: Always use `GetMultipleMovieDetailsAsync()` for multiple movies
- **CacheService**: 15-minute expiration for user blacklist/wishlist IDs
- **Pagination**: 20 items per page with proper navigation
- **Index Usage**: Composite indexes on `UserId+Title` for fast searches

### 🎨 UI/UX Standards
- **Cinema Gold branding**: `.cinelog-gold-title` class for section titles
- **Consistent layout**: Card-based design for movie displays
- **Dark theme**: Cyborg Bootstrap theme throughout
- **Mutual exclusion**: Prevents movies in both wishlist and blacklist
- **Real-time updates**: AJAX without page reloads


---

## ✅ Code Quality Standards

### 📝 Documentation & Comments (MANDATORY)
**CRITICAL: ALL new code MUST be professionally commented - no exceptions**

#### 🎯 XML Documentation (Required)
- **ALL new methods** must have comprehensive XML documentation
- **Include purpose, parameters, returns, and remarks** when applicable
- **English only** for international collaboration
- **Example format**:
```csharp
/// <summary>
/// Brief description of what the method does and its purpose.
/// 
/// FIX/FEATURE: Add context for significant changes or new functionality.
/// </summary>
/// <param name="paramName">Description of parameter and its constraints</param>
/// <returns>Description of return value and possible states</returns>
```

#### 🔧 Inline Comments (Professional Standards)
- **Explain "why" not "what"** - focus on business logic and reasoning
- **Use FIX/FEATURE/ENHANCEMENT prefixes** for significant changes
- **English only** - replace any Spanish comments with English equivalents
- **Examples**:
```csharp
// FIX: Check if director has available movies before adding to queue
// This prevents showing "No more suggestions available" message
if (await HasAvailableMoviesForDirector(trimmed, userId))

// PERFORMANCE: Use batch API calls to prevent N+1 queries
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);
```

### 🚨 Error Handling
- **Structured logging** using `_logger` throughout
- **Try-catch blocks** for external API calls
- **Graceful fallbacks** for suggestion system edge cases

### ⚡ Performance
- **Always** use async/await for database and API calls
- **Batch operations** to minimize round trips
- **Cache** expensive operations appropriately

---

## ⚙️ Configuration

### 🔐 Required Secrets
- **`TMDB:AccessToken`**: TMDB API bearer token (User Secrets)

### 🗄️ Database & Sessions
- **Connection**: SQL Server, hardcoded in Program.cs for development
- **Sessions**: 20-minute timeout for anti-repetition and sequencing