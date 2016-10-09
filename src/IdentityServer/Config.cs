using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer
{
    using System.Security.Claims;

    using IdentityServer4.Models;
    using IdentityServer4.Services.InMemory;

    public static class Config
    {

        public static List<InMemoryUser> GetUsers()
        {
            return new List<InMemoryUser>
                       {
                           new InMemoryUser
                               {
                                   Subject = "1",
                                   Username = "test@mail.com",
                                   Password = "very-secure",
                                   Claims =
                                       new List<Claim>
                                           {
                                               new Claim(Claims.WebDev.Docs, "Read"),
                                               new Claim(Claims.WebDev.AccessLevel, AccessLevel.Root),
                                               new Claim("given_name", "Francesco"),
                                               new Claim("name", "lef")
                                           }
                               }
                       };
        }

        public static IEnumerable<Scope> GetScopes()
        {
            return new List<Scope>
                       {
                           StandardScopes.OpenId,
                           StandardScopes.Profile,
                           new Scope
                               {
                                   Name = Scopes.WebApplication,
                                   DisplayName = "Web access functionalities",
                                   Description = "Required to be able to access all website functionalities.",
                                   Claims =
                                       new List<ScopeClaim>
                                           {
                                               new ScopeClaim(Claims.WebDev.Docs),
                                               new ScopeClaim(Claims.WebDev.AccessLevel)
                                           },
                                   Type = ScopeType.Identity
                               }
                       };
        }

        public static IEnumerable<Client> GetClients(Settings settings)
        {
            return new List<Client>
                       {
                           new Client
                               {
                                   ClientId = "mvc",
                                   ClientName = "MVC Client",
                                   AllowedGrantTypes = GrantTypes.Implicit,
                                   RedirectUris = settings.MvcRedirectUris.ToList(),
                                   PostLogoutRedirectUris = settings.MvcPostLogoutRedirectUris.ToList(),
                                   AllowedScopes =
                                       new List<string>
                                           {
                                               StandardScopes.OpenId.Name,
                                               StandardScopes.Profile.Name,
                                               Scopes.WebApplication,
                                               Scopes.Trello
                                           }
                               }
                       };
        }
    }
}
