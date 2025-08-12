---
description: "Intelligently analyzes recent work and updates only relevant documentation files based on context and changes made"
allowed-tools: ["Read", "Write", "Edit", "MultiEdit", "Bash", "Glob", "Grep", "LS", "WebFetch"]
argument-hint: "[optional: brief description of what was accomplished]"
---

# Smart Documentation Update Command

You are an intelligent documentation coordinator that analyzes recent work and selectively updates only the documentation files that are actually impacted by the changes made.

## Context
This is an ASP.NET Core movie logging application with sophisticated suggestion algorithms, TMDB API integration, and performance optimizations. After completing development tasks, this command intelligently determines which documentation files need updates and what content should be added.

## Your Smart Analysis Process

1. **Context Analysis**: 
   - Review the current conversation to understand what was just accomplished
   - Examine any code changes, new features, or improvements made
   - Identify the scope and nature of the work completed

2. **Impact Assessment**:
   - **README.md** - Update if: new features added, architecture changed, setup process modified
   - **CHANGELOG.md** - Update if: any user-facing changes, new features, bug fixes, performance improvements
   - **PERFORMANCE_OPTIMIZATION_SUMMARY.md** - Update if: performance improvements, caching changes, API optimizations
   - **CLAUDE.md** - Update if: development patterns changed, new AI guidance needed, workflow updates
   - **SESSION_NOTES.md** - Always update with session accomplishments and context

3. **Selective Updates**:
   - **Only update files that are actually impacted** by the recent work
   - **Skip files that don't need changes** - don't update everything just because
   - **Provide clear reasoning** for which files were updated and why
   - **Maintain consistency** across all updated files

## Smart Update Examples

### Example 1: Feature Implementation
```
User completes: "Added user profile page"
Smart Analysis: 
✅ README.md - Add to features section
✅ CHANGELOG.md - Add feature entry  
❌ PERFORMANCE_OPTIMIZATION_SUMMARY.md - No performance impact
❌ CLAUDE.md - No AI guidance changes needed
✅ SESSION_NOTES.md - Always update with session context
```

### Example 2: Performance Optimization  
```
User completes: "Optimized database queries"
Smart Analysis:
❌ README.md - No user-facing changes
✅ CHANGELOG.md - Add performance entry
✅ PERFORMANCE_OPTIMIZATION_SUMMARY.md - Add optimization details
❌ CLAUDE.md - Patterns already documented
✅ SESSION_NOTES.md - Always update with session context
```

### Example 3: Documentation Work
```
User completes: "Cleaned up CLAUDE.md file"
Smart Analysis:
❌ README.md - No feature/architecture changes
❌ CHANGELOG.md - Documentation cleanup not user-facing
❌ PERFORMANCE_OPTIMIZATION_SUMMARY.md - No performance impact
❌ CLAUDE.md - Already updated in the work itself
✅ SESSION_NOTES.md - Always update with session context
```

## Update Guidelines

### When to Update Each File:
- **README.md**: New features, architecture changes, setup modifications, security enhancements
- **CHANGELOG.md**: User-facing changes, new features, bug fixes, major optimizations, deployments  
- **PERFORMANCE_OPTIMIZATION_SUMMARY.md**: Query optimizations, caching improvements, API efficiency gains
- **CLAUDE.md**: New development patterns, AI guidance updates, workflow changes
- **SESSION_NOTES.md**: ALWAYS update with session accomplishments and context

### Smart Decision Making:
1. **Analyze what was actually accomplished** in the recent work
2. **Determine real impact** - don't assume all files need updates
3. **Be selective and purposeful** - quality over quantity
4. **Provide clear reasoning** - explain why files were/weren't updated
5. **Maintain consistency** across all files that are updated

## Change Description
$ARGUMENTS

## Execution Process

1. **Read SESSION_NOTES.md** for current project context
2. **Analyze recent conversation** to understand what was accomplished  
3. **Make intelligent decisions** about which files need updates
4. **Update only relevant files** with appropriate content
5. **Report what was updated and why** - be transparent about decisions
6. **Always update SESSION_NOTES.md** with session context and accomplishments

Remember: **Intelligence over automation** - don't update everything, update what matters.