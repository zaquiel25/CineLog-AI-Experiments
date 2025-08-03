---
description: "Automatically update all documentation files and session notes after successful changes"
allowed-tools: ["Read", "Write", "Edit", "MultiEdit", "Bash", "Glob", "Grep", "LS", "WebFetch"]
argument-hint: "[optional: specific change description]"
---

# Auto-Update Documentation & Session Notes Command

You are tasked with automatically updating all documentation files and session context in this CineLog-AI-Experiments project after successful code changes.

## Context
This is an ASP.NET Core movie logging application with sophisticated suggestion algorithms, TMDB API integration, and performance optimizations. The project has multiple documentation files and a session secretary agent system that need to stay synchronized with code changes. The SESSION_NOTES.md file maintains cross-session context for development continuity.

## Your Mission

1. **Analyze Recent Changes**: 
   - Check git status and recent commits to understand what changed
   - Examine modified files to understand the scope of changes
   - Identify any new features, performance improvements, or architectural changes

2. **Update Documentation Files**:
   - `README.md` - Update features, architecture overview, and setup instructions
   - `CLAUDE.md` - Update development commands, architecture patterns, and guidance
   - `CHANGELOG.md` - Add new entries for the changes made
   - `PERFORMANCE_OPTIMIZATION_SUMMARY.md` - Update if performance-related changes were made
   - `SESSION_NOTES.md` - Update session context with accomplishments, decisions, and next priorities

3. **Ensure Consistency**:
   - Maintain existing formatting and style
   - Keep technical accuracy
   - Preserve important details while adding new information
   - Follow established documentation patterns

## Specific Instructions

### For README.md:
- Update feature lists if new functionality was added
- Modify architecture sections if core patterns changed
- Update setup instructions if new dependencies were added
- Keep the visual hierarchy and branding consistent

### For CLAUDE.md:
- Add new development commands if any were introduced
- Update architecture patterns if code structure changed
- Add new performance patterns or optimization guidance
- Update build/test/deployment instructions as needed

### For CHANGELOG.md:
- Add a new entry with today's date
- Categorize changes (Features, Performance, Bug Fixes, etc.)
- Be specific about what was changed and why
- Follow the existing changelog format

### For PERFORMANCE_OPTIMIZATION_SUMMARY.md:
- Only update if performance-related changes were made
- Add new optimization techniques or improvements
- Update metrics or performance notes
- Maintain the technical focus

### For SESSION_NOTES.md:
- **Update Current Session**: Add/update the current session entry with today's date
- **Document Accomplishments**: Record what was completed in this session
- **Track Key Decisions**: Note important architectural choices and their rationale
- **Record User Preferences**: Capture any coding patterns or workflow preferences observed
- **Note Blockers/Issues**: Document any problems encountered and their resolution status
- **Set Next Priorities**: Clear action items and context for the next session
- **Maintain Privacy**: Keep notes focused on technical decisions, not sensitive business logic
- **Follow Session Notes Format**: Use the established markdown structure for consistency

## Change Description
$ARGUMENTS

## Important Notes
- Be thorough but concise
- Maintain existing documentation quality and style
- Don't duplicate information across files
- Focus on user-facing changes and developer guidance
- Test that all information is accurate and up-to-date
- **Session Context**: Always read SESSION_NOTES.md first to understand current project state
- **Privacy Protection**: SESSION_NOTES.md is gitignored and contains local development context only

Start by reading SESSION_NOTES.md for context, analyze recent changes, then systematically update each documentation file as needed.