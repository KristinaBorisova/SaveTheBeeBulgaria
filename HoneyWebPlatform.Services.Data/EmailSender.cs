using HoneyWebPlatform.Services.Data.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using HoneyWebPlatform.Services.Data.Models;


public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;
    

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string email, string subject, string message, string number)
    {
        try
        {
            Console.WriteLine($"DEBUG: EmailSender - Attempting to send email to: {email}");
            Console.WriteLine($"DEBUG: EmailSender - SMTP Server: {_emailSettings.SmtpServer}:{_emailSettings.SmtpPort}");
            Console.WriteLine($"DEBUG: EmailSender - SMTP Username: {_emailSettings.SmtpUsername}");
            
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000 // 30 seconds timeout
            };

            var mailMessage = new MailMessage(from: _emailSettings.SmtpUsername,
                to: email, 
                subject, 
                message)
            {
                IsBodyHtml = true
            };

            Console.WriteLine($"DEBUG: EmailSender - About to send email...");
            Console.WriteLine($"DEBUG: EmailSender - From: {_emailSettings.SmtpUsername}, To: {email}, Subject: {subject}");
            
            try
            {
                await client.SendMailAsync(mailMessage);
                Console.WriteLine($"DEBUG: EmailSender - Email sent successfully to: {email}");
            }
            catch (Exception smtpEx)
            {
                Console.WriteLine($"DEBUG: EmailSender - SMTP Error: {smtpEx.Message}");
                Console.WriteLine($"DEBUG: EmailSender - SMTP Error Type: {smtpEx.GetType().Name}");
                throw;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG: EmailSender - Failed to send email to {email}: {ex.Message}");
            Console.WriteLine($"DEBUG: EmailSender - Error details: {ex.StackTrace}");
            throw;
        }
    }
}