---
name: docs
description: Update project documentation after code changes. Analyzes recent work and selectively updates only impacted files (README, CHANGELOG, CLAUDE.md, PERFORMANCE_OPTIMIZATION_SUMMARY).
disable-model-invocation: true
allowed-tools: ["Read", "Write", "Edit", "MultiEdit", "Bash", "Glob", "Grep"]
---

# Smart Documentation Update

Analyze recent changes and update only the documentation files that are actually impacted.

## Process

1. **Detect changes**: Run `git status` and `git log --oneline -5` to identify what changed
2. **Assess impact**: Determine which docs need updates:
   - **README.md** — if: new features, architecture changes, setup modifications
   - **CHANGELOG.md** — if: any user-facing changes, features, bug fixes, performance improvements
   - **PERFORMANCE_OPTIMIZATION_SUMMARY.md** — if: performance improvements, caching changes
   - **CLAUDE.md** — if: development patterns changed, new workflow patterns
   - **SESSION_NOTES.md** — always update with session context
3. **Update selectively**: Only touch files that need it. Skip files with no relevant changes.
4. **Report**: Explain which files were updated and why, and which were skipped.

## Guidelines

- Maintain existing formatting and style in each file
- Don't duplicate content between files
- Be concise — quality over quantity
- CHANGELOG entries use categories: Added, Changed, Fixed, Performance, Security

## Context

$ARGUMENTS
