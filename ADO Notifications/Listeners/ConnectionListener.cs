using ADO_Notifications.ADO;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Threading.Tasks;

namespace ADO_Notifications.Listeners
{
    internal class ConnectionListener : AbstractListener
    {
        private bool _connected = false;

        public event EventHandler<VssConnection> OnConnected;
        public event EventHandler<VssConnection> OnDisconnected;

        protected override async Task TimeoutAsync()
        {
            if (!_connected && (ConnectionHolder.Connection?.HasAuthenticated ?? false))
            {
                _connected = true;

                OnConnected?.Invoke(this, ConnectionHolder.Connection);
            }

            if (_connected && !(ConnectionHolder.Connection?.HasAuthenticated ?? false))
            {
                _connected = false;

                OnDisconnected?.Invoke(this, ConnectionHolder.Connection);
            }

            await Task.CompletedTask;
        }
    }
}
