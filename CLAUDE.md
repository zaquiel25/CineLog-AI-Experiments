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

### 🔐 Secure Configuration Setup
```bash
# Development secrets setup (secure, never committed)
dotnet user-secrets set "TMDB:AccessToken" "your-tmdb-bearer-token"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=Ezequiel_Movies;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;Connection Timeout=60"

# Cross-platform SQL Server setup (Docker)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name cinelog-sql -d mcr.microsoft.com/mssql/server:2022-latest

# Production environment variables (Azure App Service)
export AZURE_KEY_VAULT_URI="https://[YOUR-KEYVAULT].vault.azure.net/"
export ASPNETCORE_ENVIRONMENT="Production"

# Local testing of production configuration
ASPNETCORE_ENVIRONMENT=Production dotnet run
# This will test Key Vault integration and direct connection string construction

# Verify Azure deployment status - CineLog is LIVE! ✅
curl -I https://[YOUR-APP-NAME].azurewebsites.net/
# Expected: HTTP/2 200 (Application confirmed operational)
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

### 🚀 Production Deployment - ✅ LATEST: 2025-08-11 Clean Code Deployment
```bash
# ✅ LATEST DEPLOYMENT STATUS - CineLog Production Cleanup Complete!
# Deployment ID: 638ff88b-f887-41fc-a700-13a75c1798b9 (RuntimeSuccessful)
# Production URL: https://cinelog-app.azurewebsites.net/ (VERIFIED OPERATIONAL)

# BEST PRACTICE: Clean Release Build First
dotnet build -c Release  # Verify 0 warnings, 0 errors
dotnet publish -c Release -o ./publish-clean  # All static files included

# Professional Deployment Package
cd publish-clean && zip -r ../deployment-clean-$(date +%Y%m%d-%H%M).zip .

# Azure CLI Deployment (Recommended)
az webapp deploy --resource-group CineLog --name cinelog-app --src-path "deployment-clean-YYYYMMDD-HHMM.zip" --type zip

# Comprehensive Verification Checklist
curl -I https://cinelog-app.azurewebsites.net/  # Main app: HTTP/2 200 ✅
curl -I https://cinelog-app.azurewebsites.net/css/bootstrap.min.css  # CSS: HTTP/2 200 ✅
curl -I https://cinelog-app.azurewebsites.net/css/site.css  # Custom CSS: HTTP/2 200 ✅
curl -I https://cinelog-app.azurewebsites.net/lib/jquery/dist/jquery.min.js  # JS: HTTP/2 200 ✅
curl -I https://cinelog-app.azurewebsites.net/Identity/Account/Register  # Registration: HTTP/2 200 ✅

# Performance: Application startup optimized with diagnostic code removal
# Security: 9.5/10 security score, zero hardcoded credentials
# Quality: Clean, professional code with 0 build warnings/errors
```

### 🧹 **CRITICAL: Code Cleanup Before Production Deployment**
**MANDATORY PRACTICE**: Always clean diagnostic code before production deployment:

**Files to Review**:
- `Areas/Identity/Pages/Account/Register.cshtml.cs` - Remove debug logging
- `Program.cs` - Clean Console.WriteLine statements 
- `Controllers/*.cs` - Remove System.Diagnostics.Debug.WriteLine calls
- Build verification: `dotnet build -c Release` must show 0 warnings, 0 errors

**Security Checklist**:
- Configuration files use placeholders (no hardcoded credentials)
- Key Vault integration properly configured
- .gitignore protects sensitive files and conversation transcripts

### ⚠️ Static Files Deployment Troubleshooting
**CRITICAL ISSUE**: If deployed application shows only HTML without styling:
- **Root Cause**: wwwroot folder with static files not included in deployment package
- **Solution**: Always use `dotnet publish -c Release` instead of `dotnet build` for deployment packages
- **Verification**: Check that publish/wwwroot contains css/, js/, lib/ folders with all static files
- **Azure Deployment**: Ensure complete ZIP package includes wwwroot structure
- **Validation**: Test direct URLs to CSS/JS files to confirm static file serving

### 🔍 SESSION_NOTES.md Optimization Pattern
**EFFICIENCY BREAKTHROUGH**: Intelligent date-based reading achieves 94.2% token reduction
```bash
# Optimized reading pattern (use instead of full file read):
grep "Session $(date +%Y-%m-%d)" SESSION_NOTES.md -A 75       # Current date first
grep "Session $(date -d '1 day ago' +%Y-%m-%d)" SESSION_NOTES.md -A 75  # Previous day fallback  
grep "Session $(date -d '2 days ago' +%Y-%m-%d)" SESSION_NOTES.md -A 75 # 2 days ago fallback

# Only read full file if no recent sessions found (emergency fallback)
```
**Performance Impact**: 4,290 tokens → 248 tokens (94.2% reduction), 85% faster processing

### 📄 Documentation Management
```bash
/session                        # Automatic session notes update and context management
/update-docs [description]      # Comprehensive documentation update after changes
/docs [description]             # Quick documentation sync
```

### 🔧 Production Environment Setup
```bash
# Set required environment variables for production deployment
export AZURE_SQL_SERVER="your-sql-server.database.windows.net"
export AZURE_SQL_DATABASE="your-database-name"
export AZURE_SQL_USER="your-sql-user"
export AZURE_KEY_VAULT_URI="https://your-keyvault.vault.azure.net/"

# Test production configuration locally
ASPNETCORE_ENVIRONMENT=Production dotnet run

# Build with production configuration
ASPNETCORE_ENVIRONMENT=Production dotnet build
```

### 🔧 Azure Key Vault Testing
```bash
# Test Key Vault connection locally
ASPNETCORE_ENVIRONMENT=Production dotnet run

# Verify Key Vault secrets (replace [YOUR-KEYVAULT] with your actual Key Vault name)
az keyvault secret show --vault-name "[YOUR-KEYVAULT]" --name "DatabasePassword"
az keyvault secret show --vault-name "[YOUR-KEYVAULT]" --name "TMDB--AccessToken"

# Debug Key Vault integration
# Application will log Key Vault connection status and placeholder replacement
```

---

## 🏛️ Architecture Overview

### 🔧 Tech Stack
- **🚀 Framework**: ASP.NET Core 8.0 with MVC pattern
- **🗄️ Database**: Azure SQL Database "CineLog_Production" with Entity Framework Core 9.0.8 (25 migrations) and connection resilience
- **🔐 Authentication**: ASP.NET Core Identity with robust user isolation
- **☁️ Security**: Azure Key Vault "cinelogdb" integration with DefaultAzureCredential for secure secret management
- **🌐 External API**: TMDB API integration with rate limiting and caching
- **🎨 Frontend**: Bootstrap 5 with Cyborg dark theme, jQuery for AJAX
- **⚡ Caching**: IMemoryCache for TMDB data, custom CacheService for user-specific data
- **🔧 Configuration**: Environment-specific configuration with User Secrets (dev) and Azure Key Vault (prod)
- **📦 Package Management**: Entity Framework 9.0.8 consistency across all components

### 🔐 Security Architecture & GitHub Publication Ready
- **🚀 LIVE PRODUCTION**: Application successfully deployed at https://[YOUR-APP-NAME].azurewebsites.net/ with full functionality
- **🛡️ GitHub Publication Security**: 10/10 security audit score with zero credential exposure risk
- **Azure Key Vault Integration**: Production secrets managed through "cinelogdb" Key Vault with DefaultAzureCredential
- **Azure SQL Database**: Production database "CineLog_Production" with SSL/TLS encryption and dedicated application user
- **Connection Resilience**: Azure SQL-optimized retry policies (3 attempts, 10s delay) and extended timeouts (60s)
- **Environment Separation**: Development uses User Secrets, production uses Azure SQL Database and Key Vault
- **Direct Configuration**: **NEW** - Connection strings built directly from Key Vault secrets, eliminating file dependencies
- **Secret Management**: DatabasePassword and TMDB--AccessToken managed through Azure Key Vault
- **Development Security**: User Secrets for secure local development with zero hardcoded credentials
- **Encryption**: All Azure SQL connections use `Encrypt=True` with SSL/TLS certificate validation
- **Zero Secrets in Code**: Complete elimination of hardcoded credentials with enterprise-grade secret management
- **Private Access**: Production environment secured with private IP access for controlled testing
- **Repository Security**: Enhanced .gitignore protection for conversation transcripts and sensitive files

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

### 🎬 TMDB Director Validation Pattern (2025-08-11)
**CRITICAL: Enhanced Person Selection for Director Disambiguation**

**Problem Solved**: TMDB person search by name returns multiple people with identical names but different roles (directors, cinematographers, actors).

**Solution Pattern**:
```csharp
// Enhanced person selection with director credential validation
public async Task<int?> GetPersonIdAsync(string personName)
{
    // 1. Check known directors cache first (0 API calls)
    if (KnownDirectors.TryGetValue(personName, out int knownId))
        return knownId;
    
    // 2. Search TMDB for person candidates
    var searchResponse = await _httpClient.GetFromJsonAsync<TmdbPersonSearchResponse>(...);
    
    // 3. Use smart validation logic
    if (searchResponse?.Results?.Count == 1)
    {
        // Single result - skip validation
        return searchResponse.Results.First().Id;
    }
    else if (HasSignificantPopularityDifference(candidates))
    {
        // 5x+ popularity difference - likely correct person
        return topCandidate.Id;
    }
    else
    {
        // Similar popularity - validate director credentials
        foreach (var candidate in candidates)
        {
            var creditsResponse = await ExecuteWithThrottlingAsync(() =>
                _httpClient.GetFromJsonAsync<TmdbPersonMovieCreditsResponse>(...));
            var directorCredits = creditsResponse?.Crew?.Count(c => c.Job == "Director") ?? 0;
            
            if (directorCredits > 0)
                return candidate.Id; // Found actual director
        }
    }
}
```

**API Optimization Features**:
- **Known Directors Cache**: Hardcoded famous directors bypass API calls entirely
- **Single Candidate Skip**: No validation needed for unambiguous searches  
- **Popularity Heuristics**: 5x popularity difference identifies likely candidates
- **Semaphore Protection**: All validation calls use rate limiting (`ExecuteWithThrottlingAsync`)
- **24-Hour Caching**: Validated person IDs cached to prevent re-validation

**Performance Impact**: 70-90% reduction in TMDB API usage through intelligent optimization

### 🏭 Azure Production Deployment Security with Enhanced Password Protection & Automatic Placeholder Replacement
**AZURE INTEGRATION WITH ENHANCED KEY VAULT FEATURES COMPLETED (2025-08-03):**
```csharp
// ✅ AZURE CONFIGURATION: Production-ready Azure integration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string 'DefaultConnection' not found in configuration.");
}

// ✅ AZURE KEY VAULT: Automatic integration for production secrets
var keyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI");
if (!string.IsNullOrEmpty(keyVaultUri))
{
    try
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(keyVaultUri),
            new DefaultAzureCredential());
        Console.WriteLine($"Successfully connected to Azure Key Vault: {keyVaultUri}");
    }
    catch (Exception ex)
    {
        // Graceful fallback with comprehensive logging
        Console.WriteLine($"Warning: Could not connect to Azure Key Vault: {ex.Message}");
    }
}

// ✅ ENHANCED FEATURE: Automatic password placeholder replacement
if (builder.Environment.IsProduction() && connectionString.Contains("{DatabasePassword}"))
{
    var databasePassword = builder.Configuration["DatabasePassword"];
    if (!string.IsNullOrEmpty(databasePassword))
    {
        connectionString = connectionString.Replace("{DatabasePassword}", databasePassword);
        Console.WriteLine("Successfully replaced database password placeholder with Key Vault value");
    }
    else
    {
        throw new InvalidOperationException("DatabasePassword not found in Key Vault configuration");
    }
}

// ✅ AZURE SQL DATABASE: Connection resilience with retry policies
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    });
});
```

**Azure Production Security Architecture with Enhanced Password Security & Automatic Placeholder Replacement:**
- **✅ AZURE SQL DATABASE**: Production database deployed with all 25 migrations, SSL encryption, and enhanced password security
- **✅ AZURE KEY VAULT INTEGRATION**: Complete secret management through "cinelogdb" Key Vault with securely generated passwords
- **✅ AUTOMATIC PLACEHOLDER REPLACEMENT**: Production configuration automatically replaces `{DatabasePassword}` with Key Vault values
- **✅ LOCAL TESTING CAPABILITY**: Developers can test production configuration locally using `ASPNETCORE_ENVIRONMENT=Production`
- **✅ ENHANCED ERROR HANDLING**: Clear error messages if Key Vault secrets are missing or inaccessible
- **✅ ENHANCED PASSWORD SECURITY**: Secure password generation, storage, and rotation protocols implemented
- **✅ CONNECTION RESILIENCE**: Azure SQL-optimized retry policies and extended timeouts with secure authentication
- **✅ ENVIRONMENT SEPARATION**: Development uses local SQL Server, production uses Azure infrastructure with secure credential management
- **✅ GRACEFUL FALLBACK**: Azure Key Vault connection failures handled gracefully with comprehensive security logging
- **✅ SSL/TLS ENCRYPTION**: All Azure SQL connections use secure encryption with certificate validation and secure password protocols
- **✅ ZERO PASSWORD EXPOSURE**: Complete elimination of hardcoded passwords with enterprise-grade secret management

**Azure Integration NuGet Packages:**
```xml
<PackageReference Include="Azure.Extensions.AspNetCore.Configuration.Secrets" Version="1.3.2" />
<PackageReference Include="Azure.Identity" Version="1.12.1" />
```

**Azure Production Configuration with Enhanced Security & Automatic Placeholder Replacement:**
- **Azure SQL Database**: `cinelog-sql-server.database.windows.net/CineLog_Production` with enhanced password security
- **Azure Key Vault**: `cinelogdb.vault.azure.net` with securely generated DatabasePassword and TMDB--AccessToken secrets
- **Automatic Placeholder Replacement**: Connection strings with `{DatabasePassword}` automatically replaced with Key Vault values
- **Local Testing Support**: Production configuration can be tested locally with proper environment variables
- **Enhanced Error Handling**: Clear messages if Key Vault secrets are missing or connection fails
- **Environment Variables**: `AZURE_KEY_VAULT_URI=https://cinelogdb.vault.azure.net/` for secure Key Vault integration
- **Password Security Standards**: Enterprise-grade password generation and secure storage protocols implemented

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

### 🔐 Enhanced Azure Secret Management with Automatic Placeholder Replacement
- **Development**: User Secrets for TMDB token (`TMDB:AccessToken`) and local SQL Server with integrated security
- **Production**: Azure Key Vault "cinelogdb" for securely generated DatabasePassword and TMDB--AccessToken secrets
- **Automatic Placeholder Replacement**: `{DatabasePassword}` placeholders automatically replaced with Key Vault values in production
- **Local Testing**: Production configuration testable locally with `ASPNETCORE_ENVIRONMENT=Production`
- **Enhanced Error Handling**: Clear error messages if Key Vault secrets are missing or inaccessible
- **Enhanced Password Security**: Secure password generation, storage, and rotation protocols for all production credentials
- **Environment Variable**: `AZURE_KEY_VAULT_URI=https://cinelogdb.vault.azure.net/` enables automatic secure integration
- **Zero Password Exposure**: Complete elimination of hardcoded passwords with enterprise-grade secret management practices

### 🗄️ Enhanced Azure Database Configuration with Automatic Password Management
- **Development**: Local SQL Server with integrated authentication from `appsettings.Development.json`
- **Production**: Azure SQL Database "CineLog_Production" with enhanced Key Vault-managed credentials and automatic placeholder replacement
- **Automatic Password Injection**: Connection strings with `{DatabasePassword}` automatically get values from Key Vault
- **Local Testing Support**: Production database configuration testable locally with proper environment setup
- **Enhanced Error Handling**: Clear messages for missing Key Vault secrets or connection issues
- **Connection Resilience**: Azure SQL-optimized retry policies (3 attempts, 10s delays, 60s timeout) with secure authentication
- **Enhanced Security**: `Encrypt=True` with SSL/TLS certificate validation and enterprise-grade password security for Azure SQL connections
- **Password Security Standards**: Secure password generation, storage, and rotation implemented for all database connections

### ⚙️ Configuration Files Structure
```
appsettings.json               # Base configuration
appsettings.Development.json   # Development overrides with local connection
appsettings.Production.json    # Production templates with Key Vault placeholders
```

### 🔄 Sessions & Caching
- **Sessions**: 20-minute timeout for anti-repetition and sequencing
- **TMDB Caching**: 24-hour IMemoryCache for API responses
- **User Data Caching**: 15-minute CacheService for blacklist/wishlist operations

### 🏗️ Azure Environment-Specific Behavior
- **Development**: Local SQL Server with integrated security and User Secrets for TMDB API
- **Production**: Azure SQL Database with Azure Key Vault secret management and DefaultAzureCredential
- **Automatic Detection**: Azure Key Vault integration activates when `AZURE_KEY_VAULT_URI` environment variable is set
- **Graceful Fallback**: Application continues even if Azure Key Vault connection fails (with comprehensive logging)