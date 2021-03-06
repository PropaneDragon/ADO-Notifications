using ADO_Notifications.ADO;
using ADO_Notifications.API;
using ADO_Notifications.Listeners;
using ADO_Notifications.Notifications;
using ADO_Notifications.Properties;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADO_Notifications.Notifiers
{
    internal class PullRequestNotifier : AbstractNotifier
    {
        private readonly PullRequestListener _pullRequestListener = new();

        private DateTime _nextFailureNotification = DateTime.UtcNow;

        public PullRequestNotifier(NotificationHandler notificationHandler) : base(notificationHandler)
        {
            _pullRequestListener.OnNewPullRequests += PullRequestListener_OnNewPullRequests;
            _pullRequestListener.OnPullRequestsAwaitingReview += PullRequestListener_OnPullRequestsAwaitingReview;
            _pullRequestListener.OnPullRequestStatusChanged += PullRequestListener_OnPullRequestStatusChanged;
            _pullRequestListener.OnPullRequestSourceUpdated += PullRequestListener_OnPullRequestSourceUpdated;
            _pullRequestListener.OnUnsuccessfulPullRequest += PullRequestListener_OnUnsuccessfulPullRequest;
            _pullRequestListener.OnSuccessfulPullRequest += PullRequestListener_OnSuccessfulPullRequest;
            _pullRequestListener.OnPullRequestReviewersAdded += PullRequestListener_OnPullRequestReviewersAdded;
            _pullRequestListener.OnPullRequestReviewersRemoved += PullRequestListener_OnPullRequestReviewersRemoved;
            _pullRequestListener.OnError += PullRequestListener_OnError;

            _pullRequestListener.StartListening(TimeSpan.FromSeconds(10));
        }

        private string ReadableStatus(PullRequestStatus? status) => (status ?? PullRequestStatus.NotSet) switch
        {
            PullRequestStatus.Completed => "Completed",
            PullRequestStatus.Abandoned => "Abandoned",
            PullRequestStatus.Active => "Active",
            _ => "Unknown"
        };

        private static bool AnyReviewersAreCurrentUser(GitPullRequest? request) => ConnectionHolder.User != null && (request?.Reviewers?.Any(reviewer => ConnectionHolder.User.CompareRef(reviewer)) ?? false);
        private static bool PullRequestIsCurrentUsers(GitPullRequest? request) => ConnectionHolder.User != null && ConnectionHolder.User.CompareRef(request?.CreatedBy);

        private static string ReadablePullRequestCreator(GitPullRequest? request) => request?.CreatedBy?.DisplayName ?? "Unknown";
        private static string ReadablePullRequestTitle(GitPullRequest? request) => request?.Title ?? "Unknown";
        private static string ConstructActionIdFromPullRequest(GitPullRequest pullRequest) => pullRequest.PullRequestId.ToString();
        private static string ConstructActionIdsFromPullRequests(IEnumerable<GitPullRequest> pullRequests) => string.Join('@', pullRequests.Select(pullRequest => ConstructActionIdFromPullRequest(pullRequest)));
        private static string ConstructActionUrlFromPullRequest(GitPullRequest pullRequest) => AzureUriBuilder.BuildProjectUri($"_git/{pullRequest.Repository?.Name ?? ""}/pullrequest/{pullRequest.PullRequestId}").ToString();
        private static string ConstructActionUrlsFromPullRequests(IEnumerable<GitPullRequest?> pullRequests) => string.Join('@', pullRequests.Select(pullRequest => ConstructActionUrlFromPullRequest(pullRequest)));

        private void PullRequestListener_OnNewPullRequests(object? sender, IEnumerable<GitPullRequest> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnNewPR)
            {
                var validPullRequests = e.Where(pullRequest => (!(pullRequest.IsDraft ?? false) || Settings.Default.NotifyOnNewPRIncludeDraft) && !PullRequestIsCurrentUsers(pullRequest) && AnyReviewersAreCurrentUser(pullRequest));
                if (validPullRequests.Any())
                {
                    var title = $"{validPullRequests.Count()} pull requests have just been created";
                    var subtitle = $"Click the button below to view them";
                    var sharedActionIds = ConstructActionIdsFromPullRequests(validPullRequests);
                    var sharedActionUrls = ConstructActionUrlsFromPullRequests(validPullRequests);

                    if (validPullRequests.Count() == 1)
                    {
                        var singularPr = validPullRequests.First();
                        title = $"{ReadablePullRequestCreator(singularPr)} has just created a new pull request";
                        subtitle = $"{singularPr.Title}";
                    }

                    NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddArgument("action", "viewPr")
                        .AddArgument("ids", sharedActionIds)
                        .AddArgument("urls", sharedActionUrls)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewPr")
                            .AddArgument("ids", sharedActionIds)
                            .AddArgument("urls", sharedActionUrls))
                        );
                }
            }
        }

        private void PullRequestListener_OnPullRequestsAwaitingReview(object? sender, IEnumerable<GitPullRequest> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.RemindAboutUnreviewedPRs)
            {
                var validPullRequests = e.Where(pullRequest => (!(pullRequest.IsDraft ?? false) || Settings.Default.NotifyOnNewPRIncludeDraft) && !PullRequestIsCurrentUsers(pullRequest) && AnyReviewersAreCurrentUser(pullRequest));
                if (validPullRequests.Any())
                {
                    var title = $"{validPullRequests.Count()} pull requests are awaiting your review";
                    var sharedActionIds = ConstructActionIdsFromPullRequests(validPullRequests);
                    var sharedActionUrls = ConstructActionUrlsFromPullRequests(validPullRequests);

                    if (validPullRequests.Count() == 1)
                    {
                        var singularPr = validPullRequests.First();
                        title = $"{ReadablePullRequestCreator(singularPr)}'s pull request is awaiting your review";
                    }

                    NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddArgument("action", "viewPr")
                        .AddArgument("ids", sharedActionIds)
                        .AddArgument("urls", sharedActionUrls)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewPr")
                            .AddArgument("ids", sharedActionIds)
                            .AddArgument("urls", sharedActionUrls))
                        );
                }
            }
        }

        private void PullRequestListener_OnPullRequestStatusChanged(object? sender, IEnumerable<Tuple<GitPullRequest, PullRequestStatus?, PullRequestStatus?>> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnUpdatedPullRequests)
{
                var validPullRequests = e.Where(pullRequest => (!(pullRequest.Item1.IsDraft ?? false) || Settings.Default.NotifyOnNewPRIncludeDraft) && (PullRequestIsCurrentUsers(pullRequest.Item1) || AnyReviewersAreCurrentUser(pullRequest.Item1)));
                if (validPullRequests.Any())
                {
                    foreach (var pullRequest in validPullRequests)
                    {
                        var name = PullRequestIsCurrentUsers(pullRequest.Item1) ? "Your" : $"{ReadablePullRequestCreator(pullRequest.Item1)}'s";
                        var title = $"{name} pull request has changed from {ReadableStatus(pullRequest.Item2 ?? PullRequestStatus.NotSet)} to {ReadableStatus(pullRequest.Item3 ?? PullRequestStatus.NotSet)}";
                        var subtitle = $"{ReadablePullRequestTitle(pullRequest.Item1)}";
                        var sharedActionId = ConstructActionIdFromPullRequest(pullRequest.Item1);
                        var sharedActionUrl = ConstructActionUrlFromPullRequest(pullRequest.Item1);

                        NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddArgument("action", "viewPr")
                        .AddArgument("ids", sharedActionId)
                        .AddArgument("urls", sharedActionUrl)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewPr")
                            .AddArgument("ids", sharedActionId)
                            .AddArgument("urls", sharedActionUrl))
                        );
                    }
                }
            }
        }

        private void PullRequestListener_OnPullRequestSourceUpdated(object? sender, IEnumerable<GitPullRequest> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnUpdatedPullRequests)
            {
                var validPullRequests = e.Where(pullRequest => (!(pullRequest.IsDraft ?? false) || Settings.Default.NotifyOnNewPRIncludeDraft) && (PullRequestIsCurrentUsers(pullRequest) || AnyReviewersAreCurrentUser(pullRequest)));
                if (validPullRequests.Any())
                {
                    foreach (var pullRequest in validPullRequests)
                    {
                        var name = PullRequestIsCurrentUsers(pullRequest) ? "your" : $"{ReadablePullRequestCreator(pullRequest)}'s";
                        var title = $"New changes have been pushed to {name} pull request";
                        var subtitle = $"{ReadablePullRequestTitle(pullRequest)}";
                        var sharedActionId = ConstructActionIdFromPullRequest(pullRequest);
                        var sharedActionUrl = ConstructActionUrlFromPullRequest(pullRequest);

                        NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddArgument("action", "viewPr")
                        .AddArgument("ids", sharedActionId)
                        .AddArgument("urls", sharedActionUrl)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewPr")
                            .AddArgument("ids", sharedActionId)
                            .AddArgument("urls", sharedActionUrl))
                        );
                    }
                }
            }
        }

        private void PullRequestListener_OnSuccessfulPullRequest(object? sender, IEnumerable<GitPullRequest> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnUpdatedPullRequests)
            {
                var validPullRequests = e.Where(pullRequest => (!(pullRequest.IsDraft ?? false) || Settings.Default.NotifyOnNewPRIncludeDraft) && (PullRequestIsCurrentUsers(pullRequest) || AnyReviewersAreCurrentUser(pullRequest)));
                if (validPullRequests.Any())
                {
                    foreach (var pullRequest in validPullRequests)
                    {
                        var name = PullRequestIsCurrentUsers(pullRequest) ? "Your" : $"{ReadablePullRequestCreator(pullRequest)}'s";
                        var title = $"{name} pull request has completed successfully ({ReadableStatus(pullRequest.Status)})";
                        var subtitle = $"{ReadablePullRequestTitle(pullRequest)}";
                        var sharedActionId = ConstructActionIdFromPullRequest(pullRequest);
                        var sharedActionUrl = ConstructActionUrlFromPullRequest(pullRequest);

                        NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddArgument("action", "viewPr")
                        .AddArgument("ids", sharedActionId)
                        .AddArgument("urls", sharedActionUrl)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewPr")
                            .AddArgument("ids", sharedActionId)
                            .AddArgument("urls", sharedActionUrl))
                        );
                    }
                }
            }
        }

        private void PullRequestListener_OnUnsuccessfulPullRequest(object? sender, IEnumerable<GitPullRequest> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyOnUpdatedPullRequests)
            {
                var validPullRequests = e.Where(pullRequest => (!(pullRequest.IsDraft ?? false) || Settings.Default.NotifyOnNewPRIncludeDraft) && (PullRequestIsCurrentUsers(pullRequest) || AnyReviewersAreCurrentUser(pullRequest)));
                if (validPullRequests.Any())
                {
                    foreach (var pullRequest in validPullRequests)
                    {
                        var name = PullRequestIsCurrentUsers(pullRequest) ? "Your" : $"{ReadablePullRequestCreator(pullRequest)}'s";
                        var title = $"{name} pull request has not completed successfully ({ReadableStatus(pullRequest.Status)})";
                        var subtitle = $"{ReadablePullRequestTitle(pullRequest)}";
                        var sharedActionId = ConstructActionIdFromPullRequest(pullRequest);
                        var sharedActionUrl = ConstructActionUrlFromPullRequest(pullRequest);

                        NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddArgument("action", "viewPr")
                        .AddArgument("ids", sharedActionId)
                        .AddArgument("urls", sharedActionUrl)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewPr")
                            .AddArgument("ids", sharedActionId)
                            .AddArgument("urls", sharedActionUrl))
                        );
                    }
                }
            }
        }

        private void PullRequestListener_OnPullRequestReviewersAdded(object? sender, IEnumerable<Tuple<GitPullRequest, IEnumerable<IdentityRefWithVote>>> e)
        {
            var user = ConnectionHolder.User;
            if (user != null && Settings.Default.NotifyWhenAddedAsReviewer)
            {
                var validPullRequests = e.Where(pullRequest => (!(pullRequest.Item1.IsDraft ?? false) || Settings.Default.NotifyOnNewPRIncludeDraft) && !PullRequestIsCurrentUsers(pullRequest.Item1) && pullRequest.Item2.Any(reviewer => user.CompareRef(reviewer)));
                if (validPullRequests.Any())
                {
                    foreach (var pullRequest in validPullRequests)
                    {
                        var title = $"You have been added as a reviewer to {ReadablePullRequestCreator(pullRequest.Item1)}'s pull request";
                        var subtitle = $"{ReadablePullRequestTitle(pullRequest.Item1)}";
                        var sharedActionId = ConstructActionIdFromPullRequest(pullRequest.Item1);
                        var sharedActionUrl = ConstructActionUrlFromPullRequest(pullRequest.Item1);

                        NotificationHandler?.AddToast(
                        new ToastContentBuilder()
                        .AddText(title)
                        .AddText(subtitle)
                        .AddArgument("action", "viewPr")
                        .AddArgument("ids", sharedActionId)
                        .AddArgument("urls", sharedActionUrl)
                        .AddButton(new ToastButton()
                            .SetContent("View")
                            .AddArgument("action", "viewPr")
                            .AddArgument("ids", sharedActionId)
                            .AddArgument("urls", sharedActionUrl))
                        );
                    }
                }
            }
        }

        private void PullRequestListener_OnPullRequestReviewersRemoved(object? sender, IEnumerable<Tuple<GitPullRequest, IEnumerable<IdentityRefWithVote>>> e)
        {
        }

        private void PullRequestListener_OnError(object? _, Exception e)
        {
            if (DateTime.UtcNow > _nextFailureNotification)
            {
                NotificationHandler?.AddToast(
                    new ToastContentBuilder()
                    .AddText("An error occurred trying to retrieve pull request information.")
                    .AddText($"{e?.Message ?? ""}"));

                _nextFailureNotification = DateTime.UtcNow.AddMinutes(30);
            }
        }
    }
}
