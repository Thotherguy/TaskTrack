using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace TaskTrack.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn,
    IEmailSender emailSender, ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();
    public class InputModel
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, StringLength(100, MinimumLength = 12), DataType(DataType.Password)]
        public string Password { get; set; } = "";
        [DataType(DataType.Password), Display(Name = "Confirm password"), Compare("Password")]
        public string ConfirmPassword { get; set; } = "";
    }
    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await signIn.GetExternalAuthenticationSchemesAsync()).ToList();
    }
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await signIn.GetExternalAuthenticationSchemesAsync()).ToList();
        if (!ModelState.IsValid) return Page();
        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        var result = await users.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return Page();
        }
        var roleResult = await users.AddToRoleAsync(user, "Member");
        if (!roleResult.Succeeded)
        {
            var cleanup = await users.DeleteAsync(user);
            if (!cleanup.Succeeded) logger.LogError("Registration role assignment and cleanup failed for {UserId}", user.Id);
            foreach (var error in roleResult.Errors) ModelState.AddModelError("", error.Description);
            return Page();
        }
        logger.LogInformation("User created a new account with password.");
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GenerateEmailConfirmationTokenAsync(user)));
        var callback = Url.Page("/Account/ConfirmEmail", pageHandler: null,
            values: new { area = "Identity", userId = user.Id, code, returnUrl }, protocol: Request.Scheme)!;
        await emailSender.SendEmailAsync(Input.Email, "Confirm your email",
            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callback)}'>clicking here</a>.");
        if (users.Options.SignIn.RequireConfirmedAccount)
            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
        await signIn.SignInAsync(user, isPersistent: false);
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/"));
    }
}
