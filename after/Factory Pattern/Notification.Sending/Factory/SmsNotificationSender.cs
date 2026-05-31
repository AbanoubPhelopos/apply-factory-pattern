using Microsoft.Extensions.Options;
using Notification.Sending.Factory.interfaces;

namespace Notification.Sending.Factory;

internal sealed class SmsNotificationSender : INotificationSender
{
    public NotificationChannel Channel => NotificationChannel.Sms;

    private readonly TwilioSettings _settings;

    public SmsNotificationSender(IOptions<TwilioSettings> options)
    {
        _settings = options.Value;
        // TwilioClient.Init is a static call — putting it here means it runs
        // exactly once when DI constructs this singleton, not on every SMS sent.
        TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
    }

    public async Task SendAsync(NotificationMessage message)
    {
        await MessageResource.CreateAsync(
            to: new PhoneNumber(message.Recipient),
            from: new PhoneNumber(_settings.FromNumber),
            body: message.Body);
    }
}
public sealed class TwilioSettings
{
    public string AccountSid { get; set; } = "ACdemo";
    public string AuthToken { get; set; } = "demo-token";
    public string FromNumber { get; set; } = "+15550000000";
}
