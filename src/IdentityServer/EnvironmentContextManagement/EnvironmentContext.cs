namespace IdentityServer.EnvironmentContextManagement
{
    using System.Collections.Generic;

    public class EnvironmentContext
    {
        public bool IsServiceFabric { get; set; }

        public IDictionary<string, string> Values { get; } = new Dictionary<string, string>();
    }
}