using System.Net;
using System.Net.Mail;

namespace MyUmbracoProject.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost  = _config["EmailSettings:SmtpHost"];
            var smtpPort  = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
            var smtpUser  = _config["EmailSettings:SmtpUser"];
            var smtpPass  = _config["EmailSettings:SmtpPass"];
            var fromEmail = _config["EmailSettings:FromEmail"];

            // ✅ Guard — skip if email not configured
            if (string.IsNullOrEmpty(fromEmail) ||
                string.IsNullOrEmpty(smtpHost)  ||
                string.IsNullOrEmpty(smtpUser))
            {
                Console.WriteLine("⚠️ Email not configured in appsettings.json — skipping send.");
                return;   // ← exits safely without crashing
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl   = true
            };

            var message = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string name)
        {
            var subject = "Welcome to Level Up Umbraco Skills!";
            var body    = $@"
                <h2>Welcome, {name}!</h2>
                <p>Thank you for registering.</p>
            ";
            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "Reset Your Password";
            var body    = $@"
                <h2>Password Reset</h2>
                <p>Click below to reset your password:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>This link expires in 24 hours.</p>
            ";
            await SendEmailAsync(toEmail, subject, body);
        }
    }
}