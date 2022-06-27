using ADO_Notifications.Notifications;

namespace ADO_Notifications.Notifiers
{
    internal abstract class AbstractNotifier
    {
        public NotificationHandler NotificationHandler { get; private set; } = null;

        private AbstractNotifier()
        { }

        public AbstractNotifier(NotificationHandler notificationHandler) : this()
        {
            NotificationHandler = notificationHandler;
        }
    }
}
