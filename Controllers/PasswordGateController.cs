using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Ezequiel_Movies.Controllers
{
    /// <summary>
    /// Controller for site-wide password protection during friend testing phase.
    /// Provides simple password gate to restrict access to entire site.
    /// </summary>
    public class PasswordGateController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PasswordGateController> _logger;

        public PasswordGateController(IConfiguration configuration, ILogger<PasswordGateController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Display password gate form for site access.
        /// Uses cookie-based authentication to check existing access.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            _logger.LogInformation("PasswordGate Index GET accessed from IP: {IpAddress}", HttpContext.Connection.RemoteIpAddress);
            _logger.LogInformation("Request path: {Path}, Query: {Query}", HttpContext.Request.Path, HttpContext.Request.QueryString);
            
            // Check if already authenticated via cookie
            var isAuthenticated = HttpContext.User.HasClaim("PasswordGate", "granted");
            _logger.LogInformation("Current authentication status: {IsAuthenticated}", isAuthenticated);
            
            if (isAuthenticated)
            {
                _logger.LogInformation("User already authenticated, redirecting to home");
                return RedirectToAction("Index", "Home");
            }

            _logger.LogInformation("Displaying password gate form");
            return View();
        }

        /// <summary>
        /// Process password gate form submission.
        /// Validates password and grants site access via cookie authentication.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string password, bool rememberMe = false)
        {
            // Get password from configuration (lowercase 'p' to match Key Vault secret)
            var sitePassword = _configuration["Sitepassword"] ?? _configuration["SiteAccess:Password"];
            
            if (string.IsNullOrEmpty(sitePassword))
            {
                _logger.LogError("Site password not configured. Check User Secrets or Azure Key Vault.");
                ViewBag.ErrorMessage = "Site access is temporarily unavailable. Please try again later.";
                return View();
            }

            if (string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Please enter the password.";
                return View();
            }

            if (password == sitePassword)
            {
                try
                {
                    // Create claims for authentication
                    var claims = new List<Claim>
                    {
                        new Claim("PasswordGate", "granted"),
                        new Claim(ClaimTypes.Name, "PasswordGateUser")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "PasswordGate");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Configure authentication properties
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = rememberMe, // Remember Me functionality
                        ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(20)
                    };

                    // Store Remember Me preference for the authentication event
                    if (rememberMe)
                    {
                        authProperties.Items["RememberMe"] = "true";
                    }

                    // Sign in the user with cookie authentication
                    await HttpContext.SignInAsync("PasswordGate", claimsPrincipal, authProperties);
                    
                    _logger.LogInformation("Site access granted from IP: {IpAddress}, RememberMe: {RememberMe}", 
                                         HttpContext.Connection.RemoteIpAddress, rememberMe);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sign in user with cookie authentication");
                    ViewBag.ErrorMessage = "Authentication failed. Please try again.";
                    return View();
                }
                
                // Redirect to home page or return URL
                var returnUrl = Request.Query["returnUrl"].ToString();
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogWarning("Failed site access attempt from IP: {IpAddress}", HttpContext.Connection.RemoteIpAddress);
                ViewBag.ErrorMessage = "Incorrect password. Please try again.";
                return View();
            }
        }

        /// <summary>
        /// FEATURE: Logout functionality for password gate authentication.
        /// Signs out user and redirects to password gate.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("PasswordGate");
            _logger.LogInformation("User signed out from password gate");
            return RedirectToAction("Index", "PasswordGate");
        }
    }
}