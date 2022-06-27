using ADO_Notifications.ADO;
using ADO_Notifications.API;
using ADO_Notifications.Properties;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ADO_Notifications.Listeners
{
    internal class PullRequestListener : AbstractListener
    {
        private bool _initialised = false;
        private List<GitPullRequest> _priorPullRequests = new();

        public DateTime LastCheck = DateTime.UtcNow;
        public DateTime NextPingForPullRequestsAwaitingReview = DateTime.UtcNow;

        public TimeSpan PullRequestsAwaitingReviewInterval => TimeSpan.FromMinutes(Settings.Default.RemindAboutUnreviewedPRIntervalMinutes);

        public event EventHandler<IEnumerable<GitPullRequest>> OnNewPullRequests;
        public event EventHandler<IEnumerable<GitPullRequest>> OnPullRequestsAwaitingReview;
        public event EventHandler<IEnumerable<GitPullRequest>> OnPullRequestSourceUpdated;
        public event EventHandler<IEnumerable<GitPullRequest>> OnSuccessfulPullRequest;
        public event EventHandler<IEnumerable<GitPullRequest>> OnUnsuccessfulPullRequest;
        public event EventHandler<IEnumerable<Tuple<GitPullRequest, IEnumerable<IdentityRefWithVote>>>> OnPullRequestReviewersAdded;
        public event EventHandler<IEnumerable<Tuple<GitPullRequest, IEnumerable<IdentityRefWithVote>>>> OnPullRequestReviewersRemoved;
        public event EventHandler<IEnumerable<Tuple<GitPullRequest, PullRequestStatus?, PullRequestStatus?>>> OnPullRequestStatusChanged;

        public async Task<IEnumerable<GitPullRequest>> GetPullRequestsAsync()
        {
            var connection = ConnectionHolder.Connection;
            if (connection?.HasAuthenticated ?? false)
            {
                var gitClient = connection.GetClient<GitHttpClient>();
                if (gitClient != null)
                {
                    CancellationTokenSource source = new(TimeSpan.FromSeconds(20));

                    return await gitClient.GetPullRequestsByProjectAsync(AzureUriBuilder.Project, new GitPullRequestSearchCriteria() { IncludeLinks = true }, cancellationToken: source.Token);
                }
            }

            return new List<GitPullRequest>();
        }

        public async Task<IEnumerable<GitPullRequest>> GetPullRequestsAwaitingReviewAsync()
        {
            var pullRequests = await GetPullRequestsAsync();
            var user = ConnectionHolder.User;

            return pullRequests != null && user != null ? pullRequests.Where(pullRequest => pullRequest.Reviewers.Any(reviewer => (!reviewer.HasDeclined ?? false) && (reviewer.Vote == 0) && (user.CompareRef(reviewer)))) : new List<GitPullRequest>();
        }

        public async Task<GitPullRequest> GetPullRequestAsync(Guid repositoryId, int pullRequestId)
        {
            var connection = ConnectionHolder.Connection;
            if (connection?.HasAuthenticated ?? false)
            {
                var gitClient = connection.GetClient<GitHttpClient>();
                if (gitClient != null)
                {
                    CancellationTokenSource source = new(TimeSpan.FromSeconds(20));

                    return await gitClient.GetPullRequestAsync(AzureUriBuilder.Project, repositoryId, pullRequestId, cancellationToken: source.Token);
                }
            }

            return null;
        }

        public void UpdateLastCheckTime() => LastCheck = DateTime.UtcNow;

        protected override async Task TimeoutAsync()
        {
            if (ConnectionHolder.Connection?.HasAuthenticated ?? false)
            {
                var activePullRequests = await GetPullRequestsAsync();

                try
                {
                    if (activePullRequests != null)
                    {
                        if (_initialised)
                        {
                            var linkedPullRequests = activePullRequests.Select(pullRequest => (current: pullRequest, previous: _priorPullRequests.FirstOrDefault(priorPulLRequest => priorPulLRequest.PullRequestId == pullRequest.PullRequestId)));
                            var newPullrequests = linkedPullRequests.Where(linkedPullRequest => linkedPullRequest.previous == default(GitPullRequest)).Select(linkedPullRequest => linkedPullRequest.current);
                            var finishedPullRequests = _priorPullRequests.Where(priorPullRequest => !activePullRequests.Any(activePullRequest => activePullRequest.PullRequestId == priorPullRequest.PullRequestId));
                            var linkedPullRequestsNotNull = linkedPullRequests.Where(linkedPullRequest => (linkedPullRequest.current != null) && (linkedPullRequest.previous != null));
                            var pullRequestsStatusUpdated = linkedPullRequestsNotNull.Where(linkedPullRequest => linkedPullRequest.current.Status != linkedPullRequest.previous.Status);
                            var pullRequestsSourceUpdated = linkedPullRequestsNotNull.Where(linkedPullRequest => linkedPullRequest.current.LastMergeSourceCommit.CommitId != linkedPullRequest.previous.LastMergeSourceCommit.CommitId);
                            var pullRequestsReviewersAdded = linkedPullRequestsNotNull.Select(linkedPullRequest => new Tuple<GitPullRequest, IEnumerable<IdentityRefWithVote>>(linkedPullRequest.current, linkedPullRequest.current.Reviewers.Where(currentReviewer => !linkedPullRequest.previous.Reviewers.Any(previousReviewer => previousReviewer.Id == currentReviewer.Id)))).Where(tuple => tuple.Item2.Any());
                            var pullRequestsReviewersRemoved = linkedPullRequestsNotNull.Select(linkedPullRequest => new Tuple<GitPullRequest, IEnumerable<IdentityRefWithVote>>(linkedPullRequest.current, linkedPullRequest.previous.Reviewers.Where(previousReviewer => !linkedPullRequest.current.Reviewers.Any(currentReviewer => currentReviewer.Id == previousReviewer.Id)))).Where(tuple => tuple.Item2.Any());

                            if (newPullrequests.Any())
                            {
                                OnNewPullRequests?.Invoke(this, newPullrequests);
                            }

                            if (finishedPullRequests.Any())
                            {
                                var updatedPullRequests = await Task.WhenAll(finishedPullRequests.Select(async pullRequest => await GetPullRequestAsync(pullRequest.Repository.Id, pullRequest.PullRequestId)));
                                var successfulPullRequests = updatedPullRequests.Where(pullRequest => pullRequest != null && (pullRequest.Status == PullRequestStatus.Completed));
                                var unsuccessfulPullRequests = updatedPullRequests.Where(pullRequest => pullRequest != null && (pullRequest.Status == PullRequestStatus.Abandoned));

                                if (successfulPullRequests.Any())
                                {
                                    OnSuccessfulPullRequest?.Invoke(this, successfulPullRequests);
                                }

                                if (unsuccessfulPullRequests.Any())
                                {
                                    OnUnsuccessfulPullRequest?.Invoke(this, unsuccessfulPullRequests);
                                }
                            }

                            if (pullRequestsStatusUpdated.Any())
                            {
                                OnPullRequestStatusChanged?.Invoke(this, pullRequestsStatusUpdated.Select(pullRequest => new Tuple<GitPullRequest, PullRequestStatus?, PullRequestStatus?>(pullRequest.current, pullRequest.previous.Status, pullRequest.current.Status)));
                            }

                            if (pullRequestsSourceUpdated.Any())
                            {
                                OnPullRequestSourceUpdated?.Invoke(this, pullRequestsSourceUpdated.Select(pullRequest => pullRequest.current));
                            }

                            if (pullRequestsReviewersAdded.Any())
                            {
                                OnPullRequestReviewersAdded?.Invoke(this, pullRequestsReviewersAdded);
                            }

                            if (pullRequestsReviewersRemoved.Any())
                            {
                                OnPullRequestReviewersRemoved?.Invoke(this, pullRequestsReviewersRemoved);
                            }
                        }
                        else
                        {
                            _initialised = true;
                        }
                    }

                    if (NextPingForPullRequestsAwaitingReview <= DateTime.UtcNow)
                    {
                        NextPingForPullRequestsAwaitingReview = DateTime.UtcNow.Add(PullRequestsAwaitingReviewInterval);

                        var pullRequestsAwaitingReview = await GetPullRequestsAwaitingReviewAsync();
                        if (pullRequestsAwaitingReview.Any())
                        {
                            OnPullRequestsAwaitingReview?.Invoke(this, pullRequestsAwaitingReview);
                        }
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    UpdateLastCheckTime();

                    _priorPullRequests = activePullRequests.ToList();
                }
            }
        }
    }
}
