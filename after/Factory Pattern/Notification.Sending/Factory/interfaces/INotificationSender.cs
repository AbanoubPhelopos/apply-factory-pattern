namespace Notification.Sending.Factory.interfaces;

public interface INotificationSender
{
    NotificationChannel Channel { get; }

    Task SendAsync(NotificationMessage message);
}

public record NotificationMessage(string Recipient, string Subject, string Body);
