# 🤖 CineLog Claude Code Agents

This folder contains specialized Claude Code agents designed for CineLog development. Each agent has deep expertise in specific architectural patterns and operates with focused context for optimal performance.

## 📋 Available Agents

### 🎭 **Master Agent Director**

| Agent | Purpose | Intelligence |
|-------|---------|-------------|
| **Master Agent Director** | Intelligent task orchestrator | Analyzes requests, routes to optimal agents, triggers strategic planning |

**🧠 Capabilities:**
- **Task Analysis** - Parses complexity (Simple/Medium/Complex/Strategic)
- **Agent Selection** - Routes to most efficient specialist agents
- **Strategic Planning** - Auto-triggers comprehensive planning for complex tasks
- **Multi-Agent Coordination** - Orchestrates sequential, parallel, and hierarchical workflows

### 🎬 **Core CineLog Agents**

| Agent | Purpose | Expertise |
|-------|---------|-----------|
| [`cinelog-movie-specialist`](./cinelog-movie-specialist.md) | Movie features & suggestions | MoviesController, suggestion algorithms, user data management |
| [`tmdb-api-expert`](./tmdb-api-expert.md) | External API integration | TMDB service, rate limiting, caching, batch operations |
| [`ef-migration-manager`](./ef-migration-manager.md) | Database operations | EF Core migrations, performance indexes, user isolation |
| [`performance-optimizer`](./performance-optimizer.md) | Performance & caching | IMemoryCache, query optimization, parallel execution |
| [`aspnet-feature-developer`](./aspnet-feature-developer.md) | Full-stack development | MVC patterns, Razor views, AJAX, UI/UX standards |
| [`docs-architect`](./docs-architect.md) | Documentation management | Technical docs, architecture patterns, change tracking |

### 🚀 **Enhanced Development Agents**

| Agent | Purpose | Auto-Trigger |
|-------|---------|--------------|
| `test-writer-fixer` | Comprehensive testing | ✅ After code changes |
| `backend-architect` | Scalable architecture | Manual/Strategic tasks |
| `ui-designer` | Visual design enhancement | ✅ After UI updates |
| `whimsy-injector` | Delightful interactions | ✅ After UI/UX changes |
| `performance-benchmarker` | Performance analysis | Performance tasks |
| `devops-automator` | CI/CD automation | Deployment tasks |
| `api-tester` | API reliability testing | API integration tasks |
| `feedback-synthesizer` | User feedback analysis | Manual/Research tasks |
| `code-refactoring-specialist` | Code quality improvement | ✅ Technical debt accumulation |

## 🎯 Quick Selection Guide

### **By Complexity Level**
- **Complex/Strategic Tasks** → **Master Agent Director** (auto-routes with planning)
- **Simple Tasks** → Direct to specialist agent
- **Uncertain Scope** → **Master Agent Director** (intelligent analysis)

### **By Task Type**
- **Movie Features** → `cinelog-movie-specialist`
- **TMDB Integration** → `tmdb-api-expert`
- **Performance Issues** → `performance-optimizer`
- **Database Changes** → `ef-migration-manager`
- **Full-Stack Features** → `aspnet-feature-developer`
- **Code Quality** → `code-refactoring-specialist`

### **By Domain**
- **Frontend/UI** → `aspnet-feature-developer` + `ui-designer` + `whimsy-injector`
- **Backend/API** → `backend-architect` + `api-tester`
- **Database** → `ef-migration-manager` + `performance-optimizer`
- **Testing** → `test-writer-fixer`
- **Documentation** → `docs-architect`

## 🔥 Auto-Triggered Agents

These agents activate automatically to enhance your development workflow:

```
Code Changes → test-writer-fixer (ensures test coverage)
UI Updates → ui-designer (enhances visual appeal)
UI/UX Changes → whimsy-injector (adds personality)
Technical Debt → code-refactoring-specialist (improves code quality)
```

## 📚 Detailed Documentation

- **[AGENTS.md](./AGENTS.md)** - Complete agent system documentation with examples
- **Individual agent files** - Specific agent configurations and prompts
- **[CLAUDE.md](../CLAUDE.md)** - Main development documentation with agent integration

## 🚀 Usage Examples

```bash
# Automatic agent selection based on task context
"Add movie rating feature" → aspnet-feature-developer
"Fix slow TMDB API calls" → tmdb-api-expert  
"Database needs new index" → ef-migration-manager
"Tests are failing" → test-writer-fixer

# Explicit agent invocation
"Use the performance-benchmarker to analyze suggestion system"
"Use the ui-designer to enhance the movie cards"
"Use the docs-architect to update documentation"
```

## 🎬 CineLog-Specific Patterns

All agents understand and follow CineLog's core patterns:
- **User Data Isolation** - All queries filtered by `UserId`
- **TMDB Integration** - Centralized service with caching and rate limiting
- **AJAX + Server-Side Rendering** - Hybrid architecture for performance
- **Cinema Gold Branding** - Consistent UI/UX standards
- **Performance-First** - Optimized caching and query patterns

---

*For complete agent system documentation and strategic planning examples, see [AGENTS.md](./AGENTS.md)*