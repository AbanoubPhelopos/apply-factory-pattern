namespace Notification.Sending.Factory.interfaces;

public interface INotificationSenderFactory
{
    INotificationSender CreateSender(NotificationChannel channel);
}
