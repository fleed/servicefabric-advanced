using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.HttpPipeline
{
    using System.IdentityModel.Tokens.Jwt;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using WebApplication.EnvironmentContextManagement;

    public static class Extensions
    {
        public static IWebHostBuilder AddEnvironmentContext(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices(services => services.AddSingleton(new EnvironmentContext()));
        }

        public static IApplicationBuilder UseAuthentication(this IApplicationBuilder app)
        {
            var settings = app.ApplicationServices.GetService<IOptions<Settings>>();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies"
            });

            var options = new OpenIdConnectOptions
            {
                AuthenticationScheme = "oidc",
                SignInScheme = "Cookies",
                AutomaticChallenge = true,
                Authority = settings.Value.Authentication.Authority,
                RequireHttpsMetadata = settings.Value.Authentication.RequireHttpsMetadata,
                PostLogoutRedirectUri = settings.Value.Authentication.PostLogoutRedirectUri,
                ClientId = "mvc",
                SaveTokens = true,
                Scope = { Scopes.WebApplication }
            };
            app.UseOpenIdConnectAuthentication(options);
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
            });

            return app;
        }
    }
}
