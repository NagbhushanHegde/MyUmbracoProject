using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyUmbracoProject.Models;
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
    public class AuthSurfaceController : SurfaceController
    {
        private readonly IMemberManager _memberManager;
        private readonly SignInManager<MemberIdentityUser> _signInManager;  // ← use this
        private readonly IMemberService _memberService;

        public AuthSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IMemberManager memberManager,
            SignInManager<MemberIdentityUser> signInManager,                // ← use this
            IMemberService memberService)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _memberManager = memberManager;
            _signInManager = signInManager;
            _memberService = memberService;
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleRegister(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var existingMember = _memberService.GetByEmail(model.Email);
            if (existingMember != null)
            {
                ModelState.AddModelError(string.Empty, "An account with this email already exists.");
                return CurrentUmbracoPage();
            }

            var identityUser = MemberIdentityUser.CreateNew(
                username: model.Email,
                email: model.Email,
                memberTypeAlias: "Member",
                isApproved: true,
                name: model.Name
            );

            var result = await _memberManager.CreateAsync(identityUser, model.Password);

            if (result.Succeeded)
            {
                
                var member = _memberService.GetByEmail(model.Email);
                if (member != null && !string.IsNullOrEmpty(model.City))
                {
                    member.SetValue("city", model.City);
                    _memberService.Save(member);
                }

              
                await _signInManager.SignInAsync(identityUser, isPersistent: false);

                TempData["SuccessMessage"] = "Registration successful! Welcome aboard.";
                return Redirect("/");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return CurrentUmbracoPage();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            var result = await _signInManager.PasswordSignInAsync(
                userName: model.Email,
                password: model.Password,
                isPersistent: false,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Login successful!";
                return Redirect("/");
            }

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Your account is locked. Please try again later.");
            else
                ModelState.AddModelError(string.Empty, "Invalid email or password.");

            return CurrentUmbracoPage();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleLogout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }
    }
}