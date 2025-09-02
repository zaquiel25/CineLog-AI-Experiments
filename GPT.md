## GPT.md

This file provides guidance to OpenAI assistants (GPT / Codex CLI) when working with code in this repository. It mirrors the intent and strictness of `CLAUDE.md`, adapted to Codex CLI tools, sandboxing, and approval flow.

---

## ⚠️ Critical Instructions

- **Do Not Invent:** Implement only what is explicitly requested. No extra features, UI tweaks, or “nice to have” improvements unless the user asks.
- **Safety & Compliance:** Before any change, consider security, privacy, legal, and production impact. Escalate concerns immediately.
- **No Auto-Commit/Push:** Never stage/commit or deploy without explicit user approval.
- **Build Gate:** A task is not done unless `dotnet build` succeeds.

---

## Production Safeguards

- **Never deploy** without explicit user permission (e.g., Azure zipdeploy or any CI/CD command).
- **Never push** to git unless explicitly requested.
- Default to working locally. Keep production stable.

---

## Sandbox, Approvals, and Tools

- **Sandbox modes:**
  - Read-only: cannot write; use patch with approval.
  - Workspace-write: can write inside workspace.
  - Network restricted: avoid external calls unless approved.
- **Approvals (on-request):** Ask for approval before any write requiring elevation or networked action.
- **Edits:** Use `apply_patch` to add/update files. Keep changes minimal and focused.
- **Search & Read:** Prefer `rg` for fast search; use `sed -n` to read file chunks (≤250 lines per chunk).

---

## Planning & Session Management

- **Plan usage:** Maintain a concise plan with `update_plan`. Keep exactly one step `in_progress` until complete.
- **Preambles:** Before running commands, send a one-sentence preamble stating what you’ll do next.
- **Momentum:** Summarize progress briefly after meaningful milestones.

---

## Development Workflow

1) Problem Analysis
- Read the request carefully; confirm assumptions when needed.
- Identify affected areas, edge cases, and user-visible impact.

2) Codebase Investigation
- Use `rg` to find related files, functions, and patterns.
- Favor existing helpers and shared patterns already used in CineLog.

3) Plan & Scope Control
- Outline small, verifiable steps with `update_plan`.
- Do not expand scope beyond the request.

4) Implementation
- Read relevant file sections before editing.
- Keep changes minimal; follow existing style.
- Add professional comments for significant logic (XML docs where applicable in C#).

5) Validation
- Run `dotnet build` to ensure compilation.
- Prefer structured logging over ad-hoc debugging.
- Keep user data isolation correct (filter by `UserId` where applicable).

---

## Commenting & Logging Standards

- **C# Methods:** Provide XML documentation for new public methods.
- **Complex Logic:** Add brief “why” comments where non-obvious decisions are made.
- **Prefixes:** Use FEATURE/FIX/ENHANCEMENT where significant and appropriate in comments/commit messages (only commit on request).
- **Telemetry:** Use CineLog telemetry patterns; avoid noisy logs.

---

## CineLog-Specific Guidance

- Follow established patterns in this repo for:
  - User data isolation and filtering
  - Caching and performance optimizations
  - TMDB API integration heuristics and rate limiting
  - Entity Framework migrations and indexing strategy
- Keep UI/UX unchanged unless explicitly requested.

---

## Differences vs CLAUDE.md (Codex/GPT Specifics)

- **Task tracking:** Use `update_plan` instead of any non-Codex tooling.
- **Command preambles:** Send brief preambles before tool calls to keep the user in sync.
- **apply_patch usage:** Required for file edits; ask approval in read-only sandboxes.
- **File reads:** Respect output limits; read in chunks.

---

## References

- `README.md`: Project overview, stack, and architecture notes.
- `.claude/agents/AGENTS.md`: Domain agent roles and patterns (for context and shared conventions).
- `CLAUDE.md`: Claude-focused guidance; mirror tone and guardrails here.

---

## Quick Command Examples

- Search broadly: `rg -n "keyword"`
- Read partial file: `sed -n '1,200p' path/to/file`
- Build: `dotnet build`

---

## Completion Checklist

- Changes limited to requested scope.
- Security/privacy implications considered; no sensitive data exposure.
- Code compiles (`dotnet build`).
- Comments added where needed; style consistent.
- Plan updated; all steps completed.
