using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace utad.reFresh.core.Services
{
    public class EmailSender(IHttpContextAccessor httpContextAccessor) : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine(htmlMessage);
            var request = httpContextAccessor.HttpContext?.Request;
            var appUrl = request != null
                ? $"{request.Scheme}://{request.Host}"
                : "https://default-url.com"; // fallback

            // Replace placeholder in the message
            var processedMessage = htmlMessage.Replace("{AppUrl}", appUrl);

            Console.WriteLine($"Email: {email}, Subject: {subject}, Message: {processedMessage}");
            
            return Task.CompletedTask;
        }
    }

}
