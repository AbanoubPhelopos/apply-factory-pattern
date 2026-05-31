using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notification.Sending;
using Notification.Sending.Factory;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
builder.Services.Configure<SlackSettings>(builder.Configuration.GetSection("Slack"));
builder.Services.Configure<TeamsSettings>(builder.Configuration.GetSection("Teams"));

builder.Services.AddSingleton<INotificationSender, EmailNotificationSender>();
builder.Services.AddSingleton<INotificationSender, SmsNotificationSender>();
builder.Services.AddSingleton<INotificationSender, SlackNotificationSender>();
builder.Services.AddSingleton<INotificationSender, TeamsNotificationSender>();

builder.Services.AddSingleton<INotificationSenderFactory, NotificationSenderFactory>();

//// KEYED SERVICES
//builder.Services.AddKeyedSingleton<INotificationSender, EmailNotificationSender>(NotificationChannel.Email);
//builder.Services.AddKeyedSingleton<INotificationSender, SmsNotificationSender>(NotificationChannel.Sms);
//builder.Services.AddKeyedSingleton<INotificationSender, SlackNotificationSender>(NotificationChannel.Slack);
//builder.Services.AddKeyedSingleton<INotificationSender, TeamsNotificationSender>(NotificationChannel.Teams);

//builder.Services.AddSingleton<INotificationSenderFactory, KeyedNotificationSenderFactory>();

builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

// ── Sample notification requests ─────────────────────────────────────────────
var requests = new List<NotificationRequest>
{
    new()
    {
        Channel   = NotificationChannel.Sms,
        Recipient = "+1 555 000 0001",
        Body      = "Your verification code is 847291. Valid for 10 minutes."
    },
    new()
    {
        Channel   = NotificationChannel.Slack,
        Recipient = "#deployments",
        Subject   = "Release",
        Body      = "v2.4.1 deployed to production successfully."
    },
    new()
    {
        Channel   = NotificationChannel.Teams,
        Recipient = "https://webhook.example.com/teams/incoming",
        Subject   = "Alert",
        Body      = "CPU usage exceeded 90% on web-01."
    },
    new()
    {
        Channel   = NotificationChannel.Email,
        Recipient = "alice@example.com",
        Subject   = "Welcome to Acme",
        Body      = "<h1>Welcome, Alice!</h1><p>Your account is ready.</p>"
    },
};

// ── AFTER: Factory-based dispatch ────────────────────────────────────────────
PrintHeader("AFTER — Factory Pattern: NotificationService");

using var scope = app.Services.CreateScope();
var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

foreach (var request in requests)
{
    Console.Write($"  Sending [{request.Channel}] → {request.Recipient} … ");

    try
    {
        await notificationService.SendAsync(
            request.Channel,
            request.Recipient,
            request.Subject ?? string.Empty,
            request.Body);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ {ex.GetType().Name}: {ex.Message}");
    }
}

static void PrintHeader(string title)
{
    Console.WriteLine();
    Console.WriteLine($"┌─ {title} {new string('─', Math.Max(0, 60 - title.Length))}┐");
    Console.WriteLine();
}
