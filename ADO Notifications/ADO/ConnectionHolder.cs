using ADO_Notifications.API;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.WebApi;
using System.Threading;
using System.Threading.Tasks;

namespace ADO_Notifications.ADO
{
    internal static class ConnectionHolder
    {
        public static VssConnection Connection { get; private set; } = null;
        public static Identity? User => Connection?.AuthorizedIdentity;

        public static async Task<bool> SetCredentialsAsync(VssCredentials credentials, CancellationToken cancellationToken = default)
        {
            Connection = new VssConnection(AzureUriBuilder.OrganisationUri, credentials);

            await Connection.ConnectAsync(cancellationToken);

            return Connection.HasAuthenticated;
        }
    }
}
