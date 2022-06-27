using System;

namespace ADO_Notifications.API
{
    internal class AzureUriBuilder
    {
        public static readonly string Organisation = "AVEVA-VSTS";
        public static readonly string Project = "Point%20Cloud%20Manager";

        public static readonly Uri BaseUri = new("https://dev.azure.com/");

        public static Uri OrganisationUri = new(BaseUri, $"{Organisation}/");
        public static Uri ProjectUri = new(OrganisationUri, $"{Project}/");

        public static Uri BuildOrganisationUri(string relativeUri) => new(OrganisationUri, relativeUri);
        public static Uri BuildProjectUri(string relativeUri) => new(ProjectUri, relativeUri);
    }
}
