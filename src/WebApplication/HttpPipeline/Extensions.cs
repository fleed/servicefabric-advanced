using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.HttpPipeline
{
    using System.IdentityModel.Tokens.Jwt;

    using IdentityModel;

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
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "cookies",
                AutomaticAuthenticate = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(60)
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var oidcOptions = new OpenIdConnectOptions
            {
                AuthenticationScheme = "oidc",
                SignInScheme = "cookies",

                Authority = settings.Value.Authentication.Authority,
                RequireHttpsMetadata = settings.Value.Authentication.RequireHttpsMetadata,
                PostLogoutRedirectUri = settings.Value.Authentication.PostLogoutRedirectUri,
                ClientId = "mvc",
                ResponseType = "id_token",
                SaveTokens = true,

                TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role,
                }
            };

            oidcOptions.Scope.Clear();
            oidcOptions.Scope.Add("openid");
            oidcOptions.Scope.Add("profile");
            oidcOptions.Scope.Add(Scopes.WebApplication);

            app.UseOpenIdConnectAuthentication(oidcOptions);
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
            });

            return app;
        }
    }
}
