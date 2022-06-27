using Microsoft.Toolkit.Uwp.Notifications;

namespace ADO_Notifications.Actions
{
    internal abstract class AbstractActionListener
    {
        public AbstractActionListener()
        {
        }

        public abstract void OnNewAction(string action, ToastArguments arguments);
    }
}
