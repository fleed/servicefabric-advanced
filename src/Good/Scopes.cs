using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication
{
    internal static class Scopes
    {
        public const string OpenId = "openid";

        public const string Profile = "profile";

        public const string WebApplication = "WebApp";

        public static readonly IEnumerable<string> AllScopes = new[] { OpenId, Profile, WebApplication };
    }
}