using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.WebApi;
using System;

namespace ADO_Notifications.ADO
{
    internal static class IdentityExtensions
    {
        public static bool CompareRef(this Identity? identity, IdentityRef? @ref) => identity != null && @ref != null && Guid.TryParse(@ref.Id, out var refGuid) && identity.Id == refGuid;
    }
}
