## 🛰️ MCP Server & Extension Usage

If a user requests information or actions related to an MCP server or extension, and the MCP is available, you should automatically fetch or use it as needed to fulfill the request. You do not need to wait for explicit invocation if the context is clear.

### Available MCP Servers:
- **microsoft-docs**: Microsoft Learn documentation for ASP.NET Core, Entity Framework, and related technologies
- **deepwiki**: Deep Wikipedia access for comprehensive research and information retrieval  
- **context7**: Upstash Context7 for enhanced contextual AI operations and reasoning
- **codacy**: Code quality analysis and review tools (requires account token configuration)

**Usage Examples:**
- ASP.NET Core questions → Automatically use microsoft-docs MCP
- Research tasks → Leverage deepwiki for comprehensive information
- Code quality issues → Utilize codacy for analysis and recommendations
- Complex reasoning tasks → Apply context7 for enhanced AI capabilities


---
**Best Practice:**
> For all AJAX POST requests (especially for Blacklist/Wishlist removal), **always include** the `X-Requested-With: XMLHttpRequest` header. This guarantees the backend returns JSON for AJAX, not HTML error pages, and prevents frontend parsing errors. This is required for robust, user-friendly error handling in all AJAX-powered UI actions.


## 🗂️ Critical Workflow System (Session 2025-08-08)

### 🚨 MANDATORY WORKFLOW - ALWAYS START HERE
**CRITICAL**: Every session MUST begin with SESSION_NOTES.md reading as first action:
1. **Context Reading**: Read SESSION_NOTES.md to understand current project state
2. **Compliance Check**: Verify request against CLAUDE.md golden rules
3. **Agent Selection**: Choose optimal agent based on enhanced decision tree
4. **Execution**: Execute with systematic 6-step pattern
5. **Verification**: Build verification and compliance checking
6. **Documentation**: Update SESSION_NOTES.md after significant milestones

### 📋 6-Step Systematic Approach
**For ALL development tasks, follow this pattern:**
1. **Problem Analysis**: Understand requirements deeply, question existing patterns
2. **Codebase Investigation**: Search, read, and understand relevant code
3. **Planning & TodoWrite**: Create detailed todo list, check off systematically
4. **Implementation**: Professional comments, build verification, CineLog patterns
5. **Debugging & Testing**: Edge cases, user isolation, structured logging
6. **Documentation Update**: Update SESSION_NOTES.md after major milestones

### 🤖 Enhanced Agent Decision Tree
**Proactive agent usage with enhanced decision criteria:**

| Task Domain | Primary Agent | Auto-Triggers | Decision Criteria |
|-------------|---------------|---------------|-------------------|
| Movie features/algorithms | `cinelog-movie-specialist` | `test-writer-fixer` | MoviesController, suggestions, TMDB |
| API integration issues | `tmdb-api-expert` | None | External API, rate limiting, caching |
| Performance problems | `performance-optimizer` | `performance-benchmarker` | Slow queries, N+1 problems, cache |
| Database changes | `ef-migration-manager` | `backend-architect` | Schema, migrations, EF Core |
| Full-stack features | `aspnet-feature-developer` | `test-writer-fixer`, `ui-designer` | Complete MVC development |
| Production deployment | `deployment-project-manager` | Multi-agent coordination | Strategic deployment, infrastructure |
| Session continuity | `session-secretary` | Auto after milestones | Context management, note updates |

### 🎯 Session Continuity Management
**SESSION_NOTES.md is critical for project context and continuity:**
- **Read First**: ALWAYS read as first action to understand current state
- **Track Progress**: Note blockers, decisions, and next priorities
- **Update Triggers**: After resolving blockers, TodoWrite completions, successful builds
- **Context Preservation**: Maintain working context across sessions (gitignored file)

---

## ⚠️ CRITICAL INSTRUCTIONS

### 🚨 GOLDEN RULE: DO NOT INVENT THINGS
**MOST IMPORTANT - User explicitly emphasized:**
- **NEVER** add features, improvements, or enhancements unless explicitly requested
- **NEVER** add loading states, animations, or visual changes unless asked
- **NEVER** assume user wants "better" UX or performance improvements
- **NEVER** add "nice-to-have" functionality or creative interpretations
- **REMEMBER**: "don't add things we don't ask for" - this is the #1 rule
- When user says something is wrong, it's because you added things they didn't want
- Implement ONLY what is specifically requested, nothing more

### 📝 MANDATORY PROFESSIONAL COMMENTS
**ALWAYS add comments when making significant changes:**
- **ALL** new methods MUST have comprehensive XML documentation
- **ALL** complex logic MUST have inline comments explaining "why"
- Use FEATURE/FIX/ENHANCEMENT prefixes for significant changes
- Explain business logic, architectural decisions, and performance considerations
- Comments MUST be in English and follow professional standards
- **NEVER** leave new code uncommented - this is unprofessional

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

### 🚫 NEVER Auto-Commit
- **NEVER** stage and commit files automatically
- Only commit when explicitly asked by the user

### 🔨 Build Requirements & Verification
- **CRITICAL**: A task is NEVER complete if the application cannot build successfully
- **ALWAYS** run `dotnet build` to verify compilation before marking tasks finished
- Build failures MUST be resolved as part of implementation, not left for later
- **PRODUCTION VERIFICATION**: Test both Development and Production environments:
  ```bash
  dotnet build                           # Verify compilation
  dotnet run --environment=Development   # Test local development configuration
  dotnet run --environment=Production    # Test Azure production configuration
  ```
- **Compliance Verification**: Built-in verification against CLAUDE.md instructions

### 🌐 Azure Cloud Commands
- **Database Migration to Azure SQL:**
  ```bash
  dotnet ef database update --environment Production  # Apply migrations to Azure SQL
  ```
- **Key Vault Secret Management:**
  ```bash
  az keyvault secret set --vault-name "[YOUR-KEYVAULT]" --name "DatabasePassword" --value "your-password"
  az keyvault secret set --vault-name "[YOUR-KEYVAULT]" --name "TMDB--AccessToken" --value "your-tmdb-token"
  ```

### 📋 Task Management & TodoWrite Integration
- **ALWAYS** use TodoWrite tool for complex multi-step tasks
- **MUST** keep working until ALL todo items are checked off
- **NEVER** end your turn until problem is completely solved and verified
- **Systematic Workflow**: Follow 6-step pattern for all complex tasks
- **Auto-Update SESSION_NOTES.md**: After TodoWrite milestone completions

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
- **Systematic Analysis**: Use 6-step workflow pattern for complex pattern adaptation

### 📐 Literal Implementation Over Creative Interpretation
**CRITICAL: Stick to requirements - DO NOT INVENT THINGS:**
- Implement **ONLY** explicitly requested functionality and logic
- **NEVER** assume, infer, or add "improvements" unless explicitly asked
- **NEVER** add extra features, UI enhancements, or "nice-to-have" functionality
- **NEVER** assume user wants optimizations, refactoring, or architectural changes
- **NEVER** add loading states, animations, or visual improvements unless requested
- Creativity is limited to accomplishing what's requested, not expanding requirements
- When in doubt, ask for clarification rather than assume
- **REMEMBER**: User feedback like "don't add things we don't ask for" means STOP INVENTING

---

<!-- AJAX REMOVAL PATTERN: Blacklist & Wishlist -->
<button type="button" class="btn btn-soft-danger btn-sm remove-from-blacklist" data-tmdb-id="12345">Remove</button>
<button type="button" class="btn btn-soft-danger btn-sm remove-from-wishlist" data-tmdb-id="12345">Remove</button>
```

#### **AJAX Removal Pattern (Blacklist/Wishlist)**
// Best Practice: Always use 'credentials: same-origin' and include the anti-forgery token in the body for secure, authenticated requests.
```javascript
// Always include X-Requested-With header to guarantee JSON response from backend
fetch('/Movies/RemoveFromBlacklist', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'X-Requested-With': 'XMLHttpRequest'
    },
    body: `tmdbId=${encodeURIComponent(tmdbId)}&__RequestVerificationToken=${encodeURIComponent(token)}`,
    credentials: 'same-origin'
});
// Robust error handling: always parse response as text, then try JSON, fallback to alert
```

#### **Troubleshooting**
- If you see a "Non-JSON response" error in the UI, ensure your AJAX request includes the `X-Requested-With: XMLHttpRequest` header and the backend action returns JSON for all AJAX cases.
+ If you see a "Non-JSON response" error in the UI, ensure your AJAX request includes the `X-Requested-With: XMLHttpRequest` header and the backend action returns JSON for all AJAX cases.
+ Quick tip: You can inspect network requests in your browser's dev tools to confirm the header is present and the response is valid JSON.
- If you see a "Non-JSON response" error in the UI, ensure your AJAX request includes the `X-Requested-With: XMLHttpRequest` header and the backend action returns JSON for all AJAX cases.

## 🛡️ DEVELOPMENT SAFETY PROTOCOL
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

## 🔄 Development Workflow

### 🚨 WORKFLOW REMINDERS (CRITICAL)
**Before starting ANY development task:**
1. **DO NOT INVENT**: Implement ONLY what is explicitly requested
2. **COMMENT EVERYTHING**: All new code must have professional comments
3. **NO ASSUMPTIONS**: Ask for clarification instead of assuming improvements

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
- **Look for unified helper methods** that implement consistent business logic
- **Check for existing patterns** before implementing new solutions

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
- **MANDATORY**: Add professional comments to ALL new code (XML docs + inline comments)
- **DO NOT ADD** features, improvements, or enhancements unless explicitly requested
- When detecting environment variables needed, proactively create .env file
- Test frequently after each change
- **ALWAYS** run `dotnet build` to verify compilation
- **Follow CineLog patterns** for user data isolation, caching, and API usage
- **Use unified helper methods** for consistent business logic

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

### 🚀 LIVE PRODUCTION DEPLOYMENT SUCCESS (2025-08-07)
**CineLog is now LIVE at https://[YOUR-APP-NAME].azurewebsites.net/ with complete Azure infrastructure:**

#### 🎯 Production Status: 100% OPERATIONAL
- ✅ **Application Status**: Live and fully functional with all features operational
- ✅ **Azure SQL Database**: "[YOUR-DATABASE]" with all 25 migrations successfully applied
- ✅ **Azure Key Vault Integration**: Complete secret management with automatic placeholder replacement
- ✅ **Connection Resilience**: Retry policies and 60s timeouts for Azure SQL reliability
- ✅ **Security Standards**: Zero hardcoded credentials, enterprise-grade secret management
- ✅ **Private Access**: Restricted access controls for controlled testing environment

#### 🔧 Production Architecture Breakthrough (2025-08-07)
**MAJOR INNOVATION**: Direct Key Vault integration eliminating configuration file dependencies:

### 🛡️ Infrastructure Security & Azure Production Configuration (2025-08-07 - UPDATED)
**CRITICAL: Infrastructure security sanitization complete - Score 9/10 enterprise-grade protection:**

#### 🔒 **Infrastructure Security Requirements (MANDATORY)**
- **NEVER** use specific Azure resource names in public code/documentation
- **ALWAYS** use placeholder format: `[YOUR-RESOURCE-NAME]` in all documentation
- **ALWAYS** use environment variables for production Azure resource configuration:
  - `AZURE_SQL_SERVER` - Your Azure SQL Server name (without .database.windows.net)
  - `AZURE_SQL_DATABASE` - Your database name
  - `AZURE_SQL_USER` - Your SQL Server user
  - `AZURE_KEY_VAULT_URI` - Full URI to your Key Vault (https://[name].vault.azure.net/)
- **NEVER** include debug `Console.WriteLine` statements in production code
- **ALWAYS** replace hardcoded resource references with environment variable lookups

#### 🏗️ **Secure Production Configuration Pattern**
```csharp
// PRODUCTION: Environment variable-based configuration for security
if (builder.Environment.IsProduction())
{
    var sqlServer = Environment.GetEnvironmentVariable("AZURE_SQL_SERVER") ?? "your-sql-server.database.windows.net";
    var sqlDatabase = Environment.GetEnvironmentVariable("AZURE_SQL_DATABASE") ?? "your-database-name";
    var sqlUser = Environment.GetEnvironmentVariable("AZURE_SQL_USER") ?? "your-sql-user";
    
    connectionString = $"Server=tcp:{sqlServer},1433;Database={sqlDatabase};User ID={sqlUser};Password={databasePassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60";
}
```

### 🛡️ Azure Cloud Integration & Production Security (2025-08-03 → 2025-08-07)
**CRITICAL: Azure SQL Database and Key Vault integration for production deployment:**

```csharp
// PRODUCTION: Use configuration-based connection strings with Azure Key Vault
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string 'DefaultConnection' not found in configuration.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // PRODUCTION: Add connection resilience with retry policies
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        
        // PRODUCTION: Set command timeout for long-running queries
        sqlOptions.CommandTimeout(60);
    });
    
    // PERFORMANCE: Only enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// PRODUCTION: Azure Key Vault integration for secure secret management
if (builder.Environment.IsProduction())
{
    var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(keyVaultUri) && !keyVaultUri.Contains("{"))
    {
        try
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
        catch (Exception ex)
        {
            // Warning: Could not connect to Key Vault - check configuration
        }
    }
}
```

**Azure Infrastructure Details:**
- **Azure SQL Database**: `[YOUR-DATABASE]` on server `[YOUR-SQL-SERVER].database.windows.net`
- **Azure Key Vault**: `[YOUR-KEYVAULT].vault.azure.net` with secrets `DatabasePassword` and `TMDB--AccessToken`
- **Connection String Format**: `Server=tcp:[YOUR-SQL-SERVER].database.windows.net,1433;Database=[YOUR-DATABASE];User ID=[YOUR-SQL-USER];Password={DatabasePassword};Encrypt=True;TrustServerCertificate=False`
- **Automatic Placeholder Replacement**: Production mode automatically replaces `{DatabasePassword}` with actual Key Vault values

**Security Requirements:**
- **NEVER** hardcode connection strings or passwords in source code
- **ALWAYS** use `Encrypt=True` for Azure SQL Database connections  
- **ALWAYS** use Azure Key Vault for production secret management with DefaultAzureCredential
- **ALWAYS** implement connection retry policies for Azure SQL resilience
- **ALWAYS** implement automatic placeholder replacement for Key Vault secrets in production
- Use `TrustServerCertificate=False` for Azure SQL (True only for localhost testing)
- **ALWAYS** validate connection strings exist before using them
- **ALWAYS** handle Key Vault connection failures gracefully with fallback patterns
- **ALWAYS** validate Key Vault secrets are available before replacing placeholders

#### 🧪 **Secure Development & Testing Practices**
**Development Environment Security:**
- **ALWAYS** use User Secrets for local development credentials:
  ```bash
  dotnet user-secrets set "TMDB:AccessToken" "your-tmdb-bearer-token"
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=Ezequiel_Movies;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  ```
- **NEVER** commit credentials to repository - verify with `git status` before commits
- **ALWAYS** use Docker SQL Server for cross-platform development:
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourPassword" -p 1433:1433 --name cinelog-sql -d mcr.microsoft.com/mssql/server:2022-latest
  ```

**Production Testing Requirements:**
```bash
# Test both environments before deployment
dotnet build                                           # Verify compilation
ASPNETCORE_ENVIRONMENT=Development dotnet build        # Test dev configuration
ASPNETCORE_ENVIRONMENT=Production dotnet build         # Test prod configuration

# Test environment variable requirements
export AZURE_SQL_SERVER="test-server" && \
export AZURE_SQL_DATABASE="test-db" && \
export AZURE_SQL_USER="test-user" && \
export AZURE_KEY_VAULT_URI="https://test-vault.vault.azure.net/" && \
ASPNETCORE_ENVIRONMENT=Production dotnet build
```

**Security Verification Checklist:**
- [ ] No hardcoded Azure resource names in code/documentation
- [ ] All production configuration uses environment variables
- [ ] No debug Console.WriteLine statements in production code
- [ ] User Secrets properly configured for development
- [ ] Application builds cleanly in both Development and Production modes

**Required NuGet Packages for Production Security:**
```xml
<PackageReference Include="Azure.Extensions.AspNetCore.Configuration.Secrets" Version="1.3.2" />
<PackageReference Include="Azure.Identity" Version="1.12.1" />
```

**Configuration Files Pattern:**
```json
// appsettings.Production.json - AZURE CLOUD CONFIGURATION
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:[YOUR-SQL-SERVER].database.windows.net,1433;Database=[YOUR-DATABASE];User ID=[YOUR-SQL-USER];Password={DatabasePassword};Encrypt=True;TrustServerCertificate=False"
  },
  "KeyVault": {
    "VaultUri": "https://[YOUR-KEYVAULT].vault.azure.net/"
  },
  "TMDB": {
    "AccessToken": "{TMDB--AccessToken}"
  }
}
```

**Azure Key Vault Secrets:**
- `DatabasePassword`: Azure SQL Database password for [YOUR-SQL-USER] user
- `TMDB--AccessToken`: TMDB API access token (uses -- instead of : for Key Vault compatibility)

**Key Vault Integration Pattern (CRITICAL for Security):**
```csharp
// REQUIRED: Automatic placeholder replacement in production
if (builder.Environment.IsProduction() && connectionString.Contains("{DatabasePassword}"))
{
    var databasePassword = builder.Configuration["DatabasePassword"];
    if (!string.IsNullOrEmpty(databasePassword))
    {
        connectionString = connectionString.Replace("{DatabasePassword}", databasePassword);
    }
    else
    {
        throw new InvalidOperationException("DatabasePassword not found in Key Vault configuration");
    }
}
```

**Local Testing of Production Configuration:**
```bash
# Test Azure Key Vault integration locally
ASPNETCORE_ENVIRONMENT=Production dotnet ef database update

# Verify Azure authentication
az account show
az keyvault secret show --vault-name cinelogdb --name DatabasePassword --query attributes.updated
```

### 🔍 **Security Audit Lessons Learned (2025-08-07)**
**Critical insights from comprehensive infrastructure security audit:**

#### 🚨 **Security Assessment Scope**
- **Lesson**: Security audits must extend beyond credentials to infrastructure exposure
- **Previous Gap**: Initial audits focused only on passwords/tokens, missed Azure resource names, IP addresses, and debug code
- **New Standard**: Comprehensive security includes infrastructure reconnaissance prevention

#### 📋 **Security Audit Checklist**
**Before any public repository publication:**
- [ ] **Credentials**: No hardcoded passwords, tokens, or connection strings
- [ ] **Infrastructure**: No specific Azure resource names (servers, databases, Key Vaults, app services)
- [ ] **Network Details**: No IP addresses, URLs, or network configuration details
- [ ] **Debug Code**: No Console.WriteLine statements or debug output in production code
- [ ] **Environment Variables**: Production uses environment variables, not hardcoded values
- [ ] **Documentation**: All setup instructions use placeholders like `[YOUR-RESOURCE-NAME]`

#### 💡 **Trust & Verify Principle**
- **Never trust initial security assessments** - always push for comprehensive review
- **Verify functionality after security changes** - ensure application still builds and runs
- **Test both development and production configurations** after security modifications
- **Document what was actually verified** - be specific about testing scope

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
**CRITICAL: Stick to requirements - DO NOT INVENT THINGS:**
- Implement **ONLY** explicitly requested functionality and logic
- **NEVER** assume, infer, or add "improvements" unless explicitly asked
- **NEVER** add extra features, UI enhancements, or "nice-to-have" functionality
- **NEVER** assume user wants optimizations, refactoring, or architectural changes
- **NEVER** add loading states, animations, or visual improvements unless requested
- Creativity is limited to accomplishing what's requested, not expanding requirements
- When in doubt, ask for clarification rather than assume
- **REMEMBER**: User feedback like "don't add things we don't ask for" means STOP INVENTING

### 📝 Documentation & Comments Standards (MANDATORY)

**CRITICAL: ALL new code MUST be professionally commented - no exceptions**

#### 🎯 XML Documentation (Required)
- **ALL new methods** (public and private) must have comprehensive XML documentation
- **Include purpose, parameters, returns, and remarks** when applicable
- **Document edge cases** and special behavior
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

#### 🚫 Avoid These Comment Patterns
- Development artifacts like "ADD THIS", "NUEVAS", "TODO"
- Shallow comments that just repeat the code
- Spanish comments (replace with English)
- Comments without business justification or technical value

#### 📋 Documentation Requirements
- **English only** for international collaboration
- **Professional tone** - avoid casual or development-only comments
- **Business-focused** - explain impact and purpose
- **Maintainable** - help future developers understand decisions
- **Consistent formatting** - follow established patterns in codebase
- **Structured logging** - use `_logger.LogInformation("English message")` instead of console output

---


### Director Suggestion Sequencing (2025-07-24)
- DirectorReshuffle implements intelligent sequencing with case-insensitive deduplication
- Priority order: recent director → frequent director → top-rated director → random
- Deduplication prevents the same director appearing twice when they qualify for multiple categories
- Example: If Steven Spielberg is both "recent" and "frequent", he only appears once in the sequence
- Session state tracks sequence position per user, advancing with each reshuffle
- Random phase includes anti-repetition to avoid immediately repeating the last random selection
- The deduplication approach is elegant: solve at data level (HashSet) rather than logic level (skip patterns)

### Dynamic Variety and Consistency Improvements
- **Dynamic Variety System**: Decade suggestions now use randomized sort criteria (`popularity.desc`, `vote_average.desc`, `release_date.desc`) and page (1-3) for every suggestion, matching the genre system.
- **Triple Fallback Logic**: Robust fallback system for each decade:
  - Primary: Random sort + random page combination
  - Fallback 1: Same sort, page 1 (if original page insufficient)
  - Fallback 2: Popular, page 1 (ultimate safety net)
- **Helper Method**: `TryGetDecadeMovies` provides consistent error handling, user filtering, and fallback logic.
- **Consistency**: Both initial load and AJAX reshuffles use identical dynamic variety logic, ensuring a unified user experience.
- **Performance**: Maintains ~1-2 TMDB API calls per user interaction, with early exit optimization and 24-hour caching per sort+page+decade combo.
- **User Filtering**: Blacklist and watched movies are filtered consistently, with all expensive operations cached outside evaluation loops.
- **User Experience**: Decade suggestions now provide varied, reliable content from the very first click, with bulletproof fallback for edge cases.

- **Deduplication**: Prevents same decade appearing multiple times in results

### Code Patterns for DecadeReshuffle
- Always use `last25Movies` for decade calculations.
- Use dynamic sort and page parameters for every suggestion, both initial and reshuffle.
- Apply triple fallback logic for reliability.
- Use `TryGetDecadeMovies` for all decade movie retrievals.
- Maintain session state for anti-repetition (immediate only).
- Cache blacklist and recent movies once per request.

### Consistency Achievement
- Decade and genre suggestion systems now share a unified dynamic variety and fallback approach, providing a consistent and reliable user experience across all suggestion types.

**Edge Case: Mass Trending Blacklisting**
- If a user attempts to blacklist the majority or all trending movies (e.g., more than 20 in a single session), they may find that the last few titles cannot be blacklisted due to state, cache, or validation limitations.
- This is an extreme and unrealistic edge case in normal app usage. Most users will never attempt to blacklist so many trending movies at once.
- Decision was made not to prioritize solving this edge case to optimize development time and focus on workflows that impact the majority of users.
- The system already shows a friendly message when no more trending suggestions are available, so the user is never "trapped".
## UI/UX Patterns
- Mutual exclusion implemented preventively via conditional rendering
- Error states avoided through visual state management
- Consistent behavior across Details and Preview pages

---
**Best Practice:**
> For all AJAX POST requests (especially for Blacklist/Wishlist removal), **always include** the `X-Requested-With: XMLHttpRequest` header. This guarantees the backend returns JSON for AJAX, not HTML error pages, and prevents frontend parsing errors. This is required for robust, user-friendly error handling in all AJAX-powered UI actions.
---
### Code Comment Standards (Legacy Note)
- Comments like "ADD THIS", "NUEVAS", or importance notes without technical justification are not permitted.
- Validation attributes and functional logic remain intact.
- Future model changes must follow established documentation standards.

## Movie Preview Card (Add/Edit Movie) - Style and Maintenance Notes (2025-07-15)

- Uses ultra-specific CSS selector for movie preview card to ensure custom styles override Bootstrap and Cyborg theme.
- Do not reduce selector specificity or change HTML structure/classes of the card without reviewing styles, as this may break visuals.
- Colors, typographic hierarchy, and hover effects are documented in the `site.css` header.
- Overview now displays complete text without scrollbar and with justified text for better readability.
- All visual and UX improvements are documented in `CHANGELOG.md`.
- After any visual changes, test the card across all browsers and devices to ensure consistency.

# Copilot Instructions for CineLog-AI-Experiments

## Project Overview

**Latest Update (2025-08-03): Azure Cloud Integration Complete**
CineLog has achieved **9.5/10 production readiness** with full Azure cloud infrastructure:
- ✅ **Azure SQL Database**: Production database "[YOUR-DATABASE]" on server "cinelog-sql-server" 
- ✅ **Azure Key Vault**: Secure secret management with "[YOUR-KEYVAULT]" Key Vault using DefaultAzureCredential
- ✅ **Connection Resilience**: Retry policies (3 attempts, 10s delay) and 60s timeouts for Azure SQL
- ✅ **Zero Hardcoded Secrets**: All sensitive data managed through Azure Key Vault
- ✅ **Environment-Specific Config**: Clean separation between local development and Azure production
- ✅ **Migration Success**: All 25 EF Core migrations successfully applied to Azure SQL Database
- ✅ **Production Testing**: Application verified working with Azure infrastructure

## 2025-07-27+ Performance, Architecture, and UX Standards

### Performance & Architecture
- **Azure SQL Database**: Production database with connection resilience, retry policies, and 60s timeouts
- **Azure Key Vault**: Enterprise secret management for database passwords and API tokens
- Always use batch API calls (e.g., `GetMultipleMovieDetailsAsync`) for Blacklist and Wishlist to avoid N+1 query problems. Never call TMDB for each movie individually.
- Use the centralized `CacheService` for user-specific data (blacklist/wishlist IDs), with 15-minute expiration and automatic invalidation on add/remove.
- Pagination for Blacklist and Wishlist must be 20 items per page, preserving search/sort across pages.
- All expensive filtering (blacklist, watched, deduplication) must be cached per request or per pool build, not per reshuffle.
- All TMDB movie details are cached for 24 hours in IMemoryCache.

### Suggestion System
- All suggestion types (Trending, Director, Genre, Cast, Decade, Surprise Me) must use unified helper methods for filtering, pool building, and randomization.
- Both initial loads and AJAX reshuffles must use the same business logic and dynamic variety systems (random sort/page, triple fallback, deduplication).
- Genre and Decade suggestions always use randomized sort criteria (`popularity.desc`, `vote_average.desc`, `release_date.desc`) and page (1-3), with triple fallback logic.
- Director and Cast suggestions use deduplicated, session-tracked priority queues and anti-repetition logic.
- "Surprise Me" uses a static, deduplicated pool (50–80 movies), built in parallel, cached for 2 hours, with cyclic rotation and anti-repetition.

### UI/UX
- All suggestion section titles use `.cinelog-gold-title` for Cinema Gold color.
- Suggestion card titles and descriptions are 1pt larger for readability.
- All AJAX-powered UI actions use event delegation and server-rendered HTML for consistency.
- Pagination controls must be consistent across Blacklist and Wishlist.

### Code Quality & Documentation
- All controller and service comments must be in English, business-logic-focused, and use XML documentation for public methods.
- All logging uses structured `_logger` calls.
- All mutual exclusion logic (wishlist/blacklist) must be enforced both in backend and UI, with clear visual feedback.
- Maintain the new commenting/documentation standards for all future contributions.

## Key Architectural Patterns
- **Separation of Concerns:**
  - Controllers handle HTTP and user logic.
  - Data models/entities are in `Models/` and `Ezequiel_Movies1.Models.Entities`.
  - External API logic is in `TmdbService.cs`.
- **User-specific Data:**
  - All movie and wishlist actions are filtered by the current user's ID (via ASP.NET Identity).
  - Example: `MoviesController` always queries with `.Where(m => m.UserId == userId)`.
- **Suggestion System:**
  - Movie suggestions are generated based on user's logged data (directors, genres, actors, decades).
  - Helper methods like `GetSuggestionsForDirector`, `GetSuggestionsForGenre`, etc., encapsulate this logic.

## Developer Workflows
- **Build:**
  - Use `dotnet build` in the project root.
- **Run:**
  - Use `dotnet run` or launch via Visual Studio/VS Code.
- **Migrations:**
  - Use `dotnet ef migrations add <Name>` and `dotnet ef database update` for schema changes.
- **Debugging:**
  - Console and logger output is used extensively in controllers for tracing data flow and debugging.

## Project Conventions
## Commenting & Documentation Standards
- All controller comments (especially in `MoviesController.cs`) now follow a professional, business-logic-focused style.
- Development artifacts, redundant, and shallow comments have been removed.
- Comments should explain "why" for business rules, suggestion systems, and session/anti-repetition logic.
- Future contributions must maintain this standard: avoid obsolete, non-English, or low-value comments.
- See `CHANGELOG.md` (2025-07-17) for details on the latest comment refactor.
- **Model Validation:**
  - Uses custom attributes (e.g., `NoFutureDateAttribute`, `ValidReleasedYearAttribute`) for model validation.
- **Session Usage:**
  - Session is used to store temporary state for suggestions (e.g., `ShownSurpriseIds`).
- **ViewModels:**
  - `AddMoviesViewModel` is used for add/edit forms, mapping to/from entity models.
- **TMDB Integration:**
  - All TMDB lookups and suggestions go through `TmdbService.cs`.
  - Example: `await _tmdbService.GetMovieDetailsAsync(tmdbId)`.

  **UI & Styling:** The project uses **Bootstrap 5** and the dark **'Cyborg' Bootswatch theme**. New UI elements should match this style, using standard Bootstrap classes (card, btn, list-group, etc.).
  - The "Sort By" dropdown on the "My Movies" page must always be visible, with a static grey background (`#6c757d`), white text, and no hover/focus effects. Use inline styles or a dedicated CSS class to ensure consistent appearance.

## UI/UX Consistency
- Always verify that UI changes are visually correct, accessible, and consistent with the app's design system.
- The "Sort By" dropdown must remain always visible and styled as described above.

## Code Efficiency & Quality
- Regularly review for DRYness, async/await usage, null safety, and error handling.
- Optimize database queries and TMDB API calls to avoid unnecessary work (e.g., use helpers, avoid redundant loops, and consider caching for repeated API data).
- Ensure all user data queries are filtered by UserId for security and privacy.
- Document any new architectural or UI conventions in this file after major changes.

## Notable Files & Directories
- `Controllers/MoviesController.cs`: Main controller for movie logic and suggestions.
- `Models/`: Contains view models, TMDB models, and validation attributes.
- `Data/ApplicationDbContext.cs`: Entity Framework Core DB context.
- `TmdbService.cs`: Handles all TMDB API calls.
- `Migrations/`: Entity Framework migration files.

## Patterns & Examples
- **User Filtering:**
  ```csharp
  var userId = _userManager.GetUserId(User);
  var movies = _dbContext.Movies.Where(m => m.UserId == userId);
  ```
- **TMDB API Usage:**
  ```csharp
  var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
  ```
- **Suggestion Helper:**
  ```csharp
  private async Task<List<TmdbMovieBrief>> GetSuggestionsForDirector(string directorName) { ... }
  ```

## Integration Points
- **TMDB API:** All external movie data and suggestions are fetched via TMDB.
- **ASP.NET Identity:** Used for user authentication and per-user data isolation.
- **Data Mapping:** Data from the TMDB API is mapped to C# classes in the 'Models/TmdbApi/' folder, such as TmdbMovieDetails and TmdbMovieBrief.

---

## Business Rules

### Mutual Exclusion Policy
- A movie cannot exist in both wishlist and blacklist for the same user
- When adding to one list, check if movie exists in the other
- Provide clear error messages guiding user to resolve conflicts

### UI/UX Principles
- Always provide a way to reshuffle suggestions, even if no results available
- Use AJAX for non-navigational actions (add/remove from lists)
- Maintain visual feedback for all user interactions
- Preserve user's place in suggestion flow during list management

### Suggestion System Behavior
- Implement bulletproof fallbacks for edge cases
- **Enhanced Cast Suggestion Logic (2025-07-31)**: Cast reshuffle now implements robust sequencing that automatically skips actors with no available movie suggestions in all categories (recent, frequent, rated, random). The sequence always advances through: recent → frequent → rated → random → random...
- **No Empty Actor Messages**: Users will never see "no suggestions for this actor" message; only valid suggestions are shown with seamless progression.
- **Edge Case Handling**: If no actors have suggestions, a generic message is displayed (extremely rare scenario).
- Track session state to avoid repetitive suggestions (Session sequencing is used for cast suggestions to maintain proper rotation through actor categories).
- Handle empty result sets gracefully with actionable next steps
- The "Reshuffle" button is implemented via event delegation and always maintains the correct context for all suggestion types.
- IMemoryCache is used for TMDB API data; Session State is used for user-specific anti-repetition and sequencing (including cast suggestion rotation).

### Genre Suggestion Variety System (2025-07-24)
- GenreReshuffle implements dynamic content variety through randomized sort criteria and pagination
- **Random Parameters**: Each reshuffle uses random combination of sort type (popular/top-rated/latest) and page (1-3)
- **Quality Filtering**: All suggestions filtered to 6.5+ rating with minimum vote counts for reliability
- **Triple Fallback System**: Primary sort+page → Same sort, page 1 → Popular, page 1 (never shows empty results)
- **Genre Sequencing**: Maintains intelligent sequence (recent → frequent → rated → random) while varying content within each genre
- **User Filtering**: Excludes user's watched movies, wishlist items, and blacklisted content
- **Performance**: Same API usage as previous system but with significantly improved content variety
- **User Experience**: Consistent "Because you watched [GENRE] movies" titles regardless of underlying sort criteria

---
### 2025-07-24 Genre Suggestion Consistency Fix

Initial genre suggestions now use the same dynamic variety system as AJAX reshuffles
Both initial load and reshuffles generate random sort criteria (popularity.desc, vote_average.desc, release_date.desc) and page (1-3)
Unified title format: "Because you watched [GENRE] movies" for both initial and reshuffles
Session state is reset on fresh start to ensure correct sequence
User experience is now consistent and varied from the very first click
No impact on caching or performance optimizations

# 2025-07-25 Trending Suggestion Unification

- **Unified Trending Logic**: Both initial `ShowSuggestions` and AJAX `TrendingReshuffle` now use the same helper method `GetTrendingMoviesWithFiltering()`
- **Consistent Filtering**: Same blacklist and recent movie exclusion logic across both endpoints
- **Consistent Pool Building**: Same 30-movie pool generation from up to 5 TMDB pages
- **Consistent Randomization**: Same shuffling algorithm for variety in both flows
- **Code Maintenance**: Single source of truth for trending movie logic, eliminating duplication
- **Performance**: Consistent caching behavior using TMDB service's built-in 90-minute cache

## Trending Suggestion Pattern
When implementing trending suggestions, always use the unified helper method:
```csharp
// For both initial and AJAX suggestions
var trendingResult = await GetTrendingMoviesWithFiltering(userId);
var suggestedMovies = trendingResult.Take(3).ToList();
```

## Comment and Documentation Standards (Updated 2025-07-25)

### Code Comments in English
- All new code comments must be written in English for international collaboration
- Use XML documentation (`///`) for public methods with clear business purpose explanations  
- Inline comments should explain "why" not "what" - focus on business logic and decision rationale
- Remove any Spanish comments and replace with English equivalents
- Use structured logging with English messages: `_logger.LogInformation("English message here")`

### Method Documentation Pattern
```csharp
/// <summary>
/// [Brief description of what the method does]
/// </summary>
/// <param name="paramName">Description of parameter purpose</param>
/// <returns>Description of return value and when it's used</returns>
/// <remarks>
/// Business rationale, performance notes, or usage patterns.
/// </remarks>
```

## Unified Logic Pattern (2025-07-25)

When creating suggestion endpoints that have both initial and AJAX variants:

### 1. Create Shared Helper Method
```csharp
private async Task<List<TmdbMovieBrief>> Get[Type]MoviesWithFiltering(string userId)
{
  // Unified filtering and pool building logic
  // Include user blacklist, recent movies, and other filters
  // Return consistent, shuffled results
}
```

### 2. Use Helper in Both Endpoints
```csharp
// In ShowSuggestions switch case:
case "trending":
  var result = await GetTrendingMoviesWithFiltering(userId);
  suggestedMovies = result.Take(3).ToList();
  break;

// In AJAX endpoint:
public async Task<IActionResult> TrendingReshuffle()
{
  var moviePool = await GetTrendingMoviesWithFiltering(userId);
  var suggestedMovies = moviePool.Take(3).ToList();
  // ... render and return JSON
}
```

### 3. Benefits of This Pattern
- **Single Source of Truth**: One method contains all business logic
- **Consistency**: Same filtering, caching, and randomization across endpoints  
- **Maintainability**: Changes only need to be made in one place
- **Testing**: Easier to unit test the core logic separately

# (Business Rules update)
- Genre suggestion system always uses dynamic variety logic, regardless of initial load or reshuffle
- Consistent user-facing behavior and titles for all genre suggestions

---

## AJAX & Hybrid Suggestion Implementation (2025-07-18)

- El sistema de sugerencias ahora implementa un patrón híbrido:
  - Para el tipo "Trending", el reshuffle se realiza vía AJAX y el endpoint devuelve HTML renderizado del servidor (partial views), no JSON puro.
  - Esto garantiza que los posters y paths de imágenes funcionen correctamente, ya que el renderizado server-side respeta la lógica de rutas y helpers de ASP.NET MVC.
  - El resto de tipos de sugerencia siguen usando navegación tradicional, pero el patrón es extensible a más tipos si se desea AJAXizar.
- Justificación técnica:
  - El renderizado HTML server-side evita problemas de rutas relativas/absolutas y CORS con imágenes de TMDB.
  - Permite reutilizar la misma partial view que en el render inicial, manteniendo DRY y consistencia visual.
  - Facilita el mantenimiento y la extensión futura del sistema de sugerencias.
- Notas de implementación:
  - El botón de reshuffle para trending usa data-suggestion-type y event delegation en JS para disparar el fetch AJAX.
  - Tras reemplazar el grid de sugerencias, siempre se re-adjuntan los event listeners para mantener la funcionalidad AJAX de los formularios internos.
  - Los comentarios en C# y JS deben documentar el propósito, el porqué del enfoque y las mejores prácticas de mantenimiento.
  - Ver ejemplos y convenciones en `MoviesController.cs` y `Views/Movies/Suggest.cshtml`.

## AJAX Implementation Notes

### Anti-Forgery Protection
- All AJAX requests include anti-forgery tokens via Razor helpers
- Use `@Html.AntiForgeryToken()` in forms submitted via JavaScript

### User Security
- All POST actions are filtered by `UserId` to ensure data isolation
- Authorization attributes protect all user-specific operations
- No user data exposure across accounts

### Error Handling
- AJAX operations include comprehensive error handling
- Failed operations provide user feedback and restore UI state
- Network errors are handled gracefully with retry options

---

For new features, always:
- Use session sequencing only on the initial suggestion click; trust client parameters for all reshuffles.
- Implement AJAX-powered UI actions using event delegation for all dynamic elements.
- Use IMemoryCache for API data and Session State for user-specific anti-repetition and sequencing.
- Follow the established patterns in `MoviesController.cs` and use `TmdbService` for all TMDB interactions.
- Filter all user data queries by user ID for privacy and correctness.

---

For new features, follow the established patterns in `MoviesController.cs` and use `TmdbService` for all TMDB interactions. Always filter data by user ID for privacy and correctness.

### Surprise Me System (2025-01-26 Major Optimization)
- Pool size reduced from 80 to 50 movies, matching real user interaction patterns
- All pool queries are now constructed and executed in parallel (up to 15 concurrent calls, throttled)
- Build time reduced from ~2,800ms to ~400-450ms (85% faster)
- Anti-repetition system tracks 3 previous pool rotations (6-hour windows) for better variety
- After initial build, all suggestions are instant (zero API calls per reshuffle)
- API usage: 15 parallel calls per build (was 25+ sequential)
- System is robust to TMDB rate limits and supports high concurrency
- The old 4-cycle system is fully replaced by this unified, scalable approach

---

## 🤖 Enhanced Claude Code Agent System (2025-08-08 Update)

### 🎭 Master Agent Director with Critical Workflow Integration
The project includes an intelligent **Master Agent Director** enhanced with the new critical workflow system:

**Intelligence Framework (Enhanced):**
- **Task Analysis Engine**: Parses requests, analyzes complexity, detects domains
- **Complexity Assessment**: Automatically classifies tasks as Simple/Medium/Complex/Strategic
- **Critical Workflow Integration**: Mandatory SESSION_NOTES.md reading as first action
- **6-Step Systematic Pattern**: Built-in systematic approach for all complex tasks
- **Strategic Planning**: Auto-triggered planning with TodoWrite integration
- **Multi-Agent Orchestration**: Coordinates sequential and parallel agent workflows
- **Session Continuity**: Automatic context preservation across sessions

**Complexity-Based Routing (Enhanced):**
- **Simple Tasks** (bug fixes) → SESSION_NOTES.md check → Direct execution to specialist agent
- **Medium Tasks** (enhancements) → SESSION_NOTES.md check → Light planning → Execute
- **Complex Tasks** (new features) → Critical workflow → Strategic planning → Multi-agent execution
- **Strategic Tasks** (major changes) → Full workflow → Deep planning → Phased execution

### 🎬 Core CineLog Subagents

#### `cinelog-movie-specialist`
**Domain Expert** for movie-specific features and suggestion algorithms:
- MoviesController patterns and CRUD operations
- Unified helper methods for consistent business logic across initial/AJAX calls
- Triple fallback systems ensuring suggestions are never empty
- Session-based anti-repetition and sequencing logic
- Dynamic variety systems with randomized sort criteria and pagination
- Proactive director filtering to prevent empty suggestion states
- Mutual exclusion logic (movies cannot be in both wishlist and blacklist)

#### `tmdb-api-expert`
**External API Integration Specialist**:
- TmdbService architecture with 24-hour caching
- Batch operations using `GetMultipleMovieDetailsAsync()` to avoid N+1 queries
- Parallel execution for pool building (up to 15 concurrent calls)
- Rate limiting with SemaphoreSlim and error handling for API failures
- Data mapping between TMDB API responses and CineLog models

#### `performance-optimizer`
**Performance & Caching Specialist**:
- IMemoryCache optimization for TMDB data (24-hour expiration)
- CacheService for user-specific data (15-minute expiration)
- Batch processing patterns to eliminate N+1 query problems
- Parallel execution strategies (85% performance improvement)
- Database query optimization with proper indexing

#### `aspnet-feature-developer`
**Full-Stack Development Specialist**:
- ASP.NET Core MVC patterns with Bootstrap 5 (Cyborg theme)
- AJAX + Server-Side Rendering hybrid architecture
- Event delegation for dynamic UI elements
- Progressive enhancement (works without JavaScript)
- Cinema Gold branding with `.cinelog-gold-title` classes
- User data isolation patterns with UserId filtering

### 🚀 Enhanced Development Subagents

#### `test-writer-fixer` (Proactive)
**Comprehensive Testing Specialist** - Auto-triggers after code changes:
- Unit, integration, and end-to-end testing for ASP.NET Core
- Movie-specific test scenarios (suggestions, CRUD, API integration)
- Test failure analysis and repair without compromising test intent

#### `ui-designer` (Proactive)
**Visual Design Enhancement Specialist** - Auto-triggers after UI updates:
- Movie-centric UI component design beyond Bootstrap
- Cinema Gold branding implementation and design systems
- Responsive design patterns for movie discovery interfaces
- Screenshot-worthy design moments for social sharing

#### `whimsy-injector` (Proactive)
**User Engagement Specialist** - Auto-triggers after UI/UX changes:
- Movie discovery and logging micro-interactions
- Achievement celebrations and playful animations
- Personality-filled copy and error states
- Shareable moments that encourage user evangelism

#### `backend-architect`
**Scalable Architecture Specialist**:
- ASP.NET Core architecture patterns and scalability design
- Database optimization and performance architecture
- API design with proper authentication and rate limiting
- System architecture for movie data management at scale

#### `performance-benchmarker`
**Performance Testing Specialist**:
- TMDB API integration performance and rate limiting testing
- Suggestion system performance profiling and optimization
- Database query performance analysis and recommendations
- Frontend rendering optimization for movie-rich interfaces

### 🎯 Agent Usage Patterns

**Automatic Delegation:**
```
"Add movie feature X" → aspnet-feature-developer + test-writer-fixer + ui-designer
"Fix suggestion bug" → cinelog-movie-specialist + test-writer-fixer
"Optimize performance" → performance-optimizer + performance-benchmarker
"TMDB API issue" → tmdb-api-expert + api-tester
"Deploy to production" → deployment-project-manager + multi-agent coordination
"Plan production deployment" → deployment-project-manager
"Choose hosting platform" → deployment-project-manager
```

**Proactive Invocation:**
```
Code changes made → test-writer-fixer (ensures test coverage)
UI/feature updates → ui-designer (enhances visual appeal)
UI/UX changes → whimsy-injector (adds personality and delight)
```

#### `deployment-project-manager`
**Strategic production deployment coordination and educational guidance**:
- **Strategic Decision Making**: Infrastructure sizing, platform selection (Azure/AWS), technology stack recommendations with cost optimization
- **Educational Guidance**: Patient explanations of complex deployment concepts with clear decision rationale and best practices
- **Cross-Agent Coordination**: Orchestrates deployment phases across all specialized agents with risk management and emergency response
- **Production Architecture**: Distributed caching (Redis), session state management, security configuration, and performance monitoring
- **Infrastructure Design**: Load balancing, monitoring setup (APM), backup/recovery strategies, and scalability planning
- **Deployment Phases**: Foundation setup → Performance infrastructure → Production deployment → Optimization & monitoring

### 📊 Enhanced Development Benefits (2025-08-08)
- **Critical Workflow System**: Mandatory systematic approach addresses previous workflow issues
- **Session Continuity**: SESSION_NOTES.md ensures context preservation across sessions
- **Systematic Progress**: 6-step pattern prevents incomplete implementations
- **Agent Utilization**: Enhanced decision tree ensures optimal agent selection
- **Compliance Verification**: Built-in verification against CLAUDE.md golden rules
- **Professional Standards**: Mandatory commenting and build verification
- **Intelligent Orchestration**: Master Director routes tasks to optimal agents automatically
- **Proactive Quality**: Automatic testing, UI enhancement, and delight injection
- **Strategic Planning**: Complex features receive proper planning before implementation
- **Comprehensive Testing**: Built-in test coverage ensures robust, reliable features
- **Enhanced User Experience**: Automatic UI enhancement and personality injection
- **Performance Excellence**: Built-in performance analysis and optimization recommendations
- **Production Deployment Expertise**: Strategic deployment guidance with educational approach and cross-agent coordination

### 🔑 Key Principles for Agent Coordination (Enhanced)
- **Critical Workflow Compliance**: All agents follow mandatory 6-step systematic pattern
- **Session Context Awareness**: Every agent starts with SESSION_NOTES.md reading
- **Build Verification**: All agents verify compilation before task completion
- **Professional Standards**: Mandatory commenting and documentation standards
- **Domain Expertise**: Each agent has deep knowledge of specific CineLog patterns
- **Consistency**: All agents follow the same architectural conventions and coding standards
- **Quality First**: Built-in quality gates and performance considerations
- **User-Centric**: Every feature considers the complete user experience
- **Performance-Aware**: All implementations consider scalability and optimization

---

## 🎯 Agent Invocation & Coordination Guide

### 📋 **Explicit Agent Invocation Guidance**

| User Request Pattern | Primary Agent(s) | Rationale |
|---------------------|------------------|-----------|
| "Add movie feature X" | `aspnet-feature-developer` → `test-writer-fixer` → `ui-designer` | Full-stack development with testing and UI enhancement |
| "Fix suggestion bug" | `cinelog-movie-specialist` → `test-writer-fixer` | Domain expertise + test coverage |
| "Make the app faster" | `performance-benchmarker` → `performance-optimizer` | Analysis first, then optimization |
| "Database changes needed" | `ef-migration-manager` → `backend-architect` | Schema changes + architecture review |
| "TMDB API not working" | `tmdb-api-expert` → `api-tester` | Integration expertise + reliability testing |
| "Code is messy/complex" | `code-refactoring-specialist` → `test-writer-fixer` | Refactoring + maintained functionality |
| "UI needs improvement" | `ui-designer` → `whimsy-injector` | Visual design + personality injection |
| "Tests are failing" | `test-writer-fixer` + Domain expert | Fix tests + address root cause |
| "Deploy to production" | `deployment-project-manager` → Multi-agent coordination | Strategic deployment planning + execution |
| "Plan production deployment" | `deployment-project-manager` | Infrastructure decisions + educational guidance |
| "Choose hosting platform" | `deployment-project-manager` | Platform selection with cost/complexity analysis |
| "Users complaining about X" | `feedback-synthesizer` → Relevant domain agent | Analyze feedback + implement solution |

### 🔄 **Simple Planning for Complex Tasks**

For complex tasks only, quickly think through:
- **What needs to be done?** (objective)
- **Which agent(s)?** (primary + follow-up)  
- **What could break?** (risks)

Keep it simple - no need for formal templates unless the task truly spans multiple domains.

### ⚡ **Agent Escalation/Delegation Rules**

**CRITICAL RULE**: If a task is ambiguous or spans multiple domains, **always escalate to the Master Agent Director** for orchestration and planning.

**Escalation Triggers**:
- Task affects 3+ architectural components
- Requirements are unclear or conflicting
- Multiple domain expertise needed simultaneously
- Risk of breaking existing functionality
- User mentions "comprehensive" or "major" changes

**Example**: *"Redesign the entire suggestion system"* → **Master Agent Director** (Strategic planning required)

### 🎭 **Multi-Agent Coordination Example**

**User Prompt**: *"Add a movie rating system with stars, save to database, and make it look good"*

**Master Agent Director Analysis**:
```
🎯 DOMAINS: Database, Backend, Frontend, UI/UX
⚡ COMPLEXITY: COMPLEX → Strategic planning activated
🚀 AGENTS: Sequential multi-agent workflow
```

**Execution Sequence**:
1. **`backend-architect`**: Design rating system schema and API structure
2. **`ef-migration-manager`**: Create database migration for ratings table
3. **`aspnet-feature-developer`**: Implement MVC components and rating logic
4. **`ui-designer`**: Create star rating component with Bootstrap integration
5. **`whimsy-injector`**: Add hover effects and rating submission animations
6. **`test-writer-fixer`**: Write comprehensive tests for rating functionality
7. **`docs-architect`**: Update documentation with new rating feature

**Coordination Benefits**: Each agent builds on previous work, ensuring cohesive implementation.

### 📚 **Documentation Updates**

Update docs when you introduce new patterns or fix significant bugs. Key files: `CLAUDE.md`, `README.md`, `CHANGELOG.md`.

### 🚨 **Error Handling**

Always be specific and actionable: "TMDB API rate limited - try again in 60 seconds" not "API error".

### ✏️ **Documentation Edits**

Don't remove working patterns or guidance unless explicitly asked. Add and enhance, don't delete.

---

## 🔍 CineLog Development Knowledge Base

*Quick reference for GitHub Copilot to access specialized knowledge when working on specific CineLog components*

### 🎬 Movie Suggestions **[WHEN: MoviesController, suggestion algorithms, AJAX reshuffles, empty states]**

#### **Core Patterns:**
```csharp
// UNIFIED HELPER METHOD PATTERN - Use for all suggestion types
private async Task<List<TmdbMovieBrief>> Get[Type]MoviesWithFiltering(string userId)
{
    // Get user blacklist and recent movies for filtering (cached per request)
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    var last25Movies = await GetLast25MoviesAsync(userId); // Cache this call
    
    // Build movie pool with variety and pagination
    // Apply deduplication using HashSet<string> for TMDB IDs
    // Return consistent results for both initial and AJAX calls
}

// USER DATA ISOLATION (CRITICAL) - Always filter by UserId
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);

// TRIPLE FALLBACK SYSTEM - Ensure suggestions are never empty
// Primary: Dynamic variety (random sort + random page)
// Fallback 1: Same sort, page 1
// Fallback 2: Popular, page 1 (ultimate safety net)
```

#### **Common Problems → Solutions:**

**Problem:** "No suggestions available for [Director]"
```csharp
// FIX: Proactive director filtering before suggestion
private async Task<bool> HasAvailableMoviesForDirector(string directorName, string userId)
{
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    // Lightweight check without fetching full movie details
    // Only include directors with at least one non-blacklisted movie
}
```

**Problem:** Repetitive suggestions / No variety
```csharp
// SOLUTION: Dynamic variety system
var randomSort = new[] { "popularity.desc", "vote_average.desc", "release_date.desc" }
    .OrderBy(x => Guid.NewGuid()).First();
var randomPage = Random.Shared.Next(1, 4);

// Use different parameters for each suggestion
var movies = await _tmdbService.GetMoviesByGenreAsync(genreId, randomSort, randomPage);
```

**Problem:** Session anti-repetition not working
```csharp
// SOLUTION: Session-based sequencing
string sequenceKey = $"DirectorTypeSequence_{userId}";
var currentStep = HttpContext.Session.GetInt32(sequenceKey) ?? 0;
HttpContext.Session.SetInt32(sequenceKey, (currentStep + 1) % 4);

// Sequencing: Recent → Frequent → Top-rated → Random
```

#### **Suggestion Type Specifics:**

**Trending:** Pool of 30 movies from multiple TMDB pages, 90-minute cache, exclude last 5 watched
**Director/Cast:** Sequenced rotation with case-insensitive deduplication, session tracking
**Genre/Decade:** Dynamic variety with 6.5+ rating filter, randomized sort/page, triple fallback
**Surprise Me:** 50-movie deduplicated pool, parallel build (15 concurrent), 2-hour cache, instant reshuffles

#### **AJAX Implementation:**
```csharp
// Return server-rendered HTML, not JSON
return PartialView("_MovieSuggestionCard", suggestedMovies);

// Event delegation pattern in JavaScript
document.addEventListener('click', function(e) {
    if (e.target.matches('[data-suggestion-type]')) {
        // Handle all reshuffle buttons dynamically
    }
});
```

#### **NEW: AJAX Suggestion Cards (2025-07-30)**
**Complete AJAX implementation for suggestion card navigation without page reloads:**

```html
<!-- Convert anchor tags to buttons with data attributes -->
<button type="button" class="suggestion-card card h-100 text-decoration-none border-0 bg-transparent w-100" 
        data-suggestion-type="trending">
    <div class="card-body text-center">
        <h5 class="card-title">Trending Movies</h5>
    </div>
</button>
```

```javascript
/*
 * FEATURE: Loads movie suggestions via AJAX without page reload.
 * Maintains identical functionality to traditional navigation while providing seamless UX.
 * Includes graceful fallback to regular navigation if AJAX fails.
 */
function loadSuggestions(suggestionType) {
    // ARCHITECTURE: Route to appropriate controller endpoint
    let endpoint = suggestionType === 'surprise_me' ? 
        '/Movies/GetSurpriseSuggestion' : '/Movies/ShowSuggestions';
    
    fetch(endpoint, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' }  // Critical for backend detection
    })
    .then(response => response.json())
    .then(data => {
        // ARCHITECTURE: Insert server-rendered HTML for styling consistency
        container.insertAdjacentHTML('beforeend', data.html);
        attachEventListeners(); // CRITICAL: Re-attach for new content
    })
    .catch(error => {
        // RELIABILITY: Graceful fallback to traditional navigation
        window.location.href = url;
    });
}
```

```csharp
/// <summary>
/// Renders the suggestion results area HTML for AJAX responses.
/// 
/// FEATURE: Added comprehensive AJAX support for suggestion cards to eliminate page reloads
/// while preserving exact original functionality and visual appearance.
/// </summary>
private async Task<string> RenderSuggestionResultsHtml(List<TmdbMovieBrief> suggestedMovies, string suggestionTitle, string nextSuggestionType, string? nextQuery)
{
    // CRITICAL: Populate movie properties (IsWatched, IsInWishlist, IsInBlacklist)
    await PopulateMovieProperties(suggestedMovies, userId!);
    
    // ARCHITECTURE: Use server-side partial view rendering for consistency
    foreach (var movie in suggestedMovies) {
        var partialViewResult = await RenderPartialViewToStringAsync("_MovieSuggestionCard", movie);
        html.Append($"<div class=\"col\">{partialViewResult}</div>");
    }
}

// FEATURE: Handle AJAX requests for suggestion card clicks
if (Request.Headers.ContainsKey("X-Requested-With") && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
{
    var html = await RenderSuggestionResultsHtml(suggestedMovies, suggestionTitle, nextSuggestionType, nextQuery);
    return Json(new { success = true, html = html });
}
```

**Key Principles:**
- **Preserve Original Functionality**: AJAX version works identically to traditional navigation
- **Server-Side Rendering**: Use existing partial views for consistent styling and image paths
- **State Preservation**: PopulateMovieProperties ensures watched badges, wishlist states remain visible
- **Graceful Fallback**: Automatic fallback to regular navigation if AJAX fails
- **No Visual Changes**: NO loading states, animations, or UI enhancements unless requested

### 🌐 TMDB API Integration **[WHEN: External API calls, rate limiting, caching, data mapping]**

#### **Core Patterns:**
```csharp
// CENTRALIZED SERVICE USAGE - Always use TmdbService
var movieDetails = await _tmdbService.GetMovieDetailsAsync(tmdbId);
var searchResults = await _tmdbService.SearchMoviesAsync(query);

// BATCH OPERATIONS - Avoid N+1 queries
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

// PARALLEL EXECUTION - For pool building (Surprise Me)
var poolTasks = buckets.Select(async bucket => 
    await _tmdbService.GetMoviesAsync(bucket.endpoint)).ToArray();
var results = await Task.WhenAll(poolTasks);

// RATE LIMITING - SemaphoreSlim throttling
private readonly SemaphoreSlim _semaphore = new(6, 6); // Max 6 concurrent
await _semaphore.WaitAsync();
try { /* API call */ } finally { _semaphore.Release(); }
```

#### **Common Problems → Solutions:**

**Problem:** TMDB API rate limiting
```csharp
// SOLUTION: Built-in throttling + retry logic
await _semaphore.WaitAsync();
try {
    var response = await _httpClient.GetAsync(url);
    if (response.StatusCode == HttpStatusCode.TooManyRequests) {
        await Task.Delay(1000); // Wait and retry
        response = await _httpClient.GetAsync(url);
    }
} finally { _semaphore.Release(); }
```

**Problem:** Slow API performance
```csharp
// SOLUTION: 24-hour caching + parallel execution
_memoryCache.Set(cacheKey, result, TimeSpan.FromHours(24));

// For multiple calls, use parallel execution
var tasks = tmdbIds.Select(id => _tmdbService.GetMovieDetailsAsync(id));
var results = await Task.WhenAll(tasks);
```

**Problem:** API failures breaking user experience
```csharp
// SOLUTION: Robust error handling with fallbacks
try {
    return await _tmdbService.GetMovieDetailsAsync(tmdbId);
} catch (HttpRequestException) {
    _logger.LogWarning("TMDB API unavailable, using cached data");
    return GetCachedMovieDetails(tmdbId) ?? CreateFallbackMovieDetails(tmdbId);
}
```

#### **Caching Strategy:**
- **TMDB Movie Details:** 24 hours in IMemoryCache
- **Search Results:** 1 hour (more dynamic)
- **Trending Data:** 90 minutes (balances freshness with performance)
- **Suggestion Pools:** 2 hours for Surprise Me, varies by type

### ⚡ Performance Optimization **[WHEN: Slow queries, cache misses, N+1 problems, memory issues]**

#### **Core Patterns:**
```csharp
// CACHING SERVICE - User-specific data
var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
// 15-minute expiration, automatic invalidation on add/remove

// BATCH PROCESSING - Eliminate N+1 queries
// BAD: Individual API calls
foreach (var item in wishlistItems) {
    var details = await _tmdbService.GetMovieDetailsAsync(item.TmdbId);
}

// GOOD: Batch API call
var tmdbIds = wishlistItems.Select(i => i.TmdbId).ToList();
var movieDetails = await _tmdbService.GetMultipleMovieDetailsAsync(tmdbIds);

// PAGINATION - 20 items per page with proper indexing
var paginatedItems = await PaginatedList<T>.CreateAsync(
    query.Where(m => m.UserId == userId), pageNumber, 20);
```

#### **Common Problems → Solutions:**

**Problem:** Database queries are slow
```csharp
// SOLUTION: Composite indexes + proper filtering
// Migration: Add index on (UserId, Title) for fast user-specific searches
migrationBuilder.CreateIndex(
    name: "IX_Movies_UserId_Title",
    table: "Movies",
    columns: new[] { "UserId", "Title" });

// Always filter by UserId first (uses index)
var userMovies = _dbContext.Movies
    .Where(m => m.UserId == userId)  // Index hit
    .Where(m => m.Title.Contains(searchTerm));
```

**Problem:** Cache misses causing performance hits
```csharp
// SOLUTION: Request-level caching for expensive operations
private Dictionary<string, object> _requestCache = new();

private async Task<List<Movies>> GetLast25MoviesAsync(string userId)
{
    var cacheKey = $"last25_{userId}";
    if (_requestCache.TryGetValue(cacheKey, out var cached))
        return (List<Movies>)cached;
        
    var result = await _dbContext.Movies
        .Where(m => m.UserId == userId)
        .OrderByDescending(m => m.DateWatched)
        .Take(25).ToListAsync();
        
    _requestCache[cacheKey] = result;
    return result;
}
```

**Problem:** Surprise Me pool building is slow
```csharp
// SOLUTION: Parallel execution (85% faster)
var buckets = CreateSurprisePoolBuckets(); // Different API endpoints
var poolTasks = buckets.Select(async bucket => {
    var semaphore = new SemaphoreSlim(1);
    await semaphore.WaitAsync();
    try {
        return await _tmdbService.GetMoviesFromBucket(bucket);
    } finally { semaphore.Release(); }
}).ToArray();

var poolResults = await Task.WhenAll(poolTasks);
// Build time: ~400ms (was ~2800ms)
```

#### **Performance Benchmarks:**
- API calls: Batch 20 movies in ~200ms vs 20 individual calls in ~4000ms
- Database pagination: 20 items with index in <50ms
- Surprise Me build: 50 movies in ~400ms with parallel execution
- Cache hit rate: >90% for TMDB data, >80% for user blacklist/wishlist

### 🏗️ ASP.NET Core Development **[WHEN: Controllers, Views, AJAX, Authentication, Routing]**

#### **Core Patterns:**
```csharp
// CONTROLLER STRUCTURE - Standard CineLog pattern
[Authorize] // Always require authentication
public class MoviesController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly TmdbService _tmdbService;
    private readonly CacheService _cacheService;
    private readonly ILogger<MoviesController> _logger;

    // Always get userId first in actions
    var userId = _userManager.GetUserId(User);
    
    // Always filter data by userId
    var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);
}

// AJAX ENDPOINTS - Return server-rendered HTML
[HttpPost]
public async Task<IActionResult> TrendingReshuffle()
{
    var userId = _userManager.GetUserId(User);
    var movies = await GetTrendingMoviesWithFiltering(userId);
    return PartialView("_MovieSuggestionCard", movies.Take(3));
}

// MUTUAL EXCLUSION - Prevent wishlist/blacklist conflicts
private async Task<bool> IsMovieInWishlist(string userId, string tmdbId)
{
    return await _dbContext.WishlistItems
        .AnyAsync(w => w.UserId == userId && w.TmdbId == tmdbId);
}
```

#### **Common Problems → Solutions:**

**Problem:** User data exposed across accounts
```csharp
// SOLUTION: Always filter by UserId (CRITICAL SECURITY)
// BAD: Exposes all users' data
var movies = _dbContext.Movies.Where(m => m.Title.Contains(search));

// GOOD: User isolation
var userId = _userManager.GetUserId(User);
var movies = _dbContext.Movies
    .Where(m => m.UserId == userId)
    .Where(m => m.Title.Contains(search));
```

**Problem:** AJAX not working with dynamic content
```csharp
// SOLUTION: Event delegation pattern
// JavaScript: Handle dynamically added buttons
document.addEventListener('click', function(e) {
    if (e.target.matches('.reshuffle-btn')) {
        handleReshuffle(e.target);
    }
});

// Controller: Return HTML, not JSON for consistent styling
return PartialView("_MovieSuggestionCard", suggestedMovies);
```

**Problem:** Anti-forgery token validation failing
```csharp
// SOLUTION: Include tokens in AJAX requests
// Razor view:
@Html.AntiForgeryToken()

// JavaScript:
var token = $('input[name="__RequestVerificationToken"]').val();
$.post(url, { __RequestVerificationToken: token, data: data });
```

#### **View Patterns:**
```html
<!-- CINEMA GOLD BRANDING -->
<h3 class="cinelog-gold-title">Trending Movies</h3>

<!-- BOOTSTRAP 5 CYBORG THEME -->
<div class="card bg-dark border-secondary">
    <div class="card-body">
        <!-- Movie content -->
    </div>
</div>

<!-- EVENT DELEGATION ATTRIBUTES -->
<button class="btn btn-outline-warning" 
        data-suggestion-type="trending"
        data-action="reshuffle">
    Reshuffle
</button>
```

### 🗄️ Database & Entity Framework **[WHEN: Migrations, queries, performance indexes, data models]**

#### **Core Patterns:**
```csharp
// USER DATA ISOLATION - Critical for all queries
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);

// AZURE SQL DATABASE CONFIGURATION - Production-ready with Azure Key Vault integration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string 'DefaultConnection' not found in configuration.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // AZURE: Connection resilience for Azure SQL Database
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        
        // AZURE: Command timeout for long-running operations
        sqlOptions.CommandTimeout(60);
    });
    
    // PERFORMANCE: Only enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// COMPOSITE INDEXES - For fast user-specific searches
migrationBuilder.CreateIndex(
    name: "IX_Movies_UserId_Title",
    table: "Movies", 
    columns: new[] { "UserId", "Title" });

// PAGINATION PATTERN
var paginatedList = await PaginatedList<Movies>.CreateAsync(
    _dbContext.Movies.Where(m => m.UserId == userId), pageNumber, 20);

// Use TotalCount for pagination (NOT viewModels.Count)
var totalCount = paginatedList.TotalCount; // Total database records
```

#### **Common Problems → Solutions:**

**Problem:** Pagination navigation broken
```csharp
// PROBLEM: Using current page count instead of total
var totalItems = viewModels.Count; // WRONG - only current page items (max 20)

// SOLUTION: Use total database count
var totalItems = paginatedList.TotalCount; // CORRECT - all user's records
var viewModel = new ViewModel {
    TotalCount = paginatedList.TotalCount,
    HasNextPage = paginatedList.HasNextPage,
    HasPreviousPage = paginatedList.HasPreviousPage
};
```

**Problem:** Slow user-specific queries
```csharp
// SOLUTION: Always filter by UserId first (uses index)
// BAD: Full table scan
var movies = _dbContext.Movies.Where(m => m.Title.Contains(search));

// GOOD: Index-optimized
var movies = _dbContext.Movies
    .Where(m => m.UserId == userId)  // Uses IX_Movies_UserId
    .Where(m => m.Title.Contains(search));
```

**Problem:** N+1 query problems in Entity Framework
```csharp
// BAD: N+1 queries
var movies = _dbContext.Movies.Where(m => m.UserId == userId);
foreach (var movie in movies) {
    // This creates additional queries
    var relatedData = movie.SomeRelatedEntity;
}

// GOOD: Include related data
var movies = _dbContext.Movies
    .Where(m => m.UserId == userId)
    .Include(m => m.SomeRelatedEntity)
    .ToListAsync();
```

#### **Entity Models:**
```csharp
// Standard CineLog entity pattern
public class Movies
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } // Always required
    [Required] public string Title { get; set; }
    public string? Director { get; set; }
    public int ReleasedYear { get; set; }
    public decimal? UserRating { get; set; }
    public DateTime DateWatched { get; set; }
    public string? WatchedLocation { get; set; }
    public bool IsRewatch { get; set; }
    public string? TmdbId { get; set; }
    public string? PosterPath { get; set; }
    public string? Overview { get; set; }
    public string? Genres { get; set; }
    public DateTime DateCreated { get; set; }
}
```

#### **Migration Best Practices:**
```csharp
// Always include UserId indexes for new user-specific tables
migrationBuilder.CreateIndex(
    name: "IX_TableName_UserId",
    table: "TableName",
    column: "UserId");

// Composite indexes for common query patterns
migrationBuilder.CreateIndex(  
    name: "IX_TableName_UserId_CommonField",
    table: "TableName",
    columns: new[] { "UserId", "CommonField" });
```

### 🎨 UI/UX & AJAX Patterns **[WHEN: Views, styling, JavaScript, responsive design, user interactions]**

> **⚡ AJAX Quick Reference**: All AJAX POSTs must include `X-Requested-With: XMLHttpRequest`, `credentials: 'same-origin'`, and antiforgery token.

#### **Core Patterns:**
```html
<!-- CINEMA GOLD BRANDING -->
<h2 class="cinelog-gold-title mb-4">Movie Suggestions</h2>

<!-- BOOTSTRAP 5 CYBORG THEME -->
<div class="container-fluid bg-dark text-light">
    <div class="card bg-dark border-secondary mb-3">
        <div class="card-body">
            <h5 class="card-title text-warning">Movie Title</h5>
            <p class="card-text">Movie description...</p>
        </div>
    </div>
</div>

<!-- EVENT DELEGATION FOR DYNAMIC CONTENT -->
<script>
document.addEventListener('click', function(e) {
    if (e.target.matches('[data-action="reshuffle"]')) {
        handleReshuffle(e.target.dataset.suggestionType);
    }
    
    if (e.target.matches('.add-to-wishlist')) {
        addToWishlist(e.target.dataset.tmdbId);
    }
});
</script>
```

#### **Common Problems → Solutions:**

**Problem:** AJAX buttons stop working after content updates
```javascript
// PROBLEM: Direct event binding breaks with dynamic content
$('.reshuffle-btn').click(function() { /* Won't work for new buttons */ });

// SOLUTION: Event delegation
$(document).on('click', '.reshuffle-btn', function() {
    // Works for all buttons, including dynamically added ones
    handleReshuffle($(this).data('suggestion-type'));
});
```

**Problem:** Inconsistent styling after AJAX updates
```csharp
// SOLUTION: Return server-rendered HTML from AJAX endpoints
[HttpPost]
public async Task<IActionResult> TrendingReshuffle()
{
    var movies = await GetTrendingMovies(userId);
    // Return partial view with consistent styling
    return PartialView("_MovieSuggestionCard", movies);
}
```

**Problem:** Mobile responsiveness issues
```html
<!-- SOLUTION: Bootstrap responsive classes -->
<div class="container-fluid">
    <div class="row">
        <div class="col-12 col-md-6 col-lg-4 mb-3">
            <!-- Movie card - stacks on mobile, 2 per row on tablet, 3 on desktop -->
            <div class="card">...</div>
        </div>
    </div>
</div>
```

#### **JavaScript Patterns:**
```javascript
// AJAX with anti-forgery tokens
function makeAjaxCall(url, data) {
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    return $.ajax({
        url: url,
        type: 'POST',
        data: { ...data, __RequestVerificationToken: token },
        success: function(html) {
            // Replace content with server-rendered HTML
            $('#suggestion-container').html(html);
        },
        error: function() {
            showErrorMessage('Failed to load suggestions');
        }
    });
}

// Progressive enhancement - works without JavaScript
function enhanceForm(formSelector) {
    $(formSelector).on('submit', function(e) {
        e.preventDefault();
        const form = $(this);
        makeAjaxCall(form.attr('action'), form.serialize());
    });
}
```

#### **CSS Patterns:**
```css
/* Cinema Gold branding */
.cinelog-gold-title {
    color: #FFD700 !important;
    font-weight: 600;
}

/* Hover effects for interactive elements */
.movie-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(255, 215, 0, 0.3);
    transition: all 0.2s ease;
}

/* Responsive spacing */
@media (max-width: 768px) {
    .movie-card {
        margin-bottom: 1rem;
    }
    
    .suggestion-grid {
        grid-template-columns: 1fr;
    }
}
```

#### **Accessibility Patterns:**
```html
<!-- Screen reader support -->
<button class="btn btn-outline-warning" 
        aria-label="Reshuffle trending movie suggestions"
        data-suggestion-type="trending">
    <i class="fas fa-refresh" aria-hidden="true"></i>
    Reshuffle
</button>

<!-- Loading states -->
<div id="loading-spinner" class="d-none" role="status" aria-live="polite">
    <span class="sr-only">Loading suggestions...</span>
    <div class="spinner-border text-warning"></div>
</div>
```

### 🔧 Testing & Debugging **[WHEN: Test failures, debugging issues, performance problems]**

#### **Core Patterns:**
```csharp
// STRUCTURED LOGGING - Use throughout controllers
_logger.LogInformation("Generating {SuggestionType} suggestions for user {UserId}", 
    suggestionType, userId);

_logger.LogWarning("Director {DirectorName} has no available movies for user {UserId}", 
    directorName, userId);

_logger.LogError(ex, "TMDB API failed for user {UserId}: {ErrorMessage}", 
    userId, ex.Message);

// DEFENSIVE PROGRAMMING - Always validate user data
private async Task<bool> ValidateUserAccess(string userId, string resourceId)
{
    return await _dbContext.SomeEntity
        .AnyAsync(e => e.Id == resourceId && e.UserId == userId);
}

// PERFORMANCE TIMING
using var activity = _logger.BeginScope("SuggestionBuild_{SuggestionType}", type);
var stopwatch = Stopwatch.StartNew();
var result = await BuildSuggestions(userId, type);
_logger.LogInformation("Built {Count} suggestions in {ElapsedMs}ms", 
    result.Count, stopwatch.ElapsedMilliseconds);
```

#### **Common Debugging Scenarios:**

**Issue:** "Suggestions are empty"
```csharp
// DEBUG: Check each filter step
_logger.LogDebug("Pool before blacklist filter: {Count} movies", poolMovies.Count);
var filteredPool = poolMovies.Where(m => !blacklistIds.Contains(m.TmdbId));
_logger.LogDebug("Pool after blacklist filter: {Count} movies", filteredPool.Count());

var finalPool = filteredPool.Where(m => !recentTmdbIds.Contains(m.TmdbId));
_logger.LogDebug("Final pool: {Count} movies", finalPool.Count());
```

**Issue:** "User seeing other users' data"
```csharp
// DEBUG: Verify UserId filtering
_logger.LogDebug("Querying movies for user {UserId}", userId);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);
_logger.LogDebug("Found {Count} movies for user {UserId}", 
    await userMovies.CountAsync(), userId);
```

**Issue:** "Performance is slow"
```csharp
// DEBUG: Profile individual operations
var sw = Stopwatch.StartNew();

var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
_logger.LogDebug("Blacklist fetch: {ElapsedMs}ms", sw.ElapsedMilliseconds);
sw.Restart();

var apiResult = await _tmdbService.GetTrendingMoviesAsync();
_logger.LogDebug("TMDB API call: {ElapsedMs}ms", sw.ElapsedMilliseconds);
```

#### **Test Patterns:**
```csharp
// User isolation testing
[Test]
public async Task GetUserMovies_ShouldOnlyReturnCurrentUserMovies()
{
    // Arrange
    var user1Id = "user1";
    var user2Id = "user2";
    
    await _dbContext.Movies.AddRangeAsync(
        new Movies { UserId = user1Id, Title = "User1 Movie" },
        new Movies { UserId = user2Id, Title = "User2 Movie" }
    );
    await _dbContext.SaveChangesAsync();
    
    // Act
    var result = await _controller.GetUserMovies(user1Id);
    
    // Assert
    Assert.That(result.All(m => m.UserId == user1Id));
}
```

### 🔧 Code Refactoring & Technical Debt **[WHEN: Complex methods, duplicate code, legacy patterns, code smells]**

#### **Core Patterns:**
```csharp
// EXTRACT METHOD PATTERN - Break down large methods
// Before: 200+ line method
public async Task<IActionResult> ShowSuggestions(string type)
{
    // Complex logic mixing concerns
}

// After: Focused, single-responsibility methods
public async Task<IActionResult> ShowSuggestions(string type)
{
    var userId = _userManager.GetUserId(User);
    var suggestionStrategy = _suggestionFactory.CreateStrategy(type);
    var viewModel = await suggestionStrategy.GenerateAsync(userId);
    return View(viewModel);
}

// ELIMINATE DUPLICATION - Common filtering logic
private async Task<List<TmdbMovieBrief>> ApplyUserFiltering(
    List<TmdbMovieBrief> movies, string userId)
{
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    var recentTmdbIds = await GetRecentMovieTmdbIds(userId);
    
    return movies
        .Where(m => !blacklistIds.Contains(m.TmdbId))
        .Where(m => !recentTmdbIds.Contains(m.TmdbId))
        .ToList();
}

// SIMPLIFY COMPLEX CONDITIONS
// Before: Complex nested conditions
if (suggestionType == "trending" && 
    (userPreferences?.IncludeTrending == true || userPreferences == null) && 
    !userMovies.Any(m => m.Genre?.Contains("Action") == true && m.Rating > 4.0))

// After: Extracted to meaningful methods
if (ShouldShowTrendingSuggestions(suggestionType, userPreferences, userMovies))
```

#### **CineLog-Specific Refactoring Patterns:**

**MoviesController Simplification:**
```csharp
// BEFORE: Mixed concerns in controller
public async Task<IActionResult> DirectorReshuffle()
{
    var userId = _userManager.GetUserId(User);
    var directors = await GetDirectorPriorityQueue(userId);
    var blacklistIds = await _cacheService.GetUserBlacklistIdsAsync(userId);
    // ... 50+ lines of filtering, API calls, and business logic
}

// AFTER: Delegated to services
public async Task<IActionResult> DirectorReshuffle()
{
    var userId = _userManager.GetUserId(User);
    var suggestions = await _directorSuggestionService.GetNextSuggestionsAsync(userId);
    return PartialView("_MovieSuggestionCard", suggestions);
}
```

**Suggestion Algorithm Unification:**
```csharp
// STRATEGY PATTERN for suggestion types
public interface ISuggestionStrategy
{
    Task<List<TmdbMovieBrief>> GenerateAsync(string userId);
    string GetSuggestionTitle();
}

public class TrendingSuggestionStrategy : ISuggestionStrategy
{
    public async Task<List<TmdbMovieBrief>> GenerateAsync(string userId)
    {
        return await GetTrendingMoviesWithFiltering(userId);
    }
}

// Factory for clean controller logic
public class SuggestionStrategyFactory
{
    public ISuggestionStrategy CreateStrategy(string type) => type switch
    {
        "trending" => new TrendingSuggestionStrategy(_tmdbService, _cacheService),
        "director" => new DirectorSuggestionStrategy(_tmdbService, _cacheService),
        _ => new DefaultSuggestionStrategy()
    };
}
```

#### **Common Refactoring Scenarios:**

**Problem:** Large, complex controller methods
```csharp
// SOLUTION: Service layer extraction
// Move business logic to dedicated services
// Controller only handles HTTP concerns and delegates to services
// Each service has single responsibility (e.g., DirectorSuggestionService)
```

**Problem:** Duplicate filtering logic across suggestion types
```csharp
// SOLUTION: Common base class or shared service
public abstract class BaseSuggestionService
{
    protected async Task<List<TmdbMovieBrief>> ApplyStandardFiltering(
        List<TmdbMovieBrief> movies, string userId)
    {
        // Common blacklist, wishlist, recent movie filtering
    }
}
```

**Problem:** Complex LINQ queries mixed with business logic
```csharp
// SOLUTION: Repository pattern with domain-specific queries
public class MovieRepository
{
    public async Task<List<Movies>> GetUserMoviesWithFilters(
        string userId, MovieFilter filters)
    {
        var query = _dbContext.Movies.Where(m => m.UserId == userId);
        
        if (filters.MinRating.HasValue)
            query = query.Where(m => m.Rating >= filters.MinRating);
            
        return await query.ToListAsync();
    }
}
```

#### **Quality Metrics to Monitor:**
```csharp
// CYCLOMATIC COMPLEXITY - Methods should have < 10 branches
// METHOD LENGTH - Keep methods under 50 lines
// CLASS SIZE - Controllers should focus on HTTP concerns only
// DUPLICATION - DRY principle, extract common patterns

// Performance impact measurement
var stopwatch = Stopwatch.StartNew();
var result = await RefactoredMethod();
_logger.LogInformation("Refactored method completed in {ElapsedMs}ms", 
    stopwatch.ElapsedMilliseconds);
```

#### **Test-Safe Refactoring:**
```csharp
// PRESERVE BEHAVIOR - Ensure existing functionality works
[Test]
public async Task RefactoredMethod_ShouldReturnSameResults()
{
    // Arrange - same test data
    var userId = "test-user";
    
    // Act - call both old and new implementations
    var oldResult = await OldSuggestionMethod(userId);
    var newResult = await NewSuggestionService.GenerateAsync(userId);
    
    // Assert - verify identical behavior
    Assert.That(newResult, Is.EqualTo(oldResult));
}
```

---

## 🔍 AI AGENT OBSERVABILITY SYSTEM (2025-08-20)
**🆕 ENHANCED WITH INDUSTRY-LEADING OBSERVABILITY PATTERNS**

### 📊 Core Observability Infrastructure
Our agent system now implements comprehensive observability based on "AI Agent Design Patterns" best practices:

#### **Key Observability Files:**
```markdown
.claude/observability/
├── README.md                    # Observability system overview
├── agent-performance.md         # Real-time metrics and success rates
├── evaluation-criteria.md       # LLM-as-judge quality framework
├── feedback-loops.md           # Continuous learning mechanisms
├── optimization-insights.md    # Data-driven improvements
└── health-dashboard.md         # System status monitoring
```

#### **SESSION_NOTES.md Enhancements:**
- **Agent System Health**: Real-time performance dashboard (8.7/10)
- **Top Performing Agents**: Success rates and execution times
- **Master Director Performance**: 92% routing accuracy
- **Agent Learning Insights**: Automated pattern recognition

### 🎯 LLM-as-Judge Evaluation Framework
**Automated Quality Assessment (1-10 Scale):**

```yaml
evaluation_criteria:
  task_completion: 30%      # Did agent fully complete the request?
  technical_excellence: 25% # Is solution technically sound?
  code_quality: 20%        # Follows best practices and conventions?
  innovation: 15%           # Shows creativity or additional value?
  user_experience: 10%     # User-friendly and intuitive?

quality_thresholds:
  excellent: 8.0+          # Agent operating at peak efficiency
  good: 6.0-7.9           # Agent meeting expectations  
  needs_improvement: 4.0-5.9 # Agent requires optimization
  critical: <4.0          # Agent needs immediate attention
```

#### **Agent Performance Metrics:**
```yaml
current_performance_stats:
  tmdb_api_expert: 98% success, 30s avg ⭐⭐⭐⭐⭐
  performance_optimizer: 96% success, 1.8m avg ⭐⭐⭐⭐⭐
  cinelog_movie_specialist: 94% success, 45s avg ⭐⭐⭐⭐⭐
  aspnet_feature_developer: 91% success, 1.2m avg ⭐⭐⭐⭐
  session_secretary: 100% success, 15s avg ⭐⭐⭐⭐⭐
```

### 🔁 Continuous Learning & Feedback Loops
**Pattern Recognition and Optimization:**

#### **Success Patterns Identified:**
```yaml
high_performance_patterns:
  - "Domain specialists 18% more effective than generalists"
  - "Strategic planning increases complex task success by 23%"
  - "performance-optimizer + performance-monitor combo: 96% success rate"
  - "Morning sessions show 12% higher success rates"

user_preferences_learned:
  - "Strong preference for detailed technical explanations"
  - "Values comprehensive safety and security assessments"
  - "Prefers local development and testing before deployment"
  - "Appreciates systematic todo tracking and completion"
```

#### **Optimization Opportunities:**
```yaml
improvement_areas:
  - "Multi-agent context handoffs: 15% improvement potential"
  - "aspnet-feature-developer: Monitor for scope creep tendency"
  - "Response time variance: 30-120s spread could be optimized"
  - "Complex task routing: Enhanced planning triggers needed"
```

### 📋 Enhanced Commands & Usage

#### **New `/agent-feedback` Command:**
```bash
# Analyze overall agent performance
/agent-feedback

# Analyze specific agent performance  
/agent-feedback cinelog-movie-specialist

# System-wide optimization analysis
/agent-feedback performance
```

#### **Agent Performance Tracking Patterns:**
```csharp
// When working with agent patterns, consider performance tracking
public class AgentPerformanceTracker 
{
    // Track agent execution metrics
    public void RecordAgentExecution(string agentName, TimeSpan duration, bool success, double qualityScore)
    {
        // Update performance metrics in SESSION_NOTES.md
        // Calculate rolling averages and success rates
        // Identify optimization opportunities
    }
    
    // LLM-as-judge quality assessment
    public async Task<double> EvaluateAgentQuality(string agentOutput, string taskContext)
    {
        // Implement automated quality scoring
        // Return score (1-10) based on evaluation criteria
        // Track quality trends over time
    }
}
```

### 🎯 Performance Targets & KPIs
**System-wide Performance Standards:**

```yaml
performance_targets:
  overall_success_rate: ">95% (Currently: 94%)"
  response_times:
    simple_tasks: "<60 seconds"
    medium_tasks: "<3 minutes" 
    complex_tasks: "<5 minutes"
  user_satisfaction: ">4.5/5"
  routing_accuracy: ">92% (Currently: 92%)"
  learning_velocity: "+5% improvement per month"

monitoring_alerts:
  performance_degradation: "Success rate drops >20%"
  execution_time_increase: "Response time increases >15%"
  quality_score_drop: "LLM-as-judge scores drop >5%"
  user_satisfaction_decline: "Satisfaction ratings drop below 4.0"
```

### 🔧 Observability Integration Patterns
**When working with our codebase, integrate observability:**

#### **Performance Monitoring Integration:**
```csharp
// Add performance tracking to service methods
[HttpPost]
public async Task<IActionResult> SuggestMovies(string type)
{
    var stopwatch = Stopwatch.StartNew();
    var userId = _userManager.GetUserId(User);
    
    try 
    {
        var suggestions = await _suggestionService.GenerateAsync(type, userId);
        
        // Track successful execution
        await _performanceTracker.RecordSuccess(
            agentName: "cinelog-movie-specialist",
            executionTime: stopwatch.Elapsed,
            qualityMetrics: suggestions.Count
        );
        
        return PartialView("_Suggestions", suggestions);
    }
    catch (Exception ex)
    {
        // Track failure patterns
        await _performanceTracker.RecordFailure(
            agentName: "cinelog-movie-specialist", 
            error: ex.Message,
            context: type
        );
        throw;
    }
}
```

#### **Quality Assessment Integration:**
```csharp
// Implement quality checks in critical workflows
public async Task<ValidationResult> ValidateAgentOutput(string agentName, object output)
{
    var qualityScore = await _llmJudge.EvaluateQuality(output, agentName);
    
    if (qualityScore < 6.0)
    {
        _logger.LogWarning("Agent {AgentName} quality below threshold: {Score}", 
            agentName, qualityScore);
        
        // Trigger optimization analysis
        await TriggerOptimizationAnalysis(agentName);
    }
    
    return new ValidationResult(qualityScore >= 6.0, qualityScore);
}
```

---

## ⚠️ DANGER ZONE - PRODUCTION DEPLOYMENT
**🚨 REQUIRES EXPLICIT USER PERMISSION 🚨**
**NEVER run these commands without user saying "deploy to production"**

### 🚀 Production Deployment Commands
```bash
# 🚨 FORBIDDEN without explicit permission:
az webapp deployment             # Deploy to Azure App Service
curl -X POST "https://cinelog-app.scm.azurewebsites.net/api/zipdeploy"  # Deploy ZIP
git push origin main             # Push code to repository
dotnet publish -c Release       # Build for production

# ✅ Safe commands (local only):
dotnet build                    # Local build
dotnet run                      # Local development server
dotnet watch run                # Local hot reload
```

### 🛡️ Safety Requirements:
1. **User must explicitly request deployment**: "deploy to production", "push to Azure", etc.
2. **Never assume user wants to deploy**: Always ask for permission
3. **Default to local development**: Keep production site stable
4. **Test locally first**: Always verify changes work locally before any deployment discussion
