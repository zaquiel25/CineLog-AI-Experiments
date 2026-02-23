# CineLog Agent Orchestration

## How It Works

Claude evaluates each request, determines complexity, and routes to the right agent(s).

## Complexity Levels

- **Simple** (single-file tweaks, typos): Work directly, no agent needed
- **Medium** (localized features, performance fixes): Route to one specialist
- **Complex** (cross-layer features, schema changes): Plan first, then multi-agent coordination

## Decision Matrix

| Task Pattern | Primary Agent | Supporting Agents |
|---|---|---|
| Movie feature | `cinelog-movie-specialist` | `aspnet-feature-developer`, `docs-architect` |
| TMDB API issue | `tmdb-api-expert` | `performance-optimizer` |
| Schema/migration | `ef-migration-manager` | `cinelog-movie-specialist` |
| Performance fix | `performance-optimizer` | `performance-monitor` |
| Documentation | `docs-architect` | Domain agent for context |
| Deployment | `deployment-project-manager` | `performance-monitor`, `docs-architect` |

## Example: Complex Feature

1. Plan: identify model changes + UI + caching impact
2. `ef-migration-manager` updates schema
3. `cinelog-movie-specialist` adjusts query logic
4. `aspnet-feature-developer` updates views
5. `performance-optimizer` reviews caching
6. `docs-architect` updates documentation

## Norms

- Prefer multi-agent handoffs over single-agent mega tasks
- Log decisions in SESSION_NOTES.md for continuity
- Pair `performance-optimizer` with `performance-monitor` for before/after evidence
- Involve `deployment-project-manager` early for deployments
