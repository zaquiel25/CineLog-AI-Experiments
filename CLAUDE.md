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

---

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

### 5. 🐛 Debugging & Testing
**Ensure robust solutions:**
- Use debugging tools to check for problems
- Determine root causes, not just symptoms
- Use print statements, logs, or temporary code for inspection
- Test edge cases rigorously - this is the #1 failure mode
- Iterate until solution is perfect and all tests pass
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

---

## 🤖 Claude Code Subagents System

CineLog uses specialized Claude Code subagents for accelerated development. Each subagent has deep knowledge of specific architectural patterns and operates in its own context window for focused expertise.

### 🎬 Available Subagents

#### 🎭 `cinelog-movie-specialist`
**🎯 Purpose**: Movie-specific features and suggestion system work

**🧠 Expertise**: 
- MoviesController patterns and movie CRUD operations
- Suggestion algorithms (trending, director, genre, cast, decade, surprise me)
- User movie data management and filtering logic
- AJAX reshuffle implementations and session state

**🔑 Key Patterns**:
```csharp
// User data isolation
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);

// Unified filtering methods
var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
```

#### 🌐 `tmdb-api-expert`  
**🎯 Purpose**: External API integration and movie data operations

**🧠 Expertise**:
- TmdbService architecture and HTTP client management
- Rate limiting with SemaphoreSlim (6 concurrent requests)
- TMDB data caching (24-hour expiration)
- Batch API operations to prevent N+1 queries

#### 🗄️ `ef-migration-manager`
**🎯 Purpose**: Database operations and schema management

**🧠 Expertise**:
- Entity Framework Core migrations with SQL Server
- Performance index optimization for user-specific queries
- Entity model configuration and Identity integration
- UserId isolation patterns and composite indexes

#### ⚡ `performance-optimizer`
**🎯 Purpose**: Performance optimization and caching strategies

**🧠 Expertise**:
- IMemoryCache optimization and CacheService patterns
- Database query performance with proper indexing
- API call efficiency and parallel execution
- Async/await patterns for scalability

#### 🏗️ `aspnet-feature-developer`
**🎯 Purpose**: Complete feature development from database to UI

**🧠 Expertise**:
- ASP.NET Core MVC patterns and controller development
- Razor view development with Bootstrap 5 (Cyborg theme)
- AJAX implementation with server-side rendering
- Cinema Gold branding and UI/UX standards

#### 📚 `docs-architect`
**🎯 Purpose**: Documentation maintenance and architecture updates

**🧠 Expertise**:
- Technical documentation for all project files
- Architecture pattern documentation
- Change tracking and CHANGELOG management
- Development workflow documentation

### 🎯 Usage Patterns

**🤖 Automatic Delegation**: Subagents are invoked automatically based on task context:
```
"Add a new suggestion type for movies by runtime" → cinelog-movie-specialist
"Improve TMDB API caching" → tmdb-api-expert  
"Add performance indexes for new queries" → ef-migration-manager
```

**👤 Explicit Invocation**: Request specific subagents:
```
"Use the performance-optimizer to improve the suggestion system"
"Use the aspnet-feature-developer to create a movie rating feature"
```

### 🚀 Development Benefits

- **⚡ Context Efficiency**: Each subagent maintains focused expertise without context pollution
- **🏗️ Architectural Consistency**: Deep knowledge of CineLog patterns ensures consistent implementation
- **🎯 Accelerated Development**: Task-specific expertise reduces implementation time
- **✅ Quality Assurance**: Specialized knowledge maintains performance and security standards

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

### 🗄️ Database Commands
```bash
dotnet ef migrations add <Name>        # Create new migration
dotnet ef database update              # Apply migrations to database
dotnet ef database drop                # Drop database (development only)
```

**📊 Entity Framework Notes:**
- Uses Entity Framework Core with SQL Server
- Connection string hardcoded in Program.cs for development

---

## 🏛️ Architecture Overview

### 🔧 Tech Stack
- **🚀 Framework**: ASP.NET Core 8.0 with MVC pattern
- **🗄️ Database**: SQL Server with Entity Framework Core
- **🔐 Authentication**: ASP.NET Core Identity 
- **🌐 External API**: The Movie Database (TMDB) API integration
- **🎨 Frontend**: Bootstrap 5 with Cyborg dark theme, jQuery for AJAX
- **⚡ Caching**: IMemoryCache for TMDB data, custom CacheService for user-specific data

### 🏗️ Core Architecture Patterns

#### 🗃️ Data Layer
- **`ApplicationDbContext`**: EF Core context with Identity integration
- **Entity models** in `Models/Entities/`: Movies, WishlistItem, BlacklistedMovie
- **User isolation**: All user data isolated by `UserId` foreign key

#### ⚙️ Service Layer
- **`TmdbService`**: Handles all external TMDB API calls with caching and rate limiting
- **`CacheService`**: Centralized caching for user blacklist/wishlist IDs (15-minute expiration)
- **Dependency injection**: Configured in `Program.cs`

#### 🎮 Controller Layer
- **`MoviesController`**: Main business logic for movie management and suggestions
- **Authentication**: Enforced via `[Authorize]` attribute
- **Security**: All data queries filtered by current user ID

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

#### 👤 User Experience
- **Mutual exclusion**: Prevents movies in both wishlist and blacklist
- **Visual feedback**: For all user actions
- **Real-time updates**: AJAX without page reloads
- **Responsive design**: Mobile-friendly interface

---

## 💻 Development Patterns

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

### 📝 Documentation
- **XML documentation** required for all public methods
- **Business logic comments** explaining "why" not "what"
- **English only** for international collaboration

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

### 🚀 Deployment Notes
- Configure TMDB API token properly
- Update database connection string
- Configure session storage for load balancing