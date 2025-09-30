# 🤖 CineLog Claude Code Agents

This directory contains the curated Claude Code specialists that power CineLog development. Each agent understands the ASP.NET Core MVC stack, TMDB workflow, and the Cinema Gold product vision, and can be routed individually or orchestrated by the Master Agent Director.

## 📋 Specialist Roster
| Agent | Focus | Typical Work |
|-------|-------|--------------|
| **Master Agent Director** | Task triage & orchestration | Classifies requests, selects specialists, coordinates follow-up |
| [`cinelog-movie-specialist`](./cinelog-movie-specialist.md) | Movie domain & suggestion logic | Feature work in `MoviesController`, watch logging, journal/collection flows |
| [`aspnet-feature-developer`](./aspnet-feature-developer.md) | Full-stack MVC features | Controllers, Razor views, AJAX enhancements, validation |
| [`tmdb-api-expert`](./tmdb-api-expert.md) | TMDB integration & caching | `TmdbService`, rate limiting, provider fallbacks |
| [`ef-migration-manager`](./ef-migration-manager.md) | Database & migrations | Entity updates, schema migrations, performance indexes |
| [`performance-optimizer`](./performance-optimizer.md) | Runtime performance | Caching strategy, query tuning, telemetry review |
| [`performance-monitor`](./performance-monitor.md) | Observability & verification | Benchmark runs, KPI tracking, regression alerts |
| [`docs-architect`](./docs-architect.md) | Documentation & change tracking | README, CLAUDE.md, change logs, writing automation |
| [`deployment-project-manager`](./deployment-project-manager.md) | Release coordination | Deployment checklists, configuration reviews, rollout comms |
| [`session-secretary`](./session-secretary.md) | Session continuity | Captures context at start/end of every engagement |

## 🔎 Selecting an Agent
- **Movie features & UI flows** → Start with `cinelog-movie-specialist`; pair with `aspnet-feature-developer` for UI/MVC updates and `docs-architect` for release notes.
- **External data issues** → Route to `tmdb-api-expert` for API troubleshooting; add `performance-optimizer` if latency is involved.
- **Schema or data model changes** → Engage `ef-migration-manager` first, then coordinate with `cinelog-movie-specialist` for domain validation.
- **Performance or telemetry questions** → Use `performance-optimizer` to implement fixes and `performance-monitor` to validate results against KPIs.
- **Process & release work** → Involve `deployment-project-manager` for go-live prep and `docs-architect` for change documentation.

## 🔁 Coordinated Workflows
- **New Feature Delivery**: Master Agent Director → `cinelog-movie-specialist` (requirements) → `aspnet-feature-developer` (MVC/UI) → `performance-optimizer` (caching/query review) → `docs-architect` (documentation).
- **Performance Regression**: Master Agent Director → `performance-monitor` (collect metrics) → `performance-optimizer` (apply fixes) → `tmdb-api-expert` / `ef-migration-manager` as needed → `docs-architect` (summarize results).
- **Deployment Readiness**: Master Agent Director → `deployment-project-manager` (checklist & rollout plan) → `performance-monitor` (health checks) → `docs-architect` (release notes).

## 📈 Observability & Feedback
`performance-monitor` maintains the agent scorecards living in `.claude/observability/`. It captures success rates, time-to-resolution, and user satisfaction signals so the Master Agent Director can refine routing. The `session-secretary` keeps concise transcripts to preserve hand-off fidelity between agents and human collaborators.

## 📚 Reference Docs
- [AGENTS.md](./AGENTS.md) – Deep dive into orchestration logic and agent behaviors.
- [../observability/](../observability) – Metrics, evaluation criteria, and feedback loops for the agent program.
- [../commands/](../commands) – Shortcut prompts for invoking agents or updating documentation.
