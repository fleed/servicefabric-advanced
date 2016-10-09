namespace Gateway
{
    using System;
    using System.Linq;
    using System.Net;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Client;

    using Serilog;

    /// <summary>
    /// The startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">
        /// The env.
        /// </param>
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            this.Configuration =
                new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.user.json", optional: true)
                    .AddEnvironmentVariables()
                    .Build();
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// Adds services to the container.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultHttpRequestDispatcherProvider();
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        /// <param name="app">
        /// The application.
        /// </param>
        /// <param name="env">
        /// The environment.
        /// </param>
        /// <param name="loggerFactory">
        /// The logger factory.
        /// </param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(this.Configuration).CreateLogger();
            app.MapGateway(string.Empty, "WebApplication");

#pragma warning disable 1998
            app.Run(
                async request =>
                    {
                        request.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await
                            request.Response.WriteAsync(
                                "Unrecognized request. The Gateway is not configured to handle it");
                    });
#pragma warning restore 1998
        }

        private ServicePartitionKey GetPartitionAsync(HttpContext context)
        {
            var pathSegments = context.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var updateId = pathSegments.First();
            var hashCode = Fnv1AHashCode.Get64BitHashCode(updateId);
            return new ServicePartitionKey(hashCode);
        }
    }
}