using ADO_Notifications.ADO;
using ADO_Notifications.Notifications;
using ADO_Notifications.Notifiers;
using ADO_Notifications.Properties;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ADO_Notifications
{
    internal class Initialiser
    {
        private List<AbstractNotifier> _notifiers = new();

        public NotificationHandler NotificationHandler { get; } = new();

        public async Task InitialiseAsync()
        {
            NotificationHandler.StartListening(TimeSpan.FromSeconds(1));

            _notifiers.Add(new PullRequestNotifier(NotificationHandler));
            _notifiers.Add(new PipelineNotifier(NotificationHandler));
            _notifiers.Add(new ConnectionNotifier(NotificationHandler));

            if (!string.IsNullOrWhiteSpace(Settings.Default.AccessToken))
            {
                try
                {
                    if (!await ConnectionHolder.SetCredentialsAsync(new VssBasicCredential(string.Empty, Settings.Default.AccessToken)))
                    {
                        NotificationHandler.AddToast(new ToastContentBuilder().AddText("Failed to connect to ADO"));
                    }
                }
                catch (Exception ex)
                {
                    NotificationHandler.AddToast(new ToastContentBuilder().AddText("Failed to connect to ADO").AddText(ex.Message));
                }
            }
        }
    }
}
