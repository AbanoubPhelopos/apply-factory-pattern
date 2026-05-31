using Microsoft.Extensions.Options;
using Notification.Sending.Factory.interfaces;

namespace Notification.Sending.Factory;

public sealed class TeamsNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Teams;

    private readonly string _webhookUrl;

    public TeamsNotificationSender(IOptions<TeamsSettings> options)
        => _webhookUrl = options.Value.WebhookUrl;

    public Task SendAsync(NotificationMessage message)
    {
        var payload = new { text = $"**{message.Subject}**\n{message.Body}" };

        // Demo stub — real code would call: await http.PostAsJsonAsync(_webhookUrl, payload)
        Console.WriteLine($"  [Teams] Webhook={_webhookUrl}  Subject={message.Subject}");
        return Task.CompletedTask;
    }
}
public sealed class TeamsSettings
{
    public string WebhookUrl { get; set; } = "https://webhook.example.com/teams/incoming";
}
