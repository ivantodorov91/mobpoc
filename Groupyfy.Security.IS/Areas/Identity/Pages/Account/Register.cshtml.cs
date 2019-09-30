using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Groupyfy.Security.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Groupyfy.Security.Persistence;
using System.Linq;

namespace Groupyfy.Security.IS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly RoleManager<GroupyfyRole> _roleManager;
        private readonly SignInManager<GroupyfyUser> _signInManager;
        private readonly GroupyfyUserManager _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            GroupyfyUserManager userManager,
            SignInManager<GroupyfyUser> signInManager,
            RoleManager<GroupyfyRole> roleManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Role { get; set; }

            public Guid? CorporateId { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid)
                return Page();

            if (Input.Role.ToLower() == "candidate")
            {
                var user = await _userManager.FindByNameAsync(Input.Email);
                if (user != null)
                {
                    var isUserInCandidateRoleForCorporate = await _userManager.IsInGroupyfyRoleAsync(user, "candidate", Input.CorporateId);
                    if (isUserInCandidateRoleForCorporate)
                    {
                        ModelState.AddModelError("candidate", "Candidate already exists for this corporate");
                        return Page();
                    }


                    var assignRoleResult = await _userManager.AddToGroupyfyRoleAsync(user, Input.Role, Input.CorporateId);

                    if (!assignRoleResult.Succeeded)
                        return LocalRedirect($"/home/error?errorId={assignRoleResult.Errors.ToArray()[0].Code}");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);

                }
                else
                {
                    user = new GroupyfyUser { UserName = Input.Email, Email = Input.Email };
                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        var assignRoleResult = await _userManager.AddToGroupyfyRoleAsync(user, "candidate", Input.CorporateId);

                        if (!assignRoleResult.Succeeded)
                            return LocalRedirect($"/home/error?errorId={assignRoleResult.Errors.ToArray()[0].Code}");

                        _logger.LogInformation("Candidate created");

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                var user = new GroupyfyUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {

                    var assignRoleResult = await _userManager.AddToGroupyfyRoleAsync(user, Input.Role, Input.CorporateId);

                    if (!assignRoleResult.Succeeded)
                        return LocalRedirect($"/home/error?errorId={assignRoleResult.Errors.ToArray()[0].Code}");

                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
