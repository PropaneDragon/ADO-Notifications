using ADO_Notifications.ADO;
using ADO_Notifications.Listeners;
using ADO_Notifications.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Services.WebApi;
using System;

namespace ADO_Notifications.Notifiers
{
    internal class ConnectionNotifier : AbstractNotifier
    {
        private readonly ConnectionListener _connectionListener = new();        

        public ConnectionNotifier(NotificationHandler notificationHandler) : base(notificationHandler)
        {
            _connectionListener.OnConnected += ConnectionListener_OnConnected;
            _connectionListener.OnDisconnected += ConnectionListener_OnDisconnected;

            _connectionListener.StartListening(TimeSpan.FromSeconds(1));
        }

        private void ConnectionListener_OnDisconnected(object? sender, VssConnection _)
        {
            NotificationHandler.AddToast(TimeSpan.FromMinutes(10), new ToastContentBuilder().AddText("Disconnected from ADO"));
        }

        private void ConnectionListener_OnConnected(object? sender, VssConnection _)
        {
            var user = ConnectionHolder.Connection.AuthorizedIdentity;
            NotificationHandler.AddToast(TimeSpan.FromMinutes(1), new ToastContentBuilder().AddText("Connected to ADO").AddText($"Hello {user.DisplayName}"));
        }
    }
}
