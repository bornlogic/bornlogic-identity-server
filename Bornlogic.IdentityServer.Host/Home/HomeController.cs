



using Bornlogic.IdentityServer.Extensions;
using Bornlogic.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bornlogic.IdentityServer.Host.Home
{
    [SecurityHeaders]
    [AllowAnonymous]
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;

        public HomeController(IIdentityServerInteractionService interaction, IWebHostEnvironment environment, ILogger<HomeController> logger)
        {
            _interaction = interaction;
            _environment = environment;
            _logger = logger;
        }

        [Route("/")]
        public IActionResult Index()
        {
            if (_environment.IsDevelopment())
            {
                if (string.IsNullOrEmpty(HttpContext.User?.GetDisplayName()))
                {
                    return Redirect("Identity/Account/Login");
                    //return RedirectToAction("Login", "Account", new { area = "Identity" });
                    //return RedirectToAction("Login", "Account");
                }

                return View();
            }

            _logger.LogInformation("Homepage is disabled in production. Returning 404.");
            return NotFound();
        }

        /// <summary>
        /// Shows the error page
        /// </summary>
        [Route("/Error")]
        [Route("/Home/Error")]
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();
            
            var message = await _interaction.GetErrorContextAsync(errorId);

            if (message != null)
            {
                vm.Error = message;
            }

            return View("Error", vm);
        }
    }
}