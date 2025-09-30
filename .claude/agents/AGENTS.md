# 🤖 Claude Code Subagents System

CineLog coordinates its Claude Code specialists through the **Master Agent Director**. The director evaluates every request, determines scope and complexity, and assigns the right combination of subagents to deliver reliable results for the ASP.NET Core MVC stack, TMDB integrations, and deployment workflows.

## 🎭 Master Agent Director

### 🧠 Task Intake Flow
1. **Parse** the request to identify user goal, affected layers (UI, service, data, infrastructure), and urgency.
2. **Assess** complexity (Simple · Medium · Complex · Strategic) using recent project history and telemetry.
3. **Plan** when Medium+ work touches multiple components or requires coordination.
4. **Match** the request to specialists with relevant context files.
5. **Route** tasks with clear objectives, success criteria, and follow-up expectations.
6. **Monitor** progress via session notes and observability metrics, re-routing if blockers surface.

### 🎚️ Complexity Bands
- **Simple** – Single-file tweaks, typo fixes, and content updates. Director routes directly to one agent (e.g., `aspnet-feature-developer` for Razor fixes, `docs-architect` for doc edits).
- **Medium** – Localized feature enhancements or performance fixes. Light planning ensures dependencies (e.g., `cinelog-movie-specialist` + `performance-optimizer` for suggestion latency).
- **Complex** – Cross-layer features or schema changes. Director drafts a mini-plan, assigns primary (`ef-migration-manager`, `aspnet-feature-developer`), and queues validation (`performance-monitor`, `docs-architect`).
- **Strategic** – Deployments, architecture shifts, or multi-sprint initiatives. Director creates a phased roadmap and orchestrates several cycles with `deployment-project-manager`, `performance-monitor`, and domain experts.

## 🧭 Decision Matrix
| Task Pattern | Primary Agent | Supporting Agents | Notes |
|--------------|---------------|-------------------|-------|
| Movie feature enhancement | `cinelog-movie-specialist` | `aspnet-feature-developer`, `docs-architect` | Domain logic + MVC updates + documentation |
| TMDB data or API issues | `tmdb-api-expert` | `performance-optimizer`, `docs-architect` | Validate caching, rate limits, and update runbooks |
| Schema/migration work | `ef-migration-manager` | `cinelog-movie-specialist`, `docs-architect` | Ensure domain alignment and record migrations |
| Performance regression | `performance-optimizer` | `performance-monitor`, `tmdb-api-expert` | Measure → optimize → re-measure |
| Documentation refresh | `docs-architect` | `cinelog-movie-specialist` or requester | Capture latest behaviors and release notes |
| Release planning | `deployment-project-manager` | `performance-monitor`, `docs-architect` | Coordinate checklists, telemetry, and change comms |

## 🚀 Example Workflows

### Complex Feature: "Add watch-location filtering to the collection view"
1. **Master Agent Director** drafts plan (model changes + UI filters + caching impact).
2. `ef-migration-manager` updates entities/migrations for watch locations.
3. `cinelog-movie-specialist` adjusts query logic and filtering helpers.
4. `aspnet-feature-developer` updates Razor views and AJAX handlers.
5. `performance-optimizer` revisits caching to avoid duplicate lookups.
6. `docs-architect` documents the workflow in README/CLAUDE.md and updates changelog.
7. `performance-monitor` verifies load time and telemetry remain within budget.

### Medium Task: "TMDB poster URLs are timing out"
1. **Master Agent Director** flags Medium complexity.
2. `tmdb-api-expert` inspects recent API responses and cache misses.
3. `performance-optimizer` adds fallback caching and adjusts retry policy.
4. `performance-monitor` captures before/after latency for observability records.

### Simple Task: "Fix typo in Password Gate hero text"
- Director routes straight to `aspnet-feature-developer`; `docs-architect` notes the change if part of a release.

## 🧑‍💻 Agent Profiles
- **`cinelog-movie-specialist`** – Owns MoviesController, journal/collection experience, suggestion algorithms, and ensures user data isolation across queries.
- **`aspnet-feature-developer`** – Full-stack MVC builder covering Razor, controllers, validation, and AJAX interactions that align with the Cinema Gold theme.
- **`tmdb-api-expert`** – Manages `TmdbService`, credential usage, rate limiting, provider fallbacks, and ensures external data stays resilient.
- **`ef-migration-manager`** – Designs and applies EF Core migrations, enforces index strategy, and keeps `ApplicationDbContext` in sync with the domain model.
- **`performance-optimizer`** – Tunes caching, database, and API performance with telemetry-driven insights; collaborates closely with the monitor.
- **`performance-monitor`** – Operates the observability toolkit under `.claude/observability/`, logging regression checks, success rates, and post-optimization reports.
- **`docs-architect`** – Maintains README, CLAUDE.md, CHANGELOG, and supporting narratives so every change is discoverable.
- **`deployment-project-manager`** – Coordinates production readiness, secret management reminders, and rollout validation.
- **`session-secretary`** – Automatically records session context and preserves continuity between handoffs.

## 📏 Operational Norms
- Always route through the Master Agent Director unless a specialist match is obvious and low risk.
- Prefer multi-agent handoffs over single-agent mega tasks to keep context windows focused.
- Log notable decisions in `SESSION_NOTES.md` or via `docs-architect` to maintain institutional memory.
- When performance or reliability is affected, pair `performance-optimizer` with `performance-monitor` to capture before/after evidence.
- For deployments, involve `deployment-project-manager` early to avoid last-minute configuration drift.

The Master Agent Director, combined with these focused specialists, keeps CineLog's development velocity high while preserving quality, observability, and documentation discipline.
