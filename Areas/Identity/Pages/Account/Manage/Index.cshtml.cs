// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ezequiel_Movies.Models;
using Ezequiel_Movies.Data;

namespace Ezequiel_Movies.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// User-friendly display name shown throughout the site.
            /// Optional - if empty, username will be displayed instead.
            /// </summary>
            [StringLength(100, ErrorMessage = "The {0} must be at most {1} characters long.")]
            [Display(Name = "Display Name")]
            public string DisplayName { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            // Access DisplayName from database via ADO.NET - column exists but not in IdentityUser model
            using var command = _dbContext.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT DisplayName FROM AspNetUsers WHERE Id = @id";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@id";
            parameter.Value = user.Id;
            command.Parameters.Add(parameter);
            await _dbContext.Database.OpenConnectionAsync();
            var displayName = await command.ExecuteScalarAsync() as string;

            Input = new InputModel
            {
                DisplayName = displayName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Update DisplayName if changed - requires direct database access
            using var readCommand = _dbContext.Database.GetDbConnection().CreateCommand();
            readCommand.CommandText = "SELECT DisplayName FROM AspNetUsers WHERE Id = @id";
            var readParameter = readCommand.CreateParameter();
            readParameter.ParameterName = "@id";
            readParameter.Value = user.Id;
            readCommand.Parameters.Add(readParameter);
            await _dbContext.Database.OpenConnectionAsync();
            var currentDisplayName = await readCommand.ExecuteScalarAsync() as string;

            if (Input.DisplayName != currentDisplayName)
            {
                // Update via SQL since DisplayName isn't in IdentityUser model
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "UPDATE AspNetUsers SET DisplayName = {0} WHERE Id = {1}",
                    Input.DisplayName ?? (object)DBNull.Value,
                    user.Id);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
