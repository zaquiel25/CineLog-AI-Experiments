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

CineLog uses specialized Claude Code subagents for accelerated development. Each subagent has deep knowledge of specific architectural patterns and operates in its own context window for focused expertise.

### 🎭 **Master Agent Director**

The **Master Agent Director** is your intelligent task orchestrator that analyzes every request and automatically routes it to the most efficient subagent(s) for optimal results.

#### 🧠 **Intelligence Framework**

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

#### 🧠 **Intelligent Planning Engine**

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

#### 📋 **Master Director Decision Logic**

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

#### 🎬 **CineLog-Specific Routing Rules**

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

#### 🎯 **Enhanced Usage Examples**

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

### 🎬 Core CineLog Subagents

#### 🎭 `cinelog-movie-specialist`
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

#### 🌐 `tmdb-api-expert`  
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

#### 🏗️ `aspnet-feature-developer`
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

#### 📚 `docs-architect`
**🎯 Purpose**: Documentation maintenance and architecture updates

**🧠 Expertise**:
- Technical documentation for all project files
- Architecture pattern documentation
- Change tracking and CHANGELOG management
- Development workflow documentation

### 🚀 Enhanced Development Subagents

#### 🧪 `test-writer-fixer`
**🎯 Purpose**: **PROACTIVE** - Comprehensive test coverage and maintenance after code changes

**🧠 Expertise**:
- Unit, integration, and end-to-end testing for ASP.NET Core
- Test-driven development and test maintenance
- Movie-specific test scenarios (suggestions, CRUD, API integration)
- Test failure analysis and repair without compromising test intent

**🔥 Auto-Trigger**: After any code modification to ensure comprehensive test coverage

#### 🏛️ `backend-architect`
**🎯 Purpose**: Scalable backend architecture and API design

**🧠 Expertise**:
- ASP.NET Core architecture patterns and scalability design
- Database optimization and performance architecture
- API design with proper authentication and rate limiting
- System architecture for movie data management at scale

#### 🎨 `ui-designer`
**🎯 Purpose**: **PROACTIVE** - Visual design enhancement and modern UI patterns

**🧠 Expertise**:
- Movie-centric UI component design beyond Bootstrap
- Cinema Gold branding implementation and design systems
- Responsive design patterns for movie discovery interfaces
- Screenshot-worthy design moments for social sharing

**🔥 Auto-Trigger**: After UI/feature updates to enhance visual appeal

#### ✨ `whimsy-injector`
**🎯 Purpose**: **PROACTIVE** - Delightful micro-interactions and user engagement

**🧠 Expertise**:
- Movie discovery and logging micro-interactions
- Achievement celebrations and playful animations
- Personality-filled copy and error states
- Shareable moments that encourage user evangelism

**🔥 Auto-Trigger**: After any UI/UX changes to add personality and delight

#### 📊 `performance-benchmarker`
**🎯 Purpose**: Comprehensive performance testing and optimization analysis

**🧠 Expertise**:
- TMDB API integration performance and rate limiting testing
- Suggestion system performance profiling and optimization
- Database query performance analysis and recommendations
- Frontend rendering optimization for movie-rich interfaces

#### 🔧 `devops-automator`
**🎯 Purpose**: CI/CD automation and deployment optimization

**🧠 Expertise**:
- ASP.NET Core deployment pipelines and automation
- Performance monitoring and alerting for movie apps
- Database migration automation and rollback strategies
- Production deployment best practices for high-traffic apps

#### 🎯 `api-tester`
**🎯 Purpose**: API reliability testing and integration validation

**🧠 Expertise**:
- TMDB API integration testing and contract validation
- Internal API endpoint testing and load simulation
- Authentication flow testing and security validation
- Rate limiting and error handling verification

#### 📈 `feedback-synthesizer`
**🎯 Purpose**: User feedback analysis and feature prioritization

**🧠 Expertise**:
- Movie app user behavior analysis and preference patterns
- Feature prioritization based on user feedback and engagement
- A/B testing analysis for movie discovery features
- User sentiment tracking and improvement recommendations

#### 🔧 `code-refactoring-specialist`
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

### 🎯 Usage Patterns

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

### 🚀 Development Benefits

- **⚡ Context Efficiency**: Each subagent maintains focused expertise without context pollution
- **🏗️ Architectural Consistency**: Deep knowledge of CineLog patterns ensures consistent implementation
- **🎯 Accelerated Development**: Task-specific expertise reduces implementation time
- **✅ Quality Assurance**: Specialized knowledge maintains performance and security standards
- **🧪 Comprehensive Testing**: Proactive test coverage ensures robust, reliable features
- **🎨 Enhanced User Experience**: Automatic UI enhancement and delight injection
- **📊 Performance Excellence**: Built-in performance analysis and optimization recommendations

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