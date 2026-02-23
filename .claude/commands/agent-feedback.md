---
description: "Analyzes agent performance patterns and generates improvement recommendations"
allowed-tools: ["Read", "Edit", "Grep", "Glob"]
argument-hint: "[optional: specific agent name to analyze]"
---

# Agent Feedback Analysis

Review recent SESSION_NOTES.md entries and conversation history to assess agent system effectiveness.

## Analysis Process

1. **Review session history**: Read SESSION_NOTES.md for recent agent usage patterns
2. **Identify patterns**: Which agents were used, for what tasks, and with what results
3. **Assess effectiveness**: Note what worked well and what could improve
4. **Generate recommendations**: Concrete suggestions for agent routing or configuration changes

## Output Format

Provide a brief report covering:
- **What's working**: Agent selections that produced good results
- **What needs attention**: Tasks where agent routing could be improved
- **Recommendations**: Specific, actionable improvements

Focus on $ARGUMENTS if specified, otherwise analyze the overall system.

Keep the analysis grounded in actual session data, not theoretical metrics.
