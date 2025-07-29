# 🤖 Claude Code Subagents System

CineLog uses specialized Claude Code subagents for accelerated development. Each subagent has deep knowledge of specific architectural patterns and operates in its own context window for focused expertise.

## 🎭 **Master Agent Director**

The **Master Agent Director** is your intelligent task orchestrator that analyzes every request and automatically routes it to the most efficient subagent(s) for optimal results.

### 🧠 **Intelligence Framework**

**🎯 Task Analysis Engine:**
- Parses user requests to identify core objectives
- Analyzes complexity and scope (simple fix vs. full feature)
- Detects technical domains (database, UI, testing, performance)
- Identifies urgency and priority levels
- Maps requirements to agent capabilities
- **Triggers strategic planning for complex tasks automatically**

**⚡ Enhanced Agent Selection Algorithm:**
```
1. PARSE: Extract task type, complexity, and domains
2. ASSESS: Determine complexity level (Simple/Medium/Complex/Strategic)
3. PLAN: If complex → Strategic planning phase first
4. MATCH: Find agents with relevant expertise
5. RANK: Score agents by efficiency and specialization
6. ROUTE: Select optimal agent(s) with clear delegation
7. MONITOR: Track progress and re-route if needed
```

**🎯 Complexity-Based Planning Triggers:**

**SIMPLE TASKS** (Direct execution):
- Bug fixes, small tweaks, single-file changes
- Route directly to specialist agent
- Example: *"Fix typo in movie title display"* → `aspnet-feature-developer`

**MEDIUM TASKS** (Light planning):
- Feature enhancements, performance optimizations
- Brief planning analysis → Execute
- Example: *"Add sorting to movie list"* → Quick analysis → `aspnet-feature-developer`

**COMPLEX TASKS** (Strategic planning required):
- New features, major changes, multi-component systems
- **PLANNING PHASE ACTIVATED**
- Example: *"Add movie comparison feature"* → **Strategic Planning** → Multi-agent execution

**STRATEGIC TASKS** (Comprehensive planning):
- Major architectural changes, new user workflows, business logic overhauls
- **DEEP PLANNING PHASE ACTIVATED**
- Example: *"Redesign entire suggestion system"* → **Comprehensive Planning** → Phased execution

### 🧠 **Intelligent Planning Engine**

**📋 Strategic Planning Phase (Auto-triggered for Complex+ tasks):**

**STEP 1: Feature Definition & Requirements**
```
🎯 OBJECTIVE: What exactly are we building?
- Core functionality definition
- User journey mapping
- Success criteria establishment
- Edge case identification
```

**STEP 2: Implementation Strategy**
```
🏗️ APPROACH: How should we build this?
- Technical architecture planning
- Database schema implications
- UI/UX flow design  
- Integration requirements
```

**STEP 3: Risk Assessment & Mitigation**
```
⚠️ RISKS: What could go wrong?
- Technical challenges identification
- User experience pitfalls
- Performance implications
- Testing strategy requirements
```

**STEP 4: Phased Execution Plan**
```
📈 ROADMAP: What's the implementation sequence?
- Phase breakdown (MVP → Full feature)
- Agent coordination strategy
- Testing checkpoints
- Success validation milestones
```

**STEP 5: Agent Orchestration Strategy**
```
🎭 EXECUTION: Which agents, in what order?
- Primary agent assignments
- Coordination requirements
- Quality gates and handoffs
- Success criteria per phase
```

**🎬 Decision Matrix for CineLog Tasks:**

| Task Pattern | Primary Agent | Secondary Agents | Rationale |
|-------------|---------------|------------------|-----------|
| "Add movie feature X" | `aspnet-feature-developer` | `test-writer-fixer`, `ui-designer` | Full-stack + auto-testing + enhancement |
| "Fix suggestion bug" | `cinelog-movie-specialist` | `test-writer-fixer` | Domain expert + test coverage |
| "Optimize performance" | `performance-optimizer` | `performance-benchmarker` | Analysis + measurement |
| "Database schema change" | `ef-migration-manager` | `backend-architect` | Migration + architecture review |
| "TMDB API issue" | `tmdb-api-expert` | `api-tester` | Integration + reliability testing |
| "UI looks boring" | `ui-designer` | `whimsy-injector` | Visual design + personality |
| "Tests are failing" | `test-writer-fixer` | Agent that changed code | Fix tests + root cause |
| "Deploy to production" | `devops-automator` | `performance-benchmarker` | Deployment + performance check |
| "Users complaining about X" | `feedback-synthesizer` | Relevant domain agent | Analyze feedback + implement fix |
| "Need comprehensive solution" | `backend-architect` | Multiple agents | Architecture + coordinated execution |
| "Code is messy/complex" | `code-refactoring-specialist` | `test-writer-fixer` | Code quality + maintained functionality |
| "Remove duplicate code" | `code-refactoring-specialist` | Domain expert agent | Refactoring + domain expertise |
| "Technical debt cleanup" | `code-refactoring-specialist` | `backend-architect` | Code quality + architecture review |

**🔥 Proactive Orchestration:**
```
Code Change Detected → test-writer-fixer + affected domain agent
UI Update Made → ui-designer + whimsy-injector  
Performance Issue → performance-optimizer + performance-benchmarker
New Feature Complete → test-writer-fixer + ui-designer + whimsy-injector
Technical Debt Accumulation → code-refactoring-specialist + backend-architect
Large Method Detected → code-refactoring-specialist + test-writer-fixer
```

**🚀 Multi-Agent Coordination:**
- **Sequential**: Agent A completes → Agent B starts with A's output
- **Parallel**: Multiple agents work simultaneously on different aspects  
- **Hierarchical**: Master agent oversees, specialist agents execute
- **Feedback Loop**: Agents report back for orchestration adjustments

### 📋 **Master Director Decision Logic**

**🎯 Single Agent Tasks:**
```
"Fix this bug" → Analyze code → Route to domain specialist
"Test this feature" → test-writer-fixer (direct routing)
"Optimize database query" → performance-optimizer (clear specialization)
```

**🔥 Multi-Agent Workflows:**
```
"Build new movie rating system" →
1. backend-architect (design architecture)
2. aspnet-feature-developer (implement feature)  
3. test-writer-fixer (comprehensive testing)
4. ui-designer (enhance interface)
5. whimsy-injector (add personality)
6. performance-benchmarker (validate performance)
```

**⚡ Emergency Routing:**
```
"Production is down!" → devops-automator (immediate)
"Tests all failing!" → test-writer-fixer (immediate)  
"Users can't log in!" → aspnet-feature-developer (immediate)
```

### 🎬 **CineLog-Specific Routing Rules**

**Movie Domain Patterns:**
- Movie CRUD operations → `cinelog-movie-specialist`
- Suggestion algorithms → `cinelog-movie-specialist` + `performance-optimizer`
- TMDB integration → `tmdb-api-expert` + `api-tester`
- User movie data → `cinelog-movie-specialist` + `ef-migration-manager`

**Performance Patterns:**
- "App is slow" → `performance-benchmarker` → `performance-optimizer`
- "Database queries slow" → `ef-migration-manager` + `performance-optimizer`
- "TMDB API timeout" → `tmdb-api-expert` + `performance-benchmarker`

**Feature Development Patterns:**
- New user-facing feature → `aspnet-feature-developer` → `ui-designer` → `whimsy-injector` → `test-writer-fixer`
- Backend API → `backend-architect` → `api-tester` → `test-writer-fixer`
- Database changes → `ef-migration-manager` → `test-writer-fixer`

**Quality Assurance Patterns:**
- After any code change → `test-writer-fixer` (always)
- After UI changes → `ui-designer` + `whimsy-injector` (automatic)
- Before deployment → `devops-automator` + `performance-benchmarker`

### 🎯 **Enhanced Usage Examples**

**🔥 COMPLEX TASK Example:** *"Add a movie comparison feature"*

**Master Director Analysis:**
```
🎯 TASK: Full feature development
🔧 DOMAINS: Backend logic, UI design, database, testing
⚡ COMPLEXITY: COMPLEX → **STRATEGIC PLANNING ACTIVATED**
🚀 STRATEGY: Plan → Sequential multi-agent workflow
```

**PLANNING PHASE (Auto-triggered):**
```
📋 STEP 1: Feature Definition
- What movies can be compared? (user's watched movies vs. suggestions)
- How many movies at once? (2-4 movies side-by-side)
- What data to compare? (ratings, genres, cast, runtime, reviews)
- Success criteria: Users find comparisons helpful for decision-making

🏗️ STEP 2: Implementation Strategy  
- Database: No schema changes needed (use existing movie data)
- Backend: New comparison controller with movie data aggregation
- Frontend: Responsive comparison table/cards with Bootstrap
- Integration: TMDB API for missing movie details

⚠️ STEP 3: Risk Assessment
- Performance risk: Multiple TMDB API calls
- UX risk: Information overload on comparison screen
- Mobile risk: Comparison table on small screens

📈 STEP 4: Phased Execution
- Phase 1: Basic 2-movie comparison (MVP)
- Phase 2: Add 3-4 movie support
- Phase 3: Add advanced comparison filters

🎭 STEP 5: Agent Orchestration
1. backend-architect → Design comparison architecture  
2. aspnet-feature-developer → Implement MVC components
3. ui-designer → Create responsive comparison interface
4. whimsy-injector → Add delightful interactions
5. test-writer-fixer → Comprehensive test coverage
6. performance-benchmarker → Validate performance
```

**EXECUTION PHASE:**
```
✅ Planning complete → Agents execute with clear roadmap
✅ Each agent has specific deliverables and success criteria
✅ Risk mitigation strategies in place
✅ Phased approach allows for early validation
```

---

**⚡ MEDIUM TASK Example:** *"The suggestion system is too slow"*

**Master Director Analysis:**
```
🎯 TASK: Performance optimization  
🔧 DOMAINS: Performance analysis, caching, algorithms
⚡ COMPLEXITY: MEDIUM → **LIGHT PLANNING ACTIVATED**
🚀 STRATEGY: Quick analysis → Implementation
```

**LIGHT PLANNING PHASE:**
```
📋 Quick Analysis:
- Likely bottlenecks: TMDB API calls, database queries, cache misses
- Success criteria: <2s suggestion load time
- Risk: Don't break existing suggestion logic
```

**EXECUTION:**
```
1. performance-benchmarker → Measure and identify bottlenecks
2. performance-optimizer → Implement caching and optimizations
3. test-writer-fixer → Ensure optimizations don't break functionality
```

---

**🎯 SIMPLE TASK Example:** *"Fix typo in movie title display"*

**Master Director Analysis:**
```
🎯 TASK: Bug fix
🔧 DOMAINS: Frontend display
⚡ COMPLEXITY: SIMPLE → **DIRECT EXECUTION**
🚀 STRATEGY: Route directly to specialist
```

**EXECUTION:**
```
→ aspnet-feature-developer (immediate routing, no planning needed)
```

## 🎬 Core CineLog Subagents

### 🎭 `cinelog-movie-specialist`
**🎯 Purpose**: Movie-specific features and suggestion system work

**🧠 Expertise**: 
- MoviesController patterns and movie CRUD operations
- Suggestion algorithms (trending, director, genre, cast, decade, surprise me)
- User movie data management and filtering logic
- AJAX reshuffle implementations and session state
- **Unified helper methods** for consistent business logic across initial/AJAX calls
- **Triple fallback systems** ensuring suggestions are never empty
- **Session-based anti-repetition** and sequencing logic
- **Dynamic variety systems** with randomized sort criteria and pagination

**🔑 Key Patterns**:
```csharp
// User data isolation (CRITICAL)
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);

// Unified filtering methods
var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);

// Suggestion helper pattern
private async Task<List<TmdbMovieBrief>> Get[Type]MoviesWithFiltering(string userId)
{
    // Same logic for initial load and AJAX reshuffles
    // Apply user filtering, variety, and deduplication
}
```

**🎬 Domain-Specific Knowledge**:
- **Suggestion Sequencing**: Recent → Frequent → Top-rated → Random patterns
- **Mutual Exclusion**: Movies cannot be in both wishlist and blacklist
- **Proactive Director Filtering**: Check available movies before suggesting directors
- **Anti-repetition Logic**: Session state prevents immediate repetition
- **Quality Filtering**: 6.5+ ratings with sufficient vote counts for reliability

### 🌐 `tmdb-api-expert`  
**🎯 Purpose**: External API integration and movie data operations

**🧠 Expertise**:
- TmdbService architecture and HTTP client management
- Rate limiting with SemaphoreSlim (6 concurrent requests)
- TMDB data caching (24-hour expiration)
- Batch API operations to prevent N+1 queries
- **Parallel execution** for pool building (up to 15 concurrent calls)
- **Error handling** for API failures and rate limits
- **Data mapping** between TMDB API responses and CineLog models

**🔑 Integration Patterns**:
```csharp
// Always use centralized service
var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);

// Batch operations for efficiency
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

// Parallel pool building for Surprise Me
var poolTasks = buckets.Select(async bucket => 
    await _tmdbService.GetMoviesAsync(bucket.endpoint)).ToArray();
var results = await Task.WhenAll(poolTasks);
```

**🚀 Performance Expertise**:
- **24-hour caching** prevents redundant API calls
- **Batch processing** avoids N+1 query problems
- **Rate limit handling** with SemaphoreSlim throttling
- **Parallel execution** reduces pool build time by 85%
- **Fallback strategies** for API failures

### 🗄️ `ef-migration-manager`
**🎯 Purpose**: Database operations and schema management

**🧠 Expertise**:
- Entity Framework Core migrations with SQL Server
- Performance index optimization for user-specific queries
- Entity model configuration and Identity integration
- UserId isolation patterns and composite indexes

### ⚡ `performance-optimizer`
**🎯 Purpose**: Performance optimization and caching strategies

**🧠 Expertise**:
- IMemoryCache optimization and CacheService patterns
- Database query performance with proper indexing
- API call efficiency and parallel execution
- Async/await patterns for scalability
- **Batch processing** to eliminate N+1 query problems
- **Parallel execution** strategies for pool building
- **Caching layers** with appropriate expiration times
- **Performance-first architecture** patterns

**🔑 Optimization Patterns**:
```csharp
// Centralized caching service
var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);

// Batch API calls instead of individual calls
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

// Parallel execution for performance
var poolTasks = buckets.Select(async bucket => 
    await ProcessBucketAsync(bucket)).ToArray();
var results = await Task.WhenAll(poolTasks);

// Cache expensive operations per request
var last25Movies = await GetLast25MoviesAsync(userId); // Cache this
```

**🚀 Performance Expertise**:
- **TMDB API**: 24-hour IMemoryCache for external data
- **User Data**: 15-minute CacheService expiration for blacklist/wishlist IDs
- **Suggestion Pools**: 2-hour caching for Surprise Me pools
- **Pagination**: 20 items per page with composite indexes
- **Build Optimization**: 85% faster pool building through parallelization

### 🏗️ `aspnet-feature-developer`
**🎯 Purpose**: Complete feature development from database to UI

**🧠 Expertise**:
- ASP.NET Core MVC patterns and controller development
- Razor view development with Bootstrap 5 (Cyborg theme)
- AJAX implementation with server-side rendering
- Cinema Gold branding and UI/UX standards
- **AJAX + Server-Side Rendering hybrid** architecture
- **Event delegation** for dynamic UI elements
- **Progressive enhancement** (works without JavaScript)
- **Mutual exclusion logic** implementation and UI enforcement

**🔑 Development Patterns**:
```csharp
// AJAX endpoints return server-rendered HTML
return PartialView("_MovieSuggestionCard", viewModel);

// User data isolation in all operations
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);

// Mutual exclusion checking
var existsInWishlist = await _dbContext.WishlistItems
    .AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
```

**🎨 UI/UX Expertise**:
- **Cinema Gold** branding with `.cinelog-gold-title` classes
- **Bootstrap 5 Cyborg** theme integration
- **Responsive design** for mobile and desktop
- **Visual feedback** for all user interactions
- **Preventive UI states** to avoid error messages

### 📚 `docs-architect`
**🎯 Purpose**: Documentation maintenance and architecture updates

**🧠 Expertise**:
- Technical documentation for all project files
- Architecture pattern documentation
- Change tracking and CHANGELOG management
- Development workflow documentation

## 🚀 Enhanced Development Subagents

### 🧪 `test-writer-fixer`
**🎯 Purpose**: **PROACTIVE** - Comprehensive test coverage and maintenance after code changes

**🧠 Expertise**:
- Unit, integration, and end-to-end testing for ASP.NET Core
- Test-driven development and test maintenance
- Movie-specific test scenarios (suggestions, CRUD, API integration)
- Test failure analysis and repair without compromising test intent

**🔥 Auto-Trigger**: After any code modification to ensure comprehensive test coverage

### 🏛️ `backend-architect`
**🎯 Purpose**: Scalable backend architecture and API design

**🧠 Expertise**:
- ASP.NET Core architecture patterns and scalability design
- Database optimization and performance architecture
- API design with proper authentication and rate limiting
- System architecture for movie data management at scale

### 🎨 `ui-designer`
**🎯 Purpose**: **PROACTIVE** - Visual design enhancement and modern UI patterns

**🧠 Expertise**:
- Movie-centric UI component design beyond Bootstrap
- Cinema Gold branding implementation and design systems
- Responsive design patterns for movie discovery interfaces
- Screenshot-worthy design moments for social sharing

**🔥 Auto-Trigger**: After UI/feature updates to enhance visual appeal

### ✨ `whimsy-injector`
**🎯 Purpose**: **PROACTIVE** - Delightful micro-interactions and user engagement

**🧠 Expertise**:
- Movie discovery and logging micro-interactions
- Achievement celebrations and playful animations
- Personality-filled copy and error states
- Shareable moments that encourage user evangelism

**🔥 Auto-Trigger**: After any UI/UX changes to add personality and delight

### 📊 `performance-benchmarker`
**🎯 Purpose**: Comprehensive performance testing and optimization analysis

**🧠 Expertise**:
- TMDB API integration performance and rate limiting testing
- Suggestion system performance profiling and optimization
- Database query performance analysis and recommendations
- Frontend rendering optimization for movie-rich interfaces

### 🔧 `devops-automator`
**🎯 Purpose**: CI/CD automation and deployment optimization

**🧠 Expertise**:
- ASP.NET Core deployment pipelines and automation
- Performance monitoring and alerting for movie apps
- Database migration automation and rollback strategies
- Production deployment best practices for high-traffic apps

### 🎯 `api-tester`
**🎯 Purpose**: API reliability testing and integration validation

**🧠 Expertise**:
- TMDB API integration testing and contract validation
- Internal API endpoint testing and load simulation
- Authentication flow testing and security validation
- Rate limiting and error handling verification

### 📈 `feedback-synthesizer`
**🎯 Purpose**: User feedback analysis and feature prioritization

**🧠 Expertise**:
- Movie app user behavior analysis and preference patterns
- Feature prioritization based on user feedback and engagement
- A/B testing analysis for movie discovery features
- User sentiment tracking and improvement recommendations

### 🔧 `code-refactoring-specialist`
**🎯 Purpose**: **PROACTIVE** - Code quality improvement and technical debt reduction

**🧠 Expertise**:
- **Legacy Code Modernization**: Upgrading outdated patterns to modern ASP.NET Core conventions
- **SOLID Principle Implementation**: Breaking down large classes and improving separation of concerns
- **DRY Pattern Enforcement**: Eliminating duplicate code across controllers, services, and views
- **Performance-Oriented Refactoring**: Identifying and fixing performance bottlenecks through code structure improvements
- **Technical Debt Assessment**: Analyzing code smells, complex methods, and coupling issues
- **Maintainability Enhancement**: Simplifying complex logic and improving code readability
- **CineLog-Specific Patterns**: Ensuring consistency with movie domain patterns and user data isolation
- **Test-Safe Refactoring**: Maintaining existing functionality while improving code structure

**🔑 Refactoring Patterns**:
```csharp
// Before: Large, complex method
public async Task<IActionResult> ShowSuggestions(string type)
{
    // 200+ lines of mixed logic
}

// After: Broken down with clear responsibilities
public async Task<IActionResult> ShowSuggestions(string type)
{
    var userId = _userManager.GetUserId(User);
    var suggestionStrategy = _suggestionFactory.CreateStrategy(type);
    var viewModel = await suggestionStrategy.GenerateAsync(userId);
    return View(viewModel);
}

// Extract method pattern for complex business logic
private async Task<List<TmdbMovieBrief>> ExtractCommonFilteringLogic(
    List<TmdbMovieBrief> movies, string userId)
{
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    return movies.Where(m => !blacklistIds.Contains(m.Id)).ToList();
}
```

**🎯 CineLog-Specific Refactoring Focus**:
- **MoviesController Simplification**: Breaking down large controller methods into focused, single-responsibility methods
- **Suggestion System Architecture**: Extracting common patterns from suggestion algorithms into reusable components
- **TMDB Service Optimization**: Improving API call patterns and caching strategies
- **Database Query Optimization**: Refactoring LINQ queries for better performance
- **View Logic Cleanup**: Moving complex logic from Razor views to ViewModels or services
- **Error Handling Standardization**: Implementing consistent error handling patterns across the application

**📊 Quality Metrics Focus**:
- **Cyclomatic Complexity**: Reducing complex methods to improve testability
- **Method Length**: Breaking large methods into smaller, focused units
- **Code Duplication**: Identifying and eliminating repeated logic patterns
- **Coupling Reduction**: Decreasing dependencies between components
- **Performance Impact**: Measuring performance improvements from structural changes

**🔥 Auto-Trigger**: After major feature additions or when technical debt accumulates

## 🎯 Usage Patterns

**🤖 Automatic Delegation**: Subagents are invoked automatically based on task context:
```
"Add a new suggestion type for movies by runtime" → cinelog-movie-specialist
"Improve TMDB API caching" → tmdb-api-expert  
"Add performance indexes for new queries" → ef-migration-manager
"Create tests for the new feature" → test-writer-fixer
"Design UI for movie comparison" → ui-designer
"Add delightful animations to success states" → whimsy-injector
"Simplify the MoviesController methods" → code-refactoring-specialist
"Remove duplicate code in suggestion algorithms" → code-refactoring-specialist
"Benchmark the suggestion system performance" → performance-benchmarker
```

**🔥 Proactive Invocation**: These agents trigger automatically:
```
Code changes made → test-writer-fixer (ensures test coverage)
UI/feature updates → ui-designer (enhances visual appeal)
UI/UX changes → whimsy-injector (adds personality and delight)
```

**👤 Explicit Invocation**: Request specific subagents:
```
"Use the performance-benchmarker to analyze TMDB API performance"
"Use the backend-architect to design a scalable rating system"
"Use the feedback-synthesizer to analyze user preferences"
```

## 🚀 Development Benefits

- **⚡ Context Efficiency**: Each subagent maintains focused expertise without context pollution
- **🏗️ Architectural Consistency**: Deep knowledge of CineLog patterns ensures consistent implementation
- **🎯 Accelerated Development**: Task-specific expertise reduces implementation time
- **✅ Quality Assurance**: Specialized knowledge maintains performance and security standards
- **🧪 Comprehensive Testing**: Proactive test coverage ensures robust, reliable features
- **🎨 Enhanced User Experience**: Automatic UI enhancement and delight injection
- **📊 Performance Excellence**: Built-in performance analysis and optimization recommendations