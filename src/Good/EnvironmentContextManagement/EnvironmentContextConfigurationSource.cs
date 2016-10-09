namespace WebApplication.EnvironmentContextManagement
{
    using Microsoft.Extensions.Configuration;

    public class EnvironmentContextConfigurationSource : IConfigurationSource
    {
        private readonly EnvironmentContext context;

        public EnvironmentContextConfigurationSource(EnvironmentContext context)
        {
            this.context = context;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new EnvironmentContextConfigurationProvider(this.context);
        }
    }
}