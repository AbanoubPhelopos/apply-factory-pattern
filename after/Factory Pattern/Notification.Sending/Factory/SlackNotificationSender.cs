using Microsoft.Extensions.Options;
using Notification.Sending.Factory.interfaces;

namespace Notification.Sending.Factory;

internal sealed class SlackNotificationSender(IOptions<SlackSettings> options) : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Slack;

    private readonly SlackApiClient _client = new(options.Value.BotToken);

    public async Task SendAsync(NotificationMessage message)
    {
        await _client.Chat.PostMessage(new Message
        {
            Channel = message.Recipient,
            Text = message.Body
        });
    }
}

public sealed class SlackSettings
{
    public string BotToken { get; set; } = "xoxb-demo";
}
