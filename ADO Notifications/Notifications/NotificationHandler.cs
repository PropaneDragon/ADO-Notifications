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
        private readonly List<ToastContentBuilder> _toaster = new();
        private readonly object _toasterLock = new();

        private DateTime _nextToast = DateTime.Now;

        public NotificationHandler()
        {
        }

        public void AddToast(ToastContentBuilder builder)
        {
            lock (_toasterLock)
            {
                _toaster.Add(builder);
            }
        }

        public void PopToast()
        {
            lock (_toasterLock)
            {
                var currentToast = _toaster.First();
                if (currentToast != null)
                {
                    currentToast.Show(toast => toast.ExpirationTime = DateTimeOffset.Now.Add(TimeSpan.FromMinutes(60)));

                    _ = _toaster.Remove(currentToast);
                }
            }
        }

        public void ShowWelcomeToast()
        {
            AddToast(new ToastContentBuilder()
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
