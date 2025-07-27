---
description: "Comprehensive documentation synchronization with git integration"
allowed-tools: ["Read", "Write", "Edit", "MultiEdit", "Bash", "Glob", "Grep", "LS", "TodoWrite"]
argument-hint: "[change-type] [description]"
---

# Documentation Synchronization Command

Automatically synchronize all project documentation with recent code changes, using git history and file analysis.

## Execution Plan

You will systematically update all documentation to reflect recent changes:

### 1. Change Analysis Phase
- Examine `git status` and `git log --oneline -10` for recent activity
- Analyze modified files to understand scope of changes
- Identify new features, bug fixes, performance improvements, or architectural changes
- Parse any commit messages for context

### 2. Documentation Update Phase

**README.md Updates:**
- Add new features to the features section
- Update setup/installation instructions if dependencies changed
- Modify architecture overview if core patterns changed
- Update any example code or usage instructions

**CLAUDE.md Updates:**
- Add new development commands or tools if introduced
- Update architecture patterns section with new code patterns
- Add new performance optimization guidance
- Update build/test/deployment instructions
- Add new development workflow patterns

**CHANGELOG.md Updates:**
- Create new entry with today's date (2025-07-27)
- Categorize changes: ✨ Features, ⚡ Performance, 🐛 Bug Fixes, 🔄 Refactoring, 📚 Documentation
- Provide specific details about what changed and why
- Follow existing changelog format and style

**PERFORMANCE_OPTIMIZATION_SUMMARY.md Updates:**
- Only update if performance-related changes were detected
- Add new optimization techniques or metrics
- Document any database, caching, or API improvements
- Update performance benchmarks if available

### 3. Quality Assurance Phase
- Ensure all cross-references between documents are accurate
- Verify no duplicate information exists across files
- Maintain consistent formatting and terminology
- Check that all new information is technically accurate

## Context Information
Change Type: $ARGUMENTS

## Execution Notes
- Be thorough but avoid unnecessary verbosity
- Focus on user-facing changes and developer guidance
- Maintain the established voice and style of each document
- Ensure all updates are factual and based on actual code changes

Begin by analyzing recent git history and modified files, then systematically update each documentation file as appropriate.