---
description: "Project health audit — evaluates build, dead code, TODOs, dependencies, and overall codebase quality"
allowed-tools: ["Read", "Bash", "Grep", "Glob"]
---

# Project Health Audit

Run a comprehensive health check on the CineLog codebase. Report findings with severity levels.

## Checks to Perform

### 1. Build Status
- Run `dotnet build` and report result (errors, warnings)

### 2. Dead Code Detection
- Search for unused `using` statements in .cs files
- Search for methods/classes that are never referenced (grep for `private` methods, verify they're called somewhere)
- Look for commented-out code blocks (lines starting with `//` that contain code patterns like `var `, `return `, `if (`)
- Check for empty catch blocks

### 3. TODO/FIXME/HACK Scan
- Grep all .cs, .cshtml, .js files for TODO, FIXME, HACK, TEMP, WORKAROUND
- Report each with file location and context

### 4. Dependency Health
- Check `Ezequiel_Movies.csproj` for NuGet package versions
- Flag any packages that look significantly outdated

### 5. Code Quality Signals
- Check for `Console.WriteLine` in production code (should use `_logger`)
- Check for missing `AsNoTracking()` on read-only queries
- Check for raw SQL or non-parameterized queries
- Verify all controllers have `[Authorize]` where appropriate

### 6. Configuration Health
- Verify no hardcoded secrets, passwords, or connection strings
- Check .gitignore covers sensitive files

## Output Format

```
## CineLog Health Audit — [date]

### Build: ✅/❌
[details]

### Dead Code: X issues found
[list each with file:line]

### TODOs/FIXMEs: X items
[list each with file:line]

### Dependencies: X outdated
[list each]

### Code Quality: X issues
[list each with severity]

### Configuration: ✅/❌
[details]

### Overall Score: X/10
[summary and top 3 recommended actions]
```

Read `lessons-learned.md` from memory before starting. After completing the audit, suggest any new lessons to add.
