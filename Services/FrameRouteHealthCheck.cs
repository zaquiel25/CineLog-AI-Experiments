using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace Ezequiel_Movies.Services
{
    /// <summary>
    /// Custom health check for FrameRoute application components.
    /// Monitors database connectivity, TMDB API availability, and cache performance
    /// to provide comprehensive production health status.
    /// </summary>
    public class FrameRouteHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<FrameRouteHealthCheck> _logger;

        public FrameRouteHealthCheck(
            ApplicationDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            IMemoryCache memoryCache,
            ILogger<FrameRouteHealthCheck> logger)
        {
            _dbContext = dbContext;
            _httpClient = httpClientFactory.CreateClient();
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthCheckData = new Dictionary<string, object>();
            var overallHealth = HealthStatus.Healthy;
            var issues = new List<string>();

            try
            {
                // 1. Database connectivity check
                var dbStopwatch = Stopwatch.StartNew();
                try
                {
                    var dbCanConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                    dbStopwatch.Stop();
                    
                    healthCheckData["DatabaseConnectivity"] = dbCanConnect;
                    healthCheckData["DatabaseResponseTime"] = $"{dbStopwatch.ElapsedMilliseconds}ms";
                    
                    if (!dbCanConnect)
                    {
                        overallHealth = HealthStatus.Unhealthy;
                        issues.Add("Database connectivity failed");
                    }
                    else if (dbStopwatch.ElapsedMilliseconds > 2000)
                    {
                        overallHealth = HealthStatus.Degraded;
                        issues.Add($"Database response time slow: {dbStopwatch.ElapsedMilliseconds}ms");
                    }

                }
                catch (Exception ex)
                {
                    dbStopwatch.Stop();
                    overallHealth = HealthStatus.Unhealthy;
                    issues.Add($"Database health check failed: {ex.Message}");
                    healthCheckData["DatabaseError"] = ex.Message;
                    
                }

                // 2. TMDB API availability check
                var tmdbStopwatch = Stopwatch.StartNew();
                try
                {
                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "FrameRoute-HealthCheck/1.0");
                    
                    var response = await _httpClient.GetAsync("https://api.themoviedb.org/3/configuration", cancellationToken);
                    tmdbStopwatch.Stop();
                    
                    healthCheckData["TmdbApiStatus"] = response.IsSuccessStatusCode;
                    healthCheckData["TmdbResponseTime"] = $"{tmdbStopwatch.ElapsedMilliseconds}ms";
                    healthCheckData["TmdbStatusCode"] = (int)response.StatusCode;
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        if (overallHealth == HealthStatus.Healthy)
                            overallHealth = HealthStatus.Degraded;
                        issues.Add($"TMDB API responded with status: {response.StatusCode}");
                    }
                    else if (tmdbStopwatch.ElapsedMilliseconds > 5000)
                    {
                        if (overallHealth == HealthStatus.Healthy)
                            overallHealth = HealthStatus.Degraded;
                        issues.Add($"TMDB API response time slow: {tmdbStopwatch.ElapsedMilliseconds}ms");
                    }

                }
                catch (Exception ex)
                {
                    tmdbStopwatch.Stop();
                    if (overallHealth == HealthStatus.Healthy)
                        overallHealth = HealthStatus.Degraded;
                    issues.Add($"TMDB API health check failed: {ex.Message}");
                    healthCheckData["TmdbError"] = ex.Message;
                    
                }

                // 3. Memory cache functionality check
                try
                {
                    var testKey = $"health_check_{DateTime.UtcNow.Ticks}";
                    var testValue = "health_check_value";
                    
                    _memoryCache.Set(testKey, testValue, TimeSpan.FromMinutes(1));
                    var retrievedValue = _memoryCache.Get<string>(testKey);
                    _memoryCache.Remove(testKey);
                    
                    healthCheckData["MemoryCacheOperational"] = retrievedValue == testValue;
                    
                    if (retrievedValue != testValue)
                    {
                        if (overallHealth == HealthStatus.Healthy)
                            overallHealth = HealthStatus.Degraded;
                        issues.Add("Memory cache not functioning properly");
                    }
                }
                catch (Exception ex)
                {
                    if (overallHealth == HealthStatus.Healthy)
                        overallHealth = HealthStatus.Degraded;
                    issues.Add($"Memory cache health check failed: {ex.Message}");
                    healthCheckData["MemoryCacheError"] = ex.Message;
                    
                }

                // 4. Performance optimization validation
                try
                {
                    // Check if recent database performance optimizations are active
                    var indexCheckStopwatch = Stopwatch.StartNew();
                    var indexExists = await CheckDatabaseIndexesAsync(cancellationToken);
                    indexCheckStopwatch.Stop();
                    
                    healthCheckData["DatabaseOptimizationsActive"] = indexExists;
                    healthCheckData["IndexCheckTime"] = $"{indexCheckStopwatch.ElapsedMilliseconds}ms";
                    
                    if (!indexExists)
                    {
                        issues.Add("Database performance optimizations not detected");
                    }

                }
                catch (Exception ex)
                {
                    issues.Add($"Performance optimization check failed: {ex.Message}");
                    healthCheckData["OptimizationCheckError"] = ex.Message;
                    
                }

                // 5. System resources check
                try
                {
                    var process = Process.GetCurrentProcess();
                    healthCheckData["WorkingSetMB"] = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2);
                    healthCheckData["PrivateMemoryMB"] = Math.Round(process.PrivateMemorySize64 / 1024.0 / 1024.0, 2);
                    healthCheckData["ProcessorTime"] = process.TotalProcessorTime.ToString();
                    
                    // Alert if memory usage is very high
                    var workingSetMB = process.WorkingSet64 / 1024.0 / 1024.0;
                    if (workingSetMB > 500) // Alert if using more than 500MB
                    {
                        if (overallHealth == HealthStatus.Healthy)
                            overallHealth = HealthStatus.Degraded;
                        issues.Add($"High memory usage: {workingSetMB:F0}MB");
                    }
                }
                catch (Exception ex)
                {
                    healthCheckData["SystemResourcesError"] = ex.Message;
                    _logger.LogWarning(ex, "Failed to collect system resource metrics during health check");
                }

                // Create result message
                var resultMessage = overallHealth == HealthStatus.Healthy 
                    ? "All FrameRoute components are healthy" 
                    : $"Health issues detected: {string.Join(", ", issues)}";


                return new HealthCheckResult(overallHealth, resultMessage, data: healthCheckData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with unexpected exception");
                
                return new HealthCheckResult(HealthStatus.Unhealthy, 
                    $"Health check failed: {ex.Message}", 
                    ex, 
                    healthCheckData);
            }
        }

        /// <summary>
        /// Checks if database performance optimization indexes are present.
        /// Validates the 70-90% performance improvement infrastructure is active.
        /// </summary>
        private async Task<bool> CheckDatabaseIndexesAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Check for existence of key performance indexes deployed on 2025-08-15
                // FIX: Query SQLite's schema table instead of SQL Server's sys.indexes
                var indexCount = await _dbContext.Database
                    .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sqlite_master WHERE type = 'index' AND name LIKE 'IX_Movies_UserId_%'")
                    .SingleAsync(cancellationToken);

                return indexCount > 0;
            }
            catch
            {
                // If index check fails, assume optimizations are not active
                return false;
            }
        }
    }
}