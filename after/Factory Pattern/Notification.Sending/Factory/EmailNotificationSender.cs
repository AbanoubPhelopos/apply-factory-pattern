using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Notification.Sending.Factory;

internal sealed class EmailNotificationSender(IOptions<SmtpSettings> smtpSettings) : INotificationSender
{
    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task SendAsync(NotificationMessage message)
    {
        using var smtp = new SmtpClient(
            _smtpSettings.Host,
            _smtpSettings.Port);
        smtp.EnableSsl = true;
        smtp.Credentials = new NetworkCredential(
            _smtpSettings.Username,
            _smtpSettings.Password);

        var mailMessage = new MailMessage(
            from: _smtpSettings.FromAddress,
            to: message.Recipient,
            subject: message.Subject,
            body: message.Body);
        mailMessage.IsBodyHtml = true;

        await smtp.SendMailAsync(mailMessage);
    }
}

public sealed class SmtpSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "demo";
    public string Password { get; set; } = "demo";
    public string FromAddress { get; set; } = "noreply@example.com";
}
