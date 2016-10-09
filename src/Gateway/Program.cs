namespace Gateway
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Fabric;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Gateway.EnvironmentContextManagement;
    using Gateway.HttpPipeline;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    using Serilog;

    using StatelessService = Microsoft.ServiceFabric.Services.Runtime.StatelessService;

    public class Program
    {
        private const string ServiceTypeName = "GatewayType";

        private const string EndpointName = "GatewayServiceEndpoint";

        private static readonly string[] LocalAddresses = { "localhost", "127.0.0.1" };

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
                ServiceRuntime.RegisterServiceAsync(ServiceTypeName, context => new WebHostingService(context, EndpointName))
                    .GetAwaiter()
                    .GetResult();

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
            private readonly string _endpointName;

            private IWebHost webHost;

            public WebHostingService(StatelessServiceContext serviceContext, string endpointName)
                : base(serviceContext)
            {
                this._endpointName = endpointName;
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
                var endpoint = FabricRuntime.GetActivationContext().GetEndpoint(this._endpointName);

                var address = FabricRuntime.GetNodeContext().IPAddressOrFQDN;
                var serverUrls =
                    ImmutableArray.Create(
                        $"{endpoint.Protocol}://{address}:{endpoint.Port}");
                if (!LocalAddresses.Contains(address))
                {
                    serverUrls = serverUrls.Add($"{endpoint.Protocol}://localhost:{endpoint.Port}");
                }

                this.webHost =
                    new WebHostBuilder().UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseStartup<Startup>()
                        .UseUrls(serverUrls.ToArray())
                        .UseServiceFabricContext(this.Context)
                        .Build();
                this.webHost.Start();

                return Task.FromResult(serverUrls[0]);
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
                new WebHostBuilder().UseKestrel()
                    .UseConfiguration(config)
                    .UseContentRoot(currentDirectory)
                    .UseIISIntegration()
                    .AddEnvironmentContext()
                    .UseStartup<Startup>()
                    .Build();
            host.Run();
        }
    }
}