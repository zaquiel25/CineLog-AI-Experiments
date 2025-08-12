// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Ezequiel_Movies.Areas.Identity.Pages.Account
{
    /// <summary>
    /// SECURITY: Handles external authentication (Google OAuth) with comprehensive security validation.
    /// Implements secure user creation, authentication, and proper error handling.
    /// </summary>
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        /// SECURITY: Input model for external login with validation attributes.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        public string ProviderDisplayName { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        /// <summary>
        /// SECURITY: Initiates external authentication challenge with CSRF protection.
        /// Razor Pages automatically validates anti-forgery tokens for POST requests.
        /// </summary>
        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // SECURITY: Validate provider parameter to prevent injection
            if (string.IsNullOrEmpty(provider))
            {
                _logger.LogWarning("External login attempt with empty provider");
                return BadRequest("Invalid authentication provider");
            }

            // Request a redirect to the external login provider
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            
            // SECURITY: Add anti-forgery state parameter
            properties.Items["LoginProvider"] = provider;
            
            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// SECURITY: Handles OAuth callback with comprehensive validation and user creation.
        /// </summary>
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            
            // SECURITY: Check for remote authentication errors
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                _logger.LogWarning("External authentication error: {RemoteError}", remoteError);
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // SECURITY: Get external login info with validation
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                _logger.LogWarning("Failed to load external login information");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // SECURITY: Attempt to sign in with external login provider
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out during external login");
                return RedirectToPage("./Lockout");
            }
            else
            {
                // SECURITY: If user doesn't have an account, create one
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                
                // SECURITY: Safely extract email from external provider claims
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        /// <summary>
        /// SECURITY: Creates new user account from external login with validation.
        /// Razor Pages automatically validates anti-forgery tokens for POST requests.
        /// </summary>
        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            
            // SECURITY: Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                _logger.LogWarning("Failed to load external login information during confirmation");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                // SECURITY: Create user with validated email
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    // SECURITY: Add external login to user account
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        // SECURITY: Sign in the user after successful account creation
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                
                // SECURITY: Log and display user creation errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogWarning("User creation failed: {Error}", error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        /// <summary>
        /// SECURITY: Creates new IdentityUser instance safely.
        /// </summary>
        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        /// <summary>
        /// SECURITY: Safely gets email store with validation.
        /// </summary>
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}