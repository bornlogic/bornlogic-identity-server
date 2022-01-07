using System.ComponentModel.DataAnnotations;
using System.Text;
using Bornlogic.IdentityServer.Email.HtmlMessageProvider.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Bornlogic.IdentityServer.Host.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IForgotPasswordEmailHtmlMessageProvider _forgotPasswordEmailHtmlMessageProvider;

        public ForgotPasswordModel
            (
                UserManager<ApplicationUser> userManager, 
                IEmailSender emailSender,
                IForgotPasswordEmailHtmlMessageProvider forgotPasswordEmailHtmlMessageProvider
                )
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _forgotPasswordEmailHtmlMessageProvider = forgotPasswordEmailHtmlMessageProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                var (emailSubject, emailHtmlMessage) = await _forgotPasswordEmailHtmlMessageProvider.GetSubjectAndHtmlMessage(null, callbackUrl);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    emailSubject,
                    emailHtmlMessage);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
