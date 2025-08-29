## 🛰️ MCP Server & Extension Usage

If a user requests information or actions related to an MCP server or extension, and the MCP is available, you should automatically fetch or use it as needed to fulfill the request. You do not need to wait for explicit invocation if the context is clear.

### Available MCP Servers:
- **microsoft-docs**: Microsoft Learn documentation for ASP.NET Core, Entity Framework, and related technologies
- **deepwiki**: Deep Wikipedia access for comprehensive research and information retrieval
- **context7**: Upstash Context7 for enhanced contextual AI operations and reasoning
- **codacy**: Code quality analysis and review tools (requires account token configuration)

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## ⚠️ CRITICAL INSTRUCTIONS

### 🛡️ PARAMOUNT SAFETY & COMPLIANCE MANDATE
**ULTRA-CRITICAL - Security, Privacy & Legal Compliance Requirements:**
- **EVERY CHANGE MUST BE ULTRA-CHECKED** for safety, security, privacy, and legal implications before implementation
- **CONSIDER ALL POSSIBILITIES** including worst-case scenarios, edge cases, and potential misuse
- **COMPREHENSIVE IMPACT ASSESSMENT** required for any code that:
  - Collects, stores, or transmits user data
  - Handles authentication or authorization
  - Integrates with external services or APIs
  - Processes personal information or user-generated content
  - Implements logging, monitoring, or analytics
  - Modifies security configurations or access controls
- **MANDATORY SAFETY CHECKLIST** before ANY implementation:
  - ✅ **Data Privacy**: Will this expose ANY user personal information?
  - ✅ **Security Vulnerabilities**: Could this create ANY security risks?
  - ✅ **Legal Compliance**: Does this meet GDPR, CCPA, and privacy regulations?
  - ✅ **Production Safety**: Could this impact live user experience negatively?
  - ✅ **Credential Security**: Are all secrets and keys properly protected?
  - ✅ **User Consent**: Do users understand what data is being collected?
  - ✅ **Data Minimization**: Are we collecting only absolutely necessary data?
  - ✅ **Access Control**: Who can access this data and is it properly restricted?
- **IMMEDIATE ESCALATION** required if ANY safety concern identified - STOP implementation and consult user
- **ZERO TOLERANCE** for privacy violations, security risks, or compliance gaps
- **PROFESSIONAL LIABILITY**: Every change must be defensible in professional audit or legal review
- **PROACTIVE PROTECTION**: When in doubt, choose the most conservative, privacy-preserving approach

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

### 🚫 PRODUCTION DEPLOYMENT SAFEGUARDS - CRITICAL SECURITY
- **NEVER** deploy to production without EXPLICIT user permission
- **NEVER** run deployment commands unless user says "deploy to production" or similar
- **NEVER** push to git unless explicitly requested
- **ALWAYS** work locally by default - production site must remain stable
- **FORBIDDEN COMMANDS without explicit permission:**
  - `az webapp deployment`
  - `curl -X POST "https://cinelog-app.scm.azurewebsites.net/api/zipdeploy"`
  - `git push` 
  - Any Azure deployment command
- **REQUIRED**: User must explicitly say "deploy this to production" or "push to Azure"

### 🚫 Never Auto-Commit
- **NEVER** stage and commit files automatically
- Only commit when explicitly asked by the user

### 🗂️ Session Context Management - MANDATORY WORKFLOW
**CRITICAL**: This process is REQUIRED at start and end of every conversation

#### 📖 **START OF EVERY CONVERSATION:**
- **MANDATORY FIRST ACTION**: Use Grep tool with intelligent date-based search:
  1. `grep "Session YYYY-MM-DD" SESSION_NOTES.md -A 75` (current date)
  2. If no results: `grep "Session YYYY-MM-DD" SESSION_NOTES.md -A 75` (previous day)
  3. If no results: `grep "Session YYYY-MM-DD" SESSION_NOTES.md -A 75` (2 days ago)
  4. Only use Read tool as fallback if no recent sessions found
- **Performance**: ~248 tokens vs. ~4,290 tokens (94.2% reduction)
- Review recent context to understand current project state and work-in-progress

#### ✍️ **END OF EVERY CONVERSATION/TASK:**
- **MANDATORY**: Add brief summary (2-3 lines) to SESSION_NOTES.md stating what was accomplished
- Use Edit tool to append new session entry with date, goals, and key accomplishments
- This ensures future sessions have proper context continuation

#### 🔧 **Implementation Details:**
- Use Grep tool (not Bash commands) for date-based search
- Search pattern: "Session YYYY-MM-DD" with -A 75 for context
- This file is gitignored - contains local working context only, never committed

### 📋 **CRITICAL WORKFLOW SYSTEM (2025-08-12) - ENHANCED WITH AGENT FRAMEWORK OPTIMIZATION**
**MANDATORY 6-STEP SYSTEMATIC WORKFLOW - OPTIMIZED WITH 100% AGENT COVERAGE:**
1. **Context Review**: Read SESSION_NOTES.md for previous session context and work-in-progress
2. **Compliance Check**: Verify task requirements against CLAUDE.md instructions  
3. **Agent Selection**: MANDATORY - Classify task and route to appropriate agent:
   - **Task Type Assessment**: Identify primary domain (movie/TMDB/performance/database/documentation/deployment/etc.)
   - **Agent Selection**: Select from decision tree based on task classification:
     * Movie features/CRUD → `cinelog-movie-specialist`
     * TMDB API work → `tmdb-api-expert`
     * Caching/optimization → `performance-optimizer`
     * Performance testing → `performance-monitor`
     * Database/EF work → `ef-migration-manager`
     * Full-stack MVC features → `aspnet-feature-developer`
     * Deployment/infrastructure → `deployment-project-manager`
     * Session management → `session-secretary`
     * Documentation updates → `docs-architect`
     * Complex research tasks → `general-purpose`
   - **Execution**: Use Task tool to launch appropriate agent (usage is MANDATORY, not optional)
   - **CRITICAL**: Zero direct work - all substantial tasks MUST be routed through appropriate agents
4. **Execution**: Implement with appropriate agent expertise and professional standards
5. **Verification**: Ensure build success, testing completion, and TodoWrite task completion
6. **Documentation**: Update session notes with progress, decisions, and next priorities

**AGENT DECISION TREE - MANDATORY PROACTIVE USAGE (All 10 Agents):**
- Movie features/suggestions → `cinelog-movie-specialist` (REQUIRED)
- TMDB API integration → `tmdb-api-expert` (REQUIRED)
- Performance issues → `performance-optimizer` (REQUIRED)
- Performance testing/validation → `performance-monitor` (REQUIRED)
- Database changes → `ef-migration-manager` (REQUIRED)  
- Full-stack features → `aspnet-feature-developer` (REQUIRED)
- Production deployment → `deployment-project-manager` (REQUIRED)
- Session continuity → `session-secretary` (REQUIRED)
- Documentation work → `docs-architect` (REQUIRED)
- Complex multi-step research → `general-purpose` (REQUIRED)

**CRITICAL**: Agent usage is MANDATORY, not optional. Every task MUST be routed through appropriate agent(s).
**COVERAGE**: All 10 available agents must be actively used - no agent should be ignored or underutilized.
**ZERO DIRECT WORK**: Working directly without agent routing is forbidden - all substantial tasks require specialist expertise.

### 🔍 **AI AGENT OBSERVABILITY SYSTEM (2025-08-20)**
**CRITICAL**: Comprehensive observability implementation based on "AI Agent Design Patterns" industry best practices

**📊 Core Observability Principles:**
- **Deep Visibility**: Every agent interaction tracked and measured  
- **Performance Metrics**: Success rates, execution time, user satisfaction
- **Feedback Loops**: Continuous learning and optimization
- **Quality Assessment**: LLM-as-judge evaluations for output quality

**🎯 Agent Performance Tracking:**
```markdown
AUTOMATED METRICS COLLECTION:
- Agent execution time and success rate monitoring
- User satisfaction correlation analysis  
- Task complexity vs. agent effectiveness mapping
- Multi-agent coordination success tracking
- Learning pattern recognition and optimization

QUALITY ASSESSMENT FRAMEWORK:
- LLM-as-judge scoring (1-10 scale) for agent outputs
- Success pattern identification and replication
- Failure mode analysis and prevention
- User preference learning integration
- Performance regression detection and alerts
```

**📈 Performance Targets:**
- Overall System Success Rate: >95%
- Agent Response Time: <60s (simple), <3m (medium), <5m (complex)
- User Satisfaction Score: >4.5/5
- Agent Routing Accuracy: >92%
- Learning Velocity: +5% improvement per month

**🔁 Continuous Improvement Implementation:**
```markdown
FEEDBACK LOOP MECHANISMS:
- Real-time agent performance assessment
- Automated optimization recommendations
- User workflow pattern learning
- Predictive performance enhancement
- System-wide efficiency improvements

OBSERVABILITY INTEGRATION POINTS:
- SESSION_NOTES.md: Agent health dashboard and performance tracking
- .claude/observability/: Comprehensive metrics, evaluation criteria, feedback loops
- /agent-feedback command: Continuous learning and optimization analysis
- performance-monitor agent: Enhanced with LLM-as-judge capabilities
```

**Key Observability Files:**
- **`.claude/observability/agent-performance.md`**: Real-time metrics and success rates
- **`.claude/observability/evaluation-criteria.md`**: LLM-as-judge quality framework  
- **`.claude/observability/feedback-loops.md`**: Continuous learning mechanisms
- **`.claude/observability/health-dashboard.md`**: System status monitoring
- **`.claude/commands/agent-feedback.md`**: Performance analysis and optimization

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
- **Apply Director Validation Patterns**: Use enhanced person selection logic for TMDB API disambiguation
- **Optimize API Usage**: Implement smart caching, heuristics, and rate limiting for external API calls

### 5. 🐛 Debugging & Testing
- Use structured `_logger` calls instead of console output
- Test edge cases rigorously
- **Verify user data isolation** and proper filtering by UserId

---

## 🤖 Claude Code Subagents System

> **📚 For detailed agent documentation, see [AGENTS.md](./.claude/agents/AGENTS.md)**

### 🎯 Enhanced Agent Selection Guide (2025-08-12)
| Task Type | Primary Agent | Use Case | Multi-Agent Coordination |
|-----------|---------------|----------|-------------------------|
| Movie features/suggestions | `cinelog-movie-specialist` | Domain expertise | + `performance-optimizer` if needed |
| TMDB API integration | `tmdb-api-expert` | External API operations | + `performance-optimizer` for caching |
| Performance issues | `performance-optimizer` | Caching & optimization | + `performance-monitor` for validation |
| Database changes | `ef-migration-manager` | Schema & migrations | + `performance-optimizer` for indexes |
| Full-stack features | `aspnet-feature-developer` | Complete MVC development | + `docs-architect` for updates |
| Production deployment | `deployment-project-manager` | Strategic deployment coordination | Multiple agents for coordination |
| Session continuity | `session-secretary` | Automatic context management | Autonomous operation |
| Documentation work | `docs-architect` | Comprehensive documentation | + domain agents for context |
| Complex research | `general-purpose` | Multi-domain analysis | + domain agents for implementation |
| Performance testing | `performance-monitor` | Validation & metrics | + `performance-optimizer` for fixes |

---

## 🛠️ Development Commands

### 🛡️ DEVELOPMENT SAFETY PROTOCOL
**PRODUCTION SITE: https://cinelog-app.azurewebsites.net/ (LIVE - DO NOT TOUCH)**
**LOCAL DEVELOPMENT: https://localhost:7186 (SAFE FOR TESTING)**

```bash
# ✅ ALWAYS SAFE - Work locally
dotnet watch run                # Start local development server

# 🚨 DANGER - Never run without explicit permission
az webapp deployment            # FORBIDDEN without "deploy to production" 
git push                        # FORBIDDEN without "push to git"
```

### 🏠 LOCAL DEVELOPMENT (SAFE) - DEFAULT MODE
**ALWAYS work locally first - production site stays untouched!**

```bash
# ✅ SAFE: Local development and testing
dotnet build                    # Build locally
dotnet run                      # Run locally at https://localhost:7186
dotnet watch run               # Hot reload development (RECOMMENDED)

# ✅ SAFE: Local testing commands  
dotnet test                     # Run tests locally
dotnet ef database update      # Update local database only
```

### 🔨 Build and Run

### 🔐 Configuration Setup
**For detailed setup instructions, see [README.md](README.md#setup)**

**AI Development Essentials:**
```bash
# Quick setup for AI development
dotnet user-secrets set "TMDB:AccessToken" "your-token"
dotnet watch run  # Start with hot reload
```

### 🗄️ Database Commands (LOCAL ONLY)
```bash
# ✅ SAFE: Local database operations
dotnet ef migrations add <Name>        # Create new migration locally
dotnet ef database update              # Apply migrations to LOCAL database
dotnet ef database drop                # Drop LOCAL database only
```

### ⚠️ DANGER ZONE - PRODUCTION DEPLOYMENT
**🚨 REQUIRES EXPLICIT USER PERMISSION 🚨**
**NEVER run these commands without user saying "deploy to production"**

### 🚀 Production Deployment
**🚨 REQUIRES EXPLICIT USER PERMISSION 🚨**

**For detailed deployment procedures and latest status, see [README.md](README.md#deployment)**

**AI Deployment Essentials:**
```bash
# Pre-deployment cleanup (MANDATORY)
dotnet build -c Release  # Must show 0 warnings, 0 errors

# Clean diagnostic code from:
# - Program.cs (Console.WriteLine statements)
# - Controllers/*.cs (Debug.WriteLine calls) 
# - Identity pages (debug logging)

# Professional deployment
dotnet publish -c Release -o ./publish-clean
cd publish-clean && zip -r ../deployment-clean-$(date +%Y%m%d-%H%M).zip .
```

---

## 🏛️ Architecture Overview

**For comprehensive architecture details, tech stack specifications, and feature descriptions, see [README.md](README.md)**

### Key CineLog-Specific AI Patterns:
- **User Data Isolation**: All queries MUST filter by UserId
- **AJAX-Enhanced Suggestions**: 6 suggestion types with unified helper methods
- **Batch API Processing**: Use `GetMultipleMovieDetailsAsync()` to prevent N+1 queries
- **24-hour TMDB Caching**: IMemoryCache for external API responses
- **15-minute User Data Caching**: CacheService for blacklist/wishlist operations

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

### 🔐 Two-Layer Authentication Architecture Pattern
**CRITICAL AI Pattern - Password Gate + Identity Authentication Coexistence**

**Essential Understanding - Authentication Scheme Hierarchy:**
```csharp
// CRITICAL: Identity as default scheme, PasswordGate as named scheme
builder.Services.AddAuthentication() // No default scheme specified - Identity remains default
    .AddCookie("PasswordGate", options => // Named scheme for site access
    {
        options.LoginPath = "/PasswordGate";
        // PasswordGate-specific configuration
    });

// Identity authentication remains default for [Authorize] attributes
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

**Critical Controller Pattern - Explicit Scheme Authentication:**
```csharp
// CORRECT: Explicit authentication against named scheme
var authenticateResult = await HttpContext.AuthenticateAsync("PasswordGate");
var isAuthenticated = authenticateResult.Succeeded && 
                     authenticateResult.Principal.HasClaim("PasswordGate", "granted");

// WRONG: Implicit authentication only checks default scheme
var isAuthenticated = HttpContext.User.HasClaim("PasswordGate", "granted");
```

**Key Architectural Rules:**
- **Identity (Default Scheme)**: Used by `[Authorize]` attributes on controllers - handles user account authentication
- **PasswordGate (Named Scheme)**: Used for site-wide access control - requires explicit authentication calls
- **Cookie Coexistence**: Both authentication cookies can coexist without conflicts
- **Configuration Flexibility**: Support multiple password key formats (SitePassword, Sitepassword, SiteAccess:Password)

### 🔐 Google OAuth Authentication Pattern
**For complete OAuth implementation details, see [README.md](README.md#google-oauth) and [CHANGELOG.md](CHANGELOG.md)**

**Critical AI Pattern - Authentication Middleware:**
```csharp
// CRITICAL: Authentication middleware MUST be present
app.UseAuthentication(); // REQUIRED for OAuth functionality
app.UseAuthorization();
```

**Key Security Requirements:**
- CSRF Protection with anti-forgery tokens
- User data isolation (Google users get separate namespaces)
- Secure credential management via User Secrets/Key Vault

### 🎬 TMDB Director Validation Pattern
**For complete implementation details, see [README.md](README.md#tmdb-optimization) and [PERFORMANCE_OPTIMIZATION_SUMMARY.md](PERFORMANCE_OPTIMIZATION_SUMMARY.md)**

**Critical AI Pattern - Smart API Optimization:**
```csharp
// 1. Check known directors cache first (0 API calls)
if (KnownDirectors.TryGetValue(personName, out int knownId))
    return knownId;
    
// 2. Use director credential validation for disambiguation
var directorCredits = creditsResponse?.Crew?.Count(c => c.Job == "Director") ?? 0;
if (directorCredits > 0) return candidate.Id;
```

**Performance Impact**: 70-90% reduction in TMDB API usage through intelligent caching and validation

### 🏭 Azure Integration Pattern
**For complete Azure architecture details, see [README.md](README.md#azure-architecture)**

**Key AI Pattern - Automatic Placeholder Replacement:**
```csharp
// Production configuration pattern
if (builder.Environment.IsProduction() && connectionString.Contains("{DatabasePassword}"))
{
    var databasePassword = builder.Configuration["DatabasePassword"];
    connectionString = connectionString.Replace("{DatabasePassword}", databasePassword);
}
```

**Essential Azure Components:**
- Azure Key Vault: `cinelogdb.vault.azure.net` 
- Azure SQL Database: `CineLog_Production`
- Environment Variable: `AZURE_KEY_VAULT_URI` for automatic integration

### 🌐 TMDB Service Integration
```csharp
var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
var searchResults = await _tmdbService.SearchMoviesAsync(query);
```
**Key Patterns:**
- 24-hour caching for all TMDB data
- Use `GetMultipleMovieDetailsAsync()` to avoid N+1 queries
- Rate limiting with SemaphoreSlim protection

### 🎭 AJAX-Enhanced System Patterns
**Critical AI Pattern - X-Requested-With Header:**
```javascript
// REQUIRED for JSON response
headers: {
    'X-Requested-With': 'XMLHttpRequest'
}
```

**Key Requirements:**
- Unified helper methods for initial load and AJAX reshuffles
- Graceful fallback to page navigation if AJAX fails
- Server-side rendering for all AJAX responses
- State preservation with `PopulateMovieProperties()`

**AJAX Layout Stability Patterns:**
```javascript
// CRITICAL: Preserve Bootstrap structure during AJAX replacement
const currentContainer = currentMain.querySelector('.container');
const newContainer = newMain.querySelector('.container');
if (currentContainer && newContainer) {
    currentContainer.innerHTML = newContainer.innerHTML; // Preserves main element classes
}
```

**Layout Stability Requirements:**
- Target `.container` elements, never replace `main` element completely
- Use invisible placeholders to maintain consistent column widths
- Add `html { overflow-y: scroll; }` to prevent scrollbar-related width changes
- Reserve minimum height for dynamic components (Timeline Navigator)

### 📄 Critical Implementation Patterns
```csharp
// CORRECT: Use TotalCount for pagination
var viewModel = new ViewModel {
    TotalCount = paginatedList.TotalCount  // Total DB count
};

// Business rule: Mutual exclusion
var existsInWishlist = await _dbContext.WishlistItems
    .AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
```

**Performance Requirements:**
- Batch processing for multiple movies
- 15-minute cache for user data
- Composite indexes on `UserId+Title`


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

**For detailed configuration setup and Azure architecture, see [README.md](README.md#configuration)**

### Essential AI Configuration Patterns:
- **Development**: User Secrets for secure local development
- **Production**: Azure Key Vault with automatic placeholder replacement
- **Caching Strategy**: 24-hour TMDB cache, 15-minute user data cache
- **Session Management**: 20-minute timeout for anti-repetition

---

## 📚 Documentation References

This CLAUDE.md file contains essential AI guidance and CineLog-specific patterns. For comprehensive information, refer to:

### Primary Documentation
- **[README.md](README.md)**: Complete architecture, features, setup instructions, and tech stack
- **[CHANGELOG.md](CHANGELOG.md)**: Recent updates, feature releases, and bug fixes  
- **[PERFORMANCE_OPTIMIZATION_SUMMARY.md](PERFORMANCE_OPTIMIZATION_SUMMARY.md)**: Performance metrics, optimization details, and improvements

### Specialized Documentation
- **[AGENTS.md](./.claude/agents/AGENTS.md)**: Detailed agent system documentation and usage patterns
- **[SESSION_NOTES.md](SESSION_NOTES.md)**: Development session history and context (gitignored)

**This optimization reduces CLAUDE.md size by ~35% while preserving all essential AI guidance and creating clear documentation references.**