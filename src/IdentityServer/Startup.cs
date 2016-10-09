namespace IdentityServer
{
    using System.IO;

    using IdentityServer.EnvironmentContextManagement;

    using IdentityServer4;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Serilog;

    public class Startup
    {
        public Startup(IHostingEnvironment env, EnvironmentContext environmentContext)
        {
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.LiterateConsole().CreateLogger();

            Log.Information("Startup");
            // Set up configuration sources.
            var currentDirectory = Directory.GetCurrentDirectory();

            var builder =
                new ConfigurationBuilder().SetBasePath(currentDirectory)
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.user.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            if (env.IsDevelopment())
            {
                Log.Debug("Development environment");
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables("ISAPP_");
            builder.AddEnvironmentContextConfiguration(environmentContext);

            this.Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(this.Configuration).CreateLogger();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Settings>(this.Configuration.GetSection("Settings"));

            var section = this.Configuration.GetSection("Settings");
            var settings = new Settings();

            new ConfigureFromConfigurationOptions<Settings>(section).Configure(settings);

            // Add framework services.
            services.AddMvc();
            services.AddDeveloperIdentityServer()
                .AddInMemoryScopes(Config.GetScopes())
                .AddInMemoryClients(Config.GetClients(settings))
                .AddInMemoryUsers(Config.GetUsers());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            loggerFactory.AddSerilog();
            app.UseDeveloperExceptionPage();

            app.UseIdentityServer();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                AutomaticAuthenticate = false,
                AutomaticChallenge = false,
            });
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
            });

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}