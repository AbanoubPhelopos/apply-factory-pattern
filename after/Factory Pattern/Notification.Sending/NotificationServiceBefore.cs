// ── BEFORE ──────────────────────────────────────────────────────────────────
// NotificationService as it looked before the Factory Pattern refactoring.
//
// Code smells highlighted in the video:
//   1. Construction in the wrong layer — SmtpClient / TwilioClient created inside the service
//   2. Config key leakage — magic strings like "Smtp:Host" scattered everywhere
//   3. Duplicated construction — SmtpClient setup copy-pasted into SendBulkAsync
//   4. Hard to test — no seam to replace SMTP with a fake
//   5. Adding a channel = modifying this class
//   6. Violates SRP — service knows how to send AND how to construct every client
//   7. Primitive obsession — channel passed as string; typos compile fine
//   8. Static side-effect inside a method — TwilioClient.Init() on every SMS call
// ────────────────────────────────────────────────────────────────────────────

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Notification.Sending;

public class NotificationServiceBefore
{
    private readonly IConfiguration _config;

    public NotificationServiceBefore(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(
        NotificationChannel channel, string recipient, string subject, string body)
    {
        if (channel == NotificationChannel.Email)
        {
            using var smtp = new SmtpClient(
                _config["Smtp:Host"],
                int.Parse(_config["Smtp:Port"]!));
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(
                _config["Smtp:Username"],
                _config["Smtp:Password"]);

            var message = new MailMessage(
                from: _config["Smtp:FromAddress"]!,
                to: recipient,
                subject: subject,
                body: body);
            message.IsBodyHtml = true;

            await smtp.SendMailAsync(message);
        }
        else if (channel == NotificationChannel.Sms)
        {
            TwilioClient.Init(
                _config["Twilio:AccountSid"]!,
                _config["Twilio:AuthToken"]!);

            await MessageResource.CreateAsync(
                to: new PhoneNumber(recipient),
                from: new PhoneNumber(_config["Twilio:FromNumber"]!),
                body: body);
        }
        else if (channel == NotificationChannel.Slack)
        {
            var slackClient = new SlackApiClient(_config["Slack:BotToken"]!);
            await slackClient.Chat.PostMessage(new Message
            {
                Channel = recipient,
                Text = body
            });
        }
        else
        {
            throw new ArgumentException($"Unsupported channel: {channel}");
        }
    }

    public async Task SendBulkAsync(IEnumerable<NotificationRequestBefore> requests)
    {
        foreach (var request in requests)
        {
            await SendAsync(
                request.Channel,
                request.Recipient,
                request.Subject ?? string.Empty,
                request.Body);
        }
    }
}

// Before: channel is a plain string — a typo like "Emal" compiles without complaint
public sealed class NotificationRequestBefore
{
    public NotificationChannel Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
}
