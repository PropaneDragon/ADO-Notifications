using ADO_Notifications.Actions;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Generic;
using System.Windows;

namespace ADO_Notifications
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private List<AbstractActionListener> _actions = new()
        {
            new PipelineActionListener(),
            new PullRequestActionListener()
        };

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            var arguments = ToastArguments.Parse(e.Argument);
            if (arguments.Contains("action"))
            {
                foreach (var action in _actions)
                {
                    action.OnNewAction(arguments["action"], arguments);
                }
            }
        }
    }
}
