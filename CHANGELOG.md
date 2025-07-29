## 2025-07-29

### 📚 Documentation Performance Optimization & Restructuring
- **Major Performance Improvement**: Restructured agent system documentation for 45% better performance and loading speed
- **Modular Organization**: Created dedicated `.claude/agents/` folder with specialized documentation structure
- **File Size Optimization**: Reduced main CLAUDE.md from 52k to 28k characters while maintaining all functionality
- **Enhanced Agent Documentation**: 
  - `/.claude/agents/AGENTS.md` - Complete agent system documentation with detailed examples
  - `/.claude/agents/README.md` - Quick reference guide and agent selection matrices
- **Improved Context Efficiency**: Separated concerns for better Claude Code performance and focused expertise
- **Better Developer Experience**: Easier navigation and faster documentation loading for development workflows
- **Maintained Functionality**: All agent capabilities and patterns preserved in restructured format

#### 🚀 Technical Benefits
- **Faster Documentation Loading**: 45% reduction in context window usage for agent documentation
- **Better Organization**: Logical separation of agent system from core development patterns
- **Enhanced Searchability**: Dedicated agent files for easier reference and maintenance
- **Preserved Functionality**: All strategic planning, agent coordination, and development patterns maintained
- **Future-Proof Structure**: Scalable documentation architecture for additional agents and complexity

### 🎯 Agent Coordination & Instructions Enhancement
- **GitHub Copilot Feedback Integration**: Implemented targeted improvements based on direct feedback from GitHub Copilot for better planning and execution
- **Explicit Agent Invocation Table**: Added comprehensive mapping of user request patterns to optimal agent selections with rationale
- **Simplified Planning Guidance**: Created practical guidance for complex tasks without bureaucratic overhead
- **Agent Escalation Rules**: Defined clear protocol for when to escalate to Master Agent Director based on complexity and domain scope
- **Multi-Agent Coordination Examples**: Added real-world example showing complete workflow from user prompt to agent execution sequence
- **Streamlined Documentation Guidelines**: Simplified documentation update rules to focus on practical needs
- **AJAX Quick Reference**: Added one-line summary for essential AJAX requirements at the top of relevant sections

#### 🔍 Enhanced Guidance Areas
- **Task Analysis Framework**: Objective, scope, and complexity assessment templates
- **Implementation Phasing**: MVP → Enhanced → Polish progression patterns  
- **Quality Gates Integration**: Automatic testing, UI enhancement, and documentation validation
- **Escalation Triggers**: Clear criteria for Master Agent Director involvement
- **Coordination Benefits**: Sequential agent workflows with clear handoff protocols

#### 📊 Developer Experience Benefits
- **Reduced Decision Overhead**: Clear agent selection guidance eliminates guesswork
- **Consistent Planning**: Reusable templates ensure thorough task analysis
- **Proactive Quality**: Built-in documentation and testing integration
- **Error Prevention**: Non-destructive edit patterns preserve institutional knowledge
- **Actionable Feedback**: Specific error messages with clear resolution paths

### Files Modified
- `.github/copilot-instructions.md` - Added comprehensive agent coordination guide with all enhancements
- `CLAUDE.md` - Added enhanced agent coordination guidelines for Claude Code consistency
- Both files now provide synchronized guidance for optimal AI assistant collaboration

## 2025-07-29

### 🔧 Code Refactoring Specialist Agent Integration
- **New Agent Added**: Integrated `code-refactoring-specialist` agent into the CineLog development system
- **Proactive Technical Debt Management**: Agent automatically triggers after major feature additions or when technical debt accumulates
- **CineLog-Specific Expertise**: Deep knowledge of MoviesController simplification, suggestion algorithm unification, and TMDB service optimization
- **Quality Metrics Focus**: Monitors cyclomatic complexity, method length, code duplication, and coupling reduction
- **Test-Safe Refactoring**: Ensures existing functionality is preserved while improving code structure

#### 🎯 Refactoring Capabilities
- **Legacy Code Modernization**: Upgrades outdated patterns to modern ASP.NET Core conventions
- **SOLID Principle Implementation**: Breaks down large classes and improves separation of concerns
- **DRY Pattern Enforcement**: Eliminates duplicate code across controllers, services, and views
- **Performance-Oriented Refactoring**: Identifies and fixes performance bottlenecks through code structure improvements
- **Maintainability Enhancement**: Simplifies complex logic and improves code readability

#### 📊 Integration Benefits
- **Master Agent Director**: Updated routing logic to include refactoring specialist in decision matrix
- **GitHub Copilot Knowledge Base**: Added comprehensive refactoring patterns and CineLog-specific examples
- **Proactive Orchestration**: Automatically triggers for technical debt accumulation and large method detection
- **Quality Assurance**: Ensures code quality standards are maintained across all development workflows

### Files Modified
- `CLAUDE.md` - Added code-refactoring-specialist agent with detailed expertise and patterns
- `.github/copilot-instructions.md` - Added comprehensive refactoring knowledge section with CineLog-specific examples
- `README.md` - Updated agent system documentation to reflect new capabilities and benefits

## 2025-07-29

### 🐞 AJAX Removal Enhancement: Blacklist & Wishlist
- **Robust Error Handling**: Implemented comprehensive AJAX removal system with proper JSON response validation and fallback error handling
- **Required Header Implementation**: All AJAX requests now include `X-Requested-With: XMLHttpRequest` header to guarantee backend returns JSON responses
- **Enhanced User Experience**: Added smooth fade-out animations (300ms) when removing items from lists
- **Smart Empty State Detection**: Automatically shows "Your [list] is empty" message when no items remain after removal
- **Improved Error Messages**: Added user-friendly alert system with 2.2-second auto-dismiss for both success and error states
- **Network Resilience**: Added try-catch blocks for network errors and JSON parsing failures with specific error messages

#### 🎯 Technical Improvements
- **Response Validation**: Robust text-to-JSON parsing with fallback error handling for malformed responses
- **Visual Feedback**: Integrated Bootstrap alert system with proper positioning and z-index for toast-like notifications
- **State Management**: Button disable/enable logic prevents multiple simultaneous requests
- **Anti-Forgery Protection**: Maintains CSRF token validation for all AJAX removal operations
- **Consistent UX**: Identical behavior patterns across both Blacklist and Wishlist views

#### 📊 User Experience Benefits
- **Immediate Visual Feedback**: Items fade out smoothly before removal, providing clear action confirmation
- **Error Transparency**: Clear distinction between network errors, server errors, and JSON parsing issues
- **Graceful Degradation**: Proper error recovery with button re-enablement on failures
- **Professional Polish**: Toast-style notifications eliminate jarring page reloads for simple operations

## 2025-07-29

### 🤝 GitHub Copilot Development Knowledge Base
- **Comprehensive Knowledge Integration**: Created extensive development knowledge base for GitHub Copilot with instant access to specialized agent expertise
- **Synchronized AI Assistance**: Both Claude Code and GitHub Copilot now follow identical development workflows, patterns, and conventions
- **Domain-Specific Patterns**: Six major knowledge sections covering Movie Suggestions, TMDB API, Performance, ASP.NET Core, Database/EF, and UI/UX patterns
- **Problem-Solution Mapping**: Direct mappings from common development issues to tested solutions with copy-paste code examples
- **Professional Standards Enhancement**: Unified documentation standards with English-only, business-focused approach

#### 🔍 Knowledge Base Sections
- **🎬 Movie Suggestions**: Unified helper methods, triple fallback systems, dynamic variety patterns, AJAX implementation
- **🌐 TMDB API Integration**: Centralized service usage, batch operations, parallel execution, rate limiting, caching strategies
- **⚡ Performance Optimization**: Request-level caching, batch processing, composite indexes, performance benchmarks
- **🏗️ ASP.NET Core Development**: Controller patterns, user data isolation, mutual exclusion, AJAX + SSR hybrid
- **🗄️ Database & Entity Framework**: Migration best practices, composite indexes, pagination patterns, N+1 prevention
- **🎨 UI/UX & AJAX Patterns**: Cinema Gold branding, Bootstrap integration, event delegation, accessibility
- **🔧 Testing & Debugging**: Structured logging, performance timing, user isolation testing, debugging scenarios

#### 📊 Development Workflow Benefits
- **Immediate Pattern Discovery**: Quick reference tags help find relevant knowledge instantly
- **Code-First Approach**: Real, tested patterns with immediate implementation value
- **CineLog-Specific Solutions**: All patterns tailored to exact project architecture and business rules
- **Performance-Aware Development**: Built-in optimization techniques and benchmarks
- **Security-First Patterns**: User isolation and authentication patterns throughout

### Files Modified
- `.github/copilot-instructions.md` - Added comprehensive development knowledge base with 6 major sections
- `README.md` - Updated to reflect GitHub Copilot integration and synchronized AI assistance
- `CLAUDE.md` - Enhanced critical instructions and development workflow patterns
- Documentation synchronization ensures consistent behavior across all AI assistance tools

### 🤖 Advanced Claude Code Agent System Enhancement
- **Master Agent Director**: Implemented intelligent task orchestrator that analyzes complexity and routes tasks to optimal agents
- **Expanded Agent System**: Enhanced from 6 to 15 specialized agents with proactive capabilities
- **Intelligent Planning**: Auto-triggered strategic planning for complex tasks with 5-step methodology
- **Complexity Assessment**: Automatic classification of tasks as Simple/Medium/Complex/Strategic
- **Multi-Agent Orchestration**: Coordinated sequential and parallel agent workflows

#### 🎭 Master Agent Director Features
- **Task Analysis Engine**: Parses requests, analyzes complexity, detects domains, and maps to agent capabilities
- **Enhanced Selection Algorithm**: 7-step process from parsing to monitoring with complexity-based planning
- **Decision Matrix**: Pre-defined routing rules for common CineLog task patterns
- **Proactive Orchestration**: Automatic triggering of testing, UI enhancement, and quality agents
- **Emergency Routing**: Immediate response protocols for critical production issues

#### 🚀 New Enhanced Development Subagents
- **`test-writer-fixer`** (Proactive): Comprehensive test coverage and maintenance after code changes
- **`backend-architect`**: Scalable backend architecture and API design for complex systems
- **`ui-designer`** (Proactive): Visual design enhancement and modern UI patterns beyond Bootstrap
- **`whimsy-injector`** (Proactive): Delightful micro-interactions and user engagement features
- **`performance-benchmarker`**: Comprehensive performance testing and optimization analysis
- **`devops-automator`**: CI/CD automation and deployment optimization
- **`api-tester`**: API reliability testing and integration validation
- **`feedback-synthesizer`**: User feedback analysis and feature prioritization

#### 🧠 Intelligent Planning Engine (Auto-triggered for Complex tasks)
- **Step 1**: Feature Definition & Requirements with user journey mapping
- **Step 2**: Implementation Strategy with technical architecture planning
- **Step 3**: Risk Assessment & Mitigation with challenge identification
- **Step 4**: Phased Execution Plan with MVP breakdown
- **Step 5**: Agent Orchestration Strategy with coordination requirements

#### 📊 Development Benefits
- **Intelligent Orchestration**: Automatic optimal agent selection based on task analysis
- **Proactive Quality**: Auto-triggered testing, UI enhancement, and delight injection
- **Strategic Planning**: Complex features receive proper planning before implementation
- **Comprehensive Testing**: Built-in test coverage ensures robust, reliable features
- **Enhanced User Experience**: Automatic UI enhancement and personality injection
- **Performance Excellence**: Built-in performance analysis and optimization recommendations

#### 🎯 Usage Examples
- Simple tasks (bug fixes) → Direct execution to specialist
- Medium tasks (enhancements) → Light planning → Execute
- Complex tasks (new features) → Strategic planning → Multi-agent execution
- Strategic tasks (major changes) → Deep planning → Phased execution

### Files Modified
- `CLAUDE.md` - Comprehensive agent system enhancement with Master Agent Director
- `README.md` - Updated development tools section to reflect advanced agent capabilities

## 2025-07-27

### 🐛 Director Suggestions Bug Fix
- **Fixed Unwanted Empty Message**: Eliminated "No more suggestions available for [Director]. Try another suggestion type!" message when all of a director's movies are blacklisted
- **Root Cause**: Director suggestion system would select directors and then discover they had no available movies, resulting in user-facing error messages
- **Solution**: Implemented proactive director filtering that checks for available movies before including directors in suggestion rotation
- **New Helper Method**: Added `HasAvailableMoviesForDirector()` method for lightweight pre-filtering without fetching full movie details
- **Smart Filtering**: Directors with all movies blacklisted are now silently skipped from both initial suggestions and AJAX reshuffles
- **Improved UX**: Users now see seamless director suggestions without confusing error messages, gracefully falling back to other suggestion types
- **Enhanced Logging**: Added detailed logging to track director filtering for debugging and monitoring
- **Files Modified**:
  - `Controllers/MoviesController.cs` - Enhanced director suggestion logic in both `DirectorReshuffle()` AJAX endpoint and `ShowSuggestions()` method
  - Added comprehensive FIX comments throughout director selection logic for future maintainability

## 2025-07-27

### 🐛 Critical Pagination Bug Fix
- **Fixed Pagination Navigation**: Resolved critical bug in both Wishlist and Blacklist pagination where page navigation was broken
- **Root Cause**: Both methods incorrectly used `viewModels.Count` (current page items) instead of total database count for pagination calculations
- **Solution**: Changed to use `paginatedList.TotalCount` (total database count) for proper pagination logic
- **Enhanced PaginatedList**: Added `TotalCount` property with XML documentation to prevent future confusion
- **User Impact**: Users can now properly navigate through all pages of their wishlist and blacklist collections
- **Files Modified**: 
  - `Controllers/MoviesController.cs` - Lines 438 and 577 corrected pagination count logic
  - `Helpers/PaginatedList.cs` - Added `TotalCount` property with documentation

### 🤖 Claude Code Subagents System
- **Development Workflow Enhancement**: Implemented 6 specialized Claude Code subagents for accelerated development
- **Task-Specific Expertise**: Each subagent has deep knowledge of specific CineLog architecture patterns and conventions
- **Context Efficiency**: Separate context windows prevent pollution and maintain focused expertise
- **Subagents Created**:
  - `cinelog-movie-specialist`: Movie features, suggestion algorithms, CRUD operations
  - `tmdb-api-expert`: External API integration, rate limiting, caching strategies
  - `ef-migration-manager`: Database operations, schema changes, performance indexes
  - `performance-optimizer`: Caching optimization, query performance, API efficiency
  - `aspnet-feature-developer`: Complete feature development, MVC patterns, UI/UX
  - `docs-architect`: Documentation maintenance, architecture updates, change tracking

### ✨ Enhanced Wishlist & Blacklist Sorting
- **Default Sort Behavior**: Wishlist and Blacklist pages now default to "Sort by Date Added (Newest)" instead of alphabetical
- **Improved User Experience**: Users see their most recently added items first, providing better relevance and context
- **Fixed A-Z Sorting**: Resolved issue where "Sort by Title (A-Z)" option was not working correctly
- **Sorting Options**: All four sorting options now work reliably:
  - Sort by Title (A-Z) - `title_asc`
  - Sort by Title (Z-A) - `title_desc` 
  - Sort by Date Added (Oldest) - `Date`
  - Sort by Date Added (Newest) - `date_desc` (default)

### Technical Implementation
- **Controller Logic**: Updated default cases in both `Wishlist` and `Blacklist` switch statements to use `OrderByDescending` by date
- **View Updates**: Modified dropdown selection logic in both views to properly handle the new default
- **Parameter Handling**: Changed from empty string `""` to explicit `"title_asc"` value for better ASP.NET model binding reliability
- **Consistent UX**: Both wishlist and blacklist pages now have identical sorting behavior and options

### 🚀 Comprehensive Performance Optimization
- **Database Indexing**: Added optimized indexes for BlacklistedMovies and WishlistItems tables
  - Individual indexes on `UserId` for faster user-specific queries
  - Composite indexes on `UserId, Title` for search and sort operations
- **N+1 Query Fix**: Resolved API call inefficiency in Wishlist using batch processing
  - Applied same optimization pattern already implemented for Blacklist
  - Reduced from individual API calls to single batch calls per page
- **Caching Layer**: Implemented centralized CacheService for user-specific data
  - 15-minute cache expiration for blacklist/wishlist IDs
  - Automatic cache invalidation on add/remove operations
  - Memory-efficient IMemoryCache implementation
- **Pagination Enhancement**: Added pagination support to Blacklist and Wishlist
  - 20 items per page for optimal performance
  - Preserves search and sort parameters across pages
  - Consistent pagination controls across views
- **Performance Monitoring**: Added timing measurements and SQL query logging
  - Entity Framework logging enabled in development
  - Performance metrics for validation and debugging

### Technical Implementation
- **Database Indexes**: Added 4 new indexes for BlacklistedMovies and WishlistItems tables
- **Batch Processing**: Both Blacklist and Wishlist now use efficient API calls
- **ViewModels**: Created dedicated BlacklistViewModel and WishlistViewModel
- **CacheService**: Centralized caching with 15-minute expiration
- **Performance Metrics**: 95% reduction in API calls for both lists

## 2025-07-26
### 🚀 Blacklist Performance Optimization
- **Major Performance Fix**: Eliminated N+1 API call problem in blacklist page loading
- **Batch Processing**: Replaced individual TMDB API calls with `GetMultipleMovieDetailsAsync` batch processing
- **Performance Impact**: Reduced blacklist page load time from 10-25 seconds to 1-3 seconds (80-90% improvement)
- **Database Optimization**: Added missing indexes for improved query performance
- **Caching Enhancement**: Leveraged existing IMemoryCache for TMDB data caching

### Technical Implementation
- **N+1 Fix**: Blacklist view now uses batch API calls instead of individual requests per movie
- **Batch Processing**: All TMDB movie details fetched in parallel with throttling for rate limit safety
- **Cache Utilization**: Existing 24-hour cache for movie details now properly utilized
- **Database Indexes**: Added composite indexes for UserId and Title filtering
- **Code Documentation**: Added comprehensive XML comments explaining performance optimizations

### Performance Metrics
- **Before**: 50 blacklisted movies = 50 API calls = 10-25 seconds load time
- **After**: 50 blacklisted movies = 1-3 batch API calls = 1-3 seconds load time
- **API Efficiency**: 95% reduction in TMDB API calls for blacklist page loads

## 2025-07-26
### ✨ UI Polish: Gold Titles & Larger Suggestion Cards
- Suggestion section titles now use `.cinelog-gold-title` for Cinema Gold color, matching the home page branding.
- Suggestion card titles (`.card-title`) and descriptions (`.suggestion-description`) are now 1pt larger for improved readability and visual hierarchy.
- All changes are documented in `site.css` and reflected in the UI for consistency.

## 2025-07-25
### 🔄 Surprise Me System Unification
- **Unified Performance**: Both initial "Surprise Me" suggestions and reshuffles now use the same optimized pool system
- **Consistent User Experience**: Eliminated performance disparity between first suggestion (slow) and reshuffles (instant)
- **Code Quality**: Removed duplicate business logic and created single source of truth for surprise suggestions
- **Performance**: Consistent zero API calls for all surprise interactions after initial pool construction
- **Maintainability**: Future changes to surprise logic only need to be made in one place (BuildSurprisePoolAsync)

### Technical Implementation
- Replaced legacy 4-cycle system in GetSurpriseSuggestion() with unified pool approach
- Both initial and reshuffle endpoints now share identical business logic and performance characteristics
- Maintained same pool building strategy (80 movies from trending/genre/director/actor buckets)
- Preserved infinite cyclic rotation and session-based anti-repetition
- Enhanced logging consistency and reduced verbosity for production environments

## 2025-07-25
### 🔄 Trending Suggestion System Unification
- **Unified Business Logic**: Both initial `ShowSuggestions` and AJAX `TrendingReshuffle` now use the same helper method `GetTrendingMoviesWithFiltering()`
- **Consistent User Experience**: Identical filtering, pool building, and randomization across all trending movie interfaces
- **Code Quality**: Eliminated code duplication and created single source of truth for trending movie logic
- **Performance**: Consistent caching behavior using TMDB service's built-in 90-minute cache
- **Maintainability**: Future changes to trending logic only need to be made in one place

### Technical Implementation
- Added `GetTrendingMoviesWithFiltering()` helper method that encapsulates all trending movie business logic
- Updated `ShowSuggestions` trending case to use unified helper
- Refactored `TrendingReshuffle` AJAX endpoint to use same helper method
- Ensured identical user filtering (blacklist + recent movies) across both endpoints
- Maintained same pool building strategy (30 movies from up to 5 TMDB pages)
- Preserved consistent randomization algorithm for variety

### Code Quality Improvements
- Removed duplicate filtering logic between initial and AJAX endpoints
- Centralized trending movie business rules in single, well-documented method
- Added comprehensive XML documentation for the new helper method
- Enhanced logging for better debugging and monitoring capabilities
- Decade-based movie suggestions now use a dynamic variety system identical to the genre system:
  - Each suggestion uses randomized sort criteria (`popularity.desc`, `vote_average.desc`, `release_date.desc`) and page (1-3).
  - Triple fallback logic ensures suggestions are always available:
    - Primary: Random sort + random page
    - Fallback 1: Same sort, page 1
    - Fallback 2: Popular, page 1
- Added `sortBy` parameter to `DiscoverMoviesByDecadeAsync` in `TmdbService` for dynamic sorting.
- Introduced `TryGetDecadeMovies` helper for robust error handling, user filtering, and fallback.
- Both initial load and AJAX reshuffles now use the same dynamic logic for decades, matching genres.
- Enhanced caching: 24-hour cache per sort+page+decade combo, with early exit optimization.
- User filtering (blacklist, watched movies) is consistently applied and cached per request.
- User experience: Decade suggestions now provide varied, reliable content from the first click, with bulletproof fallback for edge cases.
- Consistency: Unified experience between decade and genre suggestions across all flows.
  - Enhanced with deduplication logic to prevent duplicate decades in results

# 2025-07-24 Genre Suggestion Dynamic Variety System

- **Major Enhancement**: Implemented dynamic variety system for genre-based movie suggestions
- **Random Sort Selection**: Each reshuffle now uses randomized sort criteria (popularity, top-rated, latest) for content variety
- **Quality Filtering**: Added 6.5+ rating filter to ensure only high-quality movie suggestions
- **Triple Fallback System**: Robust fallback logic prevents empty results for any genre
  - Primary: Requested sort + page combination
  - Fallback 1: Same sort, page 1 (if original page insufficient)
  - Fallback 2: Popular, page 1 (ultimate safety net)
- **Consistent User Experience**: Unified "Because you watched [GENRE] movies" titles for all suggestions
- **Performance Maintained**: Same API usage pattern as previous system while delivering significantly more variety
- **Enhanced Logging**: Comprehensive logging for debugging sort/page combinations and fallback usage
- **User Filtering Integration**: Maintains existing blacklist, wishlist, and watched movie filtering
- **Page Quality Control**: Restricts pagination to pages 1-3 to ensure high-quality content discovery

### Technical Implementation
- Updated `GetSuggestionsForGenre` method to accept dynamic sort and page parameters
- Enhanced `DiscoverMoviesByGenreAsync` in TmdbService with vote_average.gte=6.5 filter
- Implemented `TryGetGenreMovies` helper for robust error handling and fallback logic
- Random parameter generation moved before API calls to ensure proper variety
- Comprehensive logging added for monitoring variety effectiveness and fallback frequency

# 2025-07-24 Director Suggestion Deduplication Fix

- Fixed DirectorReshuffle logic to prevent duplicate directors in suggestion sequence
- Implemented case-insensitive deduplication using HashSet with StringComparer.OrdinalIgnoreCase
- Resolved issue where directors appearing in multiple categories (e.g., both "recent" and "frequent") would be suggested repeatedly
- Simplified selection logic using index-based access to deduplicated priority queue
- Enhanced logging to track director analysis, deduplication process, and final queue composition
- Improved user experience by ensuring varied director suggestions without complex skip patterns
- Technical approach: solve duplication at data level (early deduplication) rather than logic level (runtime skipping)
# 2025-07-24 Cast Suggestion Anti-Repetition

- Added logic to prevent immediate repetition of the same actor in cast-based suggestions (CastReshuffle).
- Now, the same actor will never be suggested twice in a row, improving perceived variety and user experience.
- No impact on performance or existing priorities; only the last actor is tracked in Session.

# 2025-07-24 Surprise Me Optimization

- Major optimization of the "Surprise Me" suggestion system:
  - Now uses a static, deduplicated pool of 80 movies, built with aggressive cascading from prioritized buckets (trending, genre, director, etc.).
  - The pool is cached for 2 hours (IMemoryCache), ensuring instant reshuffles and consistent suggestions.
  - Infinite cyclic rotation: each reshuffle advances the pointer, wrapping around as needed.
  - Blacklist and recent filters are applied during pool build, not per reshuffle.
  - Deduplication by TMDB ID is enforced during pool construction.
  - Performance: Only ~5 TMDB API calls are made during initial pool build; all reshuffles are API-free.
  - All outdated references to the previous 4-cycle logic and per-reshuffle discovery calls have been removed from documentation and code comments.

# 2025-07-24 Genre Suggestion Consistency Fix

- Initial genre suggestions now use the same dynamic variety system as AJAX reshuffles
- Both initial load and reshuffles generate random sort criteria (popularity.desc, vote_average.desc, release_date.desc) and page (1-3)
- Unified title format: "Because you watched [GENRE] movies" for both initial and reshuffles
- Session state is reset on fresh start to ensure correct sequence
- User experience is now consistent and varied from the very first click
- No impact on caching or performance optimizations

# 2025-07-23
- Added prioritized genre queue logic for user suggestions (recent, frequent, highest-rated genre).
- Implemented per-user caching for genre priority queue (1 hour expiration).
- Enabled AJAX-powered reshuffle for "By Genre" suggestions, with server-rendered HTML and anti-repetition logic.
- Updated controller and documentation comments to match business
