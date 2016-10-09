namespace Gateway
{
    using System;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.ServiceFabric.AspNetCore.Gateway;
    using Microsoft.ServiceFabric.Services.Communication.Client;

    /// <summary>
    /// Defines extension methods useful to configure the gateway behavior.
    /// </summary>
    public static class Extensions
    {
        public static IApplicationBuilder MapGateway(this IApplicationBuilder app, string path, string serviceName)
        {
            var options = GetOptions(path, GetApplicationUri(serviceName));
            return app.Map(
                path,
                subApp =>
                {
                    subApp.RunGateway(options);
                });
        }

        private static GatewayOptions GetOptions(string relativePath, Uri serviceUri)
        {
            var unitServiceOptions = new GatewayOptions
            {
                RelativePath = new Uri(relativePath, UriKind.Relative),
                ServiceUri = serviceUri,
                OperationRetrySettings =
                                                 new OperationRetrySettings(
                                                 TimeSpan.FromSeconds(2),
                                                 TimeSpan.FromSeconds(2),
                                                 30)
            };
            return unitServiceOptions;
        }

        internal const string ApplicationName = "SFApplication";

        private static Uri GetApplicationUri(string serviceName)
        {
            return new Uri($"fabric:/{ApplicationName}/{serviceName}", UriKind.Absolute);
        }
    }
}