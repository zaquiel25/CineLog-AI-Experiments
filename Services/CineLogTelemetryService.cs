using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics;

namespace Ezequiel_Movies.Services
{
    /// <summary>
    /// CineLog-specific telemetry service for Application Insights monitoring.
    /// Provides comprehensive production monitoring for user experience, performance optimization validation,
    /// and business intelligence tracking for the CineLog movie suggestion platform.
    /// 
    /// FEATURE: Production monitoring for 70-90% database performance improvements validation
    /// FEATURE: TMDB API response time and usage tracking
    /// FEATURE: User authentication success rate monitoring
    /// FEATURE: Suggestion system performance and accuracy metrics
    /// FEATURE: Cache hit rate optimization tracking
    /// </summary>
    public class CineLogTelemetryService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<CineLogTelemetryService> _logger;

        public CineLogTelemetryService(TelemetryClient telemetryClient, ILogger<CineLogTelemetryService> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        #region User Authentication Monitoring

        /// <summary>
        /// Tracks user authentication attempts and success rates.
        /// Critical for monitoring user experience and identifying authentication issues.
        /// </summary>
        public void TrackUserAuthentication(string authenticationType, bool isSuccessful, string? userId = null, TimeSpan? duration = null)
        {
            var eventTelemetry = new EventTelemetry("UserAuthentication");
            eventTelemetry.Properties["AuthenticationType"] = authenticationType; // "Identity", "Google"
            eventTelemetry.Properties["IsSuccessful"] = isSuccessful.ToString();
            eventTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            
            if (duration.HasValue)
            {
                eventTelemetry.Metrics["AuthenticationDurationMs"] = duration.Value.TotalMilliseconds;
            }

            _telemetryClient.TrackEvent(eventTelemetry);
            _logger.LogInformation("Authentication tracking: Type={AuthType}, Success={Success}, Duration={Duration}ms", 
                authenticationType, isSuccessful, duration?.TotalMilliseconds);
        }

        #endregion

        #region Database Performance Monitoring

        /// <summary>
        /// Tracks database query performance to validate recent 70-90% performance improvements.
        /// Essential for monitoring the production impact of database optimization indexes.
        /// </summary>
        public void TrackDatabaseQuery(string queryType, TimeSpan duration, int resultCount, string? userId = null)
        {
            var dependencyTelemetry = new DependencyTelemetry
            {
                Type = "SQL",
                Name = $"Database_{queryType}",
                Data = queryType,
                Duration = duration,
                Success = true
            };

            dependencyTelemetry.Properties["QueryType"] = queryType; // "SuggestionQuery", "WishlistCheck", "SearchQuery", etc.
            dependencyTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            dependencyTelemetry.Metrics["ResultCount"] = resultCount;
            dependencyTelemetry.Metrics["DurationMs"] = duration.TotalMilliseconds;

            _telemetryClient.TrackDependency(dependencyTelemetry);
            
            // Log performance alerts for regressions
            if (duration.TotalMilliseconds > 2000) // Alert if query takes longer than 2 seconds
            {
                _logger.LogWarning("Slow database query detected: {QueryType} took {Duration}ms, returned {Count} results", 
                    queryType, duration.TotalMilliseconds, resultCount);
            }
        }

        /// <summary>
        /// Tracks cache performance to validate optimization effectiveness.
        /// </summary>
        public void TrackCacheOperation(string cacheType, string operation, bool isHit, TimeSpan? duration = null)
        {
            var eventTelemetry = new EventTelemetry("CacheOperation");
            eventTelemetry.Properties["CacheType"] = cacheType; // "TMDB", "UserBlacklist", "UserWishlist", "MovieDetails"
            eventTelemetry.Properties["Operation"] = operation; // "Get", "Set", "Remove", "Invalidate"
            eventTelemetry.Properties["IsHit"] = isHit.ToString();
            
            if (duration.HasValue)
            {
                eventTelemetry.Metrics["DurationMs"] = duration.Value.TotalMilliseconds;
            }

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        #endregion

        #region TMDB API Monitoring

        /// <summary>
        /// Tracks TMDB API calls for performance, rate limiting, and cost optimization monitoring.
        /// Critical for validating API usage optimization and monitoring external dependency health.
        /// </summary>
        public void TrackTmdbApiCall(string endpoint, TimeSpan duration, bool isSuccessful, int? httpStatusCode = null, bool isCacheHit = false)
        {
            var dependencyTelemetry = new DependencyTelemetry
            {
                Type = "HTTP",
                Name = $"TMDB_{endpoint}",
                Data = $"https://api.themoviedb.org/3/{endpoint}",
                Duration = duration,
                Success = isSuccessful,
                ResultCode = httpStatusCode?.ToString()
            };

            dependencyTelemetry.Properties["Endpoint"] = endpoint;
            dependencyTelemetry.Properties["IsCacheHit"] = isCacheHit.ToString();
            dependencyTelemetry.Metrics["DurationMs"] = duration.TotalMilliseconds;

            _telemetryClient.TrackDependency(dependencyTelemetry);

            // Track API rate limiting and performance issues
            if (!isSuccessful || duration.TotalMilliseconds > 5000)
            {
                _logger.LogWarning("TMDB API issue: Endpoint={Endpoint}, Success={Success}, Duration={Duration}ms, Status={Status}", 
                    endpoint, isSuccessful, duration.TotalMilliseconds, httpStatusCode);
            }
        }

        /// <summary>
        /// Tracks TMDB API usage statistics for cost and quota monitoring.
        /// </summary>
        public void TrackTmdbApiUsage(string endpoint, int requestCount, string? userId = null)
        {
            var eventTelemetry = new EventTelemetry("TmdbApiUsage");
            eventTelemetry.Properties["Endpoint"] = endpoint;
            eventTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            eventTelemetry.Metrics["RequestCount"] = requestCount;

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        #endregion

        #region Suggestion System Monitoring

        /// <summary>
        /// Tracks suggestion system performance and user engagement.
        /// Essential for validating recent performance optimizations and user experience improvements.
        /// </summary>
        public void TrackSuggestionGeneration(string suggestionType, int suggestionCount, TimeSpan duration, string? userId = null)
        {
            var eventTelemetry = new EventTelemetry("SuggestionGeneration");
            eventTelemetry.Properties["SuggestionType"] = suggestionType; // "Director", "Actor", "Genre", "Decade", "Trending", "Random"
            eventTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            eventTelemetry.Metrics["SuggestionCount"] = suggestionCount;
            eventTelemetry.Metrics["GenerationDurationMs"] = duration.TotalMilliseconds;

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        /// <summary>
        /// Tracks user interaction with suggestions to measure engagement and effectiveness.
        /// </summary>
        public void TrackSuggestionInteraction(string suggestionType, string action, string? userId = null, int? tmdbId = null)
        {
            var eventTelemetry = new EventTelemetry("SuggestionInteraction");
            eventTelemetry.Properties["SuggestionType"] = suggestionType;
            eventTelemetry.Properties["Action"] = action; // "AddToWishlist", "AddToBlacklist", "ViewDetails", "Shuffle"
            eventTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            
            if (tmdbId.HasValue)
            {
                eventTelemetry.Properties["TmdbId"] = tmdbId.Value.ToString();
            }

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        #endregion

        #region User Experience Monitoring

        /// <summary>
        /// Tracks page load performance to monitor user experience improvements.
        /// </summary>
        public void TrackPageLoad(string pageName, TimeSpan duration, bool isSuccessful = true, string? userId = null)
        {
            var pageViewTelemetry = new PageViewTelemetry(pageName)
            {
                Duration = duration
            };

            pageViewTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            pageViewTelemetry.Properties["IsSuccessful"] = isSuccessful.ToString();
            pageViewTelemetry.Metrics["LoadDurationMs"] = duration.TotalMilliseconds;

            _telemetryClient.TrackPageView(pageViewTelemetry);

            // Alert on slow page loads
            if (duration.TotalMilliseconds > 3000)
            {
                _logger.LogWarning("Slow page load detected: {PageName} took {Duration}ms", pageName, duration.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Tracks AJAX operations for dynamic content loading performance.
        /// </summary>
        public void TrackAjaxOperation(string operation, TimeSpan duration, bool isSuccessful, string? userId = null)
        {
            var eventTelemetry = new EventTelemetry("AjaxOperation");
            eventTelemetry.Properties["Operation"] = operation; // "ShuffleSuggestions", "AddToWishlist", "RemoveFromBlacklist"
            eventTelemetry.Properties["IsSuccessful"] = isSuccessful.ToString();
            eventTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            eventTelemetry.Metrics["DurationMs"] = duration.TotalMilliseconds;

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        #endregion

        #region Business Intelligence Monitoring

        /// <summary>
        /// Tracks feature usage patterns for business intelligence and optimization insights.
        /// </summary>
        public void TrackFeatureUsage(string featureName, string? userId = null, Dictionary<string, string>? additionalProperties = null)
        {
            var eventTelemetry = new EventTelemetry("FeatureUsage");
            eventTelemetry.Properties["FeatureName"] = featureName; // "GoogleAuth", "WishlistManagement", "BlacklistManagement"
            eventTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            eventTelemetry.Properties["Timestamp"] = DateTimeOffset.UtcNow.ToString();

            if (additionalProperties != null)
            {
                foreach (var property in additionalProperties)
                {
                    eventTelemetry.Properties[property.Key] = property.Value;
                }
            }

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        /// <summary>
        /// Tracks user session information for analytics and user journey mapping.
        /// </summary>
        public void TrackUserSession(string sessionEvent, string? userId = null, TimeSpan? sessionDuration = null)
        {
            var eventTelemetry = new EventTelemetry("UserSession");
            eventTelemetry.Properties["SessionEvent"] = sessionEvent; // "Start", "End", "Timeout"
            eventTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            
            if (sessionDuration.HasValue)
            {
                eventTelemetry.Metrics["SessionDurationMs"] = sessionDuration.Value.TotalMilliseconds;
            }

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        #endregion

        #region Error and Exception Monitoring

        /// <summary>
        /// Tracks custom exceptions with CineLog-specific context.
        /// </summary>
        public void TrackException(Exception exception, string context, string? userId = null, Dictionary<string, string>? additionalProperties = null)
        {
            var exceptionTelemetry = new ExceptionTelemetry(exception);
            exceptionTelemetry.Properties["Context"] = context; // "TmdbApiCall", "DatabaseQuery", "SuggestionGeneration"
            exceptionTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            exceptionTelemetry.Properties["Timestamp"] = DateTimeOffset.UtcNow.ToString();

            if (additionalProperties != null)
            {
                foreach (var property in additionalProperties)
                {
                    exceptionTelemetry.Properties[property.Key] = property.Value;
                }
            }

            _telemetryClient.TrackException(exceptionTelemetry);
            _logger.LogError(exception, "Exception tracked in Application Insights: Context={Context}, UserId={UserId}", context, userId);
        }

        #endregion

        #region Performance Optimization Validation

        /// <summary>
        /// Tracks performance metrics specifically for validating recent database optimizations.
        /// Enables A/B testing and regression detection for the 70-90% performance improvements.
        /// </summary>
        public void TrackPerformanceOptimization(string optimizationType, string metricName, double value, string? userId = null)
        {
            var eventTelemetry = new EventTelemetry("PerformanceOptimization");
            eventTelemetry.Properties["OptimizationType"] = optimizationType; // "DatabaseIndexes", "CacheStrategy", "TmdbApiOptimization"
            eventTelemetry.Properties["MetricName"] = metricName; // "QueryDuration", "CacheHitRate", "ApiCallCount"
            eventTelemetry.Properties["UserId"] = userId ?? "Anonymous";
            eventTelemetry.Metrics[metricName] = value;

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        /// <summary>
        /// Creates a comprehensive performance tracking scope for detailed operation monitoring.
        /// </summary>
        public IDisposable CreatePerformanceTrackingScope(string operationName, string? userId = null)
        {
            return new PerformanceTrackingScope(_telemetryClient, operationName, userId, _logger);
        }

        #endregion

        /// <summary>
        /// Flushes all pending telemetry data to Application Insights.
        /// Should be called before application shutdown.
        /// </summary>
        public void Flush()
        {
            _telemetryClient.Flush();
            _logger.LogInformation("Application Insights telemetry flushed");
        }
    }

    /// <summary>
    /// Performance tracking scope for detailed operation monitoring with automatic duration calculation.
    /// </summary>
    public class PerformanceTrackingScope : IDisposable
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly string _operationName;
        private readonly string? _userId;
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;
        private readonly IOperationHolder<RequestTelemetry> _operation;

        public PerformanceTrackingScope(TelemetryClient telemetryClient, string operationName, string? userId, ILogger logger)
        {
            _telemetryClient = telemetryClient;
            _operationName = operationName;
            _userId = userId;
            _logger = logger;
            _stopwatch = Stopwatch.StartNew();
            
            // Start Application Insights operation tracking
            _operation = _telemetryClient.StartOperation<RequestTelemetry>(_operationName);
            _operation.Telemetry.Properties["UserId"] = userId ?? "Anonymous";
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            
            // Complete the operation with duration
            _operation.Telemetry.Duration = _stopwatch.Elapsed;
            _operation.Telemetry.Success = true;
            
            _telemetryClient.StopOperation(_operation);
            
            _logger.LogInformation("Performance tracking completed: {OperationName} took {Duration}ms", 
                _operationName, _stopwatch.ElapsedMilliseconds);
        }
    }
}