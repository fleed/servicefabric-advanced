using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer
{
    using Newtonsoft.Json;

    public class Settings
    {   
        public string[] MvcRedirectUris { get; set; }
        
        public string[] MvcPostLogoutRedirectUris { get; set; }
        
        public string[] ApiRedirectUris { get; set; }
    }
}