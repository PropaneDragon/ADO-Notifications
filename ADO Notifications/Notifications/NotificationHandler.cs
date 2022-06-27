using ADO_Notifications.Listeners;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADO_Notifications.Notifications
{
    internal class NotificationHandler : AbstractListener
    {
        private readonly List<(ToastContentBuilder Toast, TimeSpan Timeout)> _toaster = new();
        private readonly object _toasterLock = new();

        private DateTime _nextToast = DateTime.Now;

        public NotificationHandler()
        {
        }

        public void AddToast(ToastContentBuilder builder) => AddToast(TimeSpan.FromMinutes(60), builder);
        public void AddToast(TimeSpan timeout, ToastContentBuilder builder)
        {
            lock (_toasterLock)
            {
                _toaster.Add((builder, timeout));
            }
        }

        public void PopToast()
        {
            lock (_toasterLock)
            {
                var currentToast = _toaster.First();
                if (currentToast.Toast != null)
                {
                    currentToast.Toast.Show(toast => toast.ExpirationTime = DateTimeOffset.Now.Add(currentToast.Timeout));

                    _ = _toaster.Remove(currentToast);
                }
            }
        }

        public void ShowWelcomeToast()
        {
            AddToast(TimeSpan.FromSeconds(10), new ToastContentBuilder()
                .AddText("Notifications from ADO will appear here")
                .AddText("You can change what type of notifications you get in the settings")
                .AddButton(new ToastButton()
                    .SetContent("Settings")
                    .AddArgument("action", "settings"))
                );
        }

        protected override async Task TimeoutAsync()
        {
            var anyToast = false;

            lock (_toasterLock)
            {
                anyToast = _toaster.Any();
            }

            if (DateTime.Now > _nextToast && anyToast)
            {
                PopToast();

                _nextToast = DateTime.Now.AddSeconds(10);
            }
        }
    }
}
