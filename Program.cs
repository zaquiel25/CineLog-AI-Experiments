using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Data;
using System.Net.Http.Headers; // <<< ADD THIS IF YOUR IDE DOESN'T ADD IT AUTOMATICALLY

var builder = WebApplication.CreateBuilder(args);

// --- VVVV START: ADDED CODE FOR TMDB TOKEN AND HTTPCLIENT VVVV ---
// Retrieve the TMDB Access Token from User Secrets
var tmdbAccessToken = builder.Configuration["TMDB:AccessToken"];

if (string.IsNullOrEmpty(tmdbAccessToken))
{
    // This is a critical configuration.
    throw new InvalidOperationException("TMDB Access Token not found in configuration. Ensure it's set in User Secrets (TMDB:AccessToken).");
}
// --- ^^^^ END: ADDED CODE FOR TMDB TOKEN ^^^^ ---


// Add services to the container.
builder.Services.AddControllersWithViews();

// Your existing DbContext registration
var conStringBuider = builder.Configuration.GetConnectionString("Ezequiel"); // This line isn't used, conString is hardcoded below
var conString = "Server=localhost,1433 ;Database=Ezequiel_Movies;User Id=sa; Password=***REMOVED***; TrustServerCertificate=True";
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(conString));

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
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();