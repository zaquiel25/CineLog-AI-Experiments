# CLAUDE.md — CineLog AI Development Guide

## Critical Rules

1. **GOLDEN RULE: Do NOT invent things.** Never add features, improvements, or enhancements unless explicitly requested. Implement ONLY what is asked.
2. **Never auto-commit or auto-deploy.** Only commit/push/deploy when the user explicitly asks.
3. **Always verify build.** Run `dotnet build` before marking any task complete. Build failures must be fixed.
4. **Professional comments required.** All new methods need XML docs. Inline comments explain "why", not "what". English only. Use FIX/FEATURE/ENHANCEMENT prefixes.
5. **Use TodoWrite** for complex multi-step tasks. Keep working until all items are done.
6. **Safety first.** Check for data privacy, security vulnerabilities, and credential exposure before implementing. When in doubt, ask.

## Production Safety

- **Production**: https://cinelog-app.azurewebsites.net/ — DO NOT TOUCH without explicit permission
- **Local dev**: https://localhost:7186 — safe for testing
- **Forbidden without explicit permission**: `az webapp deployment`, `git push`, any Azure deploy command
- Always work locally by default

## Session Continuity

- **Start of conversation**: Search SESSION_NOTES.md for recent context using Grep (date-based search)
- **End of conversation**: Append brief summary to SESSION_NOTES.md (date, goals, accomplishments)
- SESSION_NOTES.md is gitignored — local working context only

## Development Commands

```bash
# Local development (safe)
dotnet build                         # Build
dotnet run                           # Run at localhost:7186
dotnet watch run                     # Hot reload (recommended)
dotnet test                          # Run tests

# Database (local only)
dotnet ef migrations add <Name>      # Create migration
dotnet ef database update            # Apply migrations

# Configuration
dotnet user-secrets set "TMDB:AccessToken" "your-token"

# Production deployment (REQUIRES EXPLICIT PERMISSION)
dotnet publish -c Release -o ./publish-clean
```

## CineLog Architecture Patterns

### User Data Isolation (CRITICAL)
All queries MUST filter by UserId. Never expose data across accounts.
```csharp
var userId = _userManager.GetUserId(User);
var userMovies = _dbContext.Movies.Where(m => m.UserId == userId);
```

### Two-Layer Authentication
- **Identity (default scheme)**: `[Authorize]` attributes for user account auth
- **PasswordGate (named scheme)**: Site-wide access control — requires explicit `HttpContext.AuthenticateAsync("PasswordGate")`
- Google OAuth supported — `app.UseAuthentication()` MUST be present before `app.UseAuthorization()`

### TMDB Integration
- 24-hour IMemoryCache for API responses
- 15-minute CacheService for user data (blacklist/wishlist)
- Batch calls with `GetMultipleMovieDetailsAsync()` to prevent N+1
- Rate limiting with SemaphoreSlim (6 concurrent max)
- Known directors cache for 0-API-call lookups

### AJAX Patterns
- `X-Requested-With: XMLHttpRequest` header required for JSON responses
- Target `.container` elements — never replace `main` completely
- Unified helper methods for initial load and reshuffles
- Server-side rendering for all AJAX responses

### URL Parameters
- Use `RouteValueDictionary` for conditional parameter inclusion
- Never include boolean params as empty strings — omit when false

### Azure Production
- Key Vault configured via `AZURE_KEY_VAULT_URI` environment variable
- Automatic `{DatabasePassword}` placeholder replacement in production
- Environment variable: `AZURE_KEY_VAULT_URI`

## Agent System

Use the Task tool with specialized agents for **complex, multi-domain tasks**. For simple tasks (typos, single-file edits, quick builds), work directly.

| Task Type | Agent |
|-----------|-------|
| Movie features, suggestions, CRUD | `cinelog-movie-specialist` |
| TMDB API, search, external data | `tmdb-api-expert` |
| Caching, query optimization | `performance-optimizer` |
| Database schema, migrations | `ef-migration-manager` |
| Full-stack MVC features | `aspnet-feature-developer` |
| Deployment coordination | `deployment-project-manager` |
| Documentation updates | `docs-architect` |
| Performance validation | `performance-monitor` |
| Session notes management | `session-secretary` |
| Complex multi-step research | `general-purpose` |

For detailed agent docs, see [AGENTS.md](.claude/agents/AGENTS.md).

## Code Quality

- Structured logging with `_logger` (never Console.WriteLine in production)
- Async/await for all DB and API calls
- Try-catch for external API calls with graceful fallbacks
- `AsNoTracking()` for read-only queries
- Pagination at 20 items per page
- Composite indexes on `UserId+Title` patterns

## MCP Servers

Use automatically when relevant:
- **microsoft-docs**: ASP.NET Core, Entity Framework docs
- **deepwiki**: Wikipedia research
- **context7**: Contextual AI operations

## Documentation References

- [README.md](README.md): Architecture, features, setup, tech stack
- [CHANGELOG.md](CHANGELOG.md): Change history
- [PERFORMANCE_OPTIMIZATION_SUMMARY.md](PERFORMANCE_OPTIMIZATION_SUMMARY.md): Performance metrics
- [.claude/patterns.md](.claude/patterns.md): Detailed code patterns and examples (reference when needed)
