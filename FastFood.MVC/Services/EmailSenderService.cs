using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace FastFood.MVC.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IConfiguration configuration, ILogger<EmailSenderService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var fromEmail = _configuration["EmailSettings:Email"]!;
            var fromPassword = _configuration["EmailSettings:Password"]!;

            using var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true,
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "Burgz Fast Food"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            _logger.LogDebug("Preparing to send email to {Email}", email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent to {Email}", email);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogWarning(smtpEx, "SMTP error sending email to {Email}", email);
            }
        }
    }
}
