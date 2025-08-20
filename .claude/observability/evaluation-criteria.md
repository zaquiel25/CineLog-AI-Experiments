# 🎯 LLM-as-Judge Evaluation Criteria

Quality assessment framework for evaluating agent performance using AI-powered evaluation metrics.

## 🧠 Core Evaluation Framework

### Task Completion Quality (1-10 Scale)

#### **10 - Exceptional**
- Task completed perfectly with innovative solutions
- Exceeds user expectations and requirements
- Demonstrates deep domain expertise
- Provides valuable insights beyond the request
- **Example**: Agent not only fixes bug but identifies root cause and prevents similar issues

#### **8-9 - Excellent**
- Task completed fully and correctly
- Meets all requirements with high quality
- Shows strong domain knowledge
- Professional implementation standards
- **Example**: Implements requested feature with proper error handling and documentation

#### **6-7 - Good**
- Task completed correctly with minor gaps
- Meets core requirements adequately
- Some room for optimization or enhancement
- Follows established patterns
- **Example**: Adds functionality but misses edge case handling

#### **4-5 - Fair**
- Task partially completed or with issues
- Meets basic requirements but lacks polish
- May require follow-up work
- Shows understanding but limited expertise
- **Example**: Implements feature but introduces performance issues

#### **1-3 - Poor**
- Task incomplete or incorrectly implemented
- Misunderstands requirements
- Creates new problems
- Requires significant rework
- **Example**: Agent misinterprets task and implements wrong functionality

## 🎭 Agent-Specific Evaluation Criteria

### `cinelog-movie-specialist`

**Technical Excellence** (Weight: 30%)
- ✅ Proper user data isolation with UserId filtering
- ✅ Correct suggestion algorithm implementation
- ✅ AJAX reshuffle functionality working
- ✅ Session state management for anti-repetition

**Domain Expertise** (Weight: 25%)
- ✅ Understanding of movie data relationships
- ✅ Proper TMDB API integration patterns
- ✅ Suggestion variety and quality
- ✅ User experience optimization

**Code Quality** (Weight: 25%)
- ✅ Professional commenting and documentation
- ✅ Following CineLog architectural patterns
- ✅ Performance-conscious implementation
- ✅ Error handling and edge cases

**User Satisfaction** (Weight: 20%)
- ✅ Meets user's stated requirements
- ✅ Intuitive and expected behavior
- ✅ No breaking changes to existing functionality
- ✅ Improves overall user experience

### `performance-optimizer`

**Performance Impact** (Weight: 40%)
- ✅ Measurable performance improvement
- ✅ Proper caching strategy implementation
- ✅ Database query optimization
- ✅ API call efficiency improvements

**Technical Implementation** (Weight: 30%)
- ✅ Correct async/await patterns
- ✅ Proper memory management
- ✅ Parallel execution where appropriate
- ✅ Monitoring and metrics integration

**System Safety** (Weight: 20%)
- ✅ No performance regression in other areas
- ✅ Maintains existing functionality
- ✅ Proper error handling for optimizations
- ✅ Rollback-friendly changes

**Documentation & Insights** (Weight: 10%)
- ✅ Clear performance metrics provided
- ✅ Optimization rationale explained
- ✅ Future improvement recommendations
- ✅ Performance monitoring suggestions

### `aspnet-feature-developer`

**Feature Completeness** (Weight: 35%)
- ✅ All requested functionality implemented
- ✅ Proper MVC pattern implementation
- ✅ Bootstrap/UI integration working
- ✅ Authentication and authorization correct

**Code Quality** (Weight: 25%)
- ✅ Clean, maintainable code structure
- ✅ Proper separation of concerns
- ✅ Following ASP.NET Core best practices
- ✅ Comprehensive error handling

**User Experience** (Weight: 25%)
- ✅ Intuitive interface design
- ✅ Responsive design implementation
- ✅ Accessibility considerations
- ✅ Performance optimization

**Integration** (Weight: 15%)
- ✅ Seamless integration with existing codebase
- ✅ No breaking changes
- ✅ Proper database integration
- ✅ API compatibility maintained

## 🔍 Automated Quality Checks

### Code Quality Indicators
```yaml
high_quality_signals:
  - comprehensive_comments: "All new methods have XML documentation"
  - error_handling: "Try-catch blocks for external calls"
  - user_isolation: "All queries filtered by UserId"
  - performance_conscious: "Async/await patterns used"
  - security_aware: "No hardcoded secrets or credentials"

quality_red_flags:
  - missing_documentation: "New methods without comments"
  - security_risks: "Exposed user data or credentials"
  - performance_issues: "N+1 queries or blocking calls"
  - broken_patterns: "Deviates from established conventions"
  - incomplete_implementation: "TODO comments or missing features"
```

### Domain-Specific Quality Patterns

#### Movie Domain Quality
```yaml
movie_specialist_excellence:
  - suggestion_quality: "Variety and relevance of suggestions"
  - data_accuracy: "Proper TMDB data mapping"
  - user_experience: "Intuitive suggestion interactions"
  - performance: "Fast suggestion loading times"

movie_specialist_issues:
  - repetitive_suggestions: "Same movies appearing repeatedly"
  - poor_filtering: "Blacklisted movies showing up"
  - slow_performance: "Long loading times for suggestions"
  - ui_problems: "Broken AJAX or navigation issues"
```

#### Performance Domain Quality
```yaml
performance_optimizer_excellence:
  - measurable_improvement: "Clear before/after metrics"
  - architectural_soundness: "Proper caching patterns"
  - monitoring_integration: "Performance tracking included"
  - scalability_consideration: "Solutions work at scale"

performance_optimizer_issues:
  - premature_optimization: "Optimizing non-bottlenecks"
  - regression_risk: "Changes that might break functionality"
  - missing_metrics: "No way to measure improvement"
  - over_engineering: "Complex solutions for simple problems"
```

## 🔁 Feedback Integration

### User Satisfaction Signals
```yaml
positive_feedback_indicators:
  - task_completion: "User marks task as complete"
  - follow_up_engagement: "User builds on the solution"
  - explicit_praise: "User expresses satisfaction"
  - repeated_agent_use: "User requests same agent again"

negative_feedback_indicators:
  - immediate_corrections: "User immediately asks for changes"
  - abandon_session: "User stops working after agent response"
  - explicit_dissatisfaction: "User expresses frustration"
  - alternative_approach: "User seeks different solution"
```

### Continuous Improvement Triggers
```yaml
improvement_opportunities:
  - pattern_recognition: "Identify recurring issues"
  - success_pattern_analysis: "Understand what works well"
  - failure_root_cause: "Diagnose systematic problems"
  - optimization_potential: "Find efficiency improvements"
```

## 📊 Quality Score Calculation

### Weighted Scoring Formula
```
Overall Quality Score = (
  Task_Completion * 0.30 +
  Technical_Excellence * 0.25 +
  Domain_Expertise * 0.20 +
  Code_Quality * 0.15 +
  User_Satisfaction * 0.10
)
```

### Agent Performance Thresholds
- **8.0+ = Excellent Performance** - Agent operating at peak efficiency
- **6.0-7.9 = Good Performance** - Agent meeting expectations
- **4.0-5.9 = Needs Improvement** - Agent requires optimization
- **<4.0 = Critical Issues** - Agent needs immediate attention

---

*This evaluation framework implements LLM-as-judge principles to create measurable, improvable agent performance standards.*