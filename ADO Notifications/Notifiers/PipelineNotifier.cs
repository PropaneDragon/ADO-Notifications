using ADO_Notifications.ADO;
using ADO_Notifications.Listeners;
using ADO_Notifications.Notifications;
using ADO_Notifications.Properties;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADO_Notifications.Notifiers
{
    internal class PipelineNotifier : AbstractNotifier
    {
        private readonly PipelineListener _pipelineListener = new();

        public PipelineNotifier(NotificationHandler notificationHandler) : base(notificationHandler)
        {
            _pipelineListener.OnNewBuilds += PipelineListener_OnNewBuilds;
            _pipelineListener.OnSuccessfulBuild += PipelineListener_OnSuccessfulBuild;
            _pipelineListener.OnUnsuccessfulBuild += PipelineListener_OnUnsuccessfulBuild;
            _pipelineListener.OnBuildStatusChanged += PipelineListener_OnBuildStatusChanged;

            _pipelineListener.StartListening(TimeSpan.FromSeconds(2));
        }

        private static string ReadableResult(BuildResult result) => result switch
        {
            BuildResult.Canceled => "Cancelled",
            BuildResult.Failed => "Failed",
            BuildResult.Succeeded => "Succeeded",
            BuildResult.PartiallySucceeded => "Partially succeeded",
            _ => "Unknown"
        };

        private static string ReadableStatus(BuildStatus status) => status switch
        {
            BuildStatus.Cancelling => "Cancelling",
            BuildStatus.Completed => "Completed",
            BuildStatus.InProgress => "In progress",
            BuildStatus.NotStarted => "Not started",
            BuildStatus.Postponed => "Postponed",
            _ => "Unknown"
        };

        private void PipelineListener_OnUnsuccessfulBuild(object? sender, IEnumerable<Build> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnCompletedBuilds && Settings.Default.NotifyOnCompletedFailedBuilds)
            {
                var validBuilds = e.Where(build => build?.RequestedFor?.Id != null && new Guid(build.RequestedFor.Id) == user.Id);

                if (validBuilds.Any())
                {
                    var title = $"{validBuilds.Count()} new builds have failed";
                    var subtitle = $"Click the button below to view them";

                    if (validBuilds.Count() == 1)
                    {
                        var singularBuild = validBuilds.First();
                        title = $"Build {singularBuild.BuildNumber} has failed";
                        subtitle = $"{singularBuild.Definition.Name ?? ""} ({ReadableResult(singularBuild?.Result ?? BuildResult.None)})";
                    }

                    NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewBuild")
                            .AddArgument("ids", string.Join('@', validBuilds.Select(build => build.Id))))
                        );
                }
            }
        }

        private void PipelineListener_OnSuccessfulBuild(object? sender, IEnumerable<Build> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnCompletedBuilds && Settings.Default.NotifyOnCompletedSuccessfulBuilds)
            {
                var validBuilds = e.Where(build => build?.RequestedFor?.Id != null && new Guid(build.RequestedFor.Id) == user.Id);

                if (validBuilds.Any())
                {
                    var title = $"{validBuilds.Count()} new builds have completed successfully";
                    var subtitle = $"Click the button below to view them";

                    if (validBuilds.Count() == 1)
                    {
                        var singularBuild = validBuilds.First();
                        title = $"Build {singularBuild.BuildNumber} has completed successfully";
                        subtitle = $"{singularBuild.Definition.Name ?? ""} ({ReadableResult(singularBuild?.Result ?? BuildResult.None)}))";
                    }

                    NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewBuild")
                            .AddArgument("ids", string.Join('@', validBuilds.Select(build => build.Id))))
                        );
                }
            }
        }

        private void PipelineListener_OnNewBuilds(object? sender, IEnumerable<Build> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnNewBuilds)
            {
                var validBuilds = e.Where(build => build?.RequestedFor?.Id != null && new Guid(build.RequestedFor.Id) == user.Id);

                if (validBuilds.Any())
                {
                    var title = $"{validBuilds.Count()} new builds have started";
                    var subtitle = $"Click the button below to view them";

                    if (validBuilds.Count() == 1)
                    {
                        var singularBuild = validBuilds.First();
                        title = $"Build {singularBuild.BuildNumber} has started";
                        subtitle = $"{singularBuild.Definition.Name ?? ""}";
                    }

                    NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewBuild")
                            .AddArgument("ids", string.Join('@', validBuilds.Select(build => build.Id))))
                        );
                }
            }
        }

        private void PipelineListener_OnBuildStatusChanged(object? sender, IEnumerable<Tuple<Build, BuildStatus?, BuildStatus?>> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnBuildStatusChanges)
            {
                var validStatuses = e.Where(statusChange => statusChange.Item1?.RequestedFor?.Id != null && new Guid(statusChange.Item1.RequestedFor.Id) == user.Id);

                foreach (var statusChange in validStatuses)
                {
                    var title = $"Build {statusChange.Item1.BuildNumber} changed from {ReadableStatus(statusChange?.Item2 ?? BuildStatus.None)} to {ReadableStatus(statusChange?.Item3 ?? BuildStatus.None)}";

                    NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewBuild")
                            .AddArgument("ids", statusChange.Item1.Id))
                        );
                }
            }
        }
    }
}
