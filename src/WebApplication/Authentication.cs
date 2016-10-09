using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication
{
    public class Authentication
    {
        public string Authority { get; set; }

        public bool RequireHttpsMetadata { get; set; }

        public string PostLogoutRedirectUri { get; set; }
    }
}
