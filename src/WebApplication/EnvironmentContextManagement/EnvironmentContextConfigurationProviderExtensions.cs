namespace WebApplication.EnvironmentContextManagement
{
    using Microsoft.Extensions.Configuration;

    public static class EnvironmentContextConfigurationProviderExtensions
    {
        public static IConfigurationBuilder AddEnvironmentContextConfiguration(this IConfigurationBuilder builder, EnvironmentContext environmentContext)
        {
            return builder.Add(new EnvironmentContextConfigurationSource(environmentContext));
        }
    }
}