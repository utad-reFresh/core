﻿using Microsoft.AspNetCore.Identity.UI.Services;

namespace utad.reFresh.core.Services;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    
    public string ApplicationUrl { get; set; } 
}

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmailSender(IOptions<EmailSettings> emailSettings, IHttpContextAccessor httpContextAccessor)
    {
        _emailSettings = emailSettings.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        
        Console.WriteLine($"Sending email to {email} with subject '{subject}' and message '{message}'");
        
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}