using ADO_Notifications.API;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;

namespace ADO_Notifications.Actions
{
    internal class PipelineActionListener : AbstractActionListener
    {
        public override void OnNewAction(string action, ToastArguments arguments)
        {
            if (action == "viewBuild")
            {
                if (arguments.Contains("ids"))
                {
                    var buildIds = arguments["ids"].Split("@");                    
                    foreach (var buildId in buildIds)
                    {
                        if (int.TryParse(buildId, out var buildIdInt))
                        {
                            _ = Process.Start(new ProcessStartInfo
                            {
                                FileName = AzureUriBuilder.BuildProjectUri($"_build/results?buildId={buildIdInt}").ToString(),
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
        }
    }
}
