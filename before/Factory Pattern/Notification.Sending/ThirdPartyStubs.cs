// ── Third-party stubs ────────────────────────────────────────────────────────
// These types stand in for the real NuGet packages used in production.
// In a real application you would reference:
//   dotnet add package Twilio
//   dotnet add package SlackNet
//
// The stubs have the same public API shape but print to the console instead of
// making real network calls, so the demo runs locally without any credentials.
// ─────────────────────────────────────────────────────────────────────────────

namespace Notification.Sending;

public static class TwilioClient
{
    public static void Init(string accountSid, string authToken)
        => Console.WriteLine($"  [Twilio] Client initialised (AccountSid={accountSid})");
}

public sealed class PhoneNumber(string number)
{
    public override string ToString() => number;
}

public static class MessageResource
{
    public static Task CreateAsync(
        PhoneNumber to,
        PhoneNumber from,
        string body)
    {
        Console.WriteLine($"  [SMS] To={to}  Body={body}");
        return Task.CompletedTask;
    }
}

public sealed class SlackApiClient(string botToken)
{
    private readonly string _botToken = botToken;
    public SlackChatEndpoint Chat { get; } = new SlackChatEndpoint(botToken);
}

public sealed class SlackChatEndpoint(string botToken)
{
    private readonly string _botToken = botToken;

    public Task PostMessage(Message message)
    {
        Console.WriteLine($"  [Slack] #{message.Channel}: {message.Text}");
        return Task.CompletedTask;
    }
}

public sealed class Message
{
    public string Channel { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}