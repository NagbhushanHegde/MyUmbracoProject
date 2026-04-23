using Microsoft.AspNetCore.Mvc;
using MyUmbracoProject.Models;
using MyUmbracoProject.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace MyUmbracoProject.Controller
{
    public class ProfileSurfaceController : SurfaceController
    {
        private readonly IMemberProfileService _profileService;
        private readonly IMemberService _memberService;
        private readonly IMemberManager _memberManager;

        public ProfileSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberProfileService profileService,
            IMemberService memberService,
            IMemberManager memberManager)
            : base(umbracoContextAccessor, databaseFactory, services,
                   appCaches, profilingLogger, publishedUrlProvider)
        {
            _profileService = profileService;
            _memberService = memberService;
            _memberManager  = memberManager;  
        }

        
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            if (!User.Identity!.IsAuthenticated)
                return Redirect("/login");

            var email = User.Identity.Name!;
            var profile = await _profileService.GetProfileAsync(email);

            if (profile == null)
                return Redirect("/login");

            var model = new ProfileViewModel
            {
                Name  = profile.Name,
                Email = profile.Email,
                City  = profile.City
            };

            return CurrentUmbracoPage();
        }

        
[HttpPost]
public async Task<IActionResult> HandleUpdateProfile(ProfileViewModel model)
{
    if (!User.Identity!.IsAuthenticated)
        return Redirect("/login");

    if (!ModelState.IsValid)
        return CurrentUmbracoPage();

    var email = User.Identity.Name!;

    var result = await _profileService.UpdateProfileAsync(
        email, model.Name, model.City);

   
    TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
        result.Success ? result.Message : result.ErrorMessage;

    return RedirectToCurrentUmbracoPage();
}


[HttpPost]
public async Task<IActionResult> HandleChangePassword(
    string currentPassword,
    string newPassword,
    string confirmPassword)
{
    if (!User.Identity!.IsAuthenticated)
        return Redirect("/login");

    if (newPassword != confirmPassword)
    {
        TempData["ErrorMessage"] = "Passwords do not match.";
        return RedirectToCurrentUmbracoPage();
    }

    if (newPassword.Length < 6)
    {
        TempData["ErrorMessage"] = "New password must be at least 6 characters.";
        return RedirectToCurrentUmbracoPage();
    }

    var email = User.Identity.Name!;


    var identityUser = await _memberManager.FindByEmailAsync(email);
    if (identityUser == null)
    {
        TempData["ErrorMessage"] = "Member not found.";
        return RedirectToCurrentUmbracoPage();
    }

    var result = await _memberManager.ChangePasswordAsync(
        identityUser,
        currentPassword,
        newPassword
    );

    if (result.Succeeded)
    {
        TempData["SuccessMessage"] = "Password changed successfully!";
    }
    else
    {
        var errors = string.Join(" ", result.Errors.Select(e => e.Description));
        TempData["ErrorMessage"] = errors;
    }

    return RedirectToCurrentUmbracoPage();
}
    }
}