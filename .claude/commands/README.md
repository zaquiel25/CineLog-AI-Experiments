# Claude Code Commands & Skills

## Skills (`.claude/skills/`)

### `/docs [description]`
Smart documentation update. Analyzes recent changes and selectively updates only impacted files (README, CHANGELOG, CLAUDE.md, PERFORMANCE_OPTIMIZATION_SUMMARY).
```
/docs Added new filtering feature to collection page
```

### `/build`
Build verification. Runs `dotnet build` and reports results. Can be auto-invoked by Claude after code changes.
```
/build
```

## Commands (`.claude/commands/`)

### `/session`
Update SESSION_NOTES.md with current session context, accomplishments, and next priorities.
```
/session
```

### `/audit`
Project health check — evaluates build, dead code, TODOs, dependencies, code quality. Run periodically.
```
/audit
```

### `/security`
Security audit — credentials scan, API security, OWASP review, user data isolation, dead code. Run before deployments or when sharing repo.
```
/security
```

### `/agent-feedback [agent name]`
Analyze agent performance patterns and generate improvement recommendations.
```
/agent-feedback cinelog-movie-specialist
```

## Documentation Files Managed

- **README.md** - Features, setup, architecture
- **CLAUDE.md** - Development guidance and patterns
- **CHANGELOG.md** - Change history
- **PERFORMANCE_OPTIMIZATION_SUMMARY.md** - Performance metrics
- **SESSION_NOTES.md** - Session continuity (gitignored)
