namespace Gateway.EnvironmentContextManagement
{
    using System;
    using System.Fabric;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public static class EnvironmentContextExtensions
    {
        public static IWebHostBuilder UseStandaloneContext(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices(c => c.AddSingleton(_ => new EnvironmentContext()));
        }

        public static IWebHostBuilder UseServiceFabricContext(
            this IWebHostBuilder builder,
            StatelessServiceContext context)
        {
            var environmentContext = new EnvironmentContext { IsServiceFabric = true };
            try
            {
                var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                foreach (var parameter in configurationPackage.Settings.Sections["Configuration"].Parameters)
                {
                    environmentContext.Values.Add(parameter.Name, parameter.Value);
                }
            }
            catch (Exception exception)
            {
            }

            return builder.ConfigureServices(c => c.AddSingleton(environmentContext));
        }
    }
}