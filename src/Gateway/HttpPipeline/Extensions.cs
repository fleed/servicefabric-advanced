using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.HttpPipeline
{
    using Gateway.EnvironmentContextManagement;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    public static class Extensions
    {
        public static IWebHostBuilder AddEnvironmentContext(this IWebHostBuilder builder)
        {
            return builder.ConfigureServices(services => services.AddSingleton(new EnvironmentContext()));
        }
    }
}
