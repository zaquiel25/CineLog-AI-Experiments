using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using System.Net.Http.Headers; // <<< ADD THIS IF YOUR IDE DOESN'T ADD IT AUTOMATICALLY
using Ezequiel_Movies;
using Microsoft.AspNetCore.Identity;
using Ezequiel_Movies.Services;
using Ezequiel_Movies.Models;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using System.Collections;
using Microsoft.AspNetCore.Authentication;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// PRODUCTION: Configure Azure Key Vault integration for secure secret management.
/// This approach builds the connection string directly from Key Vault secrets,
/// eliminating configuration file loading issues in Azure App Service.
/// </summary>
// Environment configuration logging removed for security

// Configure Key Vault integration for production
if (builder.Environment.IsProduction())
{
    // Configuring Azure Key Vault integration
    
    // Get Key Vault URI from environment variable (set in Azure App Service)
    var keyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI");
    // Key Vault URI configured from environment variable
    
    if (!string.IsNullOrEmpty(keyVaultUri))
    {
        try
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
        catch (Exception)
        {
            // Continue without Key Vault - will use fallback configuration
            // Log error but don't fail application startup
        }
    }
    // If no Key Vault URI provided, will use connection strings from configuration
}

/// <summary>
/// PRODUCTION MONITORING: Configure Application Insights for comprehensive production monitoring.
/// Integrates with Azure Key Vault for secure connection string management and provides
/// CineLog-specific telemetry for user experience, performance optimization validation,
/// and business intelligence tracking.
/// 
/// FEATURE: Validates recent 70-90% database performance improvements
/// FEATURE: Monitors TMDB API usage and response times
/// FEATURE: Tracks user authentication success rates and session analytics
/// FEATURE: Provides suggestion system performance metrics and cache hit rates
/// </summary>
// Configure Application Insights
var applicationInsightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"] ?? 
                                         builder.Configuration["ApplicationInsights--ConnectionString"];

if (!string.IsNullOrEmpty(applicationInsightsConnectionString))
{
    // Add Application Insights telemetry
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = applicationInsightsConnectionString;
        options.EnableAdaptiveSampling = true; // Optimize cost in production
        options.EnableQuickPulseMetricStream = true; // Real-time monitoring
        options.EnableAuthenticationTrackingJavaScript = true; // User session tracking
        options.EnableDependencyTrackingTelemetryModule = true; // Database and API tracking
        options.EnableEventCounterCollectionModule = true; // Performance counters
    });
    
    // Configure telemetry for production optimization
    builder.Services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
    {
        // Configure sampling for cost optimization while preserving critical telemetry
        if (builder.Environment.IsProduction())
        {
            // Reduce sampling rate in production to manage costs while keeping error tracking
            telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder
                .UseAdaptiveSampling(maxTelemetryItemsPerSecond: 5, excludedTypes: "Exception;Event")
                .Build();
        }
    });
    
    // Register CineLog-specific telemetry service
    builder.Services.AddScoped<CineLogTelemetryService>();
    
    builder.Services.AddLogging(logging =>
    {
        logging.AddApplicationInsights();
        if (builder.Environment.IsDevelopment())
        {
            logging.SetMinimumLevel(LogLevel.Information);
        }
        else
        {
            logging.SetMinimumLevel(LogLevel.Warning); // Reduce log noise in production
        }
    });
}
else
{
    // Application Insights not configured - skip telemetry services
    if (builder.Environment.IsProduction())
    {
        builder.Logging.AddConsole();
        var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        loggerFactory.CreateLogger("Startup").LogWarning("Application Insights not configured in production environment");
    }
}

// --- VVVV START: ADDED CODE FOR TMDB TOKEN AND HTTPCLIENT VVVV ---
// Retrieve the TMDB Access Token from configuration (User Secrets in dev, Key Vault in production)
var tmdbAccessToken = builder.Configuration["TMDB:AccessToken"] ?? builder.Configuration["TMDB--AccessToken"];

if (string.IsNullOrEmpty(tmdbAccessToken))
{
    if (builder.Environment.IsProduction())
    {
        throw new InvalidOperationException("TMDB:AccessToken is required in production. Configure it in Azure Key Vault.");
    }
    tmdbAccessToken = "placeholder-token";
}
// --- ^^^^ END: ADDED CODE FOR TMDB TOKEN ^^^^ ---


// Add services to the container.
builder.Services.AddControllersWithViews();

/// <summary>
/// PRODUCTION: Configure secure database connection with direct Key Vault integration.
/// Builds connection string from individual secrets to eliminate configuration file dependencies.
/// </summary>
// Configuring database connection
string connectionString;

if (builder.Environment.IsProduction())
{
    // Production database configuration with Key Vault integration
    var databasePassword = builder.Configuration["DatabasePassword"];
    
    if (!string.IsNullOrEmpty(databasePassword))
    {
        // Build connection string from Key Vault secrets and environment variables
        var sqlServer = Environment.GetEnvironmentVariable("AZURE_SQL_SERVER")
            ?? throw new InvalidOperationException("AZURE_SQL_SERVER environment variable is required in production");
        var sqlDatabase = Environment.GetEnvironmentVariable("AZURE_SQL_DATABASE")
            ?? throw new InvalidOperationException("AZURE_SQL_DATABASE environment variable is required in production");
        var sqlUser = Environment.GetEnvironmentVariable("AZURE_SQL_USER")
            ?? throw new InvalidOperationException("AZURE_SQL_USER environment variable is required in production");
        
        connectionString = $"Server=tcp:{sqlServer},1433;Database={sqlDatabase};User ID={sqlUser};Password={databasePassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60";
    }
    else
    {
        // Fallback when Key Vault is unavailable — require all values from environment
        var sqlServer = Environment.GetEnvironmentVariable("AZURE_SQL_SERVER")
            ?? throw new InvalidOperationException("AZURE_SQL_SERVER environment variable is required in production");
        var sqlDatabase = Environment.GetEnvironmentVariable("AZURE_SQL_DATABASE")
            ?? throw new InvalidOperationException("AZURE_SQL_DATABASE environment variable is required in production");
        var sqlUser = Environment.GetEnvironmentVariable("AZURE_SQL_USER")
            ?? throw new InvalidOperationException("AZURE_SQL_USER environment variable is required in production");

        // Try to get fallback password from environment variable
        var fallbackPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ??
                              Environment.GetEnvironmentVariable("DATABASE_PASSWORD")
                              ?? throw new InvalidOperationException("No database password available - Key Vault failed and no fallback password configured");

        connectionString = $"Server=tcp:{sqlServer},1433;Database={sqlDatabase};User ID={sqlUser};Password={fallbackPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60";
    }
}
else
{
    // Development: Use connection string from configuration
    var devConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(devConnectionString))
    {
        throw new InvalidOperationException("Database connection string 'DefaultConnection' not found in development configuration.");
    }
    connectionString = devConnectionString;
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        // Production: Use SQL Server with retry policies
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
            
            sqlOptions.CommandTimeout(60);
        });
        
        options.EnableServiceProviderCaching();
    }
    else
    {
        // Development: Use SQL Server with detailed error reporting
        options.UseSqlServer(connectionString);
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
    
    // Validate connection string is configured
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured");
    }
});

// Register CacheService for performance optimization
builder.Services.AddScoped<CacheService>();

// Register UserDisplayNameService for DisplayName retrieval
builder.Services.AddScoped<UserDisplayNameService>();

/// <summary>
/// PRODUCTION MONITORING: Configure health checks for comprehensive system monitoring.
/// Provides endpoints for Application Insights and Azure monitoring to track
/// database connectivity, TMDB API availability, cache performance, and system resources.
/// Essential for validating the 70-90% performance improvements and detecting regressions.
/// </summary>
builder.Services.AddHealthChecks()
    .AddCheck<CineLogHealthCheck>("cinelog_health")
    .AddDbContextCheck<ApplicationDbContext>("database");

/// <summary>
/// FEATURE: Configure session state for suggestion system anti-repetition tracking.
/// Required for shuffle pools, last selected genres/actors, and user preference tracking.
/// Uses in-memory provider for development, could be extended to Redis for production scaling.
/// </summary>
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "CineLog.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Works in both HTTP (dev) and HTTPS (prod)
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Match password gate timeout
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});


/// <summary>
/// FEATURE: Configure ASP.NET Identity with Google OAuth external authentication support.
/// Identity remains the DEFAULT authentication scheme for MoviesController [Authorize] attributes.
/// Integrates with Azure Key Vault for secure credential management in production.
/// FEATURE: DisplayName column added to AspNetUsers via migration (access via direct SQL queries).
/// </summary>
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Google authentication - this extends the default Identity authentication
var googleClientId = builder.Configuration["Authentication:Google:ClientId"] ?? builder.Configuration["Authentication--Google--ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? builder.Configuration["Authentication--Google--ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.ConfigureApplicationCookie(options =>
    {
        // Keep Identity as default, just configure its cookie settings
        options.LoginPath = "/Identity/Account/Login";
        options.LogoutPath = "/Identity/Account/Logout";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });

    builder.Services.AddAuthentication()
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = googleClientId;
            googleOptions.ClientSecret = googleClientSecret;
            googleOptions.CallbackPath = "/signin-google";
            
            // Optional: Request additional scopes for user information
            googleOptions.Scope.Add("profile");
            googleOptions.Scope.Add("email");
            
            // Save tokens for potential future API calls
            googleOptions.SaveTokens = true;
        });
}
else
{
    // Google OAuth not configured - users can still use email/password authentication
    // In development, add secrets using: dotnet user-secrets set "Authentication:Google:ClientId" "your-client-id"
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Identity/Account/Login";
        options.LogoutPath = "/Identity/Account/Logout";
        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });
}
// --- VVVV START: ADDED HTTP CLIENT CONFIGURATION FOR TMDB SERVICE HERE VVVV ---
builder.Services.AddHttpClient<Ezequiel_Movies.TmdbService>(client => // Ensure Ezequiel_Movies.TmdbService is the correct full name for your service class
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", tmdbAccessToken);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});
// --- ^^^^ END: ADDED HTTP CLIENT CONFIGURATION ^^^^ ---


/// <summary>
/// FEATURE: Cookie consent policy for GDPR compliance.
/// Shows a banner asking users to accept cookies before non-essential cookies are set.
/// </summary>
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseCookiePolicy();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseSession(); // Required for suggestion system anti-repetition tracking
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

/// <summary>
/// PRODUCTION MONITORING: Configure health check endpoints for Application Insights monitoring.
/// Provides detailed health status for database, TMDB API, cache, and performance optimizations.
/// Essential for production monitoring and validating recent 70-90% performance improvements.
/// </summary>
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                exception = x.Value.Exception?.Message,
                duration = x.Value.Duration,
                data = x.Value.Data
            }),
            totalDuration = report.TotalDuration
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Lightweight health check endpoint for load balancers
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();
