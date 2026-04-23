using Microsoft.AspNetCore.Mvc;
using MyUmbracoProject.Models;
using MyUmbracoProject.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace MyUmbracoProject.Controller
{
    public class ForgotPasswordSurfaceController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberManager memberManager,
            IEmailService emailService)
            : base(umbracoContextAccessor, databaseFactory, services,
                   appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _emailService  = emailService;
        }

        
        [HttpPost]
        public async Task<IActionResult> HandleForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var user = await _memberManager.FindByEmailAsync(model.Email);

           
            if (user == null)
            {
                TempData["SuccessMessage"] =
                    "If that email exists, a reset link has been sent.";
                return RedirectToCurrentUmbracoPage();
            }

            
            var token = await _memberManager.GeneratePasswordResetTokenAsync(user);

          
            var request  = HttpContext.Request;
            var baseUrl  = $"{request.Scheme}://{request.Host}";
            var resetUrl = $"{baseUrl}/reset-password?email=" +
                           $"{Uri.EscapeDataString(model.Email)}&token=" +
                           $"{Uri.EscapeDataString(token)}";

           
            await _emailService.SendPasswordResetEmailAsync(model.Email, resetUrl);

            TempData["SuccessMessage"] =
                "If that email exists, a reset link has been sent.";

            return RedirectToCurrentUmbracoPage();
        }

        
        [HttpPost]
        public async Task<IActionResult> HandleResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var user = await _memberManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToCurrentUmbracoPage();
            }

            var result = await _memberManager.ResetPasswordAsync(
                user, model.Token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] =
                    "Password reset successful! You can now login.";
                return Redirect("/login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return CurrentUmbracoPage();
        }
    }
}