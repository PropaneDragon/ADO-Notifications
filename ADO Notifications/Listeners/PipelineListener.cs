using ADO_Notifications.ADO;
using ADO_Notifications.API;
using Microsoft.TeamFoundation.Build.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ADO_Notifications.Listeners
{
    internal class PipelineListener : AbstractListener
    {
        private bool _initialised = false;
        private List<Build> _priorBuilds = new();

        public event EventHandler<IEnumerable<Build>> OnNewBuilds;
        public event EventHandler<IEnumerable<Tuple<Build, BuildStatus?, BuildStatus?>>> OnBuildStatusChanged;
        public event EventHandler<IEnumerable<Build>> OnSuccessfulBuild;
        public event EventHandler<IEnumerable<Build>> OnUnsuccessfulBuild;

        public async Task<IEnumerable<Build>> GetBuildsInProgressAsync()
        {
            var connection = ConnectionHolder.Connection;
            if (connection?.HasAuthenticated ?? false)
            {
                var buildClient = connection.GetClient<BuildHttpClient>();
                if (buildClient != null)
                {
                    CancellationTokenSource source = new(TimeSpan.FromSeconds(20));

                    return await buildClient.GetBuildsAsync(AzureUriBuilder.Project, statusFilter: BuildStatus.NotStarted | BuildStatus.Postponed | BuildStatus.InProgress | BuildStatus.Cancelling, cancellationToken: source.Token);
                }
            }

            return new List<Build>();
        }

        public async Task<Build> GetBuildAsync(int id)
        {
            var connection = ConnectionHolder.Connection;
            if (connection?.HasAuthenticated ?? false)
            {
                var buildClient = connection.GetClient<BuildHttpClient>();
                if (buildClient != null)
                {
                    CancellationTokenSource source = new(TimeSpan.FromSeconds(20));

                    return await buildClient.GetBuildAsync(AzureUriBuilder.Project, id, cancellationToken: source.Token);
                }
            }

            return null;
        }

        protected override async Task TimeoutAsync()
        {
            var activeBuilds = await GetBuildsInProgressAsync();

            try
            {
                if (activeBuilds != null)
                {
                    if (_initialised)
                    {
                        var linkedBuilds = activeBuilds.Select(build => (current: build, previous: _priorBuilds.FirstOrDefault(priorBuild => priorBuild.Id == build.Id)));
                        var newBuilds = linkedBuilds.Where(linkedBuild => linkedBuild.previous == default(Build)).Select(linkedBuild => linkedBuild.current);
                        var finishedBuilds = _priorBuilds.Where(priorBuild => !activeBuilds.Any(activeBuild => activeBuild.BuildNumber == priorBuild.BuildNumber));
                        var buildStatusChanged = linkedBuilds.Where(linkedBuild => (linkedBuild.current != null) && (linkedBuild.previous != null) && (linkedBuild.current.Status != linkedBuild.previous.Status));

                        if (newBuilds.Any())
                        {
                            OnNewBuilds?.Invoke(this, newBuilds);
                        }

                        if (finishedBuilds.Any())
                        {
                            var updatedBuilds = await Task.WhenAll(finishedBuilds.Select(async build => await GetBuildAsync(build.Id)));
                            var successfulBuilds = updatedBuilds.Where(build => build != null && (build.Result == BuildResult.Succeeded || build.Result == BuildResult.PartiallySucceeded));
                            var unsuccessfulBuilds = updatedBuilds.Where(build => build != null && (build.Result == BuildResult.Failed || build.Result == BuildResult.Canceled));

                            if (successfulBuilds.Any())
                            {
                                OnSuccessfulBuild?.Invoke(this, successfulBuilds);
                            }

                            if (unsuccessfulBuilds.Any())
                            {
                                OnUnsuccessfulBuild?.Invoke(this, unsuccessfulBuilds);
                            }
                        }

                        if (buildStatusChanged.Any())
                        {
                            OnBuildStatusChanged?.Invoke(this, buildStatusChanged.Select(build => new Tuple<Build, BuildStatus?, BuildStatus?>(build.current, build.previous.Status, build.current.Status)));
                        }
                    }
                    else
                    {
                        _initialised = true;
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                _priorBuilds = activeBuilds.ToList();
            }
        }            
    }
}
