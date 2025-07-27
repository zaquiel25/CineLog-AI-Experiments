---
description: "Quick documentation sync after changes"
allowed-tools: ["Read", "Write", "Edit", "MultiEdit", "Bash", "Glob", "Grep", "LS"]
---

# Quick Documentation Sync

Quickly synchronize all documentation files with recent code changes.

## Task: Update Documentation After Code Changes

1. **Check Recent Changes**:
   - Run `git status` and `git log --oneline -5` to see recent commits
   - Identify what files and features were modified

2. **Update Core Documentation**:
   - Update `README.md` if new features or setup steps were added
   - Update `CLAUDE.md` with any new development patterns or commands
   - Add entries to `CHANGELOG.md` for significant changes
   - Update `PERFORMANCE_OPTIMIZATION_SUMMARY.md` if performance improvements were made

3. **Ensure Consistency**:
   - Maintain existing formatting and style
   - Keep information accurate and current
   - Don't duplicate content between files

Arguments provided: $ARGUMENTS

Execute this task systematically, focusing on keeping documentation synchronized with the current codebase state.