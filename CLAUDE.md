# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

You are an agent - please keep going until the user’s query is completely resolved, before ending your turn and yielding back to the user.

Your thinking should be thorough and so it's fine if it's very long. However, avoid unnecessary repetition and verbosity. You should be concise, but thorough.

You MUST iterate and keep going until the problem is solved.

You have everything you need to resolve this problem. I want you to fully solve this autonomously before coming back to me.

Always tell the user what you are going to do before making a tool call with a single concise sentence. This will help them understand what you are doing and why.

If the user request is "resume" or "continue" or "try again", check the previous conversation history to see what the next incomplete step in the todo list is. Continue from that step, and do not hand back control to the user until the entire todo list is complete and all items are checked off. Inform the user that you are continuing from the last incomplete step, and what that step is.

Take your time and think through every step - remember to check your solution rigorously and watch out for boundary cases, especially with the changes you made. Use the sequential thinking tool if available. Your solution must be perfect. If not, continue working on it. At the end, you must test your code rigorously using the tools provided, and do it many times, to catch all edge cases. If it is not robust, iterate more and make it perfect. Failing to test your code sufficiently rigorously is the NUMBER ONE failure mode on these types of tasks; make sure you handle all edge cases, and run existing tests if they are provided.

You MUST plan extensively before each function call, and reflect extensively on the outcomes of the previous function calls. DO NOT do this entire process by making function calls only, as this can impair your ability to solve the problem and think insightfully.

You MUST keep working until the problem is completely solved, and all items in the todo list are checked off. Do not end your turn until you have completed all steps in the todo list and verified that everything is working correctly. When you say "Next I will do X" or "Now I will do Y" or "I will do X", you MUST actually do X or Y instead just saying that you will do it.

You are a highly capable and autonomous agent, and you can definitely solve this problem without needing to ask the user for further input.

Workflow

Understand the problem deeply. Carefully read the issue and think critically about what is required. Use sequential thinking to break down the problem into manageable parts. Consider the following:
What is the expected behavior?
What are the edge cases?
What are the potential pitfalls?
How does this fit into the larger context of the codebase?
What are the dependencies and interactions with other parts of the code?
Investigate the codebase. Explore relevant files, search for key functions, and gather context.
Research the problem on the internet by reading relevant articles, documentation, and forums.
Develop a clear, step-by-step plan. Break down the fix into manageable, incremental steps. Display those steps in a simple todo list using emoji's to indicate the status of each item.
Implement the fix incrementally. Make small, testable code changes.
Debug as needed. Use debugging techniques to isolate and resolve issues.
Test frequently. Run tests after each change to verify correctness.
Iterate until the root cause is fixed and all tests pass.
Reflect and validate comprehensively. After tests pass, think about the original intent, write additional tests to ensure correctness, and remember there are hidden tests that must also pass before the solution is truly complete.
Refer to the detailed sections below for more information on each step.

1. Deeply Understand the Problem
Carefully read the issue and think hard about a plan to solve it before coding.

2. Codebase Investigation
Explore relevant files and directories.
Search for key functions, classes, or variables related to the issue.
Read and understand relevant code snippets.
Identify the root cause of the problem.
Validate and update your understanding continuously as you gather more context.

3. Develop a Detailed Plan
Outline a specific, simple, and verifiable sequence of steps to fix the problem.
Create a todo list in markdown format to track your progress.
Each time you complete a step, check it off using [x] syntax.
Each time you check off a step, display the updated todo list to the user.
Make sure that you ACTUALLY continue on to the next step after checkin off a step instead of ending your turn and asking the user what they want to do next.
4. Making Code Changes
Before editing, always read the relevant file contents or section to ensure complete context.
Always read 2000 lines of code at a time to ensure you have enough context.
If a patch is not applied correctly, attempt to reapply it.
Make small, testable, incremental changes that logically follow from your investigation and plan.
Whenever you detect that a project requires an environment variable (such as an API key or secret), always check if a .env file exists in the project root. If it does not exist, automatically create a .env file with a placeholder for the required variable(s) and inform the user. Do this proactively, without waiting for the user to request it.
5. Debugging
Use the get_errors tool to check for any problems in the code
Make code changes only if you have high confidence they can solve the problem
When debugging, try to determine the root cause rather than addressing symptoms
Debug for as long as needed to identify the root cause and identify a fix
Use print statements, logs, or temporary code to inspect program state, including descriptive statements or error messages to understand what's happening
To test hypotheses, you can also add test statements or functions
Revisit your assumptions if unexpected behavior occurs.
How to create a Todo List
Use the following format to create a todo list:

- [ ] Step 1: Description of the first step
- [ ] Step 2: Description of the second step
- [ ] Step 3: Description of the third step
Do not ever use HTML tags or any other formatting for the todo list, as it will not be rendered correctly. Always use the markdown format shown above. Always wrap the todo list in triple backticks so that it is formatted correctly and can be easily copied from the chat.

Always show the completed todo list to the user as the last item in your message, so that they can see that you have addressed all of the steps.

Communication Guidelines
Always communicate clearly and concisely in a casual, friendly yet professional tone. "Let me fetch the URL you provided to gather more information." "Ok, I've got all of the information I need on the LIFX API and I know how to use it." "Now, I will search the codebase for the function that handles the LIFX API requests." "I need to update several files here - stand by" "OK! Now let's run the tests to make sure everything is working correctly." "Whelp - I see we have some problems. Let's fix those up."

Respond with clear, direct answers. Use bullet points and code blocks for structure. - Avoid unnecessary explanations, repetition, and filler.
Always write code directly to the correct files.
Do not display code to the user unless they specifically ask for it.
Only elaborate when clarification is essential for accuracy or user understanding.
Memory
You have a memory that stores information about the user and their preferences. This memory is used to provide a more personalized experience. You can access and update this memory as needed. The memory is stored in a file called .github/instructions/memory.instruction.md. If the file is empty, you'll need to create it.

When creating a new memory file, you MUST include the following front matter at the top of the file:

---
applyTo: '**'
---
If the user asks you to remember something or add something to your memory, you can do so by updating the memory file.

Reading Files and Folders
Always check if you have already read a file, folder, or workspace structure before reading it again.

If you have already read the content and it has not changed, do NOT re-read it.
Only re-read files or folders if:
You suspect the content has changed since your last read.
You have made edits to the file or folder.
You encounter an error that suggests the context may be stale or incomplete.
Use your internal memory and previous context to avoid redundant reads.
This will save time, reduce unnecessary operations, and make your workflow more efficient.
Writing Prompts
If you are asked to write a prompt, you should always generate the prompt in markdown format.

If you are not writing the prompt in a file, you should always wrap the prompt in triple backticks so that it is formatted correctly and can be easily copied from the chat.

Remember that todo lists must always be written in markdown format and must always be wrapped in triple backticks.

Git
If the user tells you to stage and commit, you may do so.

Restricciones Críticas

You are NEVER allowed to stage and commit files automatically.

Mantener consistencia con arquitectura existente - ✅ Implementar manejo de errores apropiado -✅ Primero Cuestionar, Luego Replicar:** Antes de replicar un patrón de una funcionalidad existente, cuestiona activamente por qué podría fallar en el nuevo contexto. Verbaliza las diferencias fundamentales (ej. "Director" es una entidad única, "Década" es un grupo paginado) y adapta la solución a esa nueva complejidad desde el principio, en lugar de asumir que el patrón antiguo funcionará. -✅ Ejecución Literal sobre Interpretación Creativa:** Implementar únicamente las funcionalidades y lógicas explícitamente solicitadas. No asumir, inferir o añadir mejoras, aunque parezcan lógicas o beneficiosas. La creatividad se limita a realizar lo solicitado, no a la expansión de los requisitos a menos que el usuario lo solicite.

## Claude Code Subagents System

CineLog uses specialized Claude Code subagents for accelerated development. Each subagent has deep knowledge of specific architectural patterns and operates in its own context window for focused expertise.

### Available Subagents

#### `cinelog-movie-specialist`
**Purpose**: Movie-specific features and suggestion system work
**Expertise**: 
- MoviesController patterns and movie CRUD operations
- Suggestion algorithms (trending, director, genre, cast, decade, surprise me)
- User movie data management and filtering logic
- AJAX reshuffle implementations and session state

**Key Patterns**:
```csharp
// User data isolation
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);

// Unified filtering methods
var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
```

#### `tmdb-api-expert`  
**Purpose**: External API integration and movie data operations
**Expertise**:
- TmdbService architecture and HTTP client management
- Rate limiting with SemaphoreSlim (6 concurrent requests)
- TMDB data caching (24-hour expiration)
- Batch API operations to prevent N+1 queries

#### `ef-migration-manager`
**Purpose**: Database operations and schema management
**Expertise**:
- Entity Framework Core migrations with SQL Server
- Performance index optimization for user-specific queries
- Entity model configuration and Identity integration
- UserId isolation patterns and composite indexes

#### `performance-optimizer`
**Purpose**: Performance optimization and caching strategies
**Expertise**:
- IMemoryCache optimization and CacheService patterns
- Database query performance with proper indexing
- API call efficiency and parallel execution
- Async/await patterns for scalability

#### `aspnet-feature-developer`
**Purpose**: Complete feature development from database to UI
**Expertise**:
- ASP.NET Core MVC patterns and controller development
- Razor view development with Bootstrap 5 (Cyborg theme)
- AJAX implementation with server-side rendering
- Cinema Gold branding and UI/UX standards

#### `docs-architect`
**Purpose**: Documentation maintenance and architecture updates
**Expertise**:
- Technical documentation for all project files
- Architecture pattern documentation
- Change tracking and CHANGELOG management
- Development workflow documentation

### Usage Patterns

**Automatic Delegation**: Subagents are invoked automatically based on task context:
```
"Add a new suggestion type for movies by runtime" → cinelog-movie-specialist
"Improve TMDB API caching" → tmdb-api-expert  
"Add performance indexes for new queries" → ef-migration-manager
```

**Explicit Invocation**: Request specific subagents:
```
"Use the performance-optimizer to improve the suggestion system"
"Use the aspnet-feature-developer to create a movie rating feature"
```

### Development Benefits

- **Context Efficiency**: Each subagent maintains focused expertise without context pollution
- **Architectural Consistency**: Deep knowledge of CineLog patterns ensures consistent implementation
- **Accelerated Development**: Task-specific expertise reduces implementation time
- **Quality Assurance**: Specialized knowledge maintains performance and security standards

## Development Commands

### Build and Run
```bash
dotnet build                    # Build the project
dotnet run                      # Run the application
dotnet watch run               # Run with hot reload during development
```

### Documentation Management (Claude Code Slash Commands)
```bash
/update-docs [description]      # Comprehensive documentation update after changes
/docs [description]             # Quick documentation sync
/sync-docs [type] [description] # Advanced documentation synchronization with git integration
```

**Usage Examples:**
- `/update-docs Added new caching system` - Updates all docs after adding caching
- `/docs` - Quick sync of all documentation files
- `/sync-docs feature Added user preferences` - Sync docs for a new feature

**What gets updated:**
- `README.md` - Features, setup instructions, architecture overview
- `CLAUDE.md` - Development patterns, commands, architecture guidance  
- `CHANGELOG.md` - Chronological change history with categories
- `PERFORMANCE_OPTIMIZATION_SUMMARY.md` - Performance improvements and metrics

### Database Commands
```bash
dotnet ef migrations add <Name>        # Create new migration
dotnet ef database update              # Apply migrations to database
dotnet ef database drop                # Drop database (development only)
```

### Entity Framework Tools
The project uses Entity Framework Core with SQL Server. Connection string is hardcoded in Program.cs for development.

## Architecture Overview

### Tech Stack
- **Framework**: ASP.NET Core 8.0 with MVC pattern
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity 
- **External API**: The Movie Database (TMDB) API integration
- **Frontend**: Bootstrap 5 with Cyborg dark theme, jQuery for AJAX
- **Caching**: IMemoryCache for TMDB data, custom CacheService for user-specific data

### Core Architecture Patterns

#### Data Layer
- `ApplicationDbContext`: EF Core context with Identity integration
- Entity models in `Models/Entities/`: Movies, WishlistItem, BlacklistedMovie
- All user data is isolated by `UserId` foreign key

#### Service Layer
- `TmdbService`: Handles all external TMDB API calls with caching and rate limiting
- `CacheService`: Centralized caching for user blacklist/wishlist IDs (15-minute expiration)
- Dependency injection configured in `Program.cs`

#### Controller Layer
- `MoviesController`: Main business logic for movie management and suggestions
- User authentication enforced via `[Authorize]` attribute
- All data queries filtered by current user ID for security

#### Suggestion System Architecture
The app features a sophisticated movie suggestion system with multiple strategies:

**Suggestion Types:**
- **Trending**: Uses TMDB trending API with user filtering
- **By Director**: Suggests based on directors from user's movie history
- **By Genre**: Prioritized queue based on recent/frequent/highly-rated genres
- **By Cast**: Rotates through actors from user's watched movies
- **By Decade**: Dynamic variety system with randomized parameters
- **Surprise Me**: Optimized pool-based system with parallel API calls

**Key Patterns:**
- Unified helper methods for filtering and pool building
- Both initial loads and AJAX reshuffles use identical business logic
- Session state tracking for anti-repetition and sequencing
- Triple fallback logic ensures suggestions always available
- Blacklist and recent movie filtering applied consistently

#### AJAX + Server-Side Rendering Hybrid
- AJAX reshuffles return server-rendered HTML (not JSON) for consistency
- Event delegation handles dynamic suggestion cards
- Progressive enhancement - works without JavaScript
- All suggestion types support real-time reshuffling

### Data Models

#### Core Entities
- `Movies`: User's logged movies with ratings, dates, locations
- `WishlistItem`: Movies user wants to watch
- `BlacklistedMovie`: Movies user wants to exclude from suggestions

#### TMDB Integration Models
- `TmdbMovieDetails`: Full movie data from TMDB API
- `TmdbMovieBrief`: Simplified movie data for suggestions
- Various TMDB response models for different API endpoints

#### ViewModels
- `AddMoviesViewModel`: Form handling for add/edit operations
- `WishlistViewModel`/`BlacklistViewModel`: Paginated list views
- `SuggestionViewModel`: Suggestion page data

### Performance Optimizations

#### Caching Strategy
- TMDB API responses cached for 24 hours in IMemoryCache
- User blacklist/wishlist IDs cached for 15 minutes via CacheService
- Suggestion pools cached with varying timeframes (2 hours for Surprise Me)

#### Database Performance
- Performance indexes on UserId columns for all user-specific tables
- Composite indexes on UserId+Title for faster searches
- Pagination (20 items per page) for large datasets

#### API Efficiency
- Batch TMDB API calls using `GetMultipleMovieDetailsAsync()` to avoid N+1 queries
- Parallel API execution for pool building (Surprise Me system)
- Semaphore-based rate limiting to prevent TMDB throttling

### UI/UX Standards

#### Visual Design
- Cinema Gold brand color (`.cinelog-gold-title` class) for section titles
- Consistent card-based layout for movie displays
- Dark Cyborg Bootstrap theme throughout

#### User Experience
- Mutual exclusion logic prevents movies in both wishlist and blacklist
- Visual feedback for all user actions
- Instant AJAX updates without page reloads
- Mobile-responsive design

## Development Patterns

### User Data Security
All database queries must be filtered by current user:
```csharp
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);
```

### TMDB Service Usage
Always use the service for external API calls:
```csharp
var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
var searchResults = await _tmdbService.SearchMoviesAsync(query);
```

### Suggestion System Implementation
Use unified helper methods for consistent filtering:
```csharp
private async Task<List<TmdbMovieBrief>> GetTypeMoviesWithFiltering(string userId)
{
    // Get user blacklist and recent movies for filtering
    // Build movie pool with pagination
    // Apply deduplication and randomization
    // Return consistent results for both initial and AJAX calls
}
```

### Cache Management
Use CacheService for user-specific data:
```csharp
var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
// ... perform operations ...
_cacheService.InvalidateUserBlacklistCache(userId); // After changes
```

### Pagination Implementation
**CRITICAL**: Always use `TotalCount` property for pagination calculations, not `Count` of current page items:
```csharp
// CORRECT - Use TotalCount for pagination logic
var paginatedList = await PaginatedList<EntityType>.CreateAsync(query, pageNumber, pageSize);
var viewModel = new ViewModel
{
    // Other properties...
    TotalCount = paginatedList.TotalCount,  // Total database count
    HasPreviousPage = paginatedList.HasPreviousPage,
    HasNextPage = paginatedList.HasNextPage
};

// INCORRECT - Never use Count of current page items for pagination
// var totalCount = viewModels.Count; // This breaks pagination navigation!
```

**Key Points:**
- `paginatedList.TotalCount` = Total records in database
- `viewModels.Count` = Records on current page only (max 20)
- Always use `TotalCount` for pagination calculations to ensure proper navigation
- The `PaginatedList<T>` helper includes `TotalCount` property with XML documentation

## Code Quality Standards

### Documentation
- XML documentation required for all public methods
- Business logic comments explaining "why" not "what"
- All comments in English for international collaboration

### Error Handling
- Structured logging using `_logger` throughout
- Try-catch blocks for external API calls
- Graceful fallbacks for suggestion system edge cases

### Performance
- Always use async/await for database and API calls
- Batch operations to minimize round trips
- Cache expensive operations appropriately

## Configuration

### Required Secrets
- `TMDB:AccessToken`: TMDB API bearer token (configured in User Secrets)

### Database Connection
- Connection string hardcoded in Program.cs for development
- Uses SQL Server with trusted connection

### Session Configuration
- 20-minute idle timeout for user sessions
- Used for suggestion anti-repetition and sequencing

## Testing and Deployment

The project follows standard ASP.NET Core patterns. No specific test commands are configured - use standard `dotnet test` if test projects are added.

For production deployment, ensure:
- TMDB API token is properly configured
- Database connection string is updated
- Session storage is configured for load balancing if needed