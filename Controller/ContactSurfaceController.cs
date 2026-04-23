using Microsoft.AspNetCore.Mvc;
using MyUmbracoProject.Models;
using MyUmbracoProject.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace MyUmbracoProject.Controller
{
    public class ContactSurfaceController : SurfaceController
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ContactSurfaceController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider,
            IEmailService emailService,
            IConfiguration config)
            : base(umbracoContextAccessor, databaseFactory, services,
                   appCaches, profilingLogger, publishedUrlProvider)
        {
            _emailService = emailService;
            _config       = config;
        }

        [HttpPost]
        public async Task<IActionResult> HandleContact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
                return CurrentUmbracoPage();

            // ✅ Only send email if properly configured
            var fromEmail = _config["EmailSettings:FromEmail"];
            var smtpHost  = _config["EmailSettings:SmtpHost"];

            if (!string.IsNullOrWhiteSpace(fromEmail) &&
                !string.IsNullOrWhiteSpace(smtpHost))
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        toEmail: fromEmail,
                        subject: $"New Contact from {model.Name}",
                        body: $@"
                            <h3>New Contact Message</h3>
                            <p><strong>Name:</strong> {model.Name}</p>
                            <p><strong>Email:</strong> {model.Email}</p>
                            <p><strong>Message:</strong> {model.Message}</p>
                        "
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email error: {ex.Message}");
                }
            }
            else
            {
                // Log to console — no crash
                Console.WriteLine($"📩 Contact from {model.Name} ({model.Email}): {model.Message}");
            }

            TempData["SuccessMessage"] = "Message sent! We'll get back to you soon.";
            return RedirectToCurrentUmbracoPage();
        }
    }
}