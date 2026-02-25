using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Ezequiel_Movies.Services
{
    /// <summary>
    /// Custom telemetry initializer for FrameRoute-specific context and metadata.
    /// Adds consistent properties to all Application Insights telemetry for better tracking and analysis.
    /// </summary>
    public class FrameRouteTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            // Add consistent FrameRoute application metadata to all telemetry
            telemetry.Context.GlobalProperties["Application"] = "FrameRoute";
            telemetry.Context.GlobalProperties["ApplicationVersion"] = GetApplicationVersion();
            telemetry.Context.GlobalProperties["Environment"] = GetEnvironmentName();
            
            // Add deployment metadata for production tracking
            telemetry.Context.GlobalProperties["DeploymentTimestamp"] = GetDeploymentTimestamp();
            telemetry.Context.GlobalProperties["DatabaseOptimizationVersion"] = "2025-08-15"; // Track performance optimization milestone
            
            // Add Azure resource context for production monitoring
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP")))
            {
                telemetry.Context.GlobalProperties["AzureResourceGroup"] = Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP");
                telemetry.Context.GlobalProperties["AzureWebsiteName"] = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
                telemetry.Context.GlobalProperties["AzureRegion"] = Environment.GetEnvironmentVariable("WEBSITE_REGION_NAME");
            }
        }

        private static string GetApplicationVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "1.0.0.0";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetEnvironmentName()
        {
            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
        }

        private static string GetDeploymentTimestamp()
        {
            // Try to get build timestamp from environment or fallback to process start time
            var buildTime = Environment.GetEnvironmentVariable("BUILD_TIMESTAMP");
            if (!string.IsNullOrEmpty(buildTime))
            {
                return buildTime;
            }

            // Fallback to process start time as deployment approximation
            var processStartTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
            return processStartTime.ToString("yyyy-MM-dd HH:mm:ss UTC");
        }
    }
}