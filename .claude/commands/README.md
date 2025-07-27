# Documentation Update Slash Commands

This directory contains Claude Code slash commands for automatically updating project documentation.

## Available Commands

### `/update-docs [description]`
**Purpose**: Comprehensive documentation update after code changes  
**Tools**: Read, Write, Edit, MultiEdit, Bash, Glob, Grep, LS, WebFetch  
**Usage**: `/update-docs Added new caching system for performance`

**What it does**:
- Analyzes recent git commits and file changes
- Updates README.md with new features and setup instructions
- Updates CLAUDE.md with new development patterns
- Adds entries to CHANGELOG.md with proper categorization
- Updates PERFORMANCE_OPTIMIZATION_SUMMARY.md if performance changes detected

### `/docs [description]`
**Purpose**: Quick documentation synchronization  
**Tools**: Read, Write, Edit, MultiEdit, Bash, Glob, Grep, LS  
**Usage**: `/docs Fixed authentication bug`

**What it does**:
- Quick check of recent changes via git
- Fast update of core documentation files
- Maintains existing formatting and style
- Focuses on essential updates only

### `/sync-docs [type] [description]`
**Purpose**: Advanced documentation sync with git integration  
**Tools**: Read, Write, Edit, MultiEdit, Bash, Glob, Grep, LS, TodoWrite  
**Usage**: `/sync-docs feature Added user preferences system`

**What it does**:
- Comprehensive git history analysis (last 10 commits)
- Categorized changelog updates with emojis (✨ Features, ⚡ Performance, 🐛 Bug Fixes)
- Cross-reference validation between documents
- Quality assurance phase to ensure accuracy

## Documentation Files Managed

1. **README.md** - Main project documentation, features, setup
2. **CLAUDE.md** - Claude Code development guidance and patterns
3. **CHANGELOG.md** - Chronological change history
4. **PERFORMANCE_OPTIMIZATION_SUMMARY.md** - Performance improvements and metrics

## Usage Workflow

1. **After making code changes**: Use `/update-docs "description of changes"`
2. **For quick updates**: Use `/docs "brief description"`
3. **For comprehensive releases**: Use `/sync-docs feature "detailed description"`

## Command Design Principles

- **Automated Analysis**: Commands automatically detect what changed via git
- **Smart Updates**: Only updates relevant sections based on change type
- **Consistency**: Maintains existing formatting and documentation style
- **Non-Destructive**: Preserves existing content while adding new information
- **Quality Focused**: Ensures technical accuracy and proper cross-references

## Installation

Commands are automatically available in this project through the `.claude/commands/` directory structure. Claude Code will detect and load them automatically.

## Contributing

When adding new slash commands:
1. Use descriptive YAML frontmatter with `description` and `allowed-tools`
2. Follow the established command structure and naming conventions
3. Test commands thoroughly before committing
4. Update this README when adding new commands