using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Ezequiel_Movies.Services
{
    /// <summary>
    /// Null implementation of TelemetryClient wrapper for development environments or when Application Insights is not configured.
    /// Prevents null reference exceptions while providing no-op telemetry functionality.
    /// </summary>
    public class NullTelemetryClient
    {
        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            // No-op implementation
        }

        public void TrackDependency(string dependencyTypeName, string dependencyName, string data, DateTimeOffset startTime, TimeSpan duration, bool success)
        {
            // No-op implementation
        }

        public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            // No-op implementation - exceptions still visible via standard logging
        }

        public void TrackPageView(string name, string? url = null, TimeSpan? duration = null, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            // No-op implementation
        }

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            // No-op implementation
        }

        public void TrackTrace(string message, Microsoft.ApplicationInsights.DataContracts.SeverityLevel? severityLevel = null, IDictionary<string, string>? properties = null)
        {
            // No-op implementation
        }

        public void TrackMetric(string name, double value, IDictionary<string, string>? properties = null)
        {
            // No-op implementation
        }

        public void Flush()
        {
            // No-op implementation
        }
    }
}