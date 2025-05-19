using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace utad.reFresh.core.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Implement your email sending logic here.
            // For example, you can use SMTP or a third-party email service.

            // For testing purposes, you can simply write the email details to the console.
            Console.WriteLine($"Email: {email}, Subject: {subject}, Message: {htmlMessage}");

            return Task.CompletedTask;
        }
    }

}
