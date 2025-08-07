using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using System.Net.Http.Headers; // <<< ADD THIS IF YOUR IDE DOESN'T ADD IT AUTOMATICALLY
using Ezequiel_Movies;
using Microsoft.AspNetCore.Identity;
using Ezequiel_Movies.Services;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;

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
            // Connecting to Azure Key Vault
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
            // Successfully connected to Key Vault
        }
        catch (Exception ex)
        {
            // Key Vault connection failed - check configuration
            throw new InvalidOperationException($"Failed to connect to Azure Key Vault: {ex.Message}", ex);
        }
    }
    else
    {
        // AZURE_KEY_VAULT_URI environment variable not set
        throw new InvalidOperationException("AZURE_KEY_VAULT_URI environment variable is required for production");
    }
}

// --- VVVV START: ADDED CODE FOR TMDB TOKEN AND HTTPCLIENT VVVV ---
// Retrieve the TMDB Access Token from configuration (User Secrets in dev, Key Vault in production)
var tmdbAccessToken = builder.Configuration["TMDB:AccessToken"];

if (string.IsNullOrEmpty(tmdbAccessToken))
{
    // This is a critical configuration.
    throw new InvalidOperationException("TMDB Access Token not found in configuration. Ensure it's set in User Secrets (TMDB:AccessToken) for development or Key Vault for production.");
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
    // Building production connection string from Key Vault secrets
    
    // Get database password directly from Key Vault
    var databasePassword = builder.Configuration["DatabasePassword"];
    // DatabasePassword retrieved from Key Vault
    
    if (string.IsNullOrEmpty(databasePassword))
    {
        // DatabasePassword not found in Key Vault
        throw new InvalidOperationException("DatabasePassword secret not found in Azure Key Vault");
    }
    
    // Build Azure SQL connection string directly from environment variables
    var sqlServer = Environment.GetEnvironmentVariable("AZURE_SQL_SERVER") ?? "your-sql-server.database.windows.net";
    var sqlDatabase = Environment.GetEnvironmentVariable("AZURE_SQL_DATABASE") ?? "your-database-name";
    var sqlUser = Environment.GetEnvironmentVariable("AZURE_SQL_USER") ?? "your-sql-user";
    connectionString = $"Server=tcp:{sqlServer},1433;Database={sqlDatabase};User ID={sqlUser};Password={databasePassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60";
    // Production connection string built successfully from Key Vault secrets
}
else
{
    // Development: Use connection string from appsettings.json
    var devConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(devConnectionString))
    {
        throw new InvalidOperationException("Database connection string 'DefaultConnection' not found in development configuration.");
    }
    connectionString = devConnectionString;
    // Development connection string loaded from configuration
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsProduction())
    {
        // PRODUCTION: Use SQL Server with retry policies
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
            
            sqlOptions.CommandTimeout(60);
        });
    }
    else
    {
        // DEVELOPMENT: Use SQL Server with simpler configuration
        options.UseSqlServer(connectionString);
        options.EnableSensitiveDataLogging();
    }
});

// Register CacheService for performance optimization
builder.Services.AddScoped<CacheService>();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
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
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
