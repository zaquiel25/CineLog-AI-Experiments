# 📚 Documentation Automation Guide

## Overview
Automated documentation update system for the CineLog-AI-Experiments project using Claude Code slash commands.

## 🚀 Quick Start

After making code changes, simply run one of these commands in Claude Code:

```bash
/update-docs "Added user authentication system"
/docs "Fixed performance issues"  
/sync-docs feature "Added movie recommendation engine"
```

## 📋 Available Commands

### 1. `/update-docs [description]` 
**Best for**: Major changes, new features, architecture updates
- Comprehensive analysis of recent git changes
- Updates all documentation files systematically
- Includes web research for best practices
- Most thorough documentation update

### 2. `/docs [description]`
**Best for**: Quick fixes, minor updates, daily development
- Fast synchronization of core documentation
- Lightweight analysis and updates
- Maintains consistency without deep analysis

### 3. `/sync-docs [type] [description]`
**Best for**: Release preparation, structured updates
- Advanced git history analysis (10+ commits)
- Categorized changelog updates with emojis
- Quality assurance and cross-reference validation
- Structured approach with todo tracking

## 📁 Files Automatically Updated

| File | Purpose | Update Frequency |
|------|---------|------------------|
| `README.md` | Project overview, features, setup | Major changes |
| `CLAUDE.md` | Development guidance, patterns | New dev patterns |
| `CHANGELOG.md` | Change history | Every update |
| `PERFORMANCE_OPTIMIZATION_SUMMARY.md` | Performance notes | Performance changes only |

## 🔄 Workflow Integration

### Daily Development
```bash
# After fixing bugs
/docs "Fixed TMDB API rate limiting issue"

# After small features  
/docs "Added movie poster caching"
```

### Feature Development
```bash
# After completing a feature
/update-docs "Implemented advanced suggestion algorithms with session tracking"

# For performance improvements
/sync-docs performance "Optimized database queries with new indexing strategy"
```

### Release Preparation
```bash
# Before releases
/sync-docs release "Version 2.1.0 - Enhanced user experience and performance optimizations"
```

## ✨ Features

### 🤖 Smart Analysis
- Automatically detects changed files via `git status`
- Analyzes commit history for context
- Identifies change types (features, bugs, performance)

### 📝 Intelligent Updates
- Maintains existing documentation style and formatting
- Avoids duplicate information across files
- Preserves technical accuracy
- Updates cross-references automatically

### 🎯 Categorized Changes
- ✨ Features - New functionality
- ⚡ Performance - Speed and optimization improvements  
- 🐛 Bug Fixes - Issue resolutions
- 🔄 Refactoring - Code structure improvements
- 📚 Documentation - Documentation updates

### 🔍 Quality Assurance
- Verifies technical accuracy
- Checks for consistent terminology
- Validates cross-references between documents
- Maintains established documentation standards

## 🛠️ Technical Details

### Command Location
```
.claude/commands/
├── update-docs.md      # Comprehensive updates
├── docs.md             # Quick sync
├── sync-docs.md        # Advanced sync
└── README.md           # Command documentation
```

### Tools Available
Each command has access to:
- File operations (Read, Write, Edit, MultiEdit)
- Search tools (Glob, Grep)
- System commands (Bash, LS)
- Web research (WebFetch - for update-docs only)
- Task tracking (TodoWrite - for sync-docs only)

## 📈 Benefits

1. **Consistency**: All documentation stays synchronized with code changes
2. **Efficiency**: Automated updates save manual documentation time
3. **Quality**: Standardized approach ensures comprehensive coverage
4. **History**: Proper changelog maintenance for project tracking
5. **Onboarding**: Always up-to-date guidance for new developers

## 🔧 Customization

To modify command behavior:
1. Edit the relevant `.claude/commands/*.md` file
2. Update the YAML frontmatter for tool permissions
3. Modify the command instructions and logic
4. Test with a sample change

## 🚨 Best Practices

### Do:
- ✅ Run documentation updates after completing features
- ✅ Provide descriptive change descriptions
- ✅ Use appropriate command for change scope
- ✅ Review generated updates for accuracy

### Don't:
- ❌ Skip documentation updates for "minor" changes
- ❌ Use vague descriptions like "misc fixes"
- ❌ Override command-generated formatting
- ❌ Manually edit documentation without updating commands

## 🎯 Next Steps

1. **Try it out**: Make a small code change and run `/docs "test update"`
2. **Customize**: Modify commands to match your specific needs
3. **Integrate**: Make documentation updates part of your development workflow
4. **Expand**: Add new slash commands for other automation needs

---

*This automation system ensures your documentation is always current, comprehensive, and helpful for both current and future developers working on the CineLog-AI-Experiments project.*