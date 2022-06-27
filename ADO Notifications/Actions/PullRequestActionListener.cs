using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;

namespace ADO_Notifications.Actions
{
    internal class PullRequestActionListener : AbstractActionListener
    {
        public override void OnNewAction(string action, ToastArguments arguments)
        {
            if (action == "viewPr")
            {
                if (arguments.Contains("urls"))
                {
                    var urls = arguments["urls"].Split("@");
                    foreach (var url in urls)
                    {
                        _ = Process.Start(new ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        });
                    }
                }
            }
        }
    }
}
