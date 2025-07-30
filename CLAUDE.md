# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

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

### 🏭 Production Deployment Requirements
**CRITICAL for production deployment:**
- **Security Configuration**: NEVER deploy with hardcoded passwords or connection strings
- **Performance Optimization**: Apply production-performance-indexes.sql before deployment
- **Scalability Assessment**: Consider distributed caching and session storage for production scale
- **Monitoring Setup**: Configure performance monitoring and alerting before production deployment

---

## 🔄 Development Workflow

### 1. 🧠 Problem Analysis
**Understand the problem deeply before coding:**
- Carefully read the issue and think critically about requirements
- Consider expected behavior, edge cases, and potential pitfalls
- Understand how it fits into the larger codebase context
- Identify dependencies and interactions with other components
- **Question existing patterns** - analyze if they apply to new contexts

### 2. 🔍 Codebase Investigation
**Explore and understand the existing code:**
- Explore relevant files and directories
- Search for key functions, classes, or variables related to the issue
- Read and understand relevant code snippets (2000 lines at a time for context)
- Identify the root cause of the problem
- Continuously validate and update understanding
- **Look for unified helper methods** that implement consistent business logic

### 3. 📋 Planning & Todo Management
**Create a detailed, step-by-step plan:**
- Outline specific, simple, and verifiable sequence of steps
- **ALWAYS** create todo list using TodoWrite tool for complex tasks
- Check off steps using [x] syntax as you complete them
- **MUST** continue to next step after checking off previous step
- Never end turn until ALL todo items are completed
- **Consider complexity** - simple tasks can skip extensive planning

### 4. ⚙️ Implementation
**Make incremental, testable changes:**
- Always read relevant file contents before editing
- Make small, logical changes that follow from investigation
- When detecting environment variables needed, proactively create .env file
- Test frequently after each change
- **ALWAYS** run `dotnet build` to verify compilation
- **Follow CineLog patterns** for user data isolation, caching, and API usage

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

## 🎯 Enhanced Agent Coordination Guidelines

### 📋 **Quick Agent Selection Guide**

| Task Type | Primary Agent | Follow-up Agents | Why This Sequence |
|-----------|---------------|------------------|-------------------|
| New movie feature | `aspnet-feature-developer` | `test-writer-fixer`, `ui-designer` | Full-stack → Testing → Polish |
| Suggestion system fix | `cinelog-movie-specialist` | `test-writer-fixer` | Domain expertise → Validation |
| Performance issue | `performance-benchmarker` | `performance-optimizer` | Measure → Improve |
| Code quality concern | `code-refactoring-specialist` | `test-writer-fixer` | Refactor → Verify |
| TMDB integration | `tmdb-api-expert` | `api-tester` | Integration → Reliability |
| Database change | `ef-migration-manager` | `backend-architect` | Schema → Architecture |

### ⚡ **Escalation Protocol**

**When to escalate to Master Agent Director:**
- Task spans 3+ architectural domains
- Requirements are ambiguous or conflicting  
- User mentions "major", "redesign", or "comprehensive"
- Risk of breaking existing functionality

**Auto-escalation triggers:**
```
"Redesign the suggestion system" → Master Agent Director
"Add comprehensive movie features" → Master Agent Director  
"Major performance overhaul" → Master Agent Director
```

### 🔄 **Planning for Complex Tasks**

For complex tasks: identify objective, select agents, consider risks. Keep it simple.

### 📚 **Documentation Updates**

Update docs when introducing new patterns or fixing significant bugs.

### 🚨 **Error Communication**

Be specific and actionable: "TMDB API rate limited - try again in 60 seconds" not "API error".

### ✏️ **Documentation Edits**

Don't remove working patterns unless explicitly asked. Add and enhance.

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

---

## 🤖 Claude Code Subagents System

> **📚 For detailed agent documentation, see [AGENTS.md](./.claude/agents/AGENTS.md)**

CineLog uses specialized Claude Code subagents for accelerated development. Each subagent has deep knowledge of specific architectural patterns.

### 🎯 Quick Agent Selection
| Task Type | Primary Agent | Use Case |
|-----------|---------------|----------|
| Movie features/suggestions | `cinelog-movie-specialist` | Domain expertise |
| TMDB API integration | `tmdb-api-expert` | External API operations |
| Performance issues | `performance-optimizer` | Caching & optimization |
| Database changes | `ef-migration-manager` | Schema & migrations |
| Full-stack features | `aspnet-feature-developer` | Complete MVC development |
| Production deployment | `deployment-project-manager` | Strategic deployment coordination |
| Code quality/refactoring | `code-refactoring-specialist` | Technical debt reduction |

### 🔥 Auto-Triggered Agents
- **test-writer-fixer** → After any code changes (ensures test coverage)
- **ui-designer** → After UI/feature updates (enhances visual appeal)
- **whimsy-injector** → After UI/UX changes (adds personality and delight)

### 🏭 Strategic Deployment Agent
- **deployment-project-manager** → Production deployment coordinator with unique capabilities:
  - **Educational Guidance**: Patient explanations of complex deployment concepts
  - **Strategic Decision Making**: Infrastructure sizing, platform selection, cost optimization
  - **Cross-Agent Coordination**: Orchestrates all specialized agents during deployment phases
  - **4-Phase Strategy**: Foundation → Performance Infrastructure → Production Deployment → Optimization
  - **Production Expertise**: Security configuration, distributed caching, monitoring, emergency response

---

## 🛠️ Development Commands

### 🔨 Build and Run
```bash
dotnet build                    # Build the project
dotnet run                      # Run the application
dotnet watch run               # Run with hot reload during development
```

### 📄 Documentation Management (Claude Code Slash Commands)
```bash
/update-docs [description]      # Comprehensive documentation update after changes
/docs [description]             # Quick documentation sync
/sync-docs [type] [description] # Advanced documentation synchronization with git integration
```

**📝 Usage Examples:**
- `/update-docs Added new caching system` - Updates all docs after adding caching
- `/docs` - Quick sync of all documentation files
- `/sync-docs feature Added user preferences` - Sync docs for a new feature

**📋 What gets updated:**
- `README.md` - Features, setup instructions, architecture overview
- `CLAUDE.md` - Development patterns, commands, architecture guidance  
- `CHANGELOG.md` - Chronological change history with categories
- `PERFORMANCE_OPTIMIZATION_SUMMARY.md` - Performance improvements and metrics
- `.github/copilot-instructions.md` - GitHub Copilot development knowledge base (when relevant)

### 🔧 Development Workflow (CCPlugins Commands)
```bash
/cleanproject                  # Clean up project files and temporary artifacts
/cleanup-types                 # Clean up TypeScript/type definitions
/commit [message]              # Interactive git commit with AI assistance
/context-cache                 # Manage context caching for better performance
/find-todos                    # Find and list TODO items in codebase
/fix-imports                   # Fix and organize import statements
/format                        # Format code according to project standards
/remove-comments               # Remove comments from code files
/review                        # AI-powered code review and suggestions
/session-end                   # End development session with cleanup
/session-start                 # Start development session with setup
/test                          # Run tests with intelligent selection
/undo                          # Undo recent changes with context awareness
```

**🎯 Usage Examples:**
- `/commit "Add movie rating feature"` - Smart commit with context analysis
- `/review` - Get comprehensive code review feedback
- `/find-todos` - List all TODO items across the project
- `/format` - Apply consistent code formatting
- `/test` - Run relevant tests based on recent changes

### 🗄️ Database Commands
```bash
dotnet ef migrations add <Name>        # Create new migration
dotnet ef database update              # Apply migrations to database
dotnet ef database drop                # Drop database (development only)
```

### 🏭 Production Database Commands
```bash
# Apply production performance indexes
SqlCmd -S {server} -d Ezequiel_Movies -i production-performance-indexes.sql

# Monitor index usage (after deployment)
SqlCmd -S {server} -d Ezequiel_Movies -Q "SELECT i.name, s.user_seeks, s.user_scans FROM sys.dm_db_index_usage_stats s INNER JOIN sys.indexes i ON s.object_id = i.object_id"

# Update statistics after index creation
SqlCmd -S {server} -d Ezequiel_Movies -Q "UPDATE STATISTICS"
```

**📊 Entity Framework Notes:**
- Uses Entity Framework Core with SQL Server (25 migrations applied)
- ⚠️ **CRITICAL**: Connection string hardcoded in Program.cs - MUST fix for production
- **Production**: Apply production-performance-indexes.sql for 50-95% query improvements
- **Security**: Create dedicated database user (not sa) for production deployment

---

## 🏛️ Architecture Overview

### 🔧 Tech Stack
- **🚀 Framework**: ASP.NET Core 8.0 with MVC pattern
- **🗄️ Database**: SQL Server with Entity Framework Core (25 migrations, production-ready)
- **🔐 Authentication**: ASP.NET Core Identity with robust user isolation
- **🌐 External API**: The Movie Database (TMDB) API integration with comprehensive rate limiting
- **🎨 Frontend**: Bootstrap 5 with Cyborg dark theme, jQuery for AJAX
- **⚡ Caching**: IMemoryCache for TMDB data, custom CacheService for user-specific data
- **🏭 Production**: Comprehensive deployment checklist and 14 performance indexes available

### 🏗️ Core Architecture Patterns

#### 🗃️ Data Layer (Production-Ready)
- **`ApplicationDbContext`**: EF Core context with Identity integration and CASCADE delete patterns
- **Entity models** in `Models/Entities/`: Movies, WishlistItem, BlacklistedMovie with robust foreign key relationships
- **User isolation**: All user data isolated by `UserId` foreign key with excellent security model
- **Performance optimization**: 14 additional production indexes available for 50-95% query improvements
- **Migration status**: 25 migrations successfully applied with no conflicts or pending changes

#### ⚙️ Service Layer (Production-Optimized)
- **`TmdbService`**: Handles all external TMDB API calls with caching and rate limiting (24-hour cache)
- **`CacheService`**: Centralized caching for user blacklist/wishlist IDs (15-minute expiration)
- **Batch processing**: N+1 query prevention with `GetMultipleMovieDetailsAsync` (95% API call reduction)
- **Production consideration**: Requires distributed caching for multi-instance deployments
- **Dependency injection**: Configured in `Program.cs`

#### 🎮 Controller Layer (Security-Hardened)
- **`MoviesController`**: Main business logic for movie management and suggestions with comprehensive AJAX support
- **Authentication**: Enforced via `[Authorize]` attribute with ASP.NET Core Identity
- **Security**: All data queries filtered by current user ID with excellent isolation patterns
- **Production security**: CSRF token validation, SQL injection prevention, and proper error handling
- **Performance**: Optimized with batch processing and efficient caching strategies

#### 🎯 Suggestion System Architecture
The app features a sophisticated movie suggestion system with multiple strategies:

**🎬 Suggestion Types:**
- **📈 Trending**: Uses TMDB trending API with user filtering
- **🎬 By Director**: Suggests based on directors from user's movie history
- **🎭 By Genre**: Prioritized queue based on recent/frequent/highly-rated genres
- **⭐ By Cast**: Rotates through actors from user's watched movies
- **📅 By Decade**: Dynamic variety system with randomized parameters
- **🎲 Surprise Me**: Optimized pool-based system with parallel API calls

**🔑 Key Patterns:**
- **Unified filtering**: Helper methods for consistent filtering and pool building
- **Identical logic**: Both initial loads and AJAX reshuffles use same business logic
- **Session tracking**: Anti-repetition and sequencing via session state
- **Triple fallback**: Logic ensures suggestions always available
- **Consistent filtering**: Blacklist and recent movie filtering applied throughout

#### 🔄 AJAX + Server-Side Rendering Hybrid
- **HTML responses**: AJAX reshuffles return server-rendered HTML (not JSON)
- **Event delegation**: Handles dynamic suggestion cards
- **Progressive enhancement**: Works without JavaScript
- **Real-time updates**: All suggestion types support reshuffling

### 📊 Data Models

#### 🗃️ Core Entities
- **`Movies`**: User's logged movies with ratings, dates, locations
- **`WishlistItem`**: Movies user wants to watch
- **`BlacklistedMovie`**: Movies user wants to exclude from suggestions

#### 🌐 TMDB Integration Models
- **`TmdbMovieDetails`**: Full movie data from TMDB API
- **`TmdbMovieBrief`**: Simplified movie data for suggestions
- **Various response models**: For different API endpoints

#### 🎨 ViewModels
- **`AddMoviesViewModel`**: Form handling for add/edit operations
- **`WishlistViewModel`/`BlacklistViewModel`**: Paginated list views
- **`SuggestionViewModel`**: Suggestion page data

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
- Verify user isolation in all movie, wishlist, and blacklist operations

### 🏭 Production Deployment Security Patterns
**CRITICAL for production deployment:**
```csharp
// ❌ NEVER do this in production:
var conString = "Server=localhost,1433;Database=Ezequiel_Movies;User Id=sa;Password=***REMOVED***;TrustServerCertificate=True";

// ✅ ALWAYS use secure configuration:
var conString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(conString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
        sqlOptions.CommandTimeout(60);
    }));
```
**Production Security Requirements:**
- **NEVER** hardcode passwords or connection strings in source code
- **ALWAYS** use Azure Key Vault or secure secret management
- **ALWAYS** create dedicated database user (not sa) with minimal permissions
- **ALWAYS** configure SSL/TLS connection encryption
- **ALWAYS** apply production-performance-indexes.sql for optimal query performance
- **ALWAYS** configure distributed caching and session storage for scalability

### 🌐 TMDB Service Integration Patterns
**Use centralized service for all external API calls:**
```csharp
var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
var searchResults = await _tmdbService.SearchMoviesAsync(query);
```
**Best Practices:**
- **24-hour caching** for TMDB data in IMemoryCache
- **Batch operations**: Use `GetMultipleMovieDetailsAsync()` to avoid N+1 queries
- **Rate limiting**: Respect TMDB API limits with SemaphoreSlim
- **Error handling**: Robust try-catch for all external API calls

### 🎭 Suggestion System Architecture
**Unified helper methods for consistency:**
```csharp
private async Task<List<TmdbMovieBrief>> Get[Type]MoviesWithFiltering(string userId)
{
    // Get user blacklist and recent movies for filtering
    // Build movie pool with pagination and variety
    // Apply deduplication and randomization
    // Return consistent results for both initial and AJAX calls
}
```

**Key Patterns:**
- **Unified Logic**: Same helper method for initial load and AJAX reshuffles
- **Dynamic Variety**: Randomized sort criteria and pagination for fresh content
- **Triple Fallback**: Primary → Fallback 1 → Ultimate fallback (never empty)
- **User Filtering**: Always exclude blacklisted movies and recent watches
- **Session State**: Anti-repetition tracking via session for sequencing
- **Deduplication**: Use HashSet<string> for TMDB ID deduplication

### 🔄 AJAX + Server-Side Rendering Hybrid
**AJAX endpoints return server-rendered HTML:**
```csharp
// Return partial view, not JSON
return PartialView("_MovieSuggestionCard", viewModel);
```
**Benefits:**
- Consistent styling and image paths
- Reuse of existing partial views
- Event delegation for dynamic button handling
- Progressive enhancement (works without JavaScript)

### 🚀 Enhanced AJAX Removal Pattern
**Robust AJAX removal with comprehensive error handling:**
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

// Robust response parsing with fallback error handling
const rawText = await response.text();
let data;
try {
    data = JSON.parse(rawText);
} catch (jsonErr) {
    showTempAlert('Non-JSON response: ' + rawText, 'danger');
    btn.disabled = false;
    return;
}
```

**Key Implementation Requirements:**
- **X-Requested-With Header**: Essential for backend to return JSON instead of HTML error pages
- **Text-First Parsing**: Always read response as text first, then parse JSON with try-catch
- **State Management**: Disable buttons during requests, re-enable on errors
- **Visual Feedback**: Fade-out animations (300ms) before removal
- **Empty State Detection**: Auto-show "empty list" messages when no items remain
- **Error Differentiation**: Distinguish between network, server, and parsing errors

### 🎬 AJAX Movie Deletion Pattern
**Real-time movie deletion from List page with comprehensive UX handling:**
```javascript
// ENHANCEMENT: Full movie deletion with count updates and empty page handling
const response = await fetch('/Movies/Delete', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'X-Requested-With': 'XMLHttpRequest'  // CRITICAL for JSON response
    },
    body: `id=${encodeURIComponent(movieId)}&__RequestVerificationToken=${encodeURIComponent(token)}`,
    credentials: 'same-origin'
});

// Text-first parsing with JSON fallback for robust error handling
const rawText = await response.text();
let data;
try {
    data = JSON.parse(rawText);
} catch (jsonErr) {
    showTempAlert('Non-JSON response: ' + rawText, 'danger');
    btn.disabled = false;
    return;
}

if (data.success) {
    // Smooth fade-out animation before removal
    card.classList.add('fade-out');
    setTimeout(() => {
        card.remove();
        
        // Update movie count badge in real-time
        const countBadge = document.querySelector('.badge.bg-secondary');
        if (countBadge) {
            const newCount = parseInt(countBadge.textContent) - 1;
            countBadge.textContent = newCount;
            
            // Handle empty list state
            if (newCount === 0) {
                window.location.reload(); // Show "no movies" message
            }
        }
        
        // Check if current page is now empty
        const container = document.getElementById('movies-grid');
        if (container && !container.querySelector('.col')) {
            window.location.reload(); // Handle pagination adjustment
        }
        
        showTempAlert(`Movie "${movieTitle}" deleted successfully!`, 'success');
    }, 300);
}
```

**Key Movie Deletion Features:**
- **Dual-Request Support**: Backend handles both AJAX (JSON response) and standard POST (redirect) requests
- **Real-Time Count Updates**: Movie count badge updates immediately after deletion
- **Smart Empty State Handling**: Automatically refreshes page when list becomes empty to show proper empty state
- **Pagination Awareness**: Detects when current page becomes empty and reloads to adjust pagination
- **Confirmation Dialog**: User confirmation required before deletion with movie title display
- **Smooth Animations**: 300ms fade-out effect provides professional visual feedback
- **Comprehensive Error Handling**: Distinguishes between network, server, and parsing errors with appropriate user messaging

### 🗃️ Business Rules Implementation

#### Mutual Exclusion Logic
```csharp
// A movie cannot exist in both wishlist and blacklist
var existsInWishlist = await _dbContext.WishlistItems
    .AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
    
if (existsInWishlist)
{
    // Handle conflict - guide user to resolve
}
```

#### Performance-First Patterns
- **Batch Processing**: Always use `GetMultipleMovieDetailsAsync()` for multiple movies
- **CacheService**: 15-minute expiration for user blacklist/wishlist IDs
- **Pagination**: 20 items per page with proper navigation
- **Index Usage**: Composite indexes on `UserId+Title` for fast searches

### 📊 Suggestion Type Patterns

#### Trending Suggestions
- **Pool Size**: 30 movies from multiple TMDB pages
- **Filtering**: Exclude blacklisted and last 5 watched movies
- **Caching**: 90-minute TMDB service cache
- **Selection**: Random 3 from filtered pool

#### Director/Cast Suggestions
- **Sequencing**: Recent → Frequent → Top-rated → Random
- **Anti-repetition**: Session-based tracking
- **Proactive Filtering**: Check for available movies before suggesting director
- **Deduplication**: Case-insensitive director/actor name matching

#### Genre/Decade Suggestions
- **Dynamic Variety**: Random sort criteria and page selection
- **Quality Filtering**: 6.5+ rating with sufficient vote counts
- **Triple Fallback**: Random → Page 1 → Popular fallback
- **Session Tracking**: Genre/decade sequence position

#### Surprise Me System
- **Pool Architecture**: 50 deduplicated movies built in parallel
- **Parallel Execution**: Up to 15 concurrent TMDB calls
- **Build Performance**: ~400-450ms (85% faster than sequential)
- **Anti-repetition**: Track 3 previous pool rotations (6-hour windows)
- **Instant Reshuffles**: Zero API calls after initial pool build

## ⚡ Performance Optimizations

### 🚀 Caching Strategy
- **TMDB API**: 24-hour cache in IMemoryCache
- **User data**: 15-minute cache for blacklist/wishlist IDs via CacheService
- **Suggestion pools**: Varying timeframes (2 hours for Surprise Me)

### 🗄️ Database Performance
- **Performance indexes**: UserId columns for all user-specific tables
- **Composite indexes**: UserId+Title for faster searches
- **Pagination**: 20 items per page for large datasets

### 🌐 API Efficiency
- **Batch operations**: `GetMultipleMovieDetailsAsync()` to avoid N+1 queries
- **Parallel execution**: For pool building (Surprise Me system)
- **Rate limiting**: Semaphore-based throttling to prevent TMDB issues

### 🎨 UI/UX Standards

#### 🎭 Visual Design
- **Cinema Gold branding**: `.cinelog-gold-title` class for section titles
- **Consistent layout**: Card-based design for movie displays
- **Dark theme**: Cyborg Bootstrap theme throughout
- **Modern Authentication**: Centered forms with friendly titles (h3) and professional typography

#### 👤 User Experience
- **Mutual exclusion**: Prevents movies in both wishlist and blacklist
- **Visual feedback**: For all user actions
- **Real-time updates**: AJAX without page reloads
- **Responsive design**: Mobile-friendly interface
- **Welcoming Authentication**: Friendly messaging ("Welcome Back", "Join CineLog") with clean, centered layouts
- **Reference-Only Display**: Streaming provider icons shown for reference without external navigation

---

## 💻 Development Patterns

### 🎨 Reference-Only UI Pattern
**For external data that should be shown but not linked:**
```html
<!-- ✅ CORRECT - Reference display only -->
<img src="@(tmdbImageBaseUrl + provider.LogoPath)"
     class="rounded"
     style="width: 50px; height: 50px;"
     title="@provider.ProviderName"
     alt="@provider.ProviderName logo">

<!-- ❌ AVOID - External navigation -->
<a href="@provider.Link" target="_blank">
    <img src="@(tmdbImageBaseUrl + provider.LogoPath)" ... >
</a>
```
**Use cases:**
- Streaming provider icons (Netflix, Disney+, etc.)
- External service references that should inform but not redirect
- Third-party branding display

### 🔒 User Data Security
**ALWAYS filter by current user:**
```csharp
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);
```

### 🌐 TMDB Service Usage
**Use service for all external API calls:**
```csharp
var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
var searchResults = await _tmdbService.SearchMoviesAsync(query);
```

### 🎯 Suggestion System Implementation
**Use unified helper methods:**
```csharp
private async Task<List<TmdbMovieBrief>> GetTypeMoviesWithFiltering(string userId)
{
    // Get user blacklist and recent movies for filtering
    // Build movie pool with pagination
    // Apply deduplication and randomization
    // Return consistent results for both initial and AJAX calls
}
```

**Director filtering pattern:**
```csharp
// Check if director has available movies before including in suggestions
private async Task<bool> HasAvailableMoviesForDirector(string directorName, string userId)
{
    // Lightweight check to prevent empty suggestion messages
    // Filters out directors with all movies blacklisted
    // Returns true only if director has at least one non-blacklisted movie
}
```

### ⚡ Cache Management
**Use CacheService for user-specific data:**
```csharp
var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
// ... perform operations ...
_cacheService.InvalidateUserBlacklistCache(userId); // After changes
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

**📋 Key Points:**
- `paginatedList.TotalCount` = Total records in database
- `viewModels.Count` = Records on current page only (max 20)
- Always use `TotalCount` for proper navigation

---

## ✅ Code Quality Standards

### 🔨 Build Requirements (CRITICAL)
- **NEVER** consider task complete if application cannot build
- **ALWAYS** run `dotnet build` before marking tasks finished
- Build failures MUST be resolved as part of implementation

### 📝 Documentation & Comments Standards

#### 🎯 XML Documentation (Required)
- **All public methods** must have comprehensive XML documentation
- **Include purpose, parameters, returns, and remarks** when applicable
- **Document edge cases** and special behavior
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

#### 📋 Documentation Requirements
- **English only** for international collaboration
- **Professional tone** - avoid casual or development-only comments
- **Business-focused** - explain impact and purpose
- **Maintainable** - help future developers understand decisions
- **Consistent formatting** - follow established patterns in codebase
- **Structured logging** - use `_logger.LogInformation("English message")` instead of console output

#### 🚫 Avoid These Comment Patterns
- Development artifacts like "ADD THIS", "NUEVAS", "TODO"
- Shallow comments that just repeat the code
- Spanish comments (replace with English)
- Comments without business justification or technical value

### 🚨 Error Handling
- **Structured logging** using `_logger` throughout
- **Try-catch blocks** for external API calls
- **Graceful fallbacks** for suggestion system edge cases
- **Proactive filtering** to prevent empty suggestion states (e.g., director blacklist filtering)

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

### 🚀 Deployment Notes
- Configure TMDB API token properly
- Update database connection string
- Configure session storage for load balancing