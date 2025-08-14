using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
        /// AZURE DIAGNOSTIC: Enhanced logging for production debugging.
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            _logger.LogInformation("PasswordGate Index GET accessed from IP: {IpAddress}", HttpContext.Connection.RemoteIpAddress);
            _logger.LogInformation("Request path: {Path}, Query: {Query}", HttpContext.Request.Path, HttpContext.Request.QueryString);
            
            try
            {
                // If already authenticated, redirect to home
                var authenticated = HttpContext.Session.GetString("SiteAccess");
                _logger.LogInformation("Current session authentication: {Auth}", authenticated ?? "NULL");
                
                if (authenticated == "granted")
                {
                    _logger.LogInformation("User already authenticated, redirecting to home");
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session access failed in PasswordGate GET");
            }

            _logger.LogInformation("Displaying password gate form");
            return View();
        }

        /// <summary>
        /// Process password gate form submission.
        /// Validates password and grants site access via session.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string password, bool rememberMe = false)
        {
            // Get password from configuration
            var sitePassword = _configuration["SitePassword"] ?? _configuration["SiteAccess:Password"];
            
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
                // Grant access via session
                HttpContext.Session.SetString("SiteAccess", "granted");

                // If "Remember Me" is checked, set a persistent cookie for 7 days
                if (rememberMe)
                {
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        HttpOnly = true,
                        Secure = Request.IsHttps, // Azure-compatible: Only secure if HTTPS
                        SameSite = SameSiteMode.Lax, // More permissive for Azure load balancer
                        Path = "/",
                        Domain = null // Let browser determine domain
                    };
                    
                    try
                    {
                        Response.Cookies.Append("SiteAccess", "granted", cookieOptions);
                        _logger.LogInformation("Persistent cookie set for site access");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to set persistent cookie");
                    }
                }

                _logger.LogInformation("Site access granted from IP: {IpAddress}", HttpContext.Connection.RemoteIpAddress);
                
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
    }
}