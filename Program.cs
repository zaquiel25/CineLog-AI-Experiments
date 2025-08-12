using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using System.Net.Http.Headers; // <<< ADD THIS IF YOUR IDE DOESN'T ADD IT AUTOMATICALLY
using Ezequiel_Movies;
using Microsoft.AspNetCore.Identity;
using Ezequiel_Movies.Services;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using System.Collections;

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

// --- VVVV START: ADDED CODE FOR TMDB TOKEN AND HTTPCLIENT VVVV ---
// Retrieve the TMDB Access Token from configuration (User Secrets in dev, Key Vault in production)
var tmdbAccessToken = builder.Configuration["TMDB:AccessToken"] ?? builder.Configuration["TMDB--AccessToken"];

if (string.IsNullOrEmpty(tmdbAccessToken))
{
    // TMDB token not found - movie features will be limited
    tmdbAccessToken = "placeholder-token"; // Prevent startup failure
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
        var sqlServer = Environment.GetEnvironmentVariable("AZURE_SQL_SERVER") ?? "cinelog-sql-server.database.windows.net";
        var sqlDatabase = Environment.GetEnvironmentVariable("AZURE_SQL_DATABASE") ?? "CineLog_Production";
        var sqlUser = Environment.GetEnvironmentVariable("AZURE_SQL_USER") ?? "cinelogadmin";
        
        connectionString = $"Server=tcp:{sqlServer},1433;Database={sqlDatabase};User ID={sqlUser};Password={databasePassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60";
    }
    else
    {
        // Fallback when Key Vault is unavailable
        var sqlServer = "cinelog-sql-server.database.windows.net";
        var sqlDatabase = "CineLog_Production";  
        var sqlUser = "cinelogadmin";
        
        // Try to get fallback password from environment variable
        var fallbackPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? 
                              Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
        
        if (!string.IsNullOrEmpty(fallbackPassword))
        {
            connectionString = $"Server=tcp:{sqlServer},1433;Database={sqlDatabase};User ID={sqlUser};Password={fallbackPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60";
        }
        else
        {
            throw new InvalidOperationException("No database password available - Key Vault failed and no fallback password configured");
        }
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

/// <summary>
/// FEATURE: Configure ASP.NET Identity with Google OAuth external authentication support.
/// Integrates with Azure Key Vault for secure credential management in production.
/// </summary>
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Google authentication
var googleClientId = builder.Configuration["Authentication:Google:ClientId"] ?? builder.Configuration["Authentication--Google--ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? builder.Configuration["Authentication--Google--ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
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


// VVVV ADD THESE TWO LINES TO ENABLE SESSION VVVV
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // The session will expire after 20 minutes of inactivity
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// ^^^^ END OF NEW LINES ^^^^

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
/// <summary>
/// SECURITY: Critical middleware for external authentication (Google OAuth).
/// Must be placed before UseAuthorization() in the pipeline.
/// </summary>
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
