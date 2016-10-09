namespace IdentityServer
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using IdentityServer.EnvironmentContextManagement;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.LiterateConsole().CreateLogger();
            try
            {
                var isServiceFabric = args.Length > 0 && args.Contains("-sf");

                if (isServiceFabric)
                {
                    Log.Logger.Information("Running under service fabric");
                    RunServiceFabric();
                    return;
                }

                Log.Information("Running as standalone application");
                RunStandalone();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error during execution. Message: {message}", exception.Message);
#if DEBUG
                Console.WriteLine("Type <Enter> to exit");
                Console.ReadLine();
#endif
            }
        }

        private static void RunServiceFabric()
        {
            try
            {
                ServiceRuntime.RegisterServiceAsync(
                    "IdentityServerType",
                    context => new WebHostingService(context, "IdentityServerServiceEndpoint")).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Error while starting Service Fabric");
            }
        }

        /// <summary>
        /// A specialized stateless service for hosting ASP.NET Core web apps.
        /// </summary>
        internal sealed class WebHostingService : StatelessService, ICommunicationListener
        {
            private readonly string endpointName;

            private IWebHost webHost;

            public WebHostingService(StatelessServiceContext serviceContext, string endpointName)
                : base(serviceContext)
            {
                this.endpointName = endpointName;
            }

            #region StatelessService

            protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
            {
                return new[] { new ServiceInstanceListener(_ => this) };
            }

            #endregion StatelessService

            #region ICommunicationListener

            void ICommunicationListener.Abort()
            {
                this.webHost?.Dispose();
            }

            Task ICommunicationListener.CloseAsync(CancellationToken cancellationToken)
            {
                this.webHost?.Dispose();

                return Task.FromResult(true);
            }

            Task<string> ICommunicationListener.OpenAsync(CancellationToken cancellationToken)
            {
                var endpoint = FabricRuntime.GetActivationContext().GetEndpoint(this.endpointName);

                string serverUrl = $"{endpoint.Protocol}://{FabricRuntime.GetNodeContext().IPAddressOrFQDN}:{endpoint.Port}";

                try
                {
                    this.webHost = new WebHostBuilder().UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<Startup>()
                        .UseUrls(serverUrl)
                        .UseServiceFabricContext(this.Context)
                        .Build();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during startup");
                    throw;
                }

                this.webHost.Start();

                return Task.FromResult(serverUrl);
            }

            #endregion ICommunicationListener
        }

        private static void RunStandalone()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var configBuilder = new ConfigurationBuilder().SetBasePath(currentDirectory);
            configBuilder = configBuilder.AddJsonFile("hosting.json");

            var config = configBuilder.Build();

            var host =
                new WebHostBuilder()
                .UseKestrel()
                    .UseConfiguration(config)
                    .UseContentRoot(currentDirectory)
                    .UseStartup<Startup>()
                    .UseStandaloneContext()
                    .Build();
            host.Run();
        }
    }
}