using System.Net;
using System.Net.Mail;
using Application.Interfaces.IServices;
using Infrastructure.Security;

namespace Infrastructure.Email;

public class SmtpEmailService(EmailSettings settings) : IEmailService
{
    public async Task SendAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(settings.Host, settings.Port);
        client.EnableSsl = settings.UseSsl;

        if (!string.IsNullOrEmpty(settings.UserName))
            client.Credentials = new NetworkCredential(settings.UserName, settings.Password);

        using var message = new MailMessage(settings.From, to, subject, body);
        
        message.IsBodyHtml = true;

        await client.SendMailAsync(message);
    }
}