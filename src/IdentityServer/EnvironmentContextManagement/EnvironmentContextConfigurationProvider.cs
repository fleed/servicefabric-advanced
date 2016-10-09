namespace IdentityServer.EnvironmentContextManagement
{
    using Microsoft.Extensions.Configuration;

    public class EnvironmentContextConfigurationProvider : ConfigurationProvider
    {
        private readonly EnvironmentContext context;

        public EnvironmentContextConfigurationProvider(EnvironmentContext context)
        {
            this.context = context;
        }

        public override void Load()
        {
            if (this.context == null || !this.context.IsServiceFabric)
            {
                return;
            }

            foreach (var value in this.context.Values)
            {
                this.Data.Add(value);
            }
        }
    }
}